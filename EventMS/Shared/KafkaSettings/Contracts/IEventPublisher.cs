namespace KafkaSettingsShared.Contracts;

public interface IEventPublisher
{
    Task PublishAsync(string topic, string key, string message);
}
