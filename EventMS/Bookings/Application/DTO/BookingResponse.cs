using Domain.Enums;

namespace Application.DTO;

public class BookingResponse
{
    public Guid BookingId {get; set;}

    public Guid EventId {get; set;}

    public BookingActionTypeEnum BookingActionType { get; set; }

    public bool IsSuccess { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
