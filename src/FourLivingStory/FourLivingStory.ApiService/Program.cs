using System.Reflection;
using FourLivingStory.Application;
using FourLivingStory.Domain;
using FourLivingStory.Infrastructure;
using FourLivingStory.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddNpgsqlDbContext<AppDbContext>("fourlivingstory");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Logto:Authority"];
        options.Audience  = builder.Configuration["Logto:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

Assembly[] moduleAssemblies =
[
    typeof(DomainAssemblyMarker).Assembly,
    typeof(ApplicationAssemblyMarker).Assembly,
    typeof(InfrastructureAssemblyMarker).Assembly,
    Assembly.GetExecutingAssembly(),
];

builder.Services.AddModules(builder.Configuration, moduleAssemblies);
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Host.UseWolverine(opts =>
{
    // Enlist Wolverine in EF Core transactions so published messages are
    // written to the outbox atomically with DbContext.SaveChangesAsync().
    opts.UseEntityFrameworkCoreTransactions();

    // Store outbox/inbox envelopes in the same PostgreSQL database.
    opts.PersistMessagesWithPostgresql(
        builder.Configuration.GetConnectionString("fourlivingstory")!,
        schemaName: "wolverine");

    // Scan Application and Infrastructure assemblies for message handlers.
    opts.Discovery.IncludeAssembly(typeof(ApplicationAssemblyMarker).Assembly);
    opts.Discovery.IncludeAssembly(typeof(InfrastructureAssemblyMarker).Assembly);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync(); // TODO: replace with MigrateAsync()
}

app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapModules(moduleAssemblies);

app.Run();
