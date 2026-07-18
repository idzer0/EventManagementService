namespace Application.Contracts;

public interface IPasswordHasher
{
    /// <summary>
    /// Возвращает hash пароля.
    /// </summary>
    string GetHashPassword(string password);

    /// <summary>
    /// Проверяет пароль пользователя
    /// </summary>
    Task<bool> CheckPassword(string login, string password, CancellationToken ct);
}
