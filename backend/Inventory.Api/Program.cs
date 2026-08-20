using Inventory.Api.Data;
using Inventory.Api.DTOs;
using Inventory.Api.Exceptions;
using Inventory.Api.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
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

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("InventoryDatabase")));

builder.Services.AddScoped<ProductService>();

builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
