using EventMS.Bookings.Application.DTO;
using KafkaSettingsShared.DTO;

namespace EventMS.Bookings.Application.Contracts;

public interface IBookingResponseService
{
    /// <summary>
    /// Подтвердить бронирование.
    /// </summary>
    Task ConfirmBookingAsync(BookingResponse bookingResponse, CancellationToken ct);

    /// <summary>
    /// Отмена брони.
    /// </summary>
    Task RejectBookingAsync(BookingResponse bookingResponse, CancellationToken ct);

    /// <summary>
    /// Удаление брони.
    /// </summary>
    Task DeleteBookingAsync(BookingResponse bookingResponse, CancellationToken ct);
}
