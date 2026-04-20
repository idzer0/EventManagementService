
using EventManagementService.Contracts;
using EventManagementService.Infrastructure;
using EventManagementService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Services;
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
    public async Task<IEnumerable<EventResponse>> GetAllAsync()
    {
        var events = await _repository.GetAllAsync();
        return events.Select(e => MapToResponse(e));
    }

    /// <inheritdoc/>
    public async Task<PaginatedResponse<EventResponse>> GetPaginatedEventsAsync(EventsFilter filter)
    {
        var events = await _repository.GetPaginatedEventsAsync(filter);
        
        return new PaginatedResponse<EventResponse>()
        {
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = await _repository.EventsCount(filter),
            Items = [.. events.Select(e => MapToResponse(e))]
        };
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var ev = await _repository.GetByIdAsync(id);

        return ev is not null ? MapToResponse(ev) : throw new KeyNotFoundException($"Событие с ID {id} не найдено.");;
    }

    /// <inheritdoc/>
    public async Task<EventResponse> CreateAsync(EventRequest createEventRequest)
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

        await _repository.CreateAsync(newEvent);

        return MapToResponse(newEvent);
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> UpdateAsync(Guid id, EventRequest updateEvent)
    {
        if (updateEvent.EndAt < updateEvent.StartAt)
            throw new ArgumentException("Дата окончания события должна быть больше или равна дате начала.");

        var entity = MapToEntity(id, updateEvent);
              
        await _repository.UpdateAsync(entity);
        
        return MapToResponse(entity);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
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
