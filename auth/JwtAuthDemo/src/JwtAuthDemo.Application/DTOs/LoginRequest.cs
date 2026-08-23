namespace JwtAuthDemo.Application.DTOs;

public sealed record LoginRequest(string UserName, string Password);
