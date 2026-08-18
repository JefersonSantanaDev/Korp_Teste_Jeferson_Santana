namespace Billing.Api.Clients.Inventory.Contracts;

public sealed record StockDeductionResponse(Guid OperationId, string Status);
