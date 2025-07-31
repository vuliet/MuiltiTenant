using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MuiltiTenant.DatabaseContext;
using MuiltiTenant.Models;

namespace MuiltiTenant.Resolver
{
    public class TenantResolver : ITenantResolver
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TenantResolver> _logger;
        private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

        public TenantResolver(
            ApplicationDbContext dbContext, 
            IMemoryCache cache,
            ILogger<TenantResolver> logger)
        {
            _dbContext = dbContext;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Tenant?> ResolveAsync(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                _logger.LogWarning("Host is null or empty");
                return null;
            }

            var cacheKey = $"tenant_resolver_{host.ToLowerInvariant()}";
            
            if (_cache.TryGetValue(cacheKey, out Tenant? cachedTenant))
            {
                _logger.LogDebug("Tenant resolved from cache for host: {Host}", host);
                return cachedTenant;
            }

            try
            {
                var tenant = await _dbContext.Tenants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Domain.ToLower() == host.ToLowerInvariant());

                if (tenant != null)
                {
                    _cache.Set(cacheKey, tenant, CacheExpiration);
                    _logger.LogInformation("Tenant resolved and cached for host: {Host}, TenantName: {TenantName}", 
                        host, tenant.Name);
                }
                else
                {
                    _logger.LogWarning("No tenant found for host: {Host}", host);
                }

                return tenant;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving tenant for host: {Host}", host);
                return null;
            }
        }

        public async Task InvalidateCacheAsync(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return;

            var cacheKey = $"tenant_resolver_{host.ToLowerInvariant()}";
            _cache.Remove(cacheKey);
            
            _logger.LogInformation("Cache invalidated for host: {Host}", host);
            await Task.CompletedTask;
        }
    }
}
