using EventManagementService.Contracts;
using EventManagementService.DomainExceptions;
using EventManagementService.Models;

namespace EventManagementService.Services;

/// <summary>
/// Сервис бронирования.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _repoBooking;
    private readonly IEventService _eventService;
    private readonly ILogger<BookingService> _logger;

    public BookingService (
        IBookingRepository repoBooking,
        IEventService eventService,
        ILogger<BookingService> logger)
    {
        _repoBooking = repoBooking;
        _eventService = eventService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        if(!await _eventService.IsExistAsync(eventId, ct))
            throw new ObjectNotFoundDomainException($"События с Id {eventId} не найдено.");

        return MapToResponse(await _repoBooking.CreateBookingAsync(eventId, BookingStatusEnum.Pending, DateTimeOffset.UtcNow, ct));
    }

    /// <inheritdoc/>
    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        var entity = await _repoBooking.GetBookingByIdAsync(bookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронь с Id {bookingId} не найдена.");

        return MapToResponse(entity);
    }

    /// <inheritdoc/>
    public Task<List<Guid>> GetBookingIdsByStatusAsync(BookingStatusEnum status, CancellationToken ct)
    {
        return _repoBooking.GetBookingIdsByStatusAsync(status, ct);
    }

    /// <inheritdoc/>
    public async Task ProcessPendingBookingAsync(Guid bookingId, CancellationToken ct)
    {
        BookingEntity? booking = await _repoBooking.GetBookingByIdAsync(bookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронирование Id {bookingId} не найдено.");

        if (booking.Status == BookingStatusEnum.Pending)
        {
            booking.Status = BookingStatusEnum.Confirmed;
            booking.ProcessedAt = DateTimeOffset.UtcNow;
            await _repoBooking.UpdateBookingAsync(booking, ct);
        }
    }

    private static BookingInfo MapToResponse(BookingEntity entity)
    {
        return new BookingInfo()
        {
            Id = entity.Id,
            EventId = entity.EventId,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            ProcessedAt = entity.ProcessedAt,
        };
    }
}
