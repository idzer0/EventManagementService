namespace EventManagementService.Models;

/// <summary>
/// Сущность для обработки событий на бизнес-слое
/// </summary>
public class Event 
{
    public Guid Id {get; init;}

    public string Title {get; set;} = string.Empty;

    public string? Description {get; set;}
    
    public DateTime StartAt {get; set;}
    
    public DateTime EndAt {get; set;}
}