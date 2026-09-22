namespace EventMS.Events.Application.DTO;

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

    [Range(0, double.MaxValue)]
    [Required(ErrorMessage = "Количество мест должно быть больше или равно нулю.")]
    public int AvailableSeats { get; set; }

    [Range(1, double.MaxValue)]
    [Required(ErrorMessage = "Количество мест должно быть больше нуля.")]
    public int TotalSeats { get; set; }
}