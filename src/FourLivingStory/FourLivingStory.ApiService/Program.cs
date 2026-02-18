using FourLivingStory.ApiService.Infrastructure.Database;
using FourLivingStory.ApiService.Infrastructure.EventBus;
using FourLivingStory.ApiService.Modules.Character;
using FourLivingStory.ApiService.Modules.Expenses;
using FourLivingStory.ApiService.Modules.Identity;
using FourLivingStory.ApiService.Modules.Inventory;
using FourLivingStory.ApiService.Modules.Notifications;
using FourLivingStory.ApiService.Modules.Rewards;
using FourLivingStory.ApiService.Modules.Scheduler;
using FourLivingStory.ApiService.Modules.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// ── Aspire ────────────────────────────────────────────────────────────────────
builder.AddServiceDefaults();

// ── Database ──────────────────────────────────────────────────────────────────
builder.AddNpgsqlDbContext<AppDbContext>("fourlivingstory");

// ── Auth ──────────────────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Logto:Authority"];
        options.Audience  = builder.Configuration["Logto:Audience"];
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });

builder.Services.AddAuthorization();

// ── CORS ──────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ── Event Bus ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEventBus, InMemoryEventBus>();

// ── Modules ───────────────────────────────────────────────────────────────────
builder.Services
    .AddIdentityModule()
    .AddCharacterModule()
    .AddInventoryModule()
    .AddTasksModule()
    .AddExpensesModule()
    .AddRewardsModule()
    .AddNotificationsModule()
    .AddSchedulerModule();

// ── API docs ──────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

// ─────────────────────────────────────────────────────────────────────────────

var app = builder.Build();

// ── Database init ─────────────────────────────────────────────────────────────
// TODO: replace with MigrateAsync() once EF Core migrations are in place.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// ── Middleware ────────────────────────────────────────────────────────────────
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ── Endpoints ─────────────────────────────────────────────────────────────────
app.MapDefaultEndpoints();

app.MapNotificationsModule()
   .MapCharacterModule()
   .MapInventoryModule()
   .MapTasksModule()
   .MapExpensesModule();

app.Run();
