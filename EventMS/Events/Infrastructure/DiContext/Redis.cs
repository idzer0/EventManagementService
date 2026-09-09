using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EventMS.Events.Infrastructure.DiContext.Redis;

public static class DependencyInjection
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisSettings>(configuration.GetSection("RedisSettings"));

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;

                var options = new ConfigurationOptions
                {
                    EndPoints = { { settings.Server, settings.Port } },
                    Password = settings.Password,
                    ConnectTimeout = settings.ConnectTimeout,
                    SyncTimeout = settings.SyncTimeout,
                    AbortOnConnectFail = settings.AbortOnConnectFail,
                    ReconnectRetryPolicy = new LinearRetry(5000),
                };

                return ConnectionMultiplexer.Connect(options);
            }
        );

        return services;
    }
}