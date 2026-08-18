using Inventory.Api.Data;
using Inventory.Api.Domain;
using Inventory.Api.Features.Products.CreateProduct;
using Inventory.Api.Features.Products.LookupProducts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Features.Products;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    InventoryDbContext dbContext,
    ILogger<ProductsController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var product = new Product(request.Code, request.Description, request.Stock);

        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Product {ProductId} with code {ProductCode} was created.",
            product.Id,
            product.Code);

        var response = ProductResponse.FromProduct(product);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Code)
            .Select(product => new ProductResponse(
                product.Id,
                product.Code,
                product.Description,
                product.Stock))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Where(product => product.Id == id)
            .Select(product => new ProductResponse(
                product.Id,
                product.Code,
                product.Description,
                product.Stock))
            .SingleOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Product not found",
                detail: "A product with the provided identifier was not found.");
        }

        return Ok(product);
    }

    /// <summary>
    /// Batch product snapshot lookup used by Billing when creating an invoice,
    /// avoiding one HTTP round-trip per item. Products that do not exist are
    /// simply absent from the response; the caller determines what is missing.
    /// </summary>
    [HttpPost("lookup")]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> Lookup(
        LookupProductsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.ProductIds.Count == 0)
        {
            return Ok(Array.Empty<ProductResponse>());
        }

        var products = await dbContext.Products
            .AsNoTracking()
            .Where(product => request.ProductIds.Contains(product.Id))
            .Select(product => new ProductResponse(
                product.Id,
                product.Code,
                product.Description,
                product.Stock))
            .ToListAsync(cancellationToken);

        return Ok(products);
    }
}
