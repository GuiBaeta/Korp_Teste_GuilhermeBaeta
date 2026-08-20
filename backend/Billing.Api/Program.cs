using Billing.Api.Data;
using Billing.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
