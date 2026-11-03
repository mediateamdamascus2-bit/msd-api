using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicalServices.Web.Services;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/login", async (
            HttpContext http,
            [FromForm] LoginRequest request,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            LoginRateLimiter limiter) =>
        {
            var userName = (request.Username ?? string.Empty).Trim();
            var password = request.Password ?? string.Empty;
            var ip = http.Connection.RemoteIpAddress?.ToString();

            if (limiter.IsBlocked(userName, ip))
                return Results.Redirect("/login?error=1&locked=1");

            var user = await userManager.FindByNameAsync(userName);
            if (user is null)
            {
                limiter.RegisterFailure(userName, ip);
                return Results.Redirect(ErrorUrl(request.ReturnUrl));
            }

            var result = await signInManager.PasswordSignInAsync(
                user, password, isPersistent: request.RememberMe, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                limiter.RegisterFailure(userName, ip);
                return Results.Redirect("/login?error=1&locked=1");
            }

            if (!result.Succeeded)
            {
                limiter.RegisterFailure(userName, ip);
                return Results.Redirect(ErrorUrl(request.ReturnUrl));
            }

            limiter.RegisterSuccess(userName, ip);

            var roles = await userManager.GetRolesAsync(user);
            var home = LoginRouting.ResolveHomePath(user, roles);

            // Station accounts ignore ReturnUrl so they cannot be pulled into the main UI.
            var isStationLocked = roles.Contains(AppRoles.StationUser) || roles.Contains(AppRoles.BookingClerk);
            if (!isStationLocked
                && !string.IsNullOrWhiteSpace(request.ReturnUrl)
                && request.ReturnUrl.StartsWith('/')
                && !request.ReturnUrl.StartsWith("//")
                && LoginRouting.IsAllowedPath(home, request.ReturnUrl, roles))
            {
                return Results.Redirect(LoginRouting.ToLocationHeader(request.ReturnUrl));
            }

            return Results.Redirect(LoginRouting.ToLocationHeader(home));
        }).DisableAntiforgery().AllowAnonymous();

        endpoints.MapGet("/auth/logout", async (SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/login");
        });

        endpoints.MapPost("/auth/logout", async (SignInManager<ApplicationUser> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Redirect("/login");
        }).DisableAntiforgery();

        return endpoints;
    }

    private static string ErrorUrl(string? returnUrl) =>
        string.IsNullOrWhiteSpace(returnUrl)
            ? "/login?error=1"
            : $"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl)}";

    public sealed class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
