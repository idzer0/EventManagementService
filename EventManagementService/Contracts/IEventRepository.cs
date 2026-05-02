using EventManagementService.Models;

namespace EventManagementService.Contracts;

public interface IEventRepository
{
    /// <summary>
    /// Получить событие по Id
    /// </summary>
    Task<EventEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Возвращает список всех событий
    /// </summary>
    Task<IEnumerable<EventEntity>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Возвращает количество событий
    /// </summary>
    Task<int> EventsCountAsync(EventsFilter filter, CancellationToken ct);

    /// <summary>
    /// Проверяет наличие события по Id
    /// </summary>
    Task<bool> IsExistsAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Возвращает события по фильтру для постраничного вывода
    /// </summary>
    /// <param name="filter">Параметры фильтрации</param>
    /// <returns>Возвращает общее количество записей и список отобранных элементов</returns>
    Task<List<EventEntity>> GetPaginatedEventsAsync(EventsFilter filter, CancellationToken ct);

    /// <summary>
    /// Создать событие
    /// </summary>
    Task<EventEntity> CreateAsync(EventEntity eventData, CancellationToken ct);

    /// <summary>
    /// Обновить событие
    /// </summary>
    Task<EventEntity?> UpdateAsync(EventEntity eventData, CancellationToken ct);

    /// <summary>
    /// Удалить событие
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}