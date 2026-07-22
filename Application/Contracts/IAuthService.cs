using Application.DTO;
using Domain.Models;

namespace Application.Contracts;

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
