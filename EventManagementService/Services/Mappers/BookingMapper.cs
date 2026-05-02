using EventManagementService.Models;

namespace EventManagementService.Services.Mappers;

public static class BookingMapper
{
    public static BookingInfo MapToResponse(BookingEntity entity)
    {
        return new BookingInfo()
        {
            Id = entity.Id,
            EventId = entity.EventId,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ProcessedAt = entity.ProcessedAt,
        };
    }
}
