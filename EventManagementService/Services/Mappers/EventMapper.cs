using EventManagementService.Models;

namespace EventManagementService.Services.Mappers;

public static class EventMapper
{
    public static EventResponse MapToResponse(EventEntity ev) =>
        new()
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt
        };

    public static EventEntity MapToEntity(Guid Id, EventRequest ev) =>
        new()
        {
            Id = Id,
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt
        };
}
