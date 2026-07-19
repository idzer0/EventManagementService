using Application.Contracts;
using Application.DTO;
using Application.Mappers;
using Domain.DomainExceptions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
/// Сервис бронирования.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _repoBooking;
    private readonly IEventRepository _repoEvents;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BookingService> _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

    public BookingService (
        IBookingRepository repoBooking,
        IEventRepository repoEvents,
        ICurrentUserService currentUserService,
        ILogger<BookingService> logger)
    {
        _repoBooking = repoBooking;
        _repoEvents = repoEvents;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        var ev = await _repoEvents.GetByIdAsync(eventId, ct)
            ?? throw new ObjectNotFoundDomainException($"События с Id {eventId} не найдено.");

        if (ev.EndAt < DateTime.UtcNow)
            throw new ObjectNotFoundDomainException($"События с Id {eventId} не найдено.");

        if (!ev.TryReserveSeats())
            throw new NoAvailableSeatsDomainException("No available seats for this event");

        try
        {
            // Сохраняем изменения – EF сгенерирует UPDATE с условием WHERE Id = ... AND xmin = @oldXmin
            await _repoEvents.UpdateAsync(ev, ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Кто-то уже изменил эту строку (другой параллельный запрос)
            throw new NoAvailableSeatsDomainException("No available seats for this event", ex);
        }

        return BookingMapper.MapToResponse(
            await _repoBooking.CreateBookingAsync(eventId, BookingStatusEnum.Pending, DateTimeOffset.UtcNow, ct));
    }

    /// <inheritdoc/>
    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, CancellationToken ct)
    {
        var entity = await _repoBooking.GetBookingByIdAsync(bookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронь с Id {bookingId} не найдена.");

        return BookingMapper.MapToResponse(entity);
    }

    /// <inheritdoc/>
    public Task<List<Guid>> GetBookingIdsByStatusAsync(BookingStatusEnum status, CancellationToken ct, int num = 10)
    {
        return _repoBooking.GetBookingIdsByStatusAsync(status, ct, num);
    }

    /// <inheritdoc/>
    public async Task ProcessPendingBookingAsync(Guid bookingId, CancellationToken ct)
    {
        BookingEntity? booking = await _repoBooking.GetBookingByIdAsync(bookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронирование Id {bookingId} не найдено.");

        EventEntity? ev = await _repoEvents.GetByIdAsync(booking.EventId, ct);
        // проверка, существует ли бронируемое событие
        if (ev is null)
        {
            if (booking.Reject())
            {
                await _repoBooking.UpdateBookingAsync(booking, ct);
                _logger.LogWarning("Событие Id {BookingEventId} отсутствует", booking.EventId);
            }
        }
        else if (booking.Status == BookingStatusEnum.Pending)
        {
            await _processingSemaphore.WaitAsync(ct);
            try
            {
                if  (booking.Confirm())
                {
                    await _repoBooking.UpdateBookingAsync(booking, ct);
                }
                else //для почти невозможного случая одновременной обработки брони с одним Id
                {
                    await BookingRejectAsync(ev, booking, ct);
                }
            }
            catch
            {
                await BookingRejectAsync(ev, booking, ct);
                throw;
            }
            finally
            {
                _processingSemaphore.Release();
            }
        }
    }

    /// <inheritdoc/>
    public async Task RejectAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await _repoBooking.GetBookingByIdAsync(bookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронь с Id {bookingId} не найдена.");

        EventEntity? ev = await _repoEvents.GetByIdAsync(booking.EventId, ct);

        await BookingRejectAsync(ev, booking, ct);
    }


    /// <inheritdoc/>
    public async Task CancelAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await _repoBooking.GetBookingByIdAsync(bookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронь с Id {bookingId} не найдена.");

        if(!_currentUserService.IsAllowUserOperation(booking.UserId))
            throw new UnauthorizedAccessDomainException("Недостаточно прав");

        EventEntity? ev = await _repoEvents.GetByIdAsync(booking.EventId, ct);

        await BookingCancelAsync(ev, booking, ct);
    }

    private async Task BookingRejectAsync(EventEntity? ev, BookingEntity booking, CancellationToken ct)
    {
        if (ev?.ReleaseSeats() is true)
        {
            try
            {
                await _repoEvents.UpdateAsync(ev, ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Кто-то уже изменил эту строку (другой параллельный запрос)
                throw new ObjectNotFoundDomainException("Не удалось изменить событие. Повторите операцию.", ex);
            }
        }

        if (booking.Reject())
            await _repoBooking.UpdateBookingAsync(booking, ct);
    }

    private async Task BookingCancelAsync(EventEntity? ev, BookingEntity booking, CancellationToken ct)
    {
        if (ev?.ReleaseSeats() is true)
        {
            try
            {
                await _repoEvents.UpdateAsync(ev, ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Кто-то уже изменил эту строку (другой параллельный запрос)
                throw new ObjectNotFoundDomainException("Не удалось изменить событие. Повторите операцию.", ex);
            }
        }

        if (booking.Cancel())
            await _repoBooking.UpdateBookingAsync(booking, ct);
    }
}
