namespace Domain.Models;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Сущность для обработки событий на бизнес-слое
/// </summary>
public class EventEntity
{
    [Key]
    public Guid Id {get; init;}

    [Required]
    public string Title {get; set;} = string.Empty;

    public string? Description {get; set;}

    [Required]
    public DateTime StartAt {get; set;}

    [Required]
    public DateTime EndAt { get; set; }

    [Required]
    public int TotalSeats { get; set; }

    [Required]
    public int AvailableSeats { get; set; }

    // Свойство для оптимистической блокировки (маппится на xmin)
    public uint Xmin { get; set; }

    // Навигационное свойство
    public virtual ICollection<BookingEntity> Bookings { get; set; } = [];


    /// <summary>
    /// Проверяет количество доступных мест "count"
    /// </summary>
    public bool CheckAvaliableSeats(int count = 1)
    {
        return AvailableSeats >= count;
    }

    /// <summary>
    /// Проверяет количество доступных мест и резервирует места в количестве "count"
    /// </summary>
    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats < count)
        {
            return false;
        }
        else
        {
            AvailableSeats -= count;
            return true;
        }
    }

    /// <summary>
    /// Освобождает зарезервированные места
    /// </summary>
    public bool ReleaseSeats(int count = 1)
    {
        if ((TotalSeats - AvailableSeats) >= count)
        {
            AvailableSeats += count;
            return true;
        }
        else
        {
            return false;
        }
    }
}