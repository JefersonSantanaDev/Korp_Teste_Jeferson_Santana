using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Inventory.Api.Common.Errors;

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
        DbUpdateException
        {
            InnerException: PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                ConstraintName: "ux_products_code"
            }
        } => new ApiError(
            StatusCodes.Status409Conflict,
            "Product code already exists",
            "A product with the provided code already exists."),
        ProductNotFoundException ex => new ApiError(
            StatusCodes.Status404NotFound,
            "Product not found",
            ex.Message),
        InsufficientStockException ex => new ApiError(
            StatusCodes.Status409Conflict,
            "Insufficient stock",
            ex.Message),
        ArgumentException => new ApiError(
            StatusCodes.Status400BadRequest,
            "Invalid product data",
            "One or more product values are invalid."),
        _ => new ApiError(
            StatusCodes.Status500InternalServerError,
            "Unexpected error",
            "An unexpected error occurred while processing the request.")
    };

    private sealed record ApiError(int Status, string Title, string Detail);
}
