using EventMS.Bookings.Application.Contracts;
using EventMS.Bookings.Application.DTO;
using EventMS.Bookings.Domain.DomainExceptions;
using EventMS.Bookings.Domain.Models;
using KafkaSettingsShared.DTO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventMS.Bookings.Application.Services;

public class BookingResponseService(
    IBookingRepository repoBooking,
    ILogger<BookingService> logger) : IBookingResponseService
{

    /// <inheritdoc/>
    public async Task ConfirmBookingAsync(BookingResponse bookingResponse, CancellationToken ct)
    {
        var booking = await repoBooking.GetBookingByIdAsync(bookingResponse.BookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронь с Id {bookingResponse.BookingId} не найдена.");

        if (booking.Status != BookingStatusEnum.InProcessing)
            return;

        if(bookingResponse.IsSuccess)
        {
            booking.Status = BookingStatusEnum.Confirmed;
            await repoBooking.UpdateBookingAsync(booking, ct);
        }
        else
        {
            booking.Status = BookingStatusEnum.Rejected;
            await repoBooking.UpdateBookingAsync(booking, ct);
            logger.LogError("Бронь не была подтверждена, BookingId: {BookingId}. Ошибка: {BookingResponseErrorMessage}", booking.Id, bookingResponse.ErrorMessage);
        }
    }

    /// <inheritdoc/>
    public async Task RejectBookingAsync(BookingResponse bookingResponse, CancellationToken ct)
    {
        var booking = await repoBooking.GetBookingByIdAsync(bookingResponse.BookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронь с Id {bookingResponse.BookingId} не найдена.");

        if(bookingResponse.IsSuccess)
        {
            if (booking.Cancel())
                await repoBooking.UpdateBookingAsync(booking, ct);
        }
        else
        {
            logger.LogError("Бронь не удалось отменить, BookingId: {BookingId}. Ошибка: {BookingResponseErrorMessage}", booking.Id, bookingResponse.ErrorMessage);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteBookingAsync(BookingResponse bookingResponse, CancellationToken ct)
    {
        var booking = await repoBooking.GetBookingByIdAsync(bookingResponse.BookingId, ct)
            ?? throw new ObjectNotFoundDomainException($"Бронь с Id {bookingResponse.BookingId} не найдена.");

        if(bookingResponse.IsSuccess)
        {
            await repoBooking.DeleteBookingAsync(booking, ct);
        }
        else
        {
            logger.LogError("Бронь не удалось удалить, BookingId: {BookingId}. Ошибка: {BookingResponseErrorMessage}", booking.Id, bookingResponse.ErrorMessage);
        }
    }
}
