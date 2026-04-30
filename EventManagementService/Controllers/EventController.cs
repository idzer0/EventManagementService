using Microsoft.AspNetCore.Mvc;
using EventManagementService.Models;
using EventManagementService.Contracts;

namespace EventManagementService.Controllers;

/// <summary>
/// Контроллер обработки событий
/// </summary>
[ApiController]
[Route("events")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;
    public EventController(IEventService eventService, IBookingService bookingService)
    {
        _eventService = eventService;
        _bookingService = bookingService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<EventResponse>>> GetAll([FromQuery] EventsFilter filter, CancellationToken ct)
    {
        var result = await _eventService.GetPaginatedEventsAsync(filter, ct);

        return Ok(result);
    }

    /// <summary>
    /// Получить событие по Id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetById(Guid id, CancellationToken ct)
    {
        var ev = await _eventService.GetByIdAsync(id, ct);

        return Ok(ev);
    }

    /// <summary>
    /// Создать событие
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventResponse>> Create([FromBody] EventRequest createEvent, CancellationToken ct)
    {
        var created = await _eventService.CreateAsync(createEvent, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventResponse>> Update(Guid id, [FromBody] EventRequest updateEvent, CancellationToken ct)
    {
        var updated = await _eventService.UpdateAsync(id, updateEvent, ct);

        return Ok(updated);
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _eventService.DeleteAsync(id, ct);

        return NoContent();
    }

    /// <summary>
    /// Забронировать событие.
    /// </summary>
    [HttpPost("{id:guid}/book")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingInfo>> CreateBookingAsync(Guid id, CancellationToken ct)
    {
        var booking = await _bookingService.CreateBookingAsync(id, ct);

        return Accepted($"/bookings/{booking.Id}", booking);
    }
}