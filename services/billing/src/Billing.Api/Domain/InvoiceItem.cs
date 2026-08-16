namespace Billing.Api.Domain;

public sealed class InvoiceItem
{
    public const int MaxProductCodeLength = 50;
    public const int MaxProductDescriptionLength = 200;

    private InvoiceItem()
    {
    }

    public InvoiceItem(
        Guid productId,
        string productCode,
        string productDescription,
        int quantity)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(productCode))
        {
            throw new ArgumentException("Product code is required.", nameof(productCode));
        }

        if (string.IsNullOrWhiteSpace(productDescription))
        {
            throw new ArgumentException("Product description is required.", nameof(productDescription));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        var normalizedProductCode = productCode.Trim().ToUpperInvariant();
        var normalizedProductDescription = productDescription.Trim();

        if (normalizedProductCode.Length > MaxProductCodeLength)
        {
            throw new ArgumentException(
                $"Product code cannot exceed {MaxProductCodeLength} characters.",
                nameof(productCode));
        }

        if (normalizedProductDescription.Length > MaxProductDescriptionLength)
        {
            throw new ArgumentException(
                $"Product description cannot exceed {MaxProductDescriptionLength} characters.",
                nameof(productDescription));
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductCode = normalizedProductCode;
        ProductDescription = normalizedProductDescription;
        Quantity = quantity;
    }

    public Guid Id { get; private set; }

    public Guid InvoiceId { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductCode { get; private set; } = string.Empty;

    public string ProductDescription { get; private set; } = string.Empty;

    public int Quantity { get; private set; }

    internal void AssignToInvoice(Guid invoiceId)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
        }

        if (InvoiceId != Guid.Empty)
        {
            throw new InvalidOperationException("Invoice item is already assigned to an invoice.");
        }

        InvoiceId = invoiceId;
    }
}
