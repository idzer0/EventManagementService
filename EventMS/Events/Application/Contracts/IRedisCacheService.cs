
namespace EventMS.Events.Application.Contracts;

public interface IRedisCacheService
{
    /// <summary>
    /// Получить строковое значение по ключу.
    /// </summary>
    /// <param name="key">Ключ.</param>
    /// <param name="databaseId">Номер БД (по умолчанию 0).</param>
    /// <returns>Значение или null, если ключ не найден.</returns>
    Task<string?> GetValueAsync(string key, int databaseId = 0);

    /// <summary>
    /// Установить строковое значение по ключу с опциональным сроком жизни.
    /// </summary>
    /// <param name="key">Ключ.</param>
    /// <param name="value">Значение.</param>
    /// <param name="expiry">Время жизни (null — бессрочно).</param>
    /// <param name="databaseId">Номер БД.</param>
    /// <returns>true, если операция успешна.</returns>
    Task<bool> SetValueAsync(string key, string value, TimeSpan? expiry = null, int databaseId = 0);

    /// <summary>
    /// Удалить ключ.
    /// </summary>
    /// <param name="key">Ключ.</param>
    /// <param name="databaseId">Номер БД.</param>
    /// <returns>true, если ключ существовал и был удалён.</returns>
    Task<bool> RemoveKeyAsync(string key, int databaseId = 0);
}