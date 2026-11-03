using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedicalServices.Domain.Enums;
using MedicalServices.Infrastructure.Identity;
using MedicalServices.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MedicalServices.Web.Api;

public static class AuthApi
{
    public static RouteGroupBuilder MapAuthApi(this RouteGroupBuilder api)
    {
        api.MapPost("/auth/login", async (
            HttpContext http,
            [FromBody] LoginDto dto,
            UserManager<ApplicationUser> users,
            IConfiguration config,
            LoginRateLimiter limiter) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return Results.BadRequest(new { error = "username_and_password_required" });

            var userName = dto.Username.Trim();
            var ip = http.Connection.RemoteIpAddress?.ToString();

            if (limiter.IsBlocked(userName, ip))
            {
                return Results.Json(
                    new { error = "too_many_attempts", message = "محاولات كثيرة. حاول لاحقاً." },
                    statusCode: StatusCodes.Status429TooManyRequests);
            }

            var user = await users.FindByNameAsync(userName);
            if (user is null)
            {
                limiter.RegisterFailure(userName, ip);
                return Results.Unauthorized();
            }

            if (await users.IsLockedOutAsync(user))
            {
                limiter.RegisterFailure(userName, ip);
                return Results.Json(
                    new { error = "locked_out", message = "الحساب مقفل مؤقتاً بسبب محاولات فاشلة." },
                    statusCode: StatusCodes.Status423Locked);
            }

            if (!await users.CheckPasswordAsync(user, dto.Password))
            {
                await users.AccessFailedAsync(user);
                limiter.RegisterFailure(userName, ip);
                return Results.Unauthorized();
            }

            await users.ResetAccessFailedCountAsync(user);
            limiter.RegisterSuccess(userName, ip);

            var roles = await users.GetRolesAsync(user);
            var token = CreateToken(user, roles, config);
            var homePath = LoginRouting.ResolveHomePath(user, roles);
            var expires = int.Parse(config["Jwt:ExpiresMinutes"] ?? "60");

            return Results.Ok(new
            {
                accessToken = token,
                tokenType = "Bearer",
                expiresInMinutes = expires,
                user = new
                {
                    username = user.UserName,
                    displayNameAr = user.DisplayNameAr,
                    roles,
                    facilityId = user.FacilityId,
                    homePath
                }
            });
        }).AllowAnonymous();

        api.MapGet("/auth/me", async (ClaimsPrincipal principal, UserManager<ApplicationUser> users) =>
        {
            var userName = principal.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userName))
                return Results.Unauthorized();

            var user = await users.FindByNameAsync(userName);
            if (user is null)
                return Results.Unauthorized();

            var roles = await users.GetRolesAsync(user);
            return Results.Ok(new
            {
                username = user.UserName,
                displayNameAr = user.DisplayNameAr,
                roles,
                facilityId = user.FacilityId,
                homePath = LoginRouting.ResolveHomePath(user, roles)
            });
        }).RequireAuthorization(new AuthorizeAttribute
        {
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
        });

        return api;
    }

    private static string CreateToken(ApplicationUser user, IList<string> roles, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresMinutes = int.Parse(config["Jwt:ExpiresMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.GivenName, user.DisplayNameAr),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (!string.IsNullOrWhiteSpace(user.FacilityId))
            claims.Add(new Claim(AppClaimTypes.FacilityId, user.FacilityId));

        var homePath = LoginRouting.ResolveHomePath(user, roles);
        claims.Add(new Claim(AppClaimTypes.HomePath, homePath));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public sealed record LoginDto(string Username, string Password);
}
