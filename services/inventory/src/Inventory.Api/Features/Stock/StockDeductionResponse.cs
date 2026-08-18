namespace Inventory.Api.Features.Stock;

public sealed record StockDeductionResponse(Guid OperationId, string Status);
