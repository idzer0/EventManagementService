namespace EventManagementService.Services;

using EventManagementService.Contracts;
using EventManagementService.Infrastructure;
using EventManagementService.Models;
using Microsoft.EntityFrameworkCore;

public class EventService : IEventService
{
    private readonly AppDbContext _context;
    private readonly ILogger<EventService> _logger;

    public EventService(AppDbContext context, ILogger<EventService> logger)
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
    public async Task<IEnumerable<EventResponse>> GetAllAsync()
    {
        var events = await _context.Events.ToListAsync();
        return events.Select(e => MapToResponse(e));
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);

        return ev is not null ? MapToResponse(ev) : null;
    }

    /// <inheritdoc/>
    public async Task<EventResponse> CreateAsync(EventRequest eventCreate)
    {
        if (eventCreate.EndAt <= eventCreate.StartAt)
            throw new ArgumentException("Дата окончания события должна быть больше или равна дате начала");

        var newEvent = new EventEntity {
            Id = Guid.NewGuid(),
            Title = eventCreate.Title,
            Description = eventCreate.Description ?? string.Empty,
            StartAt = eventCreate.StartAt,
            EndAt = eventCreate.EndAt
        };

        await _context.Events.AddAsync(newEvent);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Событие создано с Id: {Id}", newEvent.Id);

        return MapToResponse(newEvent);
    }

    /// <inheritdoc/>
    public async Task<EventResponse?> UpdateAsync(Guid id, EventRequest updateEvent)
    {
        var existing = _context.Events.FirstOrDefault(e => e.Id == id);
        if (existing is null)
            return null;

        // Бизнес-валидация: EndAt > StartAt
        if (updateEvent.EndAt <= updateEvent.StartAt)
            throw new ArgumentException("Дата окончания события должна быть больше или равна дате начала");

        var entity = MapToEntity(id, updateEvent);
        _context.Events.Update(entity);        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Событие обновлено с Id: {Id}", id);
        return MapToResponse(entity);
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
