using System.Security.Claims;
using HealthPlus.Domain.Entities;

namespace HealthPlus.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    DateTime GetAccessTokenExpiry();
}
