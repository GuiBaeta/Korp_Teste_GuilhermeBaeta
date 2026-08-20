using Inventory.Api.Data;
using Inventory.Api.DTOs;
using Inventory.Api.Entities;
using Inventory.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Tests;

public class ProductServiceTests
{
    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsProduct()
    {
        await using var dbContext = CreateDbContext();
        var service = new ProductService(dbContext);

        var response = await service.CreateAsync(new CreateProductRequest
        {
            Code = "PROD-001",
            Description = "Produto de teste",
            StockQuantity = 10
        });

        var persistedProduct = await dbContext.Products.SingleAsync();

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("PROD-001", response.Code);
        Assert.Equal(10, response.StockQuantity);
        Assert.Equal(response.Id, persistedProduct.Id);
        Assert.Equal("Produto de teste", persistedProduct.Description);
    }

    [Fact]
    public async Task CreateAsync_WhenCodeAlreadyExists_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(new Product
        {
            Id = Guid.NewGuid(),
            Code = "PROD-001",
            Description = "Produto existente",
            StockQuantity = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = new ProductService(dbContext);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateProductRequest
            {
                Code = "PROD-001",
                Description = "Produto duplicado",
                StockQuantity = 3
            }));

        Assert.Equal(
            "A product with the provided code already exists.",
            exception.Message);
        Assert.Single(await dbContext.Products.ToListAsync());
    }

    private static InventoryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new InventoryDbContext(options);
    }
}
