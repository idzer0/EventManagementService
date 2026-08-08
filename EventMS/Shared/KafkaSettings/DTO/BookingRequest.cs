using KafkaSettingsShared.Enums;

namespace KafkaSettingsShared.DTO;

public class BookingRequest
{
    public Guid BookingId {get; set;}

    public Guid EventId {get; set;}

    public BookingActionTypeEnum BookingActionType { get; set; }

    public DateTime CreatedAt { get; set; }
}
