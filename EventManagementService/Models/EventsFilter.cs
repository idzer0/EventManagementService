namespace EventManagementService.Models;

/// <summary>
/// Фильтр для получения событий для постраничного вывода
/// </summary>
public class EventsFilter
{
    /// <summary>
    /// Подстрока для фильтрации по наименованию
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Значение для вильтрации по StartAt
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Значение для фильтрации по EndAt
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// Номер запрашиваемой страницы
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Размер запрашиваемой страницы
    /// </summary>
    public int PageSize { get; set; } = 10;
}