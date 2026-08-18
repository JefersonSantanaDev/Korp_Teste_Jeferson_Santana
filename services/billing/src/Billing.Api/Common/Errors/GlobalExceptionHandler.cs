using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Common.Errors;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var error = MapException(exception);

        if (error.Status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "An unexpected error occurred while processing the request.");
        }
        else
        {
            logger.LogWarning(
                "Request failed with status {StatusCode}: {ErrorTitle}",
                error.Status,
                error.Title);
        }

        httpContext.Response.StatusCode = error.Status;

        await Results.Problem(
                statusCode: error.Status,
                title: error.Title,
                detail: error.Detail,
                extensions: new Dictionary<string, object?>
                {
                    ["traceId"] = httpContext.TraceIdentifier
                })
            .ExecuteAsync(httpContext);

        return true;
    }

    private static ApiError MapException(Exception exception) => exception switch
    {
        ProductNotFoundException ex => new ApiError(
            StatusCodes.Status404NotFound,
            "Product not found",
            ex.Message),
        InventoryUnavailableException => new ApiError(
            StatusCodes.Status503ServiceUnavailable,
            "Inventory unavailable",
            "The inventory service is temporarily unavailable. Please try again."),
        ArgumentException => new ApiError(
            StatusCodes.Status400BadRequest,
            "Invalid invoice data",
            "One or more invoice values are invalid."),
        _ => new ApiError(
            StatusCodes.Status500InternalServerError,
            "Unexpected error",
            "An unexpected error occurred while processing the request.")
    };

    private sealed record ApiError(int Status, string Title, string Detail);
}
