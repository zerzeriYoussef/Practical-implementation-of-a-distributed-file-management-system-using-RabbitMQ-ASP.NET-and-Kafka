namespace JwtAuthDemo.Application.DTOs;

public sealed record RegisterRequest(
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    string Password);
