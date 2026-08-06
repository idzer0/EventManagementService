namespace KafkaSettingsShared.DTO;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string IncomingTopic { get; set; } = string.Empty;
    public string OutgoingTopic { get; set; } = string.Empty;
    public string? SaslUsername { get; set; }
    public string? SaslPassword { get; set; }
    public string SecurityProtocol { get; set; } = "Plaintext";
    public bool EnableAutoCommit { get; set; } = false;
    public string AutoOffsetReset { get; set; } = "Earliest";
}
