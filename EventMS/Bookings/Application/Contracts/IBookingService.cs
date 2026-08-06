using Application.DTO;
using Domain.Models;
using KafkaSettingsShared.Enums;

namespace Application.Contracts;

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
    Task<List<Guid>> GetBookingIdsByStatusAsync(BookingStatusEnum status, CancellationToken ct, int num);

    /// <summary>
    /// Обработать ожидающую заявку на бронирование
    /// </summary>
    Task ProcessPendingBookingAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Отклонить бронь
    /// </summary>
    Task RejectAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Отмена брони
    /// </summary>
    Task CancelAsync(Guid bookingId, CancellationToken ct);

    /// <summary>
    /// Удалить бронирование.
    /// </summary>
    Task DeleteBookingAsync(Guid bookingId, CancellationToken ct);
}