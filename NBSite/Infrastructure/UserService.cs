using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.Security.Claims;

namespace NBSite.Infrastructure
{
    public interface IUserService
    {
        Task<AuthUser?> GetUserByEmailAsync(string email);
        Task<AuthUser?> GetUserByUsernameAsync(string username);
        Task<bool> ValidateUserAsync(string emailOrUsername, string password);
    }

    public class UserService : IUserService
    {
        private readonly NbshopContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(NbshopContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthUser?> GetUserByEmailAsync(string email)
        {
            return await _context.AuthUsers
                .Include(u => u.AccountsProfile)
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<AuthUser?> GetUserByUsernameAsync(string username)
        {
            return await _context.AuthUsers
                .Include(u => u.AccountsProfile)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<bool> ValidateUserAsync(string emailOrUsername, string password)
        {
            var user = await _context.AuthUsers
                .FirstOrDefaultAsync(u =>
                    (u.Email == emailOrUsername || u.Username == emailOrUsername) &&
                    u.IsActive);

            if (user == null)
                return false;

            // Используем PasswordHasher вместо статического метода
            return _passwordHasher.VerifyPassword(user.Password, password);
        }
    }
}
