namespace EventManagementService.Models;

/// <summary>
/// Сущность для ответов метода контроллера обработки событий
/// </summary>
public class EventResponse
{
    public Guid Id {get; init;}

    public string Title {get; set;} = string.Empty;

    public string? Description {get; set;}
    
    public DateTime StartAt {get; set;}

    public DateTime EndAt {get; set;}

}