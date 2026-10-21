using MasterNet.Application.Core;
using Microsoft.AspNetCore.Mvc;

namespace MasterNet.WebApi.Extensions;

public static class ResultExtensions
{
    // Maps Result<T> to a consistent IActionResult:
    // - ValidationErrors -> 400 ValidationProblemDetails
    // - Other failures   -> 400 ProblemDetails with message
    // - Success          -> 200 OK with value (or custom success status)
    public static IActionResult FromResult<T>(
        this ControllerBase controller,
        Result<T> result,
        int successStatusCode = StatusCodes.Status200OK)
    {
        if (result is null)
        {
            return controller.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected null result");
        }

        if (result.IsSuccess)
        {
            return successStatusCode == StatusCodes.Status200OK
                ? controller.Ok(result.Value)
                : controller.StatusCode(successStatusCode, result.Value);
        }

        if (result.ValidationErrors is { Count: > 0 })
        {
            var errorsByField = result.ValidationErrors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var pd = new ValidationProblemDetails(errorsByField)
            {
                Title = "Validation Errors",
                Status = StatusCodes.Status400BadRequest
            };

            return controller.BadRequest(pd);
        }

        return controller.Problem(
            detail: string.IsNullOrWhiteSpace(result.Error) ? "Request failed." : result.Error,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Bad Request");
    }
}