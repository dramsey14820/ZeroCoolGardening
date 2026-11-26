using ZeroCool.Core.Storage;
using ZeroCool.Data;
using ZeroCool.UI.Services;
using ZeroCool.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpContextAccessor();

// Per-browser JSON file storage under App_Data/{clientId}/ (no accounts in v1; use JSON Export/Import to move across devices).
const string ClientCookieName = "zc_client_id";
var appDataRoot = Path.Combine(builder.Environment.ContentRootPath, "App_Data");

builder.Services.AddScoped<IFileStore>(sp =>
{
    var http = sp.GetService<IHttpContextAccessor>()?.HttpContext;
    var rawId = http?.Items[ClientCookieName] as string
        ?? http?.Request.Cookies[ClientCookieName];
    var safeId = SanitizeClientId(rawId);
    return new LocalFileStore(Path.Combine(appDataRoot, safeId));
});

builder.Services.AddZeroCoolUI();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Assign a persistent per-browser client ID cookie on first request
app.Use(async (context, next) =>
{
    var id = context.Request.Cookies[ClientCookieName];
    if (string.IsNullOrWhiteSpace(id) || id.Length > 40 || id.Any(c => !char.IsLetterOrDigit(c)))
    {
        id = Guid.NewGuid().ToString("N");
        context.Response.Cookies.Append(ClientCookieName, id, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddYears(2),
            IsEssential = true
        });
    }
    context.Items[ClientCookieName] = id;
    await next();
});

app.UseAntiforgery();
app.MapStaticAssets();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", app = "ZeroCoolGardening" }));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(ZeroCool.UI.Pages.Home).Assembly);

app.Run();

static string SanitizeClientId(string? id)
{
    if (string.IsNullOrWhiteSpace(id)) return "default";
    var clean = new string(id.Where(char.IsLetterOrDigit).Take(40).ToArray());
    return string.IsNullOrEmpty(clean) ? "default" : clean;
}
