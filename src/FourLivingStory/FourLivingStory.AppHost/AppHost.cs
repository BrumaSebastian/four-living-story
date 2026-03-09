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
var logtoAdminEndpoint = $"http://localhost:{logtoAdminPort}";
var logtoAuthority = $"{logtoEndpoint}/oidc";

var logto = builder.AddContainer("logto", "svhd/logto", "latest")
    .WithHttpEndpoint(port: logtoOidcPort, targetPort: 3001, name: "oidc")
    .WithHttpEndpoint(port: logtoAdminPort, targetPort: 3002, name: "admin")
    .WithEnvironment("TRUST_PROXY_HEADER", "1")
    .WithEnvironment("ENDPOINT", logtoEndpoint)
    .WithEnvironment("ADMIN_ENDPOINT", logtoAdminEndpoint)
    .WithEnvironment("DB_URL", logtoDb)
    .WaitFor(logtoDb);

// ── Services ──────────────────────────────────────────────────────────────────
var apiService = builder.AddProject<Projects.FourLivingStory_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(db)
    .WaitFor(db)
    .WithEnvironment("Logto__Authority", logtoAuthority)
    .WithEnvironment("Logto__Audience", "http://localhost:7509/");

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
