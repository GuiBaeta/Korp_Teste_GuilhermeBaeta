using Inventory.Api.DTOs;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Inventory.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ProductResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ProductResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll()
    {
        var products = await _productService.GetAllAsync();

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ProductResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);

        if (product is null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        return Ok(product);
    }

    [HttpPost("deduct-stock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeductStock(DeductStockRequest request)
    {
        await _productService.DeductStockAsync(request);

        return NoContent();
    }
}
