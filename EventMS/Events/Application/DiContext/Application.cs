using Application.Contracts;
using Application.Services;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Application.DiContext;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEventService, EventService>();

        // Регистрация продюсера
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
            var config = new ProducerConfig
            {
                BootstrapServers = settings.BootstrapServers,
                // При необходимости добавьте аутентификацию:
                // SaslUsername = settings.SaslUsername,
                // SaslPassword = settings.SaslPassword,
                // SecurityProtocol = Enum.Parse<SecurityProtocol>(settings.SecurityProtocol)
            };

            return new ProducerBuilder<string, string>(config).Build();
        });

        // Регистрация потребителя (будет использован в фоновом сервисе)
        services.AddSingleton<IConsumer<string, string>>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
            var config = new ConsumerConfig
            {
                BootstrapServers = settings.BootstrapServers,
                GroupId = settings.GroupId,
                EnableAutoCommit = settings.EnableAutoCommit,
                AutoOffsetReset = Enum.Parse<AutoOffsetReset>(settings.AutoOffsetReset),
                // SaslUsername = settings.SaslUsername,
                // SaslPassword = settings.SaslPassword,
                // SecurityProtocol = Enum.Parse<SecurityProtocol>(settings.SecurityProtocol)
            };

            return new ConsumerBuilder<string, string>(config).Build();
        });


        services.Configure<KafkaSettings>(configuration.GetSection("KafkaSettings"));

        services.AddScoped<IEventPublisher, KafkaEventPublisher>();        return services;
    }
}