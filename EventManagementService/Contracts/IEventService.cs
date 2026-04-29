using EventManagementService.Models;

namespace EventManagementService.Contracts;

public interface IEventService
{
    /// <summary>
    /// Возвращает список всех событий
    /// </summary>
    Task<IEnumerable<EventResponse>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Возвращает события по фильтру для постраничного вывода
    /// </summary>
    Task<PaginatedResponse<EventResponse>> GetPaginatedEventsAsync(EventsFilter filter, CancellationToken ct);

    /// <summary>
    /// Получить событие по Id
    /// </summary>
    Task<EventResponse?> GetByIdAsync(Guid eventId, CancellationToken ct);

    /// <summary>
    /// Создать событие
    /// </summary>
    Task<EventResponse> CreateAsync(EventRequest createEventRequest, CancellationToken ct);

    /// <summary>
    /// Обновить событие
    /// </summary>
    Task<EventResponse?> UpdateAsync(Guid id, EventRequest updateEventRequest, CancellationToken ct);

    /// <summary>
    /// Удалить событие
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Проверить наличие события
    /// </summary>
    Task<bool> IsExistAsync(Guid id, CancellationToken ct);
}