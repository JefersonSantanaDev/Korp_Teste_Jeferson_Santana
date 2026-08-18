using System.Net;
using System.Net.Http.Json;
using Billing.Api.Clients.Inventory.Contracts;
using Billing.Api.Common.Errors;

namespace Billing.Api.Clients.Inventory;

public sealed class InventoryClient(HttpClient httpClient, ILogger<InventoryClient> logger)
    : IInventoryClient
{
    public async Task<string> DeductStockAsync(
        Guid operationId,
        IReadOnlyList<StockDeductionItem> items,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/stock/deductions",
                new DeductStockRequest(operationId, items),
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                throw new InsufficientStockException(
                    "Inventory reported insufficient stock for one or more items.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new InventoryUnavailableException(
                    $"Inventory responded with status {(int)response.StatusCode}.");
            }

            var result = await response.Content.ReadFromJsonAsync<StockDeductionResponse>(
                cancellationToken: cancellationToken);

            return result?.Status ?? "processed";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            logger.LogWarning(ex, "Inventory is unavailable while deducting stock for operation {OperationId}.", operationId);
            throw new InventoryUnavailableException("Inventory service is unavailable.", ex);
        }
    }

    public async Task<IReadOnlyList<InventoryProductResponse>> GetProductsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return [];
        }

        try
        {
            var response = await httpClient.PostAsJsonAsync(
                "api/products/lookup",
                new LookupProductsRequest(productIds.ToList()),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new InventoryUnavailableException(
                    $"Inventory responded with status {(int)response.StatusCode}.");
            }

            var products = await response.Content.ReadFromJsonAsync<List<InventoryProductResponse>>(
                cancellationToken: cancellationToken);

            return products ?? [];
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw;
            }

            logger.LogWarning(ex, "Inventory is unavailable while looking up products.");
            throw new InventoryUnavailableException("Inventory service is unavailable.", ex);
        }
    }
}
