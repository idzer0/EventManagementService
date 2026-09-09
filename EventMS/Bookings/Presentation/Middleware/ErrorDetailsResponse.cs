using Microsoft.AspNetCore.Mvc;

namespace EventMS.Bookings.Middleware;

public class ErrorDetailsResponse : ProblemDetails
{
    public int StatusCode { get; set; }
    public string? ErrorType { get; set; }
    public string? Message { get; set; }
}