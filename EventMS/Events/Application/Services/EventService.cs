using System.Text.Json;
using Application.Contracts;
using Application.DTO;
using Application.Mappers;
using Domain.DomainExceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Сервис обработки событий.
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _repository;
    private readonly ICacheService _cache;
    private readonly ILogger<EventService> _logger;

    public EventService(IEventRepository repository, ICacheService cache, ILogger<EventService> logger)
    {
        _repository = repository;
        _cache = cache;
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
    public async Task<EventResponse[]> GetTopSaledAsync(CancellationToken ct)
    {
        EventEntity[] top10 = await _cache.GetValueAsync<EventEntity[]>("event:top10");

        if (top10 is null)
        {
            top10 = await _repository.GetTopSaledAsync(ct);
            await _cache.SetValueAsync("event:top10", JsonSerializer.Serialize(top10));
        }

        return Array.ConvertAll(top10, e => EventMapper.MapToResponse(e));
    }

    /// <inheritdoc/>
    public async Task<EventResponse> GetByIdAsync(Guid id, CancellationToken ct)
    {
        EventEntity? ev = await _cache.GetValueAsync<EventEntity>($"event:{id}");

        if (ev is null)
        {
            ev = await _repository.GetByIdAsync(id, ct)
                ?? throw new ObjectNotFoundDomainException($"Событие с Id {id} не найдено.");

            await _cache.SetValueAsync<EventEntity>($"event:{id}", ev);
        }

        return EventMapper.MapToResponse(ev);
    }

    /// <inheritdoc/>
    public async Task<EventResponse> CreateAsync(EventRequest createEventRequest, CancellationToken ct)
    {
        if (createEventRequest.EndAt < createEventRequest.StartAt)
            throw new ValidationDomainException("Дата окончания события должна быть больше или равна дате начала.");

        if (string.IsNullOrEmpty(createEventRequest.Title))
            throw new ValidationDomainException("Название события не может быть пустым.");

        if (createEventRequest.TotalSeats < 1)
            throw new ValidationDomainException("Количество мест должно быть больше нуля.");

        var newEvent = new EventEntity {
            Id = Guid.NewGuid(),
            Title = createEventRequest.Title,
            Description = createEventRequest.Description ?? string.Empty,
            StartAt = createEventRequest.StartAt,
            EndAt = createEventRequest.EndAt,
            TotalSeats = createEventRequest.TotalSeats,
            AvailableSeats = createEventRequest.TotalSeats,
        };

        await _repository.CreateAsync(newEvent, ct);

        return EventMapper.MapToResponse(newEvent);
    }

    /// <inheritdoc/>
    public async Task<EventResponse> UpdateAsync(Guid id, EventRequest? updateEvent, CancellationToken ct)
    {
        if (updateEvent is null)
            throw new ValidationDomainException("Обновляемый объект не определен.");

        if (updateEvent.EndAt < updateEvent.StartAt)
            throw new ValidationDomainException("Дата окончания события должна быть больше или равна дате начала.");

        var entity = EventMapper.MapToEntity(id, updateEvent);

        return EventMapper.MapToResponse(await _repository.UpdateAsync(entity, ct));
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

    /// <inheritdoc/>
    public async Task<bool> ReserveSeat(Guid eventId, CancellationToken ct)
    {
        var ev = await _repository.GetByIdAsync(eventId, ct)
            ?? throw new ObjectNotFoundDomainException($"Событие не найдено. EventId: {eventId}");

        if (ev.StartAt <= DateTime.UtcNow)
            throw new ValidationDomainException($"Нельзя зарезервировать место на событие, которое уже началось. EventId: {eventId}");

        var result = ev.TryReserveSeats();

        if (result)
            await _repository.UpdateAsync(ev, ct);

        return result;
    }

    /// <inheritdoc/>
    public async Task<bool> ReleaseSeat(Guid eventId, CancellationToken ct)
    {
        var ev = await _repository.GetByIdAsync(eventId, ct)
            ?? throw new ValidationDomainException($"Событие не найдено. EventId: {eventId}");

        var result = ev.ReleaseSeats();

        if (result)
            await _repository.UpdateAsync(ev, ct);

        return result;
    }
}
