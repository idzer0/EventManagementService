using EventManagementService.Contracts;
using EventManagementService.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Controllers;

/// <summary>
/// Контроллер бронирований.
/// </summary>
[ApiController]
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
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingInfo>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var booking = await _service.GetBookingByIdAsync(id, ct);

        return Ok(booking);
    }
}
