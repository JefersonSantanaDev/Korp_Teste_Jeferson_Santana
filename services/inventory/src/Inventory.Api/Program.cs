using Inventory.Api.Common.Errors;
using Inventory.Api.Data;
using Microsoft.EntityFrameworkCore;

const string frontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

var inventoryConnectionString = builder.Configuration.GetConnectionString("InventoryDatabase")
    ?? throw new InvalidOperationException("Connection string 'InventoryDatabase' is not configured.");
var frontendOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        if (frontendOrigins.Length > 0)
        {
            policy
                .WithOrigins(frontendOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});
builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(inventoryConnectionString));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Inventory API v1");
        options.DocumentTitle = "Inventory API";
    });
}

app.UseHttpsRedirection();

app.UseCors(frontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
