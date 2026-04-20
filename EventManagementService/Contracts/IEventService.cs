using EventManagementService.Models;

namespace EventManagementService.Contracts;

public interface IEventService
{
    /// <summary>
    /// Возвращает список всех событий
    /// </summary>
    Task<IEnumerable<EventResponse>> GetAllAsync();

    /// <summary>
    /// Возвращает события по фильтру для постраничного вывода
    /// </summary>
    Task<PaginatedResponse<EventResponse>> GetPaginatedEventsAsync(EventsFilter filter);

    /// <summary>
    /// Получить событие по Id
    /// </summary>
    Task<EventResponse?> GetByIdAsync(Guid eventId);

    /// <summary>
    /// Создать событие
    /// </summary>
    Task<EventResponse> CreateAsync(EventRequest createEventRequest);
    
    /// <summary>
    /// Обновить событие
    /// </summary>
    Task<EventResponse?> UpdateAsync(Guid id, EventRequest updateEventRequest);

    /// <summary>
    /// Удалить событие
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}