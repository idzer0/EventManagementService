using Application.Contracts;
using Application.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

/// <summary>
/// Контроллер бронирований.
/// </summary>
[ApiController]
[Authorize]
[Route("bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingController(IBookingService service)
    {
        _service = service;
    }

    /// <summary>
    /// Получить бронирование по Id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingInfo>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var booking = await _service.GetBookingByIdAsync(id, ct);

        return Ok(booking);
    }

    /// <summary>
    /// Отмена брони
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelAsync(Guid id, CancellationToken ct)
    {
        await _service.CancelAsync(id, ct);

        return Accepted($"/bookings/{id}");
    }

    /// <summary>
    /// Удалить бронирование.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteBookingAsync(Guid id, CancellationToken ct)
    {
        await _service.DeleteBookingAsync(id, ct);

        return Accepted($"/bookings/{id}");
    }

    /// <summary>
    /// Забронировать событие.
    /// </summary>
    [HttpPost("{id:guid}/book")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingInfo>> CreateBookingAsync(Guid id, CancellationToken ct)
    {
        var booking = await _service.CreateBookingAsync(id, ct);

        return Accepted($"/bookings/{booking.Id}", booking);
    }
}
