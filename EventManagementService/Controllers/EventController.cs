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
    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<EventResponse>>> GetAll([FromQuery] EventsFilter filter)
    {
        var result = await _eventService.GetPaginatedEventsAsync(filter);
        
        return Ok(result);
    }

    /// <summary>
    /// Получить событие по Id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetById(Guid id)
    {
        var ev = await _eventService.GetByIdAsync(id);

        return Ok(ev);
    }

    /// <summary>
    /// Создать событие
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventResponse>> Create([FromBody] EventRequest createEvent)
    {
        var created = await _eventService.CreateAsync(createEvent);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Обновить событие
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventResponse>> Update(Guid id, [FromBody] EventRequest updateEvent)
    {
        var updated = await _eventService.UpdateAsync(id, updateEvent);

        return Ok(updated);
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _eventService.DeleteAsync(id);

        return NoContent();
    }

}