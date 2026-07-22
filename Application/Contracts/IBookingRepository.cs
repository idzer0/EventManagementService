using Domain.Models;

namespace Application.Contracts;

public interface IBookingRepository
{
    /// <summary>
    /// Создание брони для указанного события
    /// </summary>
    Task<BookingEntity> CreateBookingAsync(Guid evendId, BookingStatusEnum status, DateTimeOffset createdAt, CancellationToken ct);

    /// <summary>
    /// Получение брони по идентификатору
    /// </summary>
    Task<BookingEntity?> GetBookingByIdAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Получить список ожидающих обработки бронирований
    /// </summary>
    Task<List<Guid>> GetBookingIdsByStatusAsync(BookingStatusEnum status, CancellationToken ct, int num);

    /// <summary>
    /// Обновить информацию о бронировании.
    /// </summary>
    Task UpdateBookingAsync(BookingEntity entity, CancellationToken ct);

    /// <summary>
    /// Удалить бронирование.
    /// </summary>
    Task DeleteBookingAsync(BookingEntity entity, CancellationToken ct);

    /// <summary>
    /// Получить список активных бронирований для пользователя.
    /// Активные брони в статусах: Pending, Confirmed
    /// </summary>
    Task<int> GetActiveBookingsAsync(CancellationToken ct);
}