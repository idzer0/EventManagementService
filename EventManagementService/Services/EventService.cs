
using EventManagementService.Contracts;
using EventManagementService.DomainExceptions;
using EventManagementService.Infrastructure;
using EventManagementService.Models;
using EventManagementService.Services.Mappers;
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
        return events.Select(e => EventMapper.MapToResponse(e));
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
            Items = [.. events.Select(e => EventMapper.MapToResponse(e))]
        };
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var ev = await _repository.GetByIdAsync(id, ct);

        return ev is not null ? EventMapper.MapToResponse(ev) : throw new ObjectNotFoundDomainException($"Событие с Id {id} не найдено.");;
    }

    /// <inheritdoc/>
    public async Task<EventResponse> CreateAsync(EventRequest createEventRequest, CancellationToken ct)
    {
        if (createEventRequest.EndAt < createEventRequest.StartAt)
            throw new ValidationDomainException("Дата окончания события должна быть больше или равна дате начала.");

        if (string.IsNullOrEmpty(createEventRequest.Title))
            throw new ValidationDomainException("Название события не может быть пустым.");


        var newEvent = new EventEntity {
            Id = Guid.NewGuid(),
            Title = createEventRequest.Title,
            Description = createEventRequest.Description ?? string.Empty,
            StartAt = createEventRequest.StartAt,
            EndAt = createEventRequest.EndAt
        };

        await _repository.CreateAsync(newEvent, ct);

        return EventMapper.MapToResponse(newEvent);
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> UpdateAsync(Guid id, EventRequest updateEvent, CancellationToken ct)
    {
        if (updateEvent.EndAt < updateEvent.StartAt)
            throw new ValidationDomainException("Дата окончания события должна быть больше или равна дате начала.");

        var entity = EventMapper.MapToEntity(id, updateEvent);

        await _repository.UpdateAsync(entity, ct);

        return EventMapper.MapToResponse(entity);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        return _repository.DeleteAsync(id, ct);
    }

    /// <inheritdoc/>
    public Task<bool> IsExistAsync(Guid id, CancellationToken ct)
    {
        return _repository.IsExistsAsync(id, ct);
    }
}
