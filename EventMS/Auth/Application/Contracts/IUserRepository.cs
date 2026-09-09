using EventMS.Auth.Domain.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace EventMS.Auth.Application.Contracts;

public interface IUserRepository
{
    /// <summary>
    /// Проверяет, есть ли пользователь с таким логином.
    /// </summary>
    Task<bool> IsExistsAsync(string login, CancellationToken ct);

    /// <summary>
    /// Поиск пользователя по логину.
    /// </summary>
    Task<UserEntity?> GetUserByLoginAsync(string login, CancellationToken ct);

    /// <summary>
    /// Создание нового пользователя.
    /// </summary>
    Task CreateUserAsync(UserEntity user, CancellationToken ct);
}
