namespace EventManagementService.Services;

using EventManagementService.Models;

public class EventService : IEventService
{
    private readonly List<Event> _events = new();
    private readonly ILogger<EventService> _logger;

    public EventService(ILogger<EventService> logger)
    {
        _logger = logger;

        //тестовое событие 
        _events.Add(new Event
        {
            Id = Guid.NewGuid(),
            Title = "Тестовое событие",
            Description = "Это событие создано с целью проверки работоспособности сервиса",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(2)
        });
    }

    /// <inheritdoc/>
    public Task<IEnumerable<EventResponse>> GetAllAsync()
    {
        var result = _events.Select(e => MapToResponse(e));
        return Task.FromResult(result);
    }

    /// <inheritdoc/>
    public Task<EventResponse?> GetByIdAsync(Guid id)
    {
        var ev = _events.FirstOrDefault(e => e.Id == id);

        return Task.FromResult(ev is not null ? MapToResponse(ev) : null);
    }

    /// <inheritdoc/>
    public Task<EventResponse> CreateAsync(EventRequest eventCreate)
    {
        if (eventCreate.EndAt <= eventCreate.StartAt)
            throw new ArgumentException("Дата окочания события должна быть больше или равна дате начала");

        var newEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = eventCreate.Title,
            Description = eventCreate.Description ?? string.Empty,
            StartAt = eventCreate.StartAt,
            EndAt = eventCreate.EndAt
        };

        _events.Add(newEvent);
        _logger.LogInformation("Событие создано с Id: {Id}", newEvent.Id);

        return Task.FromResult(MapToResponse(newEvent));
    }

    /// <inheritdoc/>
    public Task<EventResponse?> UpdateAsync(Guid id, EventRequest updateEvent)
    {
        var existing = _events.FirstOrDefault(e => e.Id == id);
        if (existing is null)
            return Task.FromResult<EventResponse?>(null);

        // Бизнес-валидация: EndAt > StartAt
        if (updateEvent.EndAt <= updateEvent.StartAt)
            throw new ArgumentException("Дата окочания события должна быть больше или равна дате начала");

        existing.Title = updateEvent.Title;
        existing.Description = updateEvent.Description ?? string.Empty;
        existing.StartAt = updateEvent.StartAt;
        existing.EndAt = updateEvent.EndAt;

        _logger.LogInformation("Событие создано с Id: {Id}", id);
        return Task.FromResult<EventResponse?>(MapToResponse(existing));
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(Guid id)
    {
        var ev = _events.FirstOrDefault(e => e.Id == id);
        if (ev is null)
            return Task.FromResult(false);

        _events.Remove(ev);
        _logger.LogInformation("Событие с: {Id} удалено", id);
        return Task.FromResult(true);
    }

    private static EventResponse MapToResponse(Event ev) =>
        new()
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            StartAt = ev.StartAt,
            EndAt = ev.EndAt
        };
}
