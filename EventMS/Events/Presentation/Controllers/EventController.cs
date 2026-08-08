using Application.Contracts;
using Application.DTO;
using Domain.Models;
using Auth.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsService.Controllers;

/// <summary>
/// Контроллер обработки событий
/// </summary>
[ApiController]
[Route("events")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [AllowAnonymous]
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
    [AllowAnonymous]
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
    [Authorize(Roles = nameof(UsersRole.Admin))] //(Roles = nameof(UsersRole.Admin))
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<EventResponse>> Create([FromBody] EventRequest createEvent, CancellationToken ct)
    {
        var created = await _eventService.CreateAsync(createEvent, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = nameof(UsersRole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> Update(Guid id, [FromBody] EventRequest updateEvent, CancellationToken ct)
    {
        var updated = await _eventService.UpdateAsync(id, updateEvent, ct);

        return Ok(updated);
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = nameof(UsersRole.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _eventService.DeleteAsync(id, ct);

        return NoContent();
    }
}