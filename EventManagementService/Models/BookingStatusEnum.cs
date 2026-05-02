namespace EventManagementService.Models;

public enum BookingStatusEnum : int
{
    /// <summary>
    /// Бронь создана, ожидает обработки
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Бронь подтверждена
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// Бронь отклонена
    /// </summary>
    Rejected = 3,
}
