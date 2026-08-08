using System.Security.Claims;
using Domain.Models;
using Auth.Contracts;

namespace Application.Contracts;

public interface ICurrentUserService
{
    int? UserId { get; }
    UsersRole? Role { get; }
    string UserName { get; }
    bool IsAuthenticated { get; }
    IEnumerable<Claim> Claims { get; }

    /// <summary>
    /// Проверяет, соответствует ли идентификатор пользователя идентификатору прошедшего аутентификацию.
    /// </summary>
    bool IsAllowUserOperation(int? userId = default);

    /// <summary>
    /// Проверяет, соответствует ли идентификатор пользователя идентификатору прошедшего аутентификацию.
    /// </summary>
    bool IsAllowAdminOperation();
}