using MuiltiTenant.Models;

namespace MuiltiTenant.Resolver
{
    public interface ITenantResolver
    {
        Tenant Resolve(string host);
    }
}
