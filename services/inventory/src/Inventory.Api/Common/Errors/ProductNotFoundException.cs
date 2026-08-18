namespace Inventory.Api.Common.Errors;

public sealed class ProductNotFoundException(IReadOnlyCollection<Guid> productIds)
    : Exception($"The following products were not found: {string.Join(", ", productIds)}")
{
    public IReadOnlyCollection<Guid> ProductIds { get; } = productIds;
}
