namespace EventManagementService.Contracts;

using EventManagementService.Models;

public interface IEventService
{
    /// <summary>
    /// Возвращает список всех событий
    /// </summary>
    Task<IEnumerable<EventResponse>> GetAllAsync();

    /// <summary>
    /// Получить событие по Id
    /// </summary>
    Task<EventResponse?> GetByIdAsync(Guid eventId);

    /// <summary>
    /// Создать событие
    /// </summary>
    Task<EventResponse> CreateAsync(EventRequest events);
    
    /// <summary>
    /// Обновить событие
    /// </summary>
    Task<EventResponse?> UpdateAsync(Guid id, EventRequest updateEvent);

    /// <summary>
    /// Удалить событие
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}