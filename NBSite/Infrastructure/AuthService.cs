using Microsoft.AspNetCore.Authentication.Cookies;
using Domain.Entities;
using System.Security.Claims;

namespace NBSite.Infrastructure
{
    public interface IAuthService
    {
        Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthUser user);
    }

    public class AuthService : IAuthService
    {
        public Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(AuthUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("IsActive", user.IsActive.ToString()),
                new Claim("IsStaff", user.IsStaff.ToString()),
                new Claim("IsSuperuser", user.IsSuperuser.ToString()),
                new Claim("FirstName", user.FirstName ?? string.Empty),
                new Claim("LastName", user.LastName ?? string.Empty)
            };

            // Добавляем роли на основе полей пользователя
            if (user.IsSuperuser)
            {
                claims.Add(new Claim(ClaimTypes.Role, "superuser"));
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
                claims.Add(new Claim(ClaimTypes.Role, "user"));
            }
            else if (user.IsStaff)
            {
                claims.Add(new Claim(ClaimTypes.Role, "admin"));
                claims.Add(new Claim(ClaimTypes.Role, "user"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "user"));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            return Task.FromResult(principal);
        }
    }
}
