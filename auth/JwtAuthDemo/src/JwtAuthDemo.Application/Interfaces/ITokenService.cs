using JwtAuthDemo.Domain.Entities;

namespace JwtAuthDemo.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
