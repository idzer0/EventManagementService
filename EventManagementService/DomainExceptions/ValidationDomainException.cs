namespace EventManagementService.DomainExceptions;

/// <summary>
/// Исключение для ошибок валидации (400)
/// </summary>
public class ValidationDomainException : DomainException
{
    public ValidationDomainException(string message) : base(message) { }
    public ValidationDomainException(string message, Exception innerException)
        : base(message, innerException) { }

    public override int StatusCode => StatusCodes.Status400BadRequest;
    public override string Title => "Ошибка валидации";

    /// <summary>
    /// Детали ошибок валидации по полям
    /// </summary>
    public override string[] ErrorDetails { get; set; } = [];
}
