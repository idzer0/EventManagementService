namespace EventMS.Events.Application.Services;

public static class RedisValueKeys
{
    /// <summary>
    /// Наименование ключа Top10
    /// </summary>
    public const string Top10Saled = "events:top10";

    /// <summary>
    /// Ключ хранения отдельного события
    /// </summary>
    public static string EventKey(Guid eventId) => $"event:{eventId}";
}
