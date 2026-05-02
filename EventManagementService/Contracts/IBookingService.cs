using EventManagementService.Models;

namespace EventManagementService.Contracts;

public interface IBookingService
{
    /// <summary>
    /// создание брони для указанного события
    /// </summary>
    Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken ct);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Получить список Id бронирований с определенным статусом
    /// </summary>
    Task<List<Guid>> GetBookingIdsByStatusAsync(BookingStatusEnum status, CancellationToken ct);

    /// <summary>
    /// Обработать ожидающую заявку на бронирование
    /// </summary>
    Task ProcessPendingBookingAsync(Guid bookingId, CancellationToken ct);
}