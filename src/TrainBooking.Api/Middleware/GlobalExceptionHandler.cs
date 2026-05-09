using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TrainBooking.Api.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning("Response has already started, exception handler skipped");
            return false;
        }

        (int statusCode, string title, string detail) = exception switch
        {
            OperationCanceledException when ct.IsCancellationRequested
                => (StatusCodes.Status499ClientClosedRequest, "Request Cancelled", "The client cancelled the request."),
            _ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error has occurred on the server.")
        };

        LogLevel logLevel = statusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
        logger.Log(logLevel, exception, "Unhandled exception: {ExceptionType}", exception.GetType().Name);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, ct);
        return true;
    }
}
