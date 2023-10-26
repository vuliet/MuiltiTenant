using MuiltiTenant.DatabasaeContext;
using MuiltiTenant.Models;

namespace MuiltiTenant.Resolver
{
    public class TenantResolver : ITenantResolver
    {
        private readonly ApplicationDbContext _dbContext;

        public TenantResolver(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Tenant Resolve(string host)
        {
            return _dbContext.Tenants.FirstOrDefault(t => t.Domain == host);
        }
    }
}
