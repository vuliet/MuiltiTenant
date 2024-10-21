using MuiltiTenant.DatabaseContext;
using MuiltiTenant.Models;

namespace MuiltiTenant.Seed
{
    public partial class DataSeeder
    {
        private static void SeedTenants(
            ApplicationDbContext context)
        {

            var tenants = new List<Tenant>()
            {
                new Tenant()
                {
                    Name = "ssprintl",
                    Domain = "localhost",
                    ConnectionString ="server=localhost;Port=3308;user=root;password=password123;database=Sprint_Db;ConnectionTimeout=120;",
                },
                new Tenant()
                {
                    Name = "infisquare",
                    Domain = "localhost2",
                    ConnectionString ="server=localhost;Port=3308;user=root;password=password123;database=infisquaredb;ConnectionTimeout=120;",
                }
            };

            foreach (var tenant in tenants)
            {
                var isExsited = context.Tenants.Any(t => t.Domain == tenant.Domain);
                if (isExsited)
                    continue;

                context.Add(tenant);
                context.SaveChanges();
            }
        }
    }
}
