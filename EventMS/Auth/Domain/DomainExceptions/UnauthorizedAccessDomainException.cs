using Microsoft.AspNetCore.Http;

namespace Domain.DomainExceptions;

/// <summary>
/// Исключение для ошибок валидации (400)
/// </summary>
public class UnauthorizedAccessDomainException : DomainException
{
    public UnauthorizedAccessDomainException(string message) : base(message) { }
    public UnauthorizedAccessDomainException(string message, Exception innerException)
        : base(message, innerException) { }

    public override int StatusCode => StatusCodes.Status403Forbidden;
    public override string Title => "Операция запрещена";

    /// <summary>
    /// Детали ошибок валидации по полям
    /// </summary>
    public override string[] ErrorDetails { get; set; } = [];
}
