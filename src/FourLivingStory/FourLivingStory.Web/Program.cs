using FourLivingStory.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// ── Aspire ────────────────────────────────────────────────────────────────────
builder.AddServiceDefaults();

// ── Blazor WASM host ──────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// ── API URL discovery ─────────────────────────────────────────────────────────
// The WASM client reads this endpoint at startup to discover the ApiService URL.
var apiServiceUrl = builder.Configuration["services:apiservice:https:0"]
    ?? builder.Configuration["services:apiservice:http:0"]
    ?? "https://localhost:7001";

// ── Logto config (injected by AppHost via env vars) ───────────────────────────
var logtoAuthority = builder.Configuration["Logto:Authority"] ?? "";
var logtoClientId = builder.Configuration["Logto:ClientId"] ?? "";

// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();

// Exposes runtime config to the WASM client.
app.MapGet("/_config", () => new
{
    ApiServiceUrl = apiServiceUrl,
    LogtoAuthority = logtoAuthority,
    LogtoClientId = logtoClientId
}).WithName("GetClientConfig");

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FourLivingStory.Web.Client._Imports).Assembly);

app.MapDefaultEndpoints();

app.Run();
