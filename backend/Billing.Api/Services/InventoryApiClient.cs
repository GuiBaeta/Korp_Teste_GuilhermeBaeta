using System.Net;
using System.Net.Http.Json;
using Billing.Api.DTOs;
using Billing.Api.Exceptions;

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
        try
        {
            using var response = await _httpClient.GetAsync(
                $"api/products/{productId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            EnsureInventoryIsAvailable(response);
            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<InventoryProductResponse>();
        }
        catch (InventoryUnavailableException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            throw new InventoryUnavailableException(exception);
        }
        catch (TaskCanceledException exception)
        {
            throw new InventoryUnavailableException(exception);
        }
    }

    public async Task DeductStockAsync(InventoryStockDeductionRequest request)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/products/deduct-stock",
                request);

            if (response.IsSuccessStatusCode)
            {
                return;
            }

            EnsureInventoryIsAvailable(response);

            var error = await response.Content
                .ReadFromJsonAsync<InventoryErrorResponse>();

            var message = string.IsNullOrWhiteSpace(error?.Message)
                ? "Inventory service rejected the stock deduction."
                : error.Message;

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new ArgumentException(message);
            }

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
        catch (InventoryUnavailableException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            throw new InventoryUnavailableException(exception);
        }
        catch (TaskCanceledException exception)
        {
            throw new InventoryUnavailableException(exception);
        }
    }

    private static void EnsureInventoryIsAvailable(HttpResponseMessage response)
    {
        if ((int)response.StatusCode >= 500)
        {
            throw new InventoryUnavailableException();
        }
    }

    private sealed class InventoryErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
