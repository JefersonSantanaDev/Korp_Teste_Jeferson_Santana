namespace Billing.Tests;

/// <summary>One PostgreSQL container for the whole assembly — starting it per test class would be needlessly slow.</summary>
[CollectionDefinition(nameof(BillingTestCollection))]
public sealed class BillingTestCollection : ICollectionFixture<BillingApiFactory>;
