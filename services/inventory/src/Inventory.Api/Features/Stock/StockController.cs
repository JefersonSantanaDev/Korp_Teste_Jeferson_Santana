using Inventory.Api.Common.Errors;
using Inventory.Api.Data;
using Inventory.Api.Domain;
using Inventory.Api.Features.Stock.DeductStock;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Inventory.Api.Features.Stock;

[ApiController]
[Route("api/stock")]
public sealed class StockController(
    InventoryDbContext dbContext,
    ILogger<StockController> logger) : ControllerBase
{
    /// <summary>
    /// Deducts stock for all items atomically (all-or-nothing) and idempotently:
    /// repeating the same <see cref="DeductStockRequest.OperationId"/> never deducts twice.
    /// </summary>
    [HttpPost("deductions")]
    [ProducesResponseType<StockDeductionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StockDeductionResponse>> Deduct(
        DeductStockRequest request,
        CancellationToken cancellationToken)
    {
        if (request.OperationId == Guid.Empty)
        {
            throw new ArgumentException("OperationId is required.", nameof(request.OperationId));
        }

        // Fast path: avoid opening a transaction for the common repeat-request case.
        var alreadyProcessed = await dbContext.StockOperations
            .AsNoTracking()
            .AnyAsync(operation => operation.OperationId == request.OperationId, cancellationToken);

        if (alreadyProcessed)
        {
            logger.LogInformation("Stock operation {OperationId} was already processed.", request.OperationId);
            return Ok(new StockDeductionResponse(request.OperationId, "already_processed"));
        }

        var productIds = request.Items.Select(item => item.ProductId).ToList();
        var existingProductIds = await dbContext.Products
            .Where(product => productIds.Contains(product.Id))
            .Select(product => product.Id)
            .ToListAsync(cancellationToken);

        var missingProductIds = productIds.Except(existingProductIds).ToList();
        if (missingProductIds.Count > 0)
        {
            throw new ProductNotFoundException(missingProductIds);
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // The StockOperation insert is the atomic idempotency gate: it must land
        // before any stock is touched, so two requests racing on the same
        // OperationId can never both deduct — the loser gets a unique-constraint
        // violation here and stops before mutating anything.
        try
        {
            dbContext.StockOperations.Add(new StockOperation(request.OperationId));
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsOperationIdConflict(ex))
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogInformation(
                "Stock operation {OperationId} was claimed concurrently by another request.",
                request.OperationId);
            return Ok(new StockDeductionResponse(request.OperationId, "already_processed"));
        }

        foreach (var item in request.Items)
        {
            var affectedRows = await dbContext.Products
                .Where(product => product.Id == item.ProductId && product.Stock >= item.Quantity)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(product => product.Stock, product => product.Stock - item.Quantity)
                        .SetProperty(product => product.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);

            if (affectedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new InsufficientStockException(item.ProductId);
            }
        }

        await transaction.CommitAsync(cancellationToken);

        logger.LogInformation(
            "Stock operation {OperationId} processed, deducting {ItemCount} item(s).",
            request.OperationId,
            request.Items.Count);

        return Ok(new StockDeductionResponse(request.OperationId, "processed"));
    }

    private static bool IsOperationIdConflict(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ux_stock_operations_operation_id"
        };
}
