using MuiltiTenant.Models;

namespace MuiltiTenant.Resolver
{
    public interface ITenantResolver
    {
        Task<Tenant?> ResolveAsync(string host);
        Task InvalidateCacheAsync(string host);
    }
}
