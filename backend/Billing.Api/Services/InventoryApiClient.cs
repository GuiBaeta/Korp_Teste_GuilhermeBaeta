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
}
