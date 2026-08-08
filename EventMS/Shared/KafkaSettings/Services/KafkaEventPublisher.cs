using Confluent.Kafka;
using KafkaSettingsShared.Contracts;
using Microsoft.Extensions.Logging;

namespace KafkaSettingsShared.Services;

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
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Ошибка при отправке сообщения в Kafka");
            throw;
        }
    }
}
