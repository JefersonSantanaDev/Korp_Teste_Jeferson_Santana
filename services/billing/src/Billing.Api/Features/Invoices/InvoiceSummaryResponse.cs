namespace Billing.Api.Features.Invoices;

public sealed record InvoiceSummaryResponse(
    Guid Id,
    long Number,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ClosedAt);
