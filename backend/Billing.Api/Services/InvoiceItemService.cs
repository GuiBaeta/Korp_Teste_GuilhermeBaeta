using Billing.Api.Data;
using Billing.Api.DTOs;
using Billing.Api.Entities;
using Billing.Api.Enums;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api.Services;

public class InvoiceItemService
{
    private readonly BillingDbContext _dbContext;
    private readonly InventoryApiClient _inventoryApiClient;

    public InvoiceItemService(
        BillingDbContext dbContext,
        InventoryApiClient inventoryApiClient)
    {
        _dbContext = dbContext;
        _inventoryApiClient = inventoryApiClient;
    }

    public async Task<List<InvoiceItemResponse>> GetAllAsync(Guid invoiceId)
    {
        var invoiceExists = await _dbContext.Invoices
            .AsNoTracking()
            .AnyAsync(invoice => invoice.Id == invoiceId);

        if (!invoiceExists)
        {
            throw new KeyNotFoundException("Invoice not found.");
        }

        return await _dbContext.InvoiceItems
            .AsNoTracking()
            .Where(item => item.InvoiceId == invoiceId)
            .OrderBy(item => item.ProductDescription)
            .Select(item => new InvoiceItemResponse
            {
                Id = item.Id,
                InvoiceId = item.InvoiceId,
                ProductId = item.ProductId,
                ProductCode = item.ProductCode,
                ProductDescription = item.ProductDescription,
                Quantity = item.Quantity
            })
            .ToListAsync();
    }

    public async Task<InvoiceItemResponse> AddAsync(
        Guid invoiceId,
        AddInvoiceItemRequest request)
    {
        await EnsureInvoiceIsOpenAsync(invoiceId);

        if (request.ProductId == Guid.Empty)
        {
            throw new ArgumentException("ProductId is required.");
        }

        var productAlreadyAdded = await _dbContext.InvoiceItems
            .AnyAsync(item =>
                item.InvoiceId == invoiceId &&
                item.ProductId == request.ProductId);

        if (productAlreadyAdded)
        {
            throw new InvalidOperationException(
                "Product is already included in this invoice.");
        }

        var product = await _inventoryApiClient.GetProductByIdAsync(
            request.ProductId);

        if (product is null)
        {
            throw new KeyNotFoundException("Product not found.");
        }

        var item = new InvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            ProductId = product.Id,
            ProductCode = product.Code,
            ProductDescription = product.Description,
            Quantity = request.Quantity
        };

        _dbContext.InvoiceItems.Add(item);

        await _dbContext.SaveChangesAsync();

        return MapToResponse(item);
    }

    public async Task<InvoiceItemResponse> UpdateQuantityAsync(
        Guid invoiceId,
        Guid itemId,
        UpdateInvoiceItemRequest request)
    {
        await EnsureInvoiceIsOpenAsync(invoiceId);

        var item = await _dbContext.InvoiceItems
            .FirstOrDefaultAsync(item =>
                item.Id == itemId &&
                item.InvoiceId == invoiceId);

        if (item is null)
        {
            throw new KeyNotFoundException("Invoice item not found.");
        }

        item.Quantity = request.Quantity;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(item);
    }

    public async Task DeleteAsync(Guid invoiceId, Guid itemId)
    {
        await EnsureInvoiceIsOpenAsync(invoiceId);

        var item = await _dbContext.InvoiceItems
            .FirstOrDefaultAsync(item =>
                item.Id == itemId &&
                item.InvoiceId == invoiceId);

        if (item is null)
        {
            throw new KeyNotFoundException("Invoice item not found.");
        }

        _dbContext.InvoiceItems.Remove(item);

        await _dbContext.SaveChangesAsync();
    }

    private async Task EnsureInvoiceIsOpenAsync(Guid invoiceId)
    {
        var invoice = await _dbContext.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(invoice => invoice.Id == invoiceId);

        if (invoice is null)
        {
            throw new KeyNotFoundException("Invoice not found.");
        }

        if (invoice.Status != InvoiceStatus.Open)
        {
            throw new InvalidOperationException(
                "Only open invoices can be modified.");
        }
    }

    private static InvoiceItemResponse MapToResponse(InvoiceItem item)
    {
        return new InvoiceItemResponse
        {
            Id = item.Id,
            InvoiceId = item.InvoiceId,
            ProductId = item.ProductId,
            ProductCode = item.ProductCode,
            ProductDescription = item.ProductDescription,
            Quantity = item.Quantity
        };
    }
}
