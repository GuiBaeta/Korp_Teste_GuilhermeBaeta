using System.ComponentModel.DataAnnotations;

namespace Billing.Api.DTOs;

public class AddInvoiceItemRequest
{
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
