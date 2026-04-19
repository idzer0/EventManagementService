using EventManagementService.Models;
using EventManagementService.Contracts;
using EventManagementService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EventManagementService.Services;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EventRepository> _logger;

    public EventRepository(AppDbContext context, ILogger<EventRepository> logger)
    {
        _context = context;
        _logger = logger;

        //добавляем тестовую запись
        if (_context.Events.Count() == 0)
        {
            _context.Events.Add(new EventEntity {
                Id = Guid.NewGuid(),
                Title = "Тестовое событие",
                Description = "Это событие создано с целью проверки работоспособности сервиса",
                StartAt = DateTime.UtcNow.AddDays(1),
                EndAt = DateTime.UtcNow.AddDays(2)      
            });
            _context.SaveChanges();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<EventEntity>> GetAllAsync()
    {
        return await _context.Events.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<EventEntity?> GetByIdAsync(Guid id)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);

        return ev;
    }

    /// <inheritdoc/>
    public async Task<bool> IsExists(Guid id)
    {
        return await _context.Events.AnyAsync(e => e.Id == id);
    }

    /// <inheritdoc/>
    public async Task<int> EventsCount()
    {
        return await _context.Events.CountAsync();
    }

    /// <inheritdoc/>
    public async Task<List<EventEntity>> GetPaginatedEventsAsync(EventsFilter filter)
    {
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
        
        // Пагинация через LINQ (Skip/Take)
        var items = await query
            .OrderBy(e => e.StartAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        
        return items;
    }


    /// <inheritdoc/>
    public async Task<EventEntity> CreateAsync(EventEntity createEventRequest)
    {
        var newEvent = new EventEntity {
            Id = Guid.NewGuid(),
            Title = createEventRequest.Title,
            Description = createEventRequest.Description ?? string.Empty,
            StartAt = createEventRequest.StartAt,
            EndAt = createEventRequest.EndAt
        };

        await _context.Events.AddAsync(newEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Событие создано с Id: {Id}", newEvent.Id);

        return newEvent;
    }

    /// <inheritdoc/>
    public async Task<EventEntity?> UpdateAsync(EventEntity updateEventRequest)
    {
        var existing = _context.Events.FirstOrDefault(e => e.Id == updateEventRequest.Id);
        if (existing is null)
            return null;

        _context.Events.Update(updateEventRequest);        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Событие обновлено с Id: {Id}", updateEventRequest.Id);

        return updateEventRequest;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var ev = _context.Events.FirstOrDefault(e => e.Id == id);
        if (ev is null)
            return false;

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Событие с: {Id} удалено", id);
        return true;
    }
}
