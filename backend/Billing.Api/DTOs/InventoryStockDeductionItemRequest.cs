namespace Billing.Api.DTOs;

public class InventoryStockDeductionItemRequest
{
    public Guid ProductId { get; set; }

    public int Quantity { get; set; }
}
