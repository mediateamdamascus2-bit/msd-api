using MedicalServices.Infrastructure.Identity;
using MedicalServices.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MedicalServices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = EnrichPooling(
            config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing."),
            config);

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.CommandTimeout(config.GetValue("Capacity:NpgsqlCommandTimeoutSeconds", 30));
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: config.GetValue("Capacity:NpgsqlMaxRetryCount", 3),
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            }));

        // Scoped context for Identity / request pipeline; factory for Blazor concurrent queries.
        services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        var tightenPasswords = !config.GetValue("Deployment:SeedDemoAccounts", false)
            && string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Environments.Production,
                StringComparison.OrdinalIgnoreCase);

        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                if (tightenPasswords)
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                }
                else
                {
                    options.Password.RequiredLength = 4;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                }

                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = config.GetValue("Deployment:LoginMaxAttempts", 5);
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(
                    config.GetValue("Deployment:LoginLockoutMinutes", 15));
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<AppClaimsPrincipalFactory>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.AccessDeniedPath = "/login";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.Cookie.Name = "msd.auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
        });

        return services;
    }

    private static string EnrichPooling(string connectionString, IConfiguration config)
    {
        var b = new Npgsql.NpgsqlConnectionStringBuilder(connectionString)
        {
            Pooling = true
        };
        if (b.MaxPoolSize <= 15)
            b.MaxPoolSize = config.GetValue("Capacity:NpgsqlMaxPoolSize", 100);
        if (b.MinPoolSize <= 0)
            b.MinPoolSize = config.GetValue("Capacity:NpgsqlMinPoolSize", 5);
        if (b.Timeout < 15)
            b.Timeout = config.GetValue("Capacity:NpgsqlConnectionTimeoutSeconds", 15);
        b.KeepAlive = config.GetValue("Capacity:NpgsqlKeepAliveSeconds", 30);
        return b.ConnectionString;
    }
}
