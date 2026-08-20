using Billing.Api.Data;
using Billing.Api.DTOs;
using Billing.Api.Exceptions;
using Billing.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var messages = context.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToList();

            var message = messages.Count == 0
                ? "Invalid request."
                : string.Join(" ", messages);

            return new BadRequestObjectResult(new ApiErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Message = message,
                TraceId = context.HttpContext.TraceIdentifier
            });
        };
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("BillingDatabase")));

builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<InvoiceItemService>();

var inventoryApiUrl = builder.Configuration["Services:InventoryApi"]
    ?? throw new InvalidOperationException(
        "Inventory API URL is not configured.");

builder.Services.AddHttpClient<InventoryApiClient>(client =>
{
    client.BaseAddress = new Uri(inventoryApiUrl);
    client.Timeout = TimeSpan.FromSeconds(5);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapControllers();

app.Run();
