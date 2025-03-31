using AnalisePanilha.Shared.Services;
using AnalisePanilha.Shared.ViewModels;
using AnalisePanilha.Web.Components;
using AnalisePanilha.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddSingleton<IExcelComparisonService, ExcelComparisonServiceWeb>();
builder.Services.AddTransient<ExcelComparisonViewModel>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(AnalisePanilha.Shared._Imports).Assembly);

app.Run();
