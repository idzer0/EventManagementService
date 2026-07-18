using System.Security.Claims;
using Application.Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var id))
                return id;
            return null;
        }
    }

    public int? Role
    {
        get
        {
            var role = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
            if (role != null && int.TryParse(role.Value, out var id))
                return id;
            return null;
        }
    }

    public string UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    public IEnumerable<Claim> Claims => _httpContextAccessor.HttpContext?.User?.Claims ?? [];

    /// <inheritdoc/>
    public bool IsAllowUserOperation(int? userId = default)
    {
        return UserId is not null && (UserId == (userId ?? UserId) || Role == (int)UsersRole.Admin);
    }

    /// <inheritdoc/>
    public bool IsAllowAdminOperation()
    {
        return UserId is not null && Role == (int)UsersRole.Admin;
    }
}
