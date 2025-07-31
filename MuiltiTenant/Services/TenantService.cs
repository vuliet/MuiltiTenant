using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MuiltiTenant.DatabaseContext;
using MuiltiTenant.Models;
using MuiltiTenant.Resolver;

namespace MuiltiTenant.Services
{
    public class TenantService : ITenantService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ITenantResolver _tenantResolver;
        private readonly ILogger<TenantService> _logger;

        public TenantService(
            ApplicationDbContext context,
            IMemoryCache cache,
            ITenantResolver tenantResolver,
            ILogger<TenantService> logger)
        {
            _context = context;
            _cache = cache;
            _tenantResolver = tenantResolver;
            _logger = logger;
        }

        public async Task<IEnumerable<Tenant>> GetAllTenantsAsync()
        {
            return await _context.Tenants.AsNoTracking().ToListAsync();
        }

        public async Task<Tenant?> GetTenantByIdAsync(int id)
        {
            return await _context.Tenants.FindAsync(id);
        }

        public async Task<Tenant?> GetTenantByDomainAsync(string domain)
        {
            return await _tenantResolver.ResolveAsync(domain);
        }

        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            // Validate connection string before creating tenant
            if (!await TestTenantConnectionAsync(tenant.ConnectionString))
            {
                throw new InvalidOperationException("Invalid connection string provided");
            }

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // Invalidate cache
            await _tenantResolver.InvalidateCacheAsync(tenant.Domain);

            _logger.LogInformation("Tenant created: {TenantName} with domain: {Domain}", 
                tenant.Name, tenant.Domain);

            return tenant;
        }

        public async Task<bool> UpdateTenantAsync(Tenant tenant)
        {
            try
            {
                var existingTenant = await _context.Tenants.FindAsync(tenant.Id);
                if (existingTenant == null)
                    return false;

                // Test new connection string if it changed
                if (existingTenant.ConnectionString != tenant.ConnectionString)
                {
                    if (!await TestTenantConnectionAsync(tenant.ConnectionString))
                    {
                        throw new InvalidOperationException("Invalid connection string provided");
                    }
                }

                var oldDomain = existingTenant.Domain;

                existingTenant.Name = tenant.Name;
                existingTenant.Domain = tenant.Domain;
                existingTenant.ConnectionString = tenant.ConnectionString;

                await _context.SaveChangesAsync();

                // Invalidate cache for both old and new domains
                await _tenantResolver.InvalidateCacheAsync(oldDomain);
                await _tenantResolver.InvalidateCacheAsync(tenant.Domain);

                _logger.LogInformation("Tenant updated: {TenantName}", tenant.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tenant: {TenantId}", tenant.Id);
                return false;
            }
        }

        public async Task<bool> DeleteTenantAsync(int id)
        {
            try
            {
                var tenant = await _context.Tenants.FindAsync(id);
                if (tenant == null)
                    return false;

                var domain = tenant.Domain;
                _context.Tenants.Remove(tenant);
                await _context.SaveChangesAsync();

                // Invalidate cache
                await _tenantResolver.InvalidateCacheAsync(domain);

                _logger.LogInformation("Tenant deleted: {TenantName}", tenant.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tenant: {TenantId}", id);
                return false;
            }
        }

        public async Task<bool> TestTenantConnectionAsync(string connectionString)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
                optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(9, 0, 1)));

                using var context = new TenantDbContext(optionsBuilder.Options);
                return await context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Connection test failed for connection string");
                return false;
            }
        }
    }
}
