using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BinFlow.Client;
using BinFlow.Client.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HTTP client to call our API
var apiBaseUrl = builder.HostEnvironment.IsDevelopment() 
    ? "http://localhost:5059/"
    : "https://your-api-app-service.azurewebsites.net/"; // Replace with your actual API URL

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Add MudBlazor services
builder.Services.AddMudServices();

// Add our services
builder.Services.AddScoped<IProductionService, ProductionService>();

await builder.Build().RunAsync();