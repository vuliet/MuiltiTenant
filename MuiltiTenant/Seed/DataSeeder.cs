using MuiltiTenant.DatabasaeContext;

namespace MuiltiTenant.Seed
{
    public partial class DataSeeder
    {
        public static void Seed(
            IServiceProvider serviceProvider,
            ApplicationDbContext context)
        {
            SeedTenants(context);
        }
    }
}
