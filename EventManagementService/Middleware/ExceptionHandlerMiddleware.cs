using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Xml;
using EventManagementService.DomainExceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementService.Middleware;

/// <summary>
/// Слой обработки ошибок, возникающих в процессе обработки запросов
/// </summary>
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly bool isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

    public ExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Обработчик ошибок
    /// </summary>
    private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
    {
        _logger.LogError(
            ex,
            "Unhandled exception. Method={Method}, Path={Path}, RequestId={RequestId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.Request.Headers["x-request-id"]);

        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var statusCode = MapStatusCode(ex);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetPublicTitle(ex, statusCode),
            Instance = httpContext.Request.Path,
            Extensions = {
                ["requestId"] = httpContext.Request.Headers["x-request-id"].ToString()
            }
        };

        if (isDevelopment)
        {
            problemDetails.Extensions["errorType"] = ex.GetType().Name;
            problemDetails.Extensions["errorMessage"] = ex.Message;

            if (ex.StackTrace != null)
            {
                problemDetails.Extensions["stackTrace"] = ex.StackTrace.Split(Environment.NewLine)
                    .Take(50) // Ограничиваем количество строк
                    .ToArray();
            }

            if (ex is DomainException domainException)
                problemDetails.Detail = String.Join(Environment.NewLine, domainException.ErrorDetails);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails);
    }

    /// <summary>
    /// Маппинг типов ошибок в коды ошибок.
    /// </summary>
    private static int MapStatusCode(Exception ex)
        => ex switch
        {
            ValidationDomainException vde => StatusCodes.Status400BadRequest,
            ObjectNotFoundDomainException onfe => StatusCodes.Status404NotFound,
            ValidationException ve => StatusCodes.Status400BadRequest,
            KeyNotFoundException nfe => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

    /// <summary>
    /// Получить наименование ошибки.
    /// </summary>
    private static string GetPublicTitle(Exception ex, int statusCode)
    {
        return (ex is DomainException domainException) ? domainException.Title
            : statusCode switch
            {
                StatusCodes.Status400BadRequest => "Некорректный запрос",
                StatusCodes.Status404NotFound => "Ресурс не найден",
                StatusCodes.Status500InternalServerError => "Внутренняя ошибка сервера",
                _ => "Ошибка при обработке запроса"
            };
    }
}