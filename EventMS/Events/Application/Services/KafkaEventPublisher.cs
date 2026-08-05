using Application.Contracts;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaEventPublisher> _logger;

    public KafkaEventPublisher(IProducer<string, string> producer, ILogger<KafkaEventPublisher> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task PublishAsync(string topic, string key, string message)
    {
        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = key,
                Value = message
            });
            _logger.LogInformation("Сообщение отправлено в топик {Topic}, партиция {Partition}, оффсет {Offset}",
                topic, result.Partition, result.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Ошибка при отправке сообщения в Kafka");
            throw;
        }
    }
}
