using Billing.Api.Clients.Inventory;
using Billing.Api.Common.Errors;
using Billing.Api.Data;
using Billing.Api.Domain;
using Billing.Api.Features.Invoices.CreateInvoice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api.Features.Invoices;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController(
    BillingDbContext dbContext,
    IInventoryClient inventoryClient,
    ILogger<InvoicesController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<InvoiceResponse>> Create(
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(item => item.ProductId).ToList();

        var products = await inventoryClient.GetProductsAsync(productIds, cancellationToken);
        var productsById = products.ToDictionary(product => product.Id);

        var missingProductIds = productIds.Where(id => !productsById.ContainsKey(id)).ToList();
        if (missingProductIds.Count > 0)
        {
            throw new ProductNotFoundException(missingProductIds);
        }

        var items = request.Items.Select(item =>
        {
            var product = productsById[item.ProductId];
            return new InvoiceItem(product.Id, product.Code, product.Description, item.Quantity);
        });

        var invoice = new Invoice(items);

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Invoice {InvoiceId} with number {InvoiceNumber} was created with {ItemCount} item(s).",
            invoice.Id,
            invoice.Number,
            invoice.Items.Count);

        var response = InvoiceResponse.FromInvoice(invoice);

        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InvoiceSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InvoiceSummaryResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var invoices = await dbContext.Invoices
            .AsNoTracking()
            .OrderByDescending(invoice => invoice.Number)
            .Select(invoice => new InvoiceSummaryResponse(
                invoice.Id,
                invoice.Number,
                invoice.Status.ToString(),
                invoice.CreatedAt,
                invoice.ClosedAt))
            .ToListAsync(cancellationToken);

        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<InvoiceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await dbContext.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.Id == id)
            .Select(invoice => new InvoiceResponse(
                invoice.Id,
                invoice.Number,
                invoice.Status.ToString(),
                invoice.CreatedAt,
                invoice.ClosedAt,
                invoice.Items
                    .Select(item => new InvoiceItemResponse(
                        item.ProductId,
                        item.ProductCode,
                        item.ProductDescription,
                        item.Quantity))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        if (invoice is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Invoice not found",
                detail: "An invoice with the provided identifier was not found.");
        }

        return Ok(invoice);
    }
}
