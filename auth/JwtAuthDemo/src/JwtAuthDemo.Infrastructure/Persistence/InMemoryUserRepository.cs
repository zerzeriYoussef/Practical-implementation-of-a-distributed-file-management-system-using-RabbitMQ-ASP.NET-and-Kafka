using JwtAuthDemo.Application.Auth;
using JwtAuthDemo.Application.Interfaces;
using JwtAuthDemo.Domain.Entities;

namespace JwtAuthDemo.Infrastructure.Persistence;

public sealed class InMemoryUserRepository : IUserRepository
{
    private static readonly List<User> Users = new()
    {
        new()
        {
            Id = Guid.Parse("8a83cfc1-7e3c-4b2f-bb2a-169394a70d16"),
            UserName = "admin",
            Email = "admin@jwtauthdemo.local",
            FirstName = "Admin",
            LastName = "User",
            PasswordHash = AuthenticationService.HashPassword("password"),
            RefreshToken = string.Empty,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        },
        new()
        {
            Id = Guid.Parse("785e2e56-0860-4df7-9825-c0df122c46e4"),
            UserName = "hamza",
            Email = "hamza@jwtauthdemo.local",
            FirstName = "Hamza",
            LastName = "User",
            PasswordHash = AuthenticationService.HashPassword("123456789"),
            RefreshToken = string.Empty,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        }
    };

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = Users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult<User?>(user);
    }

    public Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var user = Users.FirstOrDefault(u => string.Equals(u.UserName, userName, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<User?>(user);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = Users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<User?>(user);
    }

    public Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = Users.FirstOrDefault(u => string.Equals(u.RefreshToken, refreshToken, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<User?>(user);
    }

    public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        Users.Add(user);
        return Task.FromResult(user);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var index = Users.FindIndex(existingUser => existingUser.Id == user.Id);
        if (index >= 0)
        {
            Users[index] = user;
        }

        return Task.CompletedTask;
    }
}
