using Inventory.Api.Data;
using Inventory.Api.DTOs;
using Inventory.Api.Entities;
using Microsoft.EntityFrameworkCore;

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
        var codeAlreadyExists = await _dbContext.Products
            .AnyAsync(product => product.Code == request.Code);

        if (codeAlreadyExists)
        {
            throw new InvalidOperationException(
                "A product with the provided code already exists.");
        }

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

        return MapToResponse(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync()
    {
        return await _dbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Description)
            .Select(product => new ProductResponse
            {
                Id = product.Id,
                Code = product.Code,
                Description = product.Description,
                StockQuantity = product.StockQuantity,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id);

        return product is null
            ? null
            : MapToResponse(product);
    }

    public async Task DeductStockAsync(DeductStockRequest request)
    {
        if (request.Items.Count == 0)
        {
            throw new ArgumentException(
                "At least one product is required for stock deduction.");
        }

        if (request.Items.Any(item => item.ProductId == Guid.Empty))
        {
            throw new ArgumentException("ProductId is required.");
        }

        var groupedItems = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => (long)item.Quantity)
            })
            .OrderBy(item => item.ProductId)
            .ToList();

        if (groupedItems.Any(item => item.Quantity > int.MaxValue))
        {
            throw new ArgumentException(
                "The requested stock quantity is too large.");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var now = DateTime.UtcNow;

        foreach (var requestedItem in groupedItems)
        {
            var quantity = (int)requestedItem.Quantity;

            var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE products
                SET "StockQuantity" = "StockQuantity" - {quantity},
                    "UpdatedAt" = {now}
                WHERE "Id" = {requestedItem.ProductId}
                  AND "StockQuantity" >= {quantity};
                """);

            if (affectedRows > 0)
            {
                continue;
            }

            var product = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(product =>
                    product.Id == requestedItem.ProductId);

            if (product is null)
            {
                throw new KeyNotFoundException(
                    $"Product '{requestedItem.ProductId}' was not found.");
            }

            throw new InvalidOperationException(
                $"Insufficient stock for product '{product.Code}'. " +
                $"Available: {product.StockQuantity}, requested: {quantity}.");
        }

        await transaction.CommitAsync();
    }

    private static ProductResponse MapToResponse(Product product)
    {
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
