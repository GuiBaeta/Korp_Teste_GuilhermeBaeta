using System.ComponentModel.DataAnnotations;

namespace Inventory.Api.DTOs;

public class DeductStockItemRequest
{
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
