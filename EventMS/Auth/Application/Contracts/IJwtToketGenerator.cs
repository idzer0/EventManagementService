using System.Security.Claims;

namespace Application.Contracts;

public interface IJwtTokenGenerator
{
    string GenerateToken(IEnumerable<Claim> claims, DateTime expires);
}