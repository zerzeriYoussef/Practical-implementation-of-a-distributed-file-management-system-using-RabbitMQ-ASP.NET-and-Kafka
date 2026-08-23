namespace JwtAuthDemo.Application.DTOs;

public sealed record RegisterResponse(
    Guid Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string Token,
    string RefreshToken,
    DateTime RefreshTokenExpiryTime);
