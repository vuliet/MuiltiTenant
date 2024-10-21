using Microsoft.EntityFrameworkCore;
using MuiltiTenant.DatabaseContext;
using MuiltiTenant.Middleware;
using MuiltiTenant.Resolver;
using MuiltiTenant.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();

var numberOfPool = 50;

var serverVersion = new MySqlServerVersion(new Version(8, 0, 29));
builder.Services.AddDbContextPool<ApplicationDbContext>(
    dbContextOptions => dbContextOptions.UseMySql(
        builder.Configuration["DbConnectionString"],
        serverVersion),
    numberOfPool);

builder.Services.AddScoped<ITenantResolver, TenantResolver>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<TenantMiddleware>();

MigrationDatabase.UseMigrationDatabase(app);

app.MapControllers();

app.Run();
