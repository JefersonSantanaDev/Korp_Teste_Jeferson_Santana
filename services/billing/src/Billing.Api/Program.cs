using Billing.Api.Clients.Inventory;
using Billing.Api.Common.Errors;
using Billing.Api.Data;
using Microsoft.EntityFrameworkCore;

const string frontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

var billingConnectionString = builder.Configuration.GetConnectionString("BillingDatabase")
    ?? throw new InvalidOperationException("Connection string 'BillingDatabase' is not configured.");
var frontendOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];
var inventoryBaseUrl = builder.Configuration["Inventory:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration 'Inventory:BaseUrl' is not set.");
var inventoryTimeoutSeconds = builder.Configuration.GetValue("Inventory:TimeoutSeconds", 5);

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
builder.Services.AddDbContext<BillingDbContext>(options =>
    options.UseNpgsql(billingConnectionString));
builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
{
    client.BaseAddress = new Uri(inventoryBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(inventoryTimeoutSeconds);
});

var app = builder.Build();

// Applies pending migrations on every startup so `docker compose up --build`
// works against an empty database without a separate manual migration step.
using (var migrationScope = app.Services.CreateScope())
{
    var dbContext = migrationScope.ServiceProvider.GetRequiredService<BillingDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Billing API v1");
        options.DocumentTitle = "Billing API";
    });
}

app.UseHttpsRedirection();

app.UseCors(frontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>Exposed so integration tests can host this API via <c>WebApplicationFactory&lt;Program&gt;</c>.</summary>
public partial class Program;
