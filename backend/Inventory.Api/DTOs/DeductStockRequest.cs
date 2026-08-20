using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs;

public class DeductStockRequest
{
    [Required]
    [MinLength(1)]
    public List<DeductStockItemRequest> Items { get; set; } = new();
}
