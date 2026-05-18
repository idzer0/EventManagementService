namespace EventManagementService.DomainExceptions;

/// <summary>
/// Исключение для ошибок "Конфликт" (409)
/// </summary>
public class NoAvailableSeatsDomainException : DomainException
{
    public NoAvailableSeatsDomainException(string message) : base(message) { }
    public NoAvailableSeatsDomainException(string message, Exception innerException)
        : base(message, innerException) { }

    public override int StatusCode => StatusCodes.Status409Conflict;
    public override string Title => "Конфликт";

    /// <summary>
    /// Детали ошибок искомых объектов
    /// </summary>
    public override string[] ErrorDetails { get; set; } = [];
}
