using AnalisePanilha.Shared.Services;
using AnalisePanilha.Shared.Services.Interfaces;
using AnalisePanilha.Shared.ViewModels;
using AnalisePanilha.Web.Components;
using AnalisePanilha.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the AnalisePanilha.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddSingleton<IExcelComparisonService, ExcelComparisonService>();
builder.Services.AddTransient<ExcelComparisonViewModel>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(AnalisePanilha.Shared._Imports).Assembly);

app.Run();
