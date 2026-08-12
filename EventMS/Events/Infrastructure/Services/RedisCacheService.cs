using Application.Contracts;
using StackExchange.Redis;

namespace Infrastructure.Services;

public class RedisCacheService(IConnectionMultiplexer redis) : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis = redis;

    /// <inheritdoc/>
    public async Task<string?> GetValueAsync(string key, int databaseId = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var db = _redis.GetDatabase(databaseId);
        RedisValue value = await db.StringGetAsync(key);

        return value.IsNullOrEmpty ? null : (string?)value;
    }

    /// <inheritdoc/>
    public async Task<bool> SetValueAsync(string key, string value, TimeSpan? expiry = null, int databaseId = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        if (value is null)
            throw new ArgumentNullException(nameof(value));

        var db = _redis.GetDatabase(databaseId);

        bool setResult = await db.StringSetAsync(key, value);
        if (!setResult) return false;

        if (expiry.HasValue)
        {
            await db.KeyExpireAsync(key, expiry.Value);
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveKeyAsync(string key, int databaseId = 0)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var db = _redis.GetDatabase(databaseId);

        return await db.KeyDeleteAsync(key);
    }
}