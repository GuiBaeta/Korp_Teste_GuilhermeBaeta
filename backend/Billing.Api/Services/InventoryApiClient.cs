using System.Net;
using System.Net.Http.Json;
using Billing.Api.DTOs;

namespace Billing.Api.Services;

public class InventoryApiClient
{
    private readonly HttpClient _httpClient;

    public InventoryApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<InventoryProductResponse?> GetProductByIdAsync(Guid productId)
    {
        using var response = await _httpClient.GetAsync($"api/products/{productId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<InventoryProductResponse>();
    }

    public async Task DeductStockAsync(InventoryStockDeductionRequest request)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/products/deduct-stock",
            request);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await response.Content.ReadFromJsonAsync<InventoryErrorResponse>();
        var message = string.IsNullOrWhiteSpace(error?.Message)
            ? "Inventory service rejected the stock deduction."
            : error.Message;

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new KeyNotFoundException(message);
        }

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException(message);
        }

        response.EnsureSuccessStatusCode();
    }

    private sealed class InventoryErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
