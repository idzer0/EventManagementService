using Application.Contracts;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Services;

public class RedisCacheService(IConnectionMultiplexer redis, IOptions<RedisSettings> options) : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IOptions<RedisSettings> _options = options;

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

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));

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
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        var db = _redis.GetDatabase(databaseId);

        return await db.KeyDeleteAsync(key);
    }
}