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

        return Created(
            $"/api/invoices/{invoice.Id}",
            invoice);
    }
}
