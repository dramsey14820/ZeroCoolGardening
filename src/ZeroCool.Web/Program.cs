using ZeroCool.UI.Services;
using ZeroCool.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Storage: JSON files under App_Data (single shared folder for now; per-client isolation is planned for web deployment).
builder.Services.AddSingleton<ZeroCool.Core.Storage.IFileStore>(
    new ZeroCool.Data.LocalFileStore(Path.Combine(builder.Environment.ContentRootPath, "App_Data", "default")));
builder.Services.AddZeroCoolUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ZeroCool.UI.Pages.Home).Assembly);

app.Run();
