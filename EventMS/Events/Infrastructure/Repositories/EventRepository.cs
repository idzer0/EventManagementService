using System.Data;
using Application.Contracts;
using Application.DTO;
using Domain.DomainExceptions;
using Domain.Models;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories;

/// <summary>
/// Репозиторий событий.
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;
    private readonly IRedisCacheService _redis;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(AppDbContext context, IRedisCacheService redis, ILogger<EventRepository> logger)
    {
        _context = context;
        _redis = redis;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventEntity>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Events.ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<EventEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

        return ev;
    }

    /// <inheritdoc/>
    public Task<EventEntity[]> GetTopSaledAsync(CancellationToken ct)
    {
        return _context.Events
            .Where(e => e.TotalSeats > 0)
            .OrderByDescending(e => (double)(e.TotalSeats - e.AvailableSeats) / e.TotalSeats)
            .Take(10)
            .ToArrayAsync(ct);
    }

    /// <inheritdoc/>
    public Task<bool> IsExistsAsync(Guid id, CancellationToken ct)
    {
        return _context.Events.AnyAsync(e => e.Id == id, ct);
    }

    /// <inheritdoc/>
    public async Task<int> EventsCountAsync(EventsFilter filter, CancellationToken ct)
    {
        var query = GetQueryByFilterEvents(filter);

        return await query.CountAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<List<EventEntity>> GetPaginatedEventsAsync(EventsFilter filter, CancellationToken ct)
    {
        // Базовый запрос
        var query = GetQueryByFilterEvents(filter);

        // Пагинация через LINQ (Skip/Take)
        var items = await query
            .OrderBy(e => e.StartAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return items;
    }


    /// <inheritdoc/>
    public async Task<EventEntity> CreateAsync(EventEntity createEventRequest, CancellationToken ct)
    {
        var newEvent = new EventEntity {
            Id = Guid.NewGuid(),
            Title = createEventRequest.Title,
            Description = createEventRequest.Description ?? string.Empty,
            StartAt = createEventRequest.StartAt,
            EndAt = createEventRequest.EndAt,
            TotalSeats = createEventRequest.TotalSeats,
            AvailableSeats = createEventRequest.AvailableSeats,
        };

        await _context.Events.AddAsync(newEvent, ct);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Событие создано с Id: {Id}", newEvent.Id);

        return newEvent;
    }

    /// <inheritdoc/>
    public async Task<EventEntity> UpdateAsync(EventEntity updateEventRequest, CancellationToken ct)
    {
        if (!await IsExistsAsync(updateEventRequest.Id, ct))
            throw new ObjectNotFoundDomainException($"Событие с Id {updateEventRequest.Id} не найдено.");


        _context.Events.Update(updateEventRequest);
        await _context.SaveChangesAsync(ct);

        if(!await _redis.RemoveKeyAsync($"event:{updateEventRequest.Id}"))
        {
            _logger.LogError("Ошибка удаления из кеша по ключу event:{value}", updateEventRequest.Id);
        }

        _logger.LogInformation("Событие обновлено с Id: {Id}", updateEventRequest.Id);

        return await _context.Events.SingleAsync(e => e.Id == updateEventRequest.Id, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new ObjectNotFoundDomainException($"Событие с Id {id} не найдено");

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync(ct);

        if(!await _redis.RemoveKeyAsync($"event:{id}"))
        {
            _logger.LogError("Ошибка удаления из кеша по ключу event:{value}", id);
        }

        _logger.LogInformation("Событие с: {Id} удалено", id);
        return true;
    }

    /// <summary>
    /// Возвращает IQueryable для Events по фильтру
    /// </summary>
    private IQueryable<EventEntity> GetQueryByFilterEvents(EventsFilter filter)
    {
        if (filter.Page == 0 || filter.PageSize == 0)
            throw new ValidationDomainException("Номер страницы и размер страницы не могут быть равны нулю.");

        // Базовый запрос
        var query = _context.Events.AsNoTracking().AsQueryable();

        // Фильтрация через LINQ
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            query = query.Where(e => EF.Functions.ILike(e.Title, $"%{filter.Title}%"));
        }

        if (filter.From.HasValue)
        {
            query = query.Where(e => e.StartAt >= filter.From.Value);
        }

        if (filter.To.HasValue)
        {
            query = query.Where(e => e.EndAt <= filter.To.Value);
        }

        return query;
    }
}
