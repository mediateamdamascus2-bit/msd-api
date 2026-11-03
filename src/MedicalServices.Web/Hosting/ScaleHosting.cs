using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;

namespace MedicalServices.Web.Hosting;

/// <summary>
/// Capacity knobs for Blazor Server + API under many concurrent hospital users.
/// Each interactive browser tab keeps a server-side SignalR circuit in memory.
/// </summary>
public static class ScaleHosting
{
    public static WebApplicationBuilder AddScaleCapacity(this WebApplicationBuilder builder)
    {
        var capacity = builder.Configuration.GetSection("Capacity");
        var maxConnections = capacity.GetValue("KestrelMaxConcurrentConnections", 2000);
        var maxUpgraded = capacity.GetValue("KestrelMaxConcurrentUpgradedConnections", 1500);
        var maxRequestBody = capacity.GetValue("MaxRequestBodyBytes", 2 * 1024 * 1024);
        var hubMessageKb = capacity.GetValue("HubMaximumReceiveMessageKb", 64);
        var keepAliveSeconds = capacity.GetValue("HubKeepAliveSeconds", 15);
        var clientTimeoutSeconds = capacity.GetValue("HubClientTimeoutSeconds", 60);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxConcurrentConnections = maxConnections;
            options.Limits.MaxConcurrentUpgradedConnections = maxUpgraded;
            options.Limits.MaxRequestBodySize = maxRequestBody;
            options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
            options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
            options.AddServerHeader = false;
        });

        var minWorker = capacity.GetValue("MinWorkerThreads", 0);
        var minIo = capacity.GetValue("MinIoThreads", 0);
        if (minWorker > 0 || minIo > 0)
        {
            ThreadPool.GetMinThreads(out var w, out var io);
            ThreadPool.SetMinThreads(
                minWorker > 0 ? minWorker : w,
                minIo > 0 ? minIo : io);
        }

        builder.Services.AddSignalR(options =>
        {
            options.MaximumReceiveMessageSize = hubMessageKb * 1024L;
            options.KeepAliveInterval = TimeSpan.FromSeconds(keepAliveSeconds);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(clientTimeoutSeconds);
            options.HandshakeTimeout = TimeSpan.FromSeconds(
                capacity.GetValue("HubHandshakeTimeoutSeconds", 15));
            options.EnableDetailedErrors = builder.Environment.IsDevelopment();
        });

        builder.Services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            [
                "application/octet-stream",
                "application/json",
                "text/plain"
            ]);
        });

        var apiPermit = capacity.GetValue("ApiPermitLimitPerMinute", 600);
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("api", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = apiPermit,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 50,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));
        });

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            // Hospital reverse proxy / load balancer on LAN.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        builder.Services.AddHealthChecks()
            .AddCheck<PostgresHealthCheck>("postgres");

        return builder;
    }

}
