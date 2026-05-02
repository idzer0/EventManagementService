namespace EventManagementService.DomainExceptions;

/// <summary>
/// Исключение для ошибок "Объект не найден" (404)
/// </summary>
public class ObjectNotFoundDomainException : DomainException
{
    public ObjectNotFoundDomainException(string message) : base(message) { }
    public ObjectNotFoundDomainException(string message, Exception innerException)
        : base(message, innerException) { }

    public override int StatusCode => StatusCodes.Status404NotFound;
    public override string Title => "Объект не найден";

    /// <summary>
    /// Детали ошибок искомых объектов
    /// </summary>
    public override string[] ErrorDetails { get; set; } = [];
}
