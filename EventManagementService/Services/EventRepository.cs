using System.Data;
using EventManagementService.Contracts;
using EventManagementService.DomainExceptions;
using EventManagementService.Infrastructure;
using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagementService.Services;

/// <summary>
/// Репозиторий событий.
/// </summary>
public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(AppDbContext context, ILogger<EventRepository> logger)
    {
        _context = context;
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
            EndAt = createEventRequest.EndAt
        };

        await _context.Events.AddAsync(newEvent, ct);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Событие создано с Id: {Id}", newEvent.Id);

        return newEvent;
    }

    /// <inheritdoc/>
    public async Task<EventEntity?> UpdateAsync(EventEntity updateEventRequest, CancellationToken ct)
    {
        var existing = await _context.Events.SingleOrDefaultAsync(e => e.Id == updateEventRequest.Id, ct)
            ?? throw new ObjectNotFoundDomainException($"Событие с Id {updateEventRequest.Id} не найдено.");

        _context.Events.Update(updateEventRequest);
        await _context.SaveChangesAsync(ct);

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
        var query = _context.Events.AsQueryable();

        // Фильтрация через LINQ
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            query = query.Where(e => e.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
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
