using System.ComponentModel.DataAnnotations;

namespace Billing.Api.DTOs;

public class UpdateInvoiceItemRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}