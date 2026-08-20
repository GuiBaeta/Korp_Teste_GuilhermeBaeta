using Billing.Api.DTOs;
using Billing.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Billing.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(InvoiceResponse),
        StatusCodes.Status201Created)]
    public async Task<ActionResult<InvoiceResponse>> Create()
    {
        var invoice = await _invoiceService.CreateAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            invoice);
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(List<InvoiceResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<List<InvoiceResponse>>> GetAll()
    {
        var invoices = await _invoiceService.GetAllAsync();

        return Ok(invoices);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(InvoiceResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InvoiceResponse>> GetById(Guid id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);

        if (invoice is null)
        {
            return NotFound();
        }

        return Ok(invoice);
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(
        typeof(InvoiceResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<InvoiceResponse>> Close(Guid id)
    {
        try
        {
            var invoice = await _invoiceService.CloseAsync(id);

            return Ok(invoice);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }
}
