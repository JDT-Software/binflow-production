using Microsoft.EntityFrameworkCore;
using BinFlow.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with Railway database connection
var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"Raw DATABASE_URL: {rawConnectionString ?? "NULL"}");

var connectionString = rawConnectionString ?? builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Initial connection string: {connectionString ?? "NULL"}");

// Convert Railway PostgreSQL URL format to .NET connection string format
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://"))
{
    try
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo?.Split(':');
        if (userInfo?.Length == 2)
        {
            connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
            Console.WriteLine("Successfully converted PostgreSQL URL to .NET format");
        }
        else
        {
            Console.WriteLine("ERROR: Invalid user info in connection string");
            connectionString = null;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR parsing connection string: {ex.Message}");
        connectionString = null;
    }
}

Console.WriteLine($"Final connection string: {(!string.IsNullOrEmpty(connectionString) ? "Valid" : "Invalid/Empty")}");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<BinFlowDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    Console.WriteLine("Warning: No database connection found. API will use mock data only.");
}

// Add CORS for Blazor WASM - more permissive configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development - allow localhost
            policy.WithOrigins("http://localhost:5108", "https://localhost:5109")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            // Production - allow Azure Static Web Apps domain
            policy.WithOrigins("https://lively-field-072633610.2.azurestaticapps.net")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorWasm");
app.UseAuthorization();
app.MapControllers();

app.Run();