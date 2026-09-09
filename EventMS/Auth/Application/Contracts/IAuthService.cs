using EventMS.Auth.Contracts;

namespace EventMS.Auth.Application.Contracts;

public interface IAuthService
{
    /// <summary>
    /// Регистрация нового пользователя.
    /// </summary>
    Task RegisterAsync(string login, string password, UsersRole role, CancellationToken ct);

    /// <summary>
    /// Аутентификация пользователя.
    /// </summary>
    Task<string> LoginAsync(string login, string password, CancellationToken ct);
}
