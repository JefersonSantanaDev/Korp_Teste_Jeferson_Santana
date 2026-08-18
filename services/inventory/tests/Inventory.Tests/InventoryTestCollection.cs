namespace Inventory.Tests;

/// <summary>One PostgreSQL container for the whole assembly — starting it per test class would be needlessly slow.</summary>
[CollectionDefinition(nameof(InventoryTestCollection))]
public sealed class InventoryTestCollection : ICollectionFixture<InventoryApiFactory>;
