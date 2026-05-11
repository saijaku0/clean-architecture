using Microsoft.AspNetCore.Mvc;
using TrainBooking.Domain.Common.Results;

namespace TrainBooking.Api.Extensions;

internal static class ResultExtension
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
        => result.IsSuccess
            ? new OkObjectResult(result.Value)
            : MapError(result.Error!);

    public static IActionResult ToActionResult(this Result result)
        => result.IsSuccess
            ? new NoContentResult()
            : MapError(result.Error!);

    private static ObjectResult MapError(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.BadRequest => StatusCodes.Status400BadRequest,
            ErrorType.TooManyRequests => StatusCodes.Status429TooManyRequests,
            ErrorType.Internal => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type.ToString(),
            Detail = error.Message,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problemDetails.Extensions["errorCode"] = error.Code;

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }
}
