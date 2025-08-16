using Microsoft.EntityFrameworkCore;
using BinFlow.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<BinFlowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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