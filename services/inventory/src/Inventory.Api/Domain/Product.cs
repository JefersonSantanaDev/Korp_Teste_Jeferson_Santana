namespace Inventory.Api.Domain;

public sealed class Product
{
    public const int MaxCodeLength = 50;
    public const int MaxDescriptionLength = 200;

    private Product()
    {
    }

    public Product(string code, string description, int stock)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Product description is required.", nameof(description));
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var normalizedDescription = description.Trim();

        if (normalizedCode.Length > MaxCodeLength)
        {
            throw new ArgumentException(
                $"Product code cannot exceed {MaxCodeLength} characters.",
                nameof(code));
        }

        if (normalizedDescription.Length > MaxDescriptionLength)
        {
            throw new ArgumentException(
                $"Product description cannot exceed {MaxDescriptionLength} characters.",
                nameof(description));
        }

        if (stock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stock), "Product stock cannot be negative.");
        }

        Id = Guid.NewGuid();
        Code = normalizedCode;
        Description = normalizedDescription;
        Stock = stock;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public int Stock { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }
}
