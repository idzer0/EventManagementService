
namespace Application.Contracts;

public interface ICacheService
{
    /// <summary>
    /// Получить и десериализовать строковое значение по ключу.
    /// </summary>
    /// <param name="key">Ключ.</param>
    /// <param name="databaseId">Номер БД (по умолчанию 0).</param>
    /// <returns>Значение или null, если ключ не найден.</returns>
    Task<T> GetValueAsync<T>(string key, int databaseId = 0) where T : class;

    /// <summary>
    /// Сериализовать и установить строковое значение по ключу с опциональным сроком жизни.
    /// </summary>
    /// <param name="key">Ключ.</param>
    /// <param name="value">Значение.</param>
    /// <param name="expiry">Время жизни (null — бессрочно).</param>
    /// <param name="databaseId">Номер БД.</param>
    /// <returns>true, если операция успешна.</returns>
    Task<bool> SetValueAsync<T>(string key, T value, TimeSpan? expiry = null, int databaseId = 0) where T : class;
}