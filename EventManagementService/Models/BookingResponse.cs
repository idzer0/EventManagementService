namespace EventManagementService.Models;

using System.ComponentModel.DataAnnotations;

public class BookingInfo
{
    /// <summary>
    /// Уникальный идентификатор брони.
    /// </summary>
    [Key]
    public Guid Id {get; set;}

    /// <summary>
    /// Идентификатор события, к которому относится бронь.
    /// </summary>
    [Required]
    public Guid EventId {get; set;}

    /// <summary>
    /// Текущий статус брони.
    /// </summary>
    [Required]
    public BookingStatusEnum Status {get; set;}

    /// <summary>
    /// Дата и время создания брони.
    /// </summary>
    [Required]
    public DateTimeOffset CreatedAt {get; set;}

    /// <summary>
    /// Дата и время обработки брони.
    /// </summary>
    public DateTimeOffset? ProcessedAt {get; set;}
}
