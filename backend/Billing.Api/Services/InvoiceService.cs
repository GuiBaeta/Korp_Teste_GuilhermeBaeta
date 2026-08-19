using Billing.Api.Data;
using Billing.Api.DTOs;
using Billing.Api.Entities;
using Billing.Api.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Billing.Api.Services;

public class InvoiceService
{
    private readonly BillingDbContext _dbContext;

    public InvoiceService(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<InvoiceResponse> CreateAsync()
    {
        var now = DateTime.UtcNow;

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        var nextNumber = await GetNextNumberAsync(
            now.Year,
            transaction);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Number = $"NF-{now.Year}-{nextNumber:D6}",
            Status = InvoiceStatus.Open,
            CreatedAt = now,
            ClosedAt = null
        };

        _dbContext.Invoices.Add(invoice);

        await _dbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return MapToResponse(invoice);
    }

    public async Task<List<InvoiceResponse>> GetAllAsync()
    {
        return await _dbContext.Invoices
            .AsNoTracking()
            .OrderByDescending(invoice => invoice.CreatedAt)
            .Select(invoice => new InvoiceResponse
            {
                Id = invoice.Id,
                Number = invoice.Number,
                Status = invoice.Status,
                CreatedAt = invoice.CreatedAt,
                ClosedAt = invoice.ClosedAt
            })
            .ToListAsync();
    }

    public async Task<InvoiceResponse?> GetByIdAsync(Guid id)
    {
        var invoice = await _dbContext.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(invoice => invoice.Id == id);

        return invoice is null
            ? null
            : MapToResponse(invoice);
    }

    private async Task<long> GetNextNumberAsync(
        int year,
        IDbContextTransaction transaction)
    {
        var connection = _dbContext.Database.GetDbConnection();

        using var command = connection.CreateCommand();

        command.Transaction = transaction.GetDbTransaction();

        command.CommandText = """
            INSERT INTO invoice_number_sequences ("Year", "LastNumber")
            VALUES (@year, 1)
            ON CONFLICT ("Year")
            DO UPDATE
            SET "LastNumber" = invoice_number_sequences."LastNumber" + 1
            RETURNING "LastNumber";
            """;

        var yearParameter = command.CreateParameter();

        yearParameter.ParameterName = "year";
        yearParameter.Value = year;

        command.Parameters.Add(yearParameter);

        var result = await command.ExecuteScalarAsync();

        if (result is null)
        {
            throw new InvalidOperationException(
                "Could not generate the invoice number.");
        }

        return Convert.ToInt64(result);
    }

    private static InvoiceResponse MapToResponse(Invoice invoice)
    {
        return new InvoiceResponse
        {
            Id = invoice.Id,
            Number = invoice.Number,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            ClosedAt = invoice.ClosedAt
        };
    }
}