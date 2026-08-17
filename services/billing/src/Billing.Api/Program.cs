using Billing.Api.Data;
using Microsoft.EntityFrameworkCore;

const string frontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

var billingConnectionString = builder.Configuration.GetConnectionString("BillingDatabase")
    ?? throw new InvalidOperationException("Connection string 'BillingDatabase' is not configured.");
var frontendOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddControllers();
builder.Services.AddOpenApi();
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

var app = builder.Build();

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
