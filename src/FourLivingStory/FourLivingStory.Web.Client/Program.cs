using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ── Discover API URL from Web host ────────────────────────────────────────────
using var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var config = await http.GetFromJsonAsync<ClientConfig>("/_config");
var apiBaseUrl = config?.ApiServiceUrl ?? builder.HostEnvironment.BaseAddress;

// ── HTTP client for ApiService ────────────────────────────────────────────────
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// ── Auth (Logto OIDC PKCE) ────────────────────────────────────────────────────
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Logto", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
});

await builder.Build().RunAsync();

internal sealed record ClientConfig(string ApiServiceUrl);
