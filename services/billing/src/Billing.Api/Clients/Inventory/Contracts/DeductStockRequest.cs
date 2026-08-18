namespace Billing.Api.Clients.Inventory.Contracts;

public sealed record DeductStockRequest(Guid OperationId, IReadOnlyList<StockDeductionItem> Items);
