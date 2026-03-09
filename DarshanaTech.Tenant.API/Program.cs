using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using DarshanaTech.Tenant.Infrastructure.Data;
using Microsoft.IdentityModel.Tokens;
using DarshanaTech.Tenant.API.Middlewares;
using DarshanaTech.Tenant.API.Services;
using DarshanaTech.Tenant.Application;
using DarshanaTech.Tenant.Infrastructure;
using Serilog;
using System.Text;

// --- Web API builder setup ---
var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured logging from appsettings
builder.Host.UseSerilog((context, config) =>
{
    config.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

// Add MVC controllers and API explorer
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Swagger with Bearer JWT security scheme
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SaaS School Management API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Register application and infrastructure (DI, DbContext, repositories, services)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// HTTP client for Auth Web API (shared authentication service)
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    var baseUrl = builder.Configuration["AuthApi:BaseUrl"] ?? "http://localhost:5142";
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
});

// JWT Bearer authentication for SuperAdmin endpoints
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!"))
        };
    });

// In-memory cache for tenant lookup (TenantSuspensionMiddleware)
builder.Services.AddMemoryCache();
// IP rate limiting from appsettings IpRateLimiting section
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

// --- Middleware pipeline (order matters) ---
// 1. Catch unhandled exceptions; return JSON error with status
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
// 2. Resolve tenant subdomain from host into HttpContext.Items
app.UseMiddleware<TenantResolutionMiddleware>();
// 3. Block 403 for suspended tenants when subdomain is present
app.UseMiddleware<TenantSuspensionMiddleware>();

app.UseSerilogRequestLogging();
app.UseIpRateLimiting();

if (app.Environment.IsDevelopment())
    app.UseSwagger().UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Run migrations and seed data on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await DbSeed.SeedAsync(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Migration or seed failed. Run 'dotnet ef database update' manually.");
    }
}

app.Run();
