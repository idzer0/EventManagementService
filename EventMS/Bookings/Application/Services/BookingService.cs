using System.Text.Json;
using EventMS.Bookings.Application.Contracts;
using EventMS.Bookings.Application.DTO;
using EventMS.Bookings.Application.Mappers;
using EventMS.Bookings.Domain.DomainExceptions;
using EventMS.Bookings.Domain.Models;
using KafkaSettingsShared.Contracts;
using KafkaSettingsShared.DTO;
using KafkaSettingsShared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BookingRequest = KafkaSettingsShared.DTO.BookingRequest;

namespace EventMS.Bookings.Application.Services;

/// <summary>
/// Сервис бронирования.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _repoBooking;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<BookingService> _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private readonly IEventPublisher _publisher;
    private readonly KafkaSettings _kafkaSettings;

    public BookingService (
        IBookingRepository repoBooking,
        ICurrentUserService currentUserService,
        IEventPublisher publisher,
        IOptions<KafkaSettings> kafkaSettings,
        ILogger<BookingService> logger)
    {
        _repoBooking = repoBooking;
        _currentUserService = currentUserService;
        _publisher = publisher;
        _kafkaSettings = kafkaSettings.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, CancellationToken ct)
    {
        if (await _repoBooking.GetActiveBookingsAsync(ct) >= 10)
            throw new NoAvailableSeatsDomainException("Бронь не может быть создана.");

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

        if (booking.Status == BookingStatusEnum.Pending)
        {
            await _processingSemaphore.WaitAsync(ct);
            try
            {
                if  (booking.Confirm())
                {
                    await _repoBooking.UpdateBookingAsync(booking, ct);

                    var eventMessage = JsonSerializer.Serialize(new BookingRequest()
                    {
                        BookingId = booking.Id,
                        EventId = booking.EventId,
                        BookingActionType = BookingActionTypeEnum.Confirm,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Публикуем в Kafka
                    await _publisher.PublishAsync(_kafkaSettings.OutgoingTopic, booking.EventId.ToString(), eventMessage);
                }
                else //для почти невозможного случая одновременной обработки брони с одним Id
                {
                    await BookingRejectAsync(booking);
                }
            }
            catch
            {
                await BookingRejectAsync(booking);
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

        await BookingRejectAsync(booking);
    }


    /// <inheritdoc/>
    public async Task CancelAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await _repoBooking.GetBookingByIdAsync(bookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронь с Id {bookingId} не найдена.");

        if(!_currentUserService.IsAllowUserOperation(booking.UserId))
            throw new UnauthorizedAccessDomainException("Недостаточно прав");

        await BookingCancelAsync(booking);
    }

    /// <inheritdoc/>
    public async Task DeleteBookingAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await _repoBooking.GetBookingByIdAsync(bookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронь с Id {bookingId} не найдена.");

        if(!_currentUserService.IsAllowUserOperation(booking.UserId))
            throw new UnauthorizedAccessDomainException("Недостаточно прав");

        await BookingDeleteAsync(booking);
    }
    
    private async Task BookingRejectAsync(BookingEntity booking)
    {
        var eventMessage = JsonSerializer.Serialize(new BookingRequest()
        {
            BookingId = booking.Id,
            EventId = booking.EventId,
            BookingActionType = BookingActionTypeEnum.Reject,
            CreatedAt = DateTime.UtcNow
        });

        // Публикуем в Kafka
        await _publisher.PublishAsync(_kafkaSettings.OutgoingTopic, booking.EventId.ToString(), eventMessage);
    }

    private async Task BookingCancelAsync(BookingEntity booking)
    {
        var eventMessage = JsonSerializer.Serialize(new BookingRequest()
        {
            BookingId = booking.Id,
            EventId = booking.EventId,
            BookingActionType = BookingActionTypeEnum.Reject,
            CreatedAt = DateTime.UtcNow
        });

        // Публикуем в Kafka
        await _publisher.PublishAsync(_kafkaSettings.OutgoingTopic, booking.EventId.ToString(), eventMessage);
    }

    private async Task BookingDeleteAsync(BookingEntity booking)
    {
        var eventMessage = JsonSerializer.Serialize(new BookingRequest()
        {
            BookingId = booking.Id,
            EventId = booking.EventId,
            BookingActionType = BookingActionTypeEnum.Delete,
            CreatedAt = DateTime.UtcNow
        });

        // Публикуем в Kafka
        await _publisher.PublishAsync(_kafkaSettings.OutgoingTopic, booking.EventId.ToString(), eventMessage);
    }
}
