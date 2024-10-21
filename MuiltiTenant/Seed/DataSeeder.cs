using MuiltiTenant.DatabaseContext;

namespace MuiltiTenant.Seed
{
    public partial class DataSeeder
    {
        public static void Seed(
            ApplicationDbContext context)
        {
            SeedTenants(context);
        }
    }
}
