namespace Billing.Api.Features.Invoices;

public sealed record InvoiceItemResponse(
    Guid ProductId,
    string ProductCode,
    string ProductDescription,
    int Quantity);
