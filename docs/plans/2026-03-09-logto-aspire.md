# Logto Aspire Integration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Run Logto as a Docker container resource in Aspire so it starts automatically alongside Postgres in local dev.

**Architecture:** Add a dedicated `logto-postgres` container + a `logto` container in AppHost. Wire Logto's Authority URL into ApiService (JWT validation) and Web (passed to WASM via `/_config`). Web.Client reads Logto Authority + ClientId from `/_config` at startup instead of static appsettings.

**Tech Stack:** .NET 10 Aspire 13.1, `svhd/logto:latest` Docker image, Aspire container resource API

---

## Background

Logto needs:
- Its own PostgreSQL database (`DB_URL` in `postgresql://user:pass@host:port/db` format)
- `ENDPOINT` = the browser-reachable URL of Logto itself (e.g. `http://localhost:3001`)
- `ADMIN_ENDPOINT` = admin console URL (e.g. `http://localhost:3002`)
- Ports: 3001 (OIDC + main app), 3002 (admin console)

The WASM client can't use Aspire service discovery (runs in browser). The existing `/_config` endpoint in Web already solves this for the API URL — we extend it to also return `LogtoAuthority` and `LogtoClientId`.

ApiService already has JWT Bearer middleware reading `Logto:Authority` from config — we just need to inject the correct value via AppHost.

---

## Task 1: Add Logto containers to AppHost

**Files:**
- Modify: `src/FourLivingStory/FourLivingStory.AppHost/AppHost.cs`

**Step 1: Add logto-postgres and Logto container**

Replace the contents of `AppHost.cs` with:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// ── App database ──────────────────────────────────────────────────────────────
var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var db = postgres.AddDatabase("fourlivingstory");

// ── Logto (OIDC provider) ─────────────────────────────────────────────────────
var logtoPostgres = builder.AddPostgres("logto-postgres")
    .WithPgAdmin();

var logtoDb = logtoPostgres.AddDatabase("logto");

// Fixed ports so ENDPOINT can be a known URL in dev.
// Logto MUST know its own public URL at startup — dynamic port allocation
// would require a more complex setup.
var logtoOidcPort = 3001;
var logtoAdminPort = 3002;
var logtoEndpoint = $"http://localhost:{logtoOidcPort}";
var logtoAuthority = $"{logtoEndpoint}/oidc";

var logto = builder.AddContainer("logto", "svhd/logto", "latest")
    .WithHttpEndpoint(port: logtoOidcPort, targetPort: 3001, name: "oidc")
    .WithHttpEndpoint(port: logtoAdminPort, targetPort: 3002, name: "admin")
    .WithEnvironment("TRUST_PROXY_HEADER", "1")
    .WithEnvironment("ENDPOINT", logtoEndpoint)
    .WithEnvironment("ADMIN_ENDPOINT", $"http://localhost:{logtoAdminPort}")
    .WithEnvironment("DB_URL", logtoDb)   // See note below
    .WaitFor(logtoDb);

// NOTE: Aspire injects the DB connection string in Npgsql format
// (Host=...;Port=...;Database=...;Username=...;Password=...).
// Logto requires PostgreSQL URL format (postgresql://user:pass@host:port/db).
// If the app fails to connect on first run, see Task 1 Step 2 below.

// ── Services ──────────────────────────────────────────────────────────────────
var apiService = builder.AddProject<Projects.FourLivingStory_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(db)
    .WaitFor(db)
    .WithEnvironment("Logto__Authority", logtoAuthority)
    .WithEnvironment("Logto__Audience", "http://localhost:7509/"); // ApiService HTTPS port

var web = builder.AddProject<Projects.FourLivingStory_Web>("webfrontend")
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithEnvironment("Logto__Authority", logtoAuthority)
    .WithEnvironment("Logto__ClientId", builder.Configuration["Logto:ClientId"] ?? "");

if (builder.Environment.EnvironmentName == "Development")
{
    web.WithExternalHttpEndpoints();
}
else
{
    builder.AddProject<Projects.FourLivingStory_Gateway>("gateway")
        .WithExternalHttpEndpoints()
        .WithHttpHealthCheck("/health")
        .WithReference(apiService)
        .WithReference(web)
        .WaitFor(web);
}

builder.Build().Run();
```

**Step 2: Handle DB_URL format (if Logto fails to connect)**

If Logto logs a DB connection error, it means the Npgsql format isn't accepted. In that case, replace the `.WithEnvironment("DB_URL", logtoDb)` line with a callback that converts the format:

```csharp
.WithEnvironment("DB_URL", ReferenceExpression.Create(
    $"postgresql://postgres:{logtoPostgres.Resource.PasswordParameter}@{logtoPostgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Host)}:{logtoPostgres.Resource.PrimaryEndpoint.Property(EndpointProperty.Port)}/logto"))
```

If `ReferenceExpression` / `EndpointProperty` aren't available, add this using at the top:
```csharp
using Aspire.Hosting.ApplicationModel;
```

**Step 3: Add Logto:ClientId to AppHost dev config**

Create `src/FourLivingStory/FourLivingStory.AppHost/appsettings.Development.json`:

```json
{
  "Logto": {
    "ClientId": ""
  }
}
```

This is filled in after first Logto startup (see Task 5).

**Step 4: Build to verify no compile errors**

```bash
dotnet build src/FourLivingStory/FourLivingStory.slnx
```

Expected: Build succeeded, 0 errors.

**Step 5: Commit**

```bash
git add src/FourLivingStory/FourLivingStory.AppHost/AppHost.cs
git add src/FourLivingStory/FourLivingStory.AppHost/appsettings.Development.json
git commit -m "feat: add Logto and logto-postgres containers to Aspire AppHost"
```

---

## Task 2: Extend /_config to return Logto config

Web.Client runs in the browser — it can't use Aspire service discovery. The `/_config` endpoint already solves this for the API URL. Extend it to also return `LogtoAuthority` and `LogtoClientId`, which the WASM app will read at startup.

**Files:**
- Modify: `src/FourLivingStory/FourLivingStory.Web/Program.cs`

**Step 1: Read Logto config and extend /_config response**

Replace the `/_config` section in `Web/Program.cs`:

```csharp
// ── API URL discovery ─────────────────────────────────────────────────────────
var apiServiceUrl = builder.Configuration["services:apiservice:https:0"]
    ?? builder.Configuration["services:apiservice:http:0"]
    ?? "https://localhost:7001";

// ── Logto config (injected by AppHost via env vars) ───────────────────────────
var logtoAuthority = builder.Configuration["Logto:Authority"] ?? "";
var logtoClientId = builder.Configuration["Logto:ClientId"] ?? "";
```

And update the `/_config` endpoint:

```csharp
app.MapGet("/_config", () => new
{
    ApiServiceUrl = apiServiceUrl,
    LogtoAuthority = logtoAuthority,
    LogtoClientId = logtoClientId
}).WithName("GetClientConfig");
```

**Step 2: Build to verify**

```bash
dotnet build src/FourLivingStory/FourLivingStory.slnx
```

Expected: Build succeeded, 0 errors.

**Step 3: Commit**

```bash
git add src/FourLivingStory/FourLivingStory.Web/Program.cs
git commit -m "feat: extend /_config to return Logto authority and client ID"
```

---

## Task 3: Update Web.Client to read Logto config from /_config

Currently `Web.Client/Program.cs` reads Logto config from static `appsettings.json`. In dev, the Authority URL is dynamic (based on Aspire port), so it must come from `/_config` instead.

**Files:**
- Modify: `src/FourLivingStory/FourLivingStory.Web.Client/Program.cs`

**Step 1: Update ClientConfig record and OIDC setup**

Replace `Web.Client/Program.cs` with:

```csharp
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ── Discover config from Web host ─────────────────────────────────────────────
using var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var config = await http.GetFromJsonAsync<ClientConfig>("/_config");

var apiBaseUrl = config?.ApiServiceUrl ?? builder.HostEnvironment.BaseAddress;

// ── HTTP client for ApiService ────────────────────────────────────────────────
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// ── Auth (Logto OIDC PKCE) ────────────────────────────────────────────────────
builder.Services.AddOidcAuthentication(options =>
{
    options.ProviderOptions.Authority = config?.LogtoAuthority ?? "";
    options.ProviderOptions.ClientId = config?.LogtoClientId ?? "";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
});

await builder.Build().RunAsync();

internal sealed record ClientConfig(string ApiServiceUrl, string LogtoAuthority, string LogtoClientId);
```

**Step 2: Build to verify**

```bash
dotnet build src/FourLivingStory/FourLivingStory.slnx
```

Expected: Build succeeded, 0 errors.

**Step 3: Commit**

```bash
git add src/FourLivingStory/FourLivingStory.Web.Client/Program.cs
git commit -m "feat: read Logto authority and client ID from /_config in Web.Client"
```

---

## Task 4: Clean up static appsettings (remove now-redundant Logto values)

The `wwwroot/appsettings*.json` files had placeholder Logto values. Since config now comes from `/_config`, these files no longer need Logto keys (they're ignored but should be cleaned up to avoid confusion).

**Files:**
- Modify: `src/FourLivingStory/FourLivingStory.Web.Client/wwwroot/appsettings.json`
- Modify: `src/FourLivingStory/FourLivingStory.Web.Client/wwwroot/appsettings.Development.json`
- Modify: `src/FourLivingStory/FourLivingStory.ApiService/appsettings.json`
- Modify: `src/FourLivingStory/FourLivingStory.ApiService/appsettings.Development.json`

**Step 1: Update Web.Client appsettings.json**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

**Step 2: Update Web.Client appsettings.Development.json**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

**Step 3: Update ApiService appsettings.json (remove empty Logto section)**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": []
  }
}
```

**Step 4: Update ApiService appsettings.Development.json**

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Cors": {
    "AllowedOrigins": [ "https://localhost:7030", "http://localhost:5017" ]
  }
}
```

**Step 5: Build to verify**

```bash
dotnet build src/FourLivingStory/FourLivingStory.slnx
```

Expected: Build succeeded, 0 errors.

**Step 6: Commit**

```bash
git add src/FourLivingStory/FourLivingStory.Web.Client/wwwroot/appsettings.json
git add src/FourLivingStory/FourLivingStory.Web.Client/wwwroot/appsettings.Development.json
git add src/FourLivingStory/FourLivingStory.ApiService/appsettings.json
git add src/FourLivingStory/FourLivingStory.ApiService/appsettings.Development.json
git commit -m "chore: remove redundant Logto placeholders from static appsettings"
```

---

## Task 5: First-run Logto setup (manual, one-time)

This task is manual — run Aspire and configure Logto through its admin console.

**Step 1: Start Aspire**

```bash
dotnet run --project src/FourLivingStory/FourLivingStory.AppHost
```

Wait for all resources to show healthy in the Aspire dashboard.

**Step 2: Complete Logto onboarding**

Open `http://localhost:3002` (Logto admin console).

- Create an admin account when prompted
- Complete the onboarding wizard

**Step 3: Create a Logto application**

In the Logto admin console:

1. Go to **Applications** → **Create application**
2. Choose **Single Page Application (SPA)**
3. Name it `FourLivingStory Dev`
4. Set redirect URI: `https://localhost:7030/authentication/login-callback`
5. Set post-logout redirect URI: `https://localhost:7030/`
6. Save — copy the **App ID** (this is your `ClientId`)

**Step 4: Create an API resource (for JWT audience)**

1. Go to **API Resources** → **Create API resource**
2. Name: `FourLivingStory API`
3. API Identifier: `http://localhost:7509/` (must match `Logto__Audience` in AppHost.cs)
4. Save

**Step 5: Fill in ClientId**

Edit `src/FourLivingStory/FourLivingStory.AppHost/appsettings.Development.json`:

```json
{
  "Logto": {
    "ClientId": "<paste App ID here>"
  }
}
```

**Step 6: Restart Aspire**

```bash
# Ctrl+C to stop, then:
dotnet run --project src/FourLivingStory/FourLivingStory.AppHost
```

**Step 7: Verify**

Open the Web frontend. Clicking login should redirect to Logto's login page at `http://localhost:3001`. After login, you should be redirected back to the app.

---

## Verification Checklist

- [ ] `dotnet build` passes with 0 errors
- [ ] Aspire dashboard shows `logto-postgres`, `logto`, all green
- [ ] `http://localhost:3002` opens Logto admin console
- [ ] `http://localhost:3001/oidc/.well-known/openid-configuration` returns OIDC discovery JSON
- [ ] Web frontend login redirects to Logto
- [ ] After login, redirect back to app succeeds
- [ ] ApiService `/health` is still healthy
