using Application.Services;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.ServicesBackground;

public abstract class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaSettings _settings;

    protected KafkaConsumerService(
        ILogger logger,
        IOptions<KafkaSettings> settings,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            EnableAutoCommit = _settings.EnableAutoCommit,
            AutoOffsetReset = Enum.Parse<AutoOffsetReset>(_settings.AutoOffsetReset),
            // ... аутентификация
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected abstract Task HandleMessageAsync(
        string key,
        string value,
        IServiceProvider scopeServiceProvider,
        CancellationToken ct);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _consumer.Subscribe(_settings.IncomingTopic);
        _logger.LogInformation("Подписка на топик {Topic} запущена", _settings.IncomingTopic);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(ct);
                    if (consumeResult == null) continue;

                    using var scope = _serviceProvider.CreateScope();
                    await HandleMessageAsync(consumeResult.Message.Key, consumeResult.Message.Value, scope.ServiceProvider, ct);

                    if (!_settings.EnableAutoCommit)
                        _consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Ошибка при чтении сообщения из Kafka");
                    await Task.Delay(1000, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Неизвестная ошибка в обработчике Kafka");
                }
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Consumer закрыт");
        }
    }
}