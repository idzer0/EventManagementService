namespace EventManagementService.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Сущность для хранения данных, передаваемых в методы контроллера управления событиями
/// </summary>
public class EventRequest
{
    [Required(ErrorMessage = "Название события обязательно к заполнению")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Дата начала события обязательна к заполнению")]
    public DateTime StartAt { get; set; }

    [Required(ErrorMessage = "Дата окончания события обязательна к заполнению")]
    public DateTime EndAt { get; set; }
}