using System.ComponentModel.DataAnnotations;

namespace Billing.Api.Features.Invoices.CreateInvoice;

public sealed record CreateInvoiceRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "Invoice must contain at least one item.")]
    public List<CreateInvoiceItemRequest> Items { get; init; } = [];
}
