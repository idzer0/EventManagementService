using Application.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Services;

public class RedisCacheService(IConnectionMultiplexer redis, IOptions<RedisSettings> options, Logger<RedisCacheService> logger) : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IOptions<RedisSettings> _options = options;

    /// <inheritdoc/>
    public async Task<string?> GetValueAsync(string key, int databaseId = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            logger.LogWarning("Key for {param} cannot be null or empty.", nameof(this.GetValueAsync));
            return null;
        }

        var db = _redis.GetDatabase(databaseId);
        RedisValue value = await db.StringGetAsync(key);

        return value.IsNullOrEmpty ? null : (string?)value;
    }

    /// <inheritdoc/>
    public async Task<bool> SetValueAsync(string key, string value, TimeSpan? expiry = null, int databaseId = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            logger.LogWarning("Key for {param} cannot be null or empty.", nameof(this.SetValueAsync));
            return false;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            logger.LogWarning("Value for {param} cannot be null or empty.", nameof(this.SetValueAsync));
            return false;
        }

        var db = _redis.GetDatabase(databaseId);

        bool setResult = await db.StringSetAsync(key, value);
        if (!setResult) return false;

        var expiration = expiry ?? TimeSpan.FromMinutes(_options.Value.DefaultTimeSpanMinutes);
        return await db.StringSetAsync(key, value, expiration);
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveKeyAsync(string key, int databaseId = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            logger.LogWarning("Key for {param} cannot be null or empty.", nameof(this.RemoveKeyAsync));
            return false;
        }

        var db = _redis.GetDatabase(databaseId);

        return await db.KeyDeleteAsync(key);
    }
}