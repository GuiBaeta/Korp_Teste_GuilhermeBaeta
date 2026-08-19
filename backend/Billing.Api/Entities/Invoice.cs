using Billing.Api.Enums;

namespace Billing.Api.Entities;

public class Invoice
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public InvoiceStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }
}