using EventManagementService.Models;

namespace EventManagementService.Contracts;

public interface IBookingRepository
{
    /// <summary>
    /// Создание брони для указанного события
    /// </summary>
    Task<BookingEntity> CreateBookingAsync(Guid evendId, BookingStatusEnum status, DateTime createdAt, CancellationToken ct);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    Task<BookingEntity?> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Получить список ожидающих обработки бронирований
    /// </summary>
    Task<List<Guid>> GetBookingIdsByStatusAsync(BookingStatusEnum status, CancellationToken ct);

    /// <summary>
    /// Обновить информацию о бронировании.
    /// </summary>
    Task<BookingEntity?> UpdateBookingAsync(BookingEntity entity, CancellationToken ct);
}