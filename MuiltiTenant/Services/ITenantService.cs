using MuiltiTenant.Models;

namespace MuiltiTenant.Services
{
    public interface ITenantService
    {
        Task<IEnumerable<Tenant>> GetAllTenantsAsync();
        Task<Tenant?> GetTenantByIdAsync(int id);
        Task<Tenant?> GetTenantByDomainAsync(string domain);
        Task<Tenant> CreateTenantAsync(Tenant tenant);
        Task<bool> UpdateTenantAsync(Tenant tenant);
        Task<bool> DeleteTenantAsync(int id);
        Task<bool> TestTenantConnectionAsync(string connectionString);
    }
}
