namespace Inventory.Api.Domain;

public sealed class StockOperation
{
    private StockOperation()
    {
    }

    public StockOperation(Guid operationId)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("Operation ID cannot be empty.", nameof(operationId));
        }

        Id = Guid.NewGuid();
        OperationId = operationId;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OperationId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
