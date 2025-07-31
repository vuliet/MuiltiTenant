using Microsoft.Extensions.Diagnostics.HealthChecks;
using MuiltiTenant.Services;

namespace MuiltiTenant.Services
{
    public class TenantHealthCheck : IHealthCheck
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantHealthCheck> _logger;

        public TenantHealthCheck(ITenantService tenantService, ILogger<TenantHealthCheck> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var tenants = await _tenantService.GetAllTenantsAsync();
                var tenantList = tenants.ToList();

                if (!tenantList.Any())
                {
                    return HealthCheckResult.Degraded("No tenants configured");
                }

                var healthyTenants = 0;
                var totalTenants = tenantList.Count;

                foreach (var tenant in tenantList)
                {
                    try
                    {
                        if (await _tenantService.TestTenantConnectionAsync(tenant.ConnectionString))
                        {
                            healthyTenants++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Health check failed for tenant: {TenantName}", tenant.Name);
                    }
                }

                var healthPercentage = (double)healthyTenants / totalTenants;

                var data = new Dictionary<string, object>
                {
                    ["total_tenants"] = totalTenants,
                    ["healthy_tenants"] = healthyTenants,
                    ["health_percentage"] = $"{healthPercentage:P}"
                };

                if (healthPercentage >= 1.0)
                {
                    return HealthCheckResult.Healthy($"All {totalTenants} tenants are healthy", data);
                }
                else if (healthPercentage >= 0.5)
                {
                    return HealthCheckResult.Degraded($"{healthyTenants}/{totalTenants} tenants are healthy", data);
                }
                else
                {
                    return HealthCheckResult.Unhealthy($"Only {healthyTenants}/{totalTenants} tenants are healthy", data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tenant health check");
                return HealthCheckResult.Unhealthy("Failed to perform tenant health check", ex);
            }
        }
    }
}
