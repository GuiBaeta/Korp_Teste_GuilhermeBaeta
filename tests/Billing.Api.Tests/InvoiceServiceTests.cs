using System.Net;
using Billing.Api.Data;
using Billing.Api.Entities;
using Billing.Api.Enums;
using Billing.Api.Exceptions;
using Billing.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api.Tests;

public class InvoiceServiceTests
{
    [Fact]
    public async Task CloseAsync_WhenInventoryAcceptsDeduction_ClosesInvoice()
    {
        await using var dbContext = CreateDbContext();
        var invoice = CreateOpenInvoiceWithItem(quantity: 2);
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        HttpRequestMessage? capturedRequest = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var service = new InvoiceService(
            dbContext,
            CreateInventoryClient(handler));

        var response = await service.CloseAsync(invoice.Id);

        Assert.Equal(InvoiceStatus.Closed, response.Status);
        Assert.NotNull(response.ClosedAt);
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest.Method);
        Assert.Equal("/api/products/deduct-stock", capturedRequest.RequestUri?.AbsolutePath);

        var persistedInvoice = await dbContext.Invoices.SingleAsync();
        Assert.Equal(InvoiceStatus.Closed, persistedInvoice.Status);
        Assert.NotNull(persistedInvoice.ClosedAt);
    }

    [Fact]
    public async Task CloseAsync_WhenInventoryIsUnavailable_KeepsInvoiceOpen()
    {
        await using var dbContext = CreateDbContext();
        var invoice = CreateOpenInvoiceWithItem(quantity: 2);
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        var handler = new StubHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var service = new InvoiceService(
            dbContext,
            CreateInventoryClient(handler));

        await Assert.ThrowsAsync<InventoryUnavailableException>(() =>
            service.CloseAsync(invoice.Id));

        var persistedInvoice = await dbContext.Invoices.SingleAsync();
        Assert.Equal(InvoiceStatus.Open, persistedInvoice.Status);
        Assert.Null(persistedInvoice.ClosedAt);
    }

    [Fact]
    public async Task CloseAsync_WhenInvoiceHasNoItems_RejectsClosing()
    {
        await using var dbContext = CreateDbContext();
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Number = "NF-2026-000001",
            Status = InvoiceStatus.Open,
            CreatedAt = DateTime.UtcNow
        };
        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        var service = new InvoiceService(
            dbContext,
            CreateInventoryClient(new StubHttpMessageHandler(_ =>
                new HttpResponseMessage(HttpStatusCode.OK))));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CloseAsync(invoice.Id));

        Assert.Equal(
            "An invoice must have at least one item before it can be closed.",
            exception.Message);
        Assert.Equal(InvoiceStatus.Open, invoice.Status);
        Assert.Null(invoice.ClosedAt);
    }

    private static BillingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BillingDbContext(options);
    }

    private static Invoice CreateOpenInvoiceWithItem(int quantity)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Number = "NF-2026-000001",
            Status = InvoiceStatus.Open,
            CreatedAt = DateTime.UtcNow
        };

        invoice.Items.Add(new InvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            ProductId = Guid.NewGuid(),
            ProductCode = "PROD-001",
            ProductDescription = "Produto de teste",
            Quantity = quantity
        });

        return invoice;
    }

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
