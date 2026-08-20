namespace Billing.Api.DTOs;

public class InventoryStockDeductionRequest
{
    public List<InventoryStockDeductionItemRequest> Items { get; set; } = new();
}
