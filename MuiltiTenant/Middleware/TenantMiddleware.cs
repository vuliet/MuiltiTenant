using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MuiltiTenant.DatabaseContext;
using MuiltiTenant.Resolver;
using System.Net;

namespace MuiltiTenant.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;
        private readonly IMemoryCache _cache;
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger, IMemoryCache cache)
        {
            _next = next;
            _logger = logger;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
        {
            try
            {
                var host = context.Request.Host.Host;
                _logger.LogInformation("Processing request for host: {Host}", host);

                var tenant = await GetTenantAsync(tenantResolver, host);

                if (tenant is null)
                {
                    _logger.LogWarning("Tenant not found for host: {Host}", host);
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await context.Response.WriteAsync($"Tenant not found for host: {host}");
                    return;
                }

                var dbContext = await CreateTenantDbContextAsync(tenant.ConnectionString);
                if (dbContext == null)
                {
                    _logger.LogError("Failed to create database context for tenant: {TenantName}", tenant.Name);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return;
                }

                context.Items["DbContext"] = dbContext;
                context.Items["Tenant"] = tenant;

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TenantMiddleware");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("Internal server error occurred");
            }
        }

        private async Task<Models.Tenant?> GetTenantAsync(ITenantResolver tenantResolver, string host)
        {
            var cacheKey = $"tenant_{host}";
            
            if (_cache.TryGetValue(cacheKey, out Models.Tenant? cachedTenant))
            {
                _logger.LogDebug("Tenant found in cache for host: {Host}", host);
                return cachedTenant;
            }

            var tenant = await tenantResolver.ResolveAsync(host);
            if (tenant != null)
            {
                _cache.Set(cacheKey, tenant, CacheExpiration);
                _logger.LogDebug("Tenant cached for host: {Host}", host);
            }

            return tenant;
        }

        private async Task<TenantDbContext?> CreateTenantDbContextAsync(string connectionString)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
                optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(9, 0, 1)));

                var dbContext = new TenantDbContext(optionsBuilder.Options);
                
                // Test connection
                await dbContext.Database.CanConnectAsync();
                
                return dbContext;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create database context with connection string: {ConnectionString}", 
                    MaskConnectionString(connectionString));
                return null;
            }
        }

        private static string MaskConnectionString(string connectionString)
        {
            // Mask sensitive information in connection string for logging
            return System.Text.RegularExpressions.Regex.Replace(
                connectionString, 
                @"password=([^;]+)", 
                "password=***", 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
    }
}
