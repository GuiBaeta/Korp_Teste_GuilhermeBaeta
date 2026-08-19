using Inventory.Api.Data;
using Inventory.Api.DTOs;
using Inventory.Api.Entities;

namespace Inventory.Api.Services;

public class ProductService
{
    private readonly InventoryDbContext _dbContext;

    public ProductService(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        var now = DateTime.UtcNow;

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Description = request.Description,
            StockQuantity = request.StockQuantity,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Products.Add(product);

        await _dbContext.SaveChangesAsync();

        return new ProductResponse
        {
            Id = product.Id,
            Code = product.Code,
            Description = product.Description,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}