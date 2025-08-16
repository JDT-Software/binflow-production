using Microsoft.EntityFrameworkCore;
using BinFlow.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with Railway database connection
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Convert Railway PostgreSQL URL format to .NET connection string format
if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("postgresql://"))
{
    var uri = new Uri(connectionString);
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
}

Console.WriteLine($"Using connection string: {(!string.IsNullOrEmpty(connectionString) ? "Found and converted" : "Not found")}");

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