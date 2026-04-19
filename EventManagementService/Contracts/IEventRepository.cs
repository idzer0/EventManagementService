using EventManagementService.Models;

namespace EventManagementService.Contracts;

public interface IEventRepository
{
    /// <summary>
    /// Получить событие по Id
    /// </summary>
    Task<EventEntity?> GetByIdAsync(Guid id);

    /// <summary>
    /// Возвращает список всех событий
    /// </summary>
    Task<IEnumerable<EventEntity>> GetAllAsync();

    /// <summary>
    /// Возвращает количество событий
    /// </summary>
    Task<int> EventsCount();

    /// <summary>
    /// Проверяет наличие события по Id
    /// </summary>
    Task<bool> IsExists(Guid id);

    /// <summary>
    /// Возвращает события по фильтру для постраничного вывода
    /// </summary>
    /// <param name="filter">Параметры фильтрации</param>
    /// <returns>Возвращает общее количество записей и список отобранных элементов</returns>
    Task<List<EventEntity>> GetPaginatedEventsAsync(EventsFilter filter);

    /// <summary>
    /// Создать событие
    /// </summary>
    Task<EventEntity> CreateAsync(EventEntity eventData);

    /// <summary>
    /// Обновить событие
    /// </summary>
    Task<EventEntity?> UpdateAsync(EventEntity eventData);

    /// <summary>
    /// Удалить событие
    /// </summary>
    Task<bool> DeleteAsync(Guid id);
}