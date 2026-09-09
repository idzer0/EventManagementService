using System.Text.Json;
using EventMS.Events.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace EventMS.Events.Application.Services;

public class CacheService(IRedisCacheService cache, ILogger<CacheService> logger) : ICacheService
{
    private readonly IRedisCacheService _cache = cache;
    private readonly ILogger<CacheService> _logger = logger;

    /// <inheritdoc/>
    public async Task<T> GetValueAsync<T>(string key, int databaseId = 0) where T : class
    {
        var valueString = await _cache.GetValueAsync(key, databaseId);

        if (string.IsNullOrEmpty(valueString))
        {
            _logger.LogWarning("Ключ {Key} не найден в Redis", key);
            return null;
        }

        try
        {
            T? result = JsonSerializer.Deserialize<T>(valueString);
            if (result is null)
                _logger.LogError("Не удалось десериализовать значение для ключа {Key}", key);

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Ошибка десериализации JSON для ключа {Key}", key);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SetValueAsync<T>(string key, T value, TimeSpan? expiry = null, int databaseId = 0) where T : class
    {
        try
        {
            string? valueString = JsonSerializer.Serialize<T>(value);
            return await _cache.SetValueAsync(key, valueString, expiry, databaseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка сохранения в Redis. Ключ: {key}", key);
            return false;
        }
    }
}
