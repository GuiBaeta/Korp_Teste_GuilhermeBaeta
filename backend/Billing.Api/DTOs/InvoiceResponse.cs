using Billing.Api.Enums;

namespace Billing.Api.DTOs;

public class InvoiceResponse
{
    public Guid Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public InvoiceStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ClosedAt { get; set; }
}
