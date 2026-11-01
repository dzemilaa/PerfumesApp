using HotChocolate;
using loginservice.Data;
using loginservice.Models;
using Microsoft.EntityFrameworkCore;

namespace loginservice.Graphql.NewFolder
{
    public class AuthQuery
    {

        [GraphQLName("getAllUsers")]
        public async Task<List<UserView>> GetAllUsers([Service] AuthDbContext context)
        {
            return await context.Users
                .Select(u => new UserView
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    IsVerified = u.IsVerified,
                    VerificationToken = u.VerificationToken
                })
                .ToListAsync();
        }

        [GraphQLName("searchUsers")]
        public async Task<List<UserView>> SearchUsers(string query, [Service] AuthDbContext context)
        {
            return await context.Users
                .Where(u => EF.Functions.ILike(u.Username, $"{query}%") ||
                            EF.Functions.ILike(u.Email, $"{query}%"))
                .Select(u => new UserView
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive
                })
                .ToListAsync();
        }
    }

    public class UserView
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsVerified { get; set; }
        public string? VerificationToken { get; set; }
    }
}