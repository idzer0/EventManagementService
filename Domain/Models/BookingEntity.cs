namespace Domain.Models;

using System.ComponentModel.DataAnnotations;

public class BookingEntity
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
    public DateTimeOffset? ProcessedAt { get; set; }

    // Навигационное свойство.
    public virtual EventEntity? Event { get; set; }

    /// <summary>
    /// Подтверждение брони.
    /// </summary>
    public bool Confirm()
    {
        if (Status != BookingStatusEnum.Pending)
            return false;

        Status = BookingStatusEnum.Confirmed;
        ProcessedAt = DateTimeOffset.UtcNow;
        return true;
    }

    /// <summary>
    /// Отказ в бронировании.
    /// </summary>
    public bool Reject()
    {
        if (Status == BookingStatusEnum.Rejected)
            return false;

        Status = BookingStatusEnum.Rejected;
        ProcessedAt = DateTimeOffset.UtcNow;
        return true;
    }
}
