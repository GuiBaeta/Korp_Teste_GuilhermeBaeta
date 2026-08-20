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
    public async Task<ActionResult<List<InvoiceItemResponse>>> GetAll(
        Guid invoiceId)
    {
        try
        {
            var items = await _invoiceItemService.GetAllAsync(invoiceId);

            return Ok(items);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceItemResponse>> Add(
        Guid invoiceId,
        AddInvoiceItemRequest request)
    {
        try
        {
            var item = await _invoiceItemService.AddAsync(
                invoiceId,
                request);

            return StatusCode(StatusCodes.Status201Created, item);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{itemId:guid}")]
    public async Task<ActionResult<InvoiceItemResponse>> UpdateQuantity(
        Guid invoiceId,
        Guid itemId,
        UpdateInvoiceItemRequest request)
    {
        try
        {
            var item = await _invoiceItemService.UpdateQuantityAsync(
                invoiceId,
                itemId,
                request);

            return Ok(item);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{itemId:guid}")]
    public async Task<IActionResult> Delete(
        Guid invoiceId,
        Guid itemId)
    {
        try
        {
            await _invoiceItemService.DeleteAsync(invoiceId, itemId);

            return NoContent();
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}