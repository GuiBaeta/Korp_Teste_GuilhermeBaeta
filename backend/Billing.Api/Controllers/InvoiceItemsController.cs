using Billing.Api.DTOs;
using Billing.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[ApiController]
[Route("api/invoices/{invoiceId:guid}/items")]
public class InvoiceItemsController : ControllerBase
{
    private readonly InvoiceItemService _invoiceItemService;

    public InvoiceItemsController(InvoiceItemService invoiceItemService)
    {
        _invoiceItemService = invoiceItemService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(List<InvoiceItemResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<InvoiceItemResponse>>> GetAll(
        Guid invoiceId)
    {
        var items = await _invoiceItemService.GetAllAsync(invoiceId);

        return Ok(items);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(InvoiceItemResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceItemResponse>> Add(
        Guid invoiceId,
        AddInvoiceItemRequest request)
    {
        var item = await _invoiceItemService.AddAsync(
            invoiceId,
            request);

        return StatusCode(StatusCodes.Status201Created, item);
    }

    [HttpPut("{itemId:guid}")]
    [ProducesResponseType(
        typeof(InvoiceItemResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InvoiceItemResponse>> UpdateQuantity(
        Guid invoiceId,
        Guid itemId,
        UpdateInvoiceItemRequest request)
    {
        var item = await _invoiceItemService.UpdateQuantityAsync(
            invoiceId,
            itemId,
            request);

        return Ok(item);
    }

    [HttpDelete("{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ApiErrorResponse),
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid invoiceId,
        Guid itemId)
    {
        await _invoiceItemService.DeleteAsync(invoiceId, itemId);

        return NoContent();
    }
}
