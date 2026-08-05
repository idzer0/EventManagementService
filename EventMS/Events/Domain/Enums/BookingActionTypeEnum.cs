namespace Domain.Enums;

public enum BookingActionTypeEnum
{
    /// <summary>
    /// Подтвердить бронь и зарезервировать места
    /// </summary>
    Confirm = 1,

    /// <summary>
    /// Отменить бронь и освободить места
    /// </summary>
    Reject = 2,

    /// <summary>
    /// Отменить бронь и освободить места
    /// </summary>
    Cancel = 3,

    /// <summary>
    /// Удалить бронь и освободить места
    /// </summary>
    Delete = 4,
}
