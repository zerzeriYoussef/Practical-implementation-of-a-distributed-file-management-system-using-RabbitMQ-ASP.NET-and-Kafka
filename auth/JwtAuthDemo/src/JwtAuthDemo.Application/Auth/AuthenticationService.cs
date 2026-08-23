using System.Security.Cryptography;
using System.Text;
using JwtAuthDemo.Application.DTOs;
using JwtAuthDemo.Application.Interfaces;
using JwtAuthDemo.Domain.Entities;

namespace JwtAuthDemo.Application.Auth;

public sealed class AuthenticationService
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;

    public AuthenticationService(IUserRepository users, ITokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public static string HashPassword(string password)
    {
        var input = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(input);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        return HashPassword(password) == storedHash;
    }

    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName)
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.FirstName)
            || string.IsNullOrWhiteSpace(request.LastName))
        {
            return null;
        }

        if (await _users.GetByUserNameAsync(request.UserName, cancellationToken) is not null)
        {
            return null;
        }

        if (await _users.GetByEmailAsync(request.Email, cancellationToken) is not null)
        {
            return null;
        }

        var user = new User
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PasswordHash = HashPassword(request.Password),
            RefreshToken = _tokens.GenerateRefreshToken(),
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
        };

        var savedUser = await _users.AddAsync(user, cancellationToken);
        var token = _tokens.GenerateToken(savedUser);

        return new RegisterResponse(
            savedUser.Id,
            savedUser.UserName,
            savedUser.Email,
            savedUser.FirstName,
            savedUser.LastName,
            token,
            savedUser.RefreshToken,
            savedUser.RefreshTokenExpiryTime);
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByUserNameAsync(request.UserName, cancellationToken);

        if (user is null || !VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        user.RefreshToken = _tokens.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _users.UpdateAsync(user, cancellationToken);

        var token = _tokens.GenerateToken(user);

        return new LoginResponse(
            token,
            user.UserName,
            user.Email,
            user.RefreshToken,
            user.RefreshTokenExpiryTime);
    }

    public async Task<LoginResponse?> RefreshTokenAsync(TokenRefreshRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null;
        }

        user.RefreshToken = _tokens.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _users.UpdateAsync(user, cancellationToken);

        var token = _tokens.GenerateToken(user);

        return new LoginResponse(
            token,
            user.UserName,
            user.Email,
            user.RefreshToken,
            user.RefreshTokenExpiryTime);
    }
}
