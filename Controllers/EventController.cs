using Microsoft.AspNetCore.Mvc;
using EventManagementService.Models;

namespace EventManagementService.Controllers;

/// <summary>
/// Контроллер обработки событий
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IEventService _eventService;
    public EventController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>
    /// Возвращает список всех событий
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll()
    {
        var events = await _eventService.GetAllAsync();
        return Ok(events);
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
        if (ev is null)
            return NotFound(new { message = $"Событие с Id {id} не найдено" });
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
        try
        {
            var created = await _eventService.CreateAsync(createEvent);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
        try
        {
            var updated = await _eventService.UpdateAsync(id, updateEvent);
            if (updated is null)
                return NotFound(new { message = $"Событие с Id {id} не найдено" });
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Удалить событие
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!await _eventService.DeleteAsync(id))
            return NotFound(new { message = $"Событие с Id {id} не найдено" });

        return NoContent();
    }

}