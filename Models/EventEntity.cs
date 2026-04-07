namespace EventManagementService.Models;

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
    public DateTime EndAt {get; set;}
}