using System.Security.Claims;

namespace Application.Contracts;

public interface ICurrentUserService
{
    int? UserId { get; }
    int? Role { get; }
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