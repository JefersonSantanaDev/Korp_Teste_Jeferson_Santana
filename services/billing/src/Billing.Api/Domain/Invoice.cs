namespace Billing.Api.Domain;

public sealed class Invoice
{
    private readonly List<InvoiceItem> _items = [];

    private Invoice()
    {
    }

    public Invoice(IEnumerable<InvoiceItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var invoiceItems = items.ToList();

        if (invoiceItems.Count == 0)
        {
            throw new ArgumentException("Invoice must contain at least one item.", nameof(items));
        }

        if (invoiceItems.Select(item => item.ProductId).Distinct().Count() != invoiceItems.Count)
        {
            throw new ArgumentException("Invoice cannot contain duplicate products.", nameof(items));
        }

        Id = Guid.NewGuid();
        Status = InvoiceStatus.Open;
        CreatedAt = DateTimeOffset.UtcNow;

        foreach (var item in invoiceItems)
        {
            item.AssignToInvoice(Id);
            _items.Add(item);
        }
    }

    public Guid Id { get; private set; }

    public long Number { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public IReadOnlyCollection<InvoiceItem> Items => _items;

    public void Close(DateTimeOffset closedAt)
    {
        if (Status != InvoiceStatus.Open)
        {
            throw new InvalidOperationException("Only an open invoice can be closed.");
        }

        if (closedAt < CreatedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(closedAt), "Closing date cannot precede creation date.");
        }

        Status = InvoiceStatus.Closed;
        ClosedAt = closedAt;
    }
}
