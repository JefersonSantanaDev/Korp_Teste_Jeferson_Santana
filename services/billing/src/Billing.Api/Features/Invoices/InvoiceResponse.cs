using Billing.Api.Domain;

namespace Billing.Api.Features.Invoices;

public sealed record InvoiceResponse(
    Guid Id,
    long Number,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt,
    IReadOnlyList<InvoiceItemResponse> Items)
{
    public static InvoiceResponse FromInvoice(Invoice invoice) =>
        new(
            invoice.Id,
            invoice.Number,
            invoice.Status.ToString(),
            invoice.CreatedAt,
            invoice.ClosedAt,
            invoice.Items
                .Select(item => new InvoiceItemResponse(
                    item.ProductId,
                    item.ProductCode,
                    item.ProductDescription,
                    item.Quantity))
                .ToList());
}
