namespace JwtAuthDemo.Application.DTOs;

public sealed record LoginResponse(
    string Token,
    string UserName,
    string Email,
    string RefreshToken,
    DateTime RefreshTokenExpiryTime);
