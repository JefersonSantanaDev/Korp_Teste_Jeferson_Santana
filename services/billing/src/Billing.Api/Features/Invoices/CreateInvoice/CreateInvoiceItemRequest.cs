using System.ComponentModel.DataAnnotations;

namespace Billing.Api.Features.Invoices.CreateInvoice;

public sealed record CreateInvoiceItemRequest
{
    [Required]
    public Guid ProductId { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; init; }
}
