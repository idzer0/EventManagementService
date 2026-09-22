using System.Security.Claims;

namespace EventMS.Auth.Application.Contracts;

public interface IJwtTokenGenerator
{
    string GenerateToken(IEnumerable<Claim> claims, DateTime expires);
}