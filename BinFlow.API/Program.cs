using Microsoft.EntityFrameworkCore;
using BinFlow.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework with Railway database connection using individual variables
var pgHost = Environment.GetEnvironmentVariable("PGHOST");
var pgPort = Environment.GetEnvironmentVariable("PGPORT");
var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
var pgUser = Environment.GetEnvironmentVariable("PGUSER");
var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");

Console.WriteLine($"PGHOST: {pgHost ?? "NULL"}");
Console.WriteLine($"PGPORT: {pgPort ?? "NULL"}");
Console.WriteLine($"PGDATABASE: {pgDatabase ?? "NULL"}");
Console.WriteLine($"PGUSER: {pgUser ?? "NULL"}");
Console.WriteLine($"PGPASSWORD: {(string.IsNullOrEmpty(pgPassword) ? "NULL" : "***")}");

string connectionString = null;

if (!string.IsNullOrEmpty(pgHost) && !string.IsNullOrEmpty(pgDatabase) && !string.IsNullOrEmpty(pgUser) && !string.IsNullOrEmpty(pgPassword))
{
    connectionString = $"Host={pgHost};Port={pgPort ?? "5432"};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true";
    Console.WriteLine("Built connection string from Railway variables");
}
else
{
    // Railway PostgreSQL fallback for local development
    connectionString = "Host=gondola.proxy.rlwy.net;Port=35904;Database=railway;Username=postgres;Password=cuXAkVzsySNtqrBDkPRIXrjqLmSguJcJ;SSL Mode=Require;Trust Server Certificate=true;Timeout=60;Command Timeout=60;Pooling=true;Connection Lifetime=0";
    Console.WriteLine("Using Railway PostgreSQL fallback connection string");
}

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<BinFlowDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    Console.WriteLine("Warning: No database connection found. API will use mock data only.");
}

// 🔧 FIXED CORS - Allow Blazor client specifically
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://localhost:5108", "https://localhost:5108")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure database is created and tables exist
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<BinFlowDbContext>();
        Console.WriteLine("Attempting to ensure database is created...");
        context.Database.EnsureCreated();
        Console.WriteLine("Database tables created successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database creation error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔧 FIXED: Use the correct CORS policy name
app.UseCors("AllowBlazorClient");

app.UseHttpsRedirection();

// 🎯 CRITICAL: Serve static files for any wwwroot content
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// 🎯 CRITICAL: Map API controllers FIRST
app.MapControllers();

// 🎯 CLEAN: Simple fallback to index.html for non-API routes
app.MapFallbackToFile("index.html");

app.Run();