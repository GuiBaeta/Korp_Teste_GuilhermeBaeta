using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Billing.Api.Data;
using Billing.Api.DTOs;
using Billing.Api.Entities;
using Billing.Api.Enums;
using Billing.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api.Tests;

public class InvoiceItemServiceTests
{
    [Fact]
    public async Task AddAsync_WhenInvoiceIsOpen_StoresProductSnapshot()
    {
        await using var dbContext = CreateDbContext();
        var invoice = CreateInvoice(InvoiceStatus.Open);
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        var productId = Guid.NewGuid();
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new InventoryProductResponse
            {
                Id = productId,
                Code = "PROD-001",
                Description = "Produto consultado no estoque",
                StockQuantity = 8
            })
        });
        var service = new InvoiceItemService(
            dbContext,
            CreateInventoryClient(handler));

        var response = await service.AddAsync(invoice.Id, new AddInvoiceItemRequest
        {
            ProductId = productId,
            Quantity = 2
        });

        var persistedItem = await dbContext.InvoiceItems.SingleAsync();

        Assert.Equal("PROD-001", response.ProductCode);
        Assert.Equal("Produto consultado no estoque", response.ProductDescription);
        Assert.Equal(2, response.Quantity);
        Assert.Equal(productId, persistedItem.ProductId);
        Assert.Equal("PROD-001", persistedItem.ProductCode);
    }

    [Fact]
    public async Task UpdateQuantityAsync_WhenInvoiceIsClosed_ThrowsInvalidOperationException()
    {
        await using var dbContext = CreateDbContext();
        var invoice = CreateInvoice(InvoiceStatus.Closed);
        var item = new InvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            ProductId = Guid.NewGuid(),
            ProductCode = "PROD-001",
            ProductDescription = "Produto de teste",
            Quantity = 1
        };
        invoice.Items.Add(item);
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        var service = new InvoiceItemService(
            dbContext,
            CreateInventoryClient(new StubHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK))));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateQuantityAsync(
                invoice.Id,
                item.Id,
                new UpdateInvoiceItemRequest { Quantity = 3 }));

        Assert.Equal("Only open invoices can be modified.", exception.Message);
        Assert.Equal(1, (await dbContext.InvoiceItems.SingleAsync()).Quantity);
    }

    private static BillingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BillingDbContext(options);
    }

    private static Invoice CreateInvoice(InvoiceStatus status) => new()
    {
        Id = Guid.NewGuid(),
        Number = $"NF-2026-{Random.Shared.Next(1, 999999):D6}",
        Status = status,
        CreatedAt = DateTime.UtcNow,
        ClosedAt = status == InvoiceStatus.Closed ? DateTime.UtcNow : null
    };

    private static InventoryApiClient CreateInventoryClient(HttpMessageHandler handler) =>
        new(new HttpClient(handler)
        {
            BaseAddress = new Uri("http://inventory.test/")
        });

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responseFactory(request));
    }
}
