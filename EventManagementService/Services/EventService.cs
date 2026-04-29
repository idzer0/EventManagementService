
using EventManagementService.Contracts;
using EventManagementService.Infrastructure;
using EventManagementService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Services;

/// <summary>
/// Сервис обработки событий.
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _repository;
    private readonly ILogger<EventService> _logger;

    public EventService(IEventRepository repository, ILogger<EventService> logger)
    {
        _repository = repository;
        _logger = logger;

    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventResponse>> GetAllAsync(CancellationToken ct)
    {
        var events = await _repository.GetAllAsync(ct);
        return events.Select(e => MapToResponse(e));
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<EventResponse>> GetPaginatedEventsAsync(EventsFilter filter, CancellationToken ct)
    {
        var events = await _repository.GetPaginatedEventsAsync(filter, ct);

        return new PaginatedResponse<EventResponse>()
        {
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = await _repository.EventsCountAsync(filter, ct),
            Items = [.. events.Select(e => MapToResponse(e))]
        };
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var ev = await _repository.GetByIdAsync(id, ct);

        return ev is not null ? MapToResponse(ev) : throw new KeyNotFoundException($"Событие с ID {id} не найдено.");;
    }

    /// <inheritdoc/>
    public async Task<EventResponse> CreateAsync(EventRequest createEventRequest, CancellationToken ct)
    {
        if (createEventRequest.EndAt < createEventRequest.StartAt)
            throw new ArgumentException("Дата окончания события должна быть больше или равна дате начала.");

        if (createEventRequest.Title is null || createEventRequest.Title == string.Empty)
            throw new ArgumentException("Название события не может быть пустым.");


        var newEvent = new EventEntity {
            Id = Guid.NewGuid(),
            Title = createEventRequest.Title,
            Description = createEventRequest.Description ?? string.Empty,
            StartAt = createEventRequest.StartAt,
            EndAt = createEventRequest.EndAt
        };

        await _repository.CreateAsync(newEvent, ct);

        return MapToResponse(newEvent);
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> UpdateAsync(Guid id, EventRequest updateEvent, CancellationToken ct)
    {
        if (updateEvent.EndAt < updateEvent.StartAt)
            throw new ArgumentException("Дата окончания события должна быть больше или равна дате начала.");

        var entity = MapToEntity(id, updateEvent);

        await _repository.UpdateAsync(entity, ct);

        return MapToResponse(entity);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        return await _repository.DeleteAsync(id, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> IsExistAsync(Guid id, CancellationToken ct)
    {
        return await _repository.IsExistsAsync(id, ct);
    }

    private static EventResponse MapToResponse(EventEntity ev) =>
        new()
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt
        };

    private static EventEntity MapToEntity(Guid Id, EventRequest ev) =>
        new()
        {
            Id = Id,
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt
        };
}
