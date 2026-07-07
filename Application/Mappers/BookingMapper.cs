using Application.DTO;
using Domain.Models;

namespace Application.Mappers;

public static class BookingMapper
{
    public static BookingInfo MapToResponse(BookingEntity entity) =>
        new()
        {
            Id = entity.Id,
            EventId = entity.Event?.Id ?? throw new Exception("Связанное событие не должно отсутствовать"),
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ProcessedAt = entity.ProcessedAt,
        };
}
