using Microsoft.EntityFrameworkCore;
using MuiltiTenant.DatabaseContext;
using MuiltiTenant.Middleware;
using MuiltiTenant.Resolver;
using MuiltiTenant.Seed;
using MuiltiTenant.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/multitenant-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MultiTenant API", Version = "v1" });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// Database configuration
var numberOfPool = 50;
var serverVersion = new MySqlServerVersion(new Version(8, 0, 29));

builder.Services.AddDbContextPool<ApplicationDbContext>(
    dbContextOptions => dbContextOptions
        .UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion)
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
        .EnableDetailedErrors(builder.Environment.IsDevelopment()),
    numberOfPool);

// Register services
builder.Services.AddScoped<ITenantResolver, TenantResolver>();
builder.Services.AddScoped<ITenantService, TenantService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database")
    .AddCheck<TenantHealthCheck>("tenant-resolution");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" })
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MultiTenant API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");

// Custom middleware
app.UseMiddleware<TenantMiddleware>();

app.UseAuthorization();

// Health checks endpoint
app.MapHealthChecks("/health");

// Initialize database
try
{
    MigrationDatabase.UseMigrationDatabase(app);
    Log.Information("Database migration and seeding completed successfully");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred during database initialization");
    throw;
}

app.MapControllers();

try
{
    Log.Information("Starting MultiTenant API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
