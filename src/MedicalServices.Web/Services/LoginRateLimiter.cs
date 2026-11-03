using System.Collections.Concurrent;

namespace MedicalServices.Web.Services;

/// <summary>Simple in-memory brute-force throttle for login endpoints (per username + IP).</summary>
public sealed class LoginRateLimiter
{
    private readonly ConcurrentDictionary<string, Window> _windows = new(StringComparer.OrdinalIgnoreCase);
    private readonly int _maxAttempts;
    private readonly TimeSpan _window;

    public LoginRateLimiter(IConfiguration config)
    {
        _maxAttempts = Math.Max(3, config.GetValue("Deployment:LoginMaxAttempts", 8));
        var minutes = Math.Max(1, config.GetValue("Deployment:LoginWindowMinutes", 15));
        _window = TimeSpan.FromMinutes(minutes);
    }

    public bool IsBlocked(string? userName, string? ip)
    {
        var key = Key(userName, ip);
        if (!_windows.TryGetValue(key, out var w))
            return false;
        if (DateTimeOffset.UtcNow - w.Started > _window)
        {
            _windows.TryRemove(key, out _);
            return false;
        }

        return w.Failures >= _maxAttempts;
    }

    public void RegisterFailure(string? userName, string? ip)
    {
        var key = Key(userName, ip);
        _windows.AddOrUpdate(
            key,
            _ => new Window(DateTimeOffset.UtcNow, 1),
            (_, existing) =>
            {
                if (DateTimeOffset.UtcNow - existing.Started > _window)
                    return new Window(DateTimeOffset.UtcNow, 1);
                return existing with { Failures = existing.Failures + 1 };
            });
    }

    public void RegisterSuccess(string? userName, string? ip)
    {
        _windows.TryRemove(Key(userName, ip), out _);
        if (!string.IsNullOrWhiteSpace(userName))
            _windows.TryRemove(Key(userName, null), out _);
    }

    private static string Key(string? userName, string? ip) =>
        $"{(userName ?? "").Trim().ToLowerInvariant()}|{ip ?? "unknown"}";

    private sealed record Window(DateTimeOffset Started, int Failures);
}
