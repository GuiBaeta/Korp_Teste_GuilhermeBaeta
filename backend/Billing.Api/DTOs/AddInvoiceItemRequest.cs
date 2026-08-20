using System.ComponentModel.DataAnnotations;

namespace Billing.Api.DTOs;

public class AddInvoiceItemRequest
{
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(50)]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string ProductDescription { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}