using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ProviderBilling.Data;
using ProviderBillingBlazor.Components;

var builder = WebApplication.CreateBuilder(args);

// Build the absolute path to the database, relative to the project directory
var projectDir = AppContext.BaseDirectory;
var dbPath = Path.GetFullPath(Path.Combine(projectDir, @"..\..\..\..\Data\database.db"));

// Ensure the directory exists
var dbDir = Path.GetDirectoryName(dbPath);
if (!Directory.Exists(dbDir))
{
    throw new Exception($"Database directory does not exist: {dbDir}");
}

builder.Services.AddSignalR(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
});

// Add services to the container.
builder.Services.AddRazorComponents()
   .AddInteractiveServerComponents();

try
{
    builder.Services.AddDbContext<ProviderBillingContext>(options =>
    {
        options.UseSqlite($"Data Source={dbPath}")
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    });
    Console.WriteLine($"Database context registered at: {dbPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"Database context error: {ex.Message}");
    throw;
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                Console.WriteLine($"Error: {exceptionHandlerPathFeature.Error}");
            }
            await Task.CompletedTask;
        });
    });
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();
