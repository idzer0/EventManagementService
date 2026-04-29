namespace EventManagementService.Models;

public class PaginatedResponse<T>
{
    /// <summary>
    /// Номер страницы
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Размер страницы
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Общее количество записей
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Количество страниц
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// Список возвращаемых объектов
    /// </summary>
    public List<T> Items { get; set; } = new();
}