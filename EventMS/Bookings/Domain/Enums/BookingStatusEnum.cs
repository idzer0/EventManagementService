namespace EventMS.Bookings.Domain.Models;

public enum BookingStatusEnum : int
{
    /// <summary>
    /// Бронь создана, ожидает обработки
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Бронь в обработке
    /// </summary>
    InProcessing = 2,

    /// <summary>
    /// Бронь подтверждена
    /// </summary>
    Confirmed = 3,

    /// <summary>
    /// Бронь отклонена
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Бронь отменена
    /// </summary>
    Canceled = 5,
}
