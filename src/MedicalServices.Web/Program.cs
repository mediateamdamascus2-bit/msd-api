using System.Globalization;
using System.Text;
using MedicalServices.Infrastructure;
using MedicalServices.Infrastructure.Persistence;
using MedicalServices.Web.Api;
using MedicalServices.Web.Components;
using MedicalServices.Web.Hosting;
using MedicalServices.Web.Services;
using MedicalServices.Web.Services.Hasia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// واجهة عربية مع أرقام لاتينية (0-9) وليس ١٢٣
var uiCulture = ArLatnCulture.Instance;
CultureInfo.DefaultThreadCurrentCulture = uiCulture;
CultureInfo.DefaultThreadCurrentUICulture = uiCulture;

var builder = WebApplication.CreateBuilder(args);

builder.AddScaleCapacity();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        var capacity = builder.Configuration.GetSection("Capacity");
        options.DisconnectedCircuitMaxRetained = capacity.GetValue("DisconnectedCircuitMaxRetained", 200);
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(
            capacity.GetValue("DisconnectedCircuitMinutes", 3));
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(
            capacity.GetValue("JsInteropTimeoutSeconds", 60));
        options.MaxBufferedUnacknowledgedRenderBatches =
            capacity.GetValue("MaxBufferedRenderBatches", 10);
    });

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("Jwt:Key missing"))),
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFacilityCatalog, FacilityCatalog>();
builder.Services.AddScoped<IAdmissionsService, AdmissionsService>();
builder.Services.AddScoped<IUnitOpsService, UnitOpsService>();
builder.Services.AddScoped<IHasiaOpsService, HasiaOpsService>();
builder.Services.AddScoped<IReceptionService, ReceptionService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IClinicService, ClinicService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IHarastaHomeStatsService, HarastaHomeStatsService>();
builder.Services.AddScoped<IFleetService, FleetService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IPatientFileService, PatientFileService>();
builder.Services.AddSingleton<IReceptionNotifier, ReceptionNotifier>();
builder.Services.AddSingleton<ICallBoard, CallBoard>();
builder.Services.AddSingleton<LoginRateLimiter>();

ConfigureCors(builder);

ValidateProductionSecrets(builder);

var app = builder.Build();

await DbSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    if (builder.Configuration.GetValue("Deployment:ForceHttps", false))
        app.UseHsts();
}

if (builder.Configuration.GetValue("Deployment:ForceHttps", false))
    app.UseHttpsRedirection();

app.UseForwardedHeaders();

app.Use(async (ctx, next) =>
{
    ctx.Response.OnStarting(() =>
    {
        var headers = ctx.Response.Headers;
        headers["X-Frame-Options"] = "SAMEORIGIN";
        headers["X-Content-Type-Options"] = "nosniff";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        if (!headers.ContainsKey("Content-Security-Policy"))
        {
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com data:; " +
                "img-src 'self' data: blob:; " +
                "connect-src 'self' ws: wss: http: https:; " +
                "frame-ancestors 'self'";
        }

        return Task.CompletedTask;
    });
    await next();
});

app.UseResponseCompression();
app.UseCors("Mobile");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (ctx, next) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true)
    {
        var path = ctx.Request.Path.Value ?? "/";
        // Static assets only — /api/ is NOT passthrough for cookie sessions; JWT APIs self-authorize.
        if (!IsPassthroughAsset(path))
        {
            if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                // Cookie-authenticated browser calls to API still respect station home lock.
                var roles = ctx.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
                var home = ctx.User.FindFirst(MedicalServices.Infrastructure.Identity.AppClaimTypes.HomePath)?.Value;
                if (!MedicalServices.Web.Services.LoginRouting.IsAllowedPath(home, path, roles)
                    && !HasBearer(ctx))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await ctx.Response.WriteAsJsonAsync(new { error = "مسار غير مسموح لهذه الجلسة" });
                    return;
                }
            }
            else
            {
                var roles = ctx.User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
                var home = ctx.User.FindFirst(MedicalServices.Infrastructure.Identity.AppClaimTypes.HomePath)?.Value;
                if (!MedicalServices.Web.Services.LoginRouting.IsAllowedPath(home, path, roles))
                {
                    var target = string.IsNullOrWhiteSpace(home) ? "/login" : home;
                    ctx.Response.Redirect(MedicalServices.Web.Services.LoginRouting.ToLocationHeader(target));
                    return;
                }
            }
        }
    }

    await next();
});
app.UseAntiforgery();

app.MapStaticAssets();
app.MapAuthEndpoints();

var api = app.MapGroup("/api");
api.RequireRateLimiting("api");
api.MapAuthApi();
api.MapFacilitiesApi();
api.MapAdmissionsApi();
api.MapUnitOpsApi();
api.MapHasiaApi();
api.MapReceptionApi();
api.MapBookingApi();
api.MapClinicsApi();
api.MapDepartmentsApi();
api.MapHarastaStatsApi();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static void ConfigureCors(WebApplicationBuilder builder)
{
    var origins = builder.Configuration.GetSection("Deployment:CorsOrigins").Get<string[]>()
                  ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Mobile", policy =>
        {
            policy.AllowAnyHeader().AllowAnyMethod();
            if (origins.Length > 0)
            {
                policy.WithOrigins(origins);
            }
            else if (builder.Environment.IsDevelopment())
            {
                // Local Flutter Chrome / WebView only when no explicit list is configured.
                policy.SetIsOriginAllowed(origin =>
                    origin.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase)
                    || origin.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                // Production must set Deployment:CorsOrigins — fail closed to same-host only.
                policy.SetIsOriginAllowed(_ => false);
            }
        });
    });
}

static bool HasBearer(HttpContext ctx)
{
    var auth = ctx.Request.Headers.Authorization.ToString();
    return auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
}

static void ValidateProductionSecrets(WebApplicationBuilder builder)
{
    if (!builder.Environment.IsProduction())
        return;

    var jwt = builder.Configuration["Jwt:Key"] ?? string.Empty;
    if (string.IsNullOrWhiteSpace(jwt)
        || jwt.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
        || jwt.Contains("Dev_Jwt", StringComparison.OrdinalIgnoreCase)
        || jwt.Length < 32)
    {
        throw new InvalidOperationException(
            "Production requires a strong Jwt:Key (32+ chars). Set via appsettings.Production.json or env Jwt__Key.");
    }

    var cs = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    if (string.IsNullOrWhiteSpace(cs)
        || cs.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
        || cs.Contains("MsdDev_", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Production requires a real ConnectionStrings:DefaultConnection (not the dev password). Set via env ConnectionStrings__DefaultConnection.");
    }
}

static bool IsPassthroughAsset(string path)
{
    if (path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/images", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/fonts", StringComparison.OrdinalIgnoreCase))
        return true;

    var ext = Path.GetExtension(path);
    return ext.Equals(".css", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".js", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".map", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".woff", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".woff2", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".ttf", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".png", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".gif", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".svg", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".ico", StringComparison.OrdinalIgnoreCase)
        || ext.Equals(".webp", StringComparison.OrdinalIgnoreCase);
}
