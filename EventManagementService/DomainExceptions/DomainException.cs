namespace EventManagementService.DomainExceptions;

/// <summary>
/// Базовое доменное исключение
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception innerException) 
        : base(message, innerException) { }

    /// <summary>
    /// HTTP статус код для этого типа исключения
    /// </summary>
    public abstract int StatusCode { get; }

    /// <summary>
    /// Публичный заголовок ошибки
    /// </summary>
    public abstract string Title { get; }

    /// <summary>
    /// Детали ошибок
    /// </summary>
    public abstract string[] ErrorDetails { get; set; }
}