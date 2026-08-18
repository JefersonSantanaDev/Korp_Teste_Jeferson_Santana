namespace Billing.Api.Clients.Inventory.Contracts;

public sealed record StockDeductionItem(Guid ProductId, int Quantity);
