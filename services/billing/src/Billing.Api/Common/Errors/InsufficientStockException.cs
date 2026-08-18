namespace Billing.Api.Common.Errors;

public sealed class InsufficientStockException : Exception
{
    public InsufficientStockException(string message)
        : base(message)
    {
    }
}
