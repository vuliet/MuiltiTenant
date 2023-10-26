using Microsoft.EntityFrameworkCore;
using MuiltiTenant.DatabasaeContext;

namespace MuiltiTenant.Seed
{
    public static class MigrationDatabase
    {
        public static void UseMigrationDatabase(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            try
            {
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();

                DataSeeder.Seed(serviceProvider, context);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<WebApplication>();
                logger.LogError(ex, "An error occurred seeding the DB.");
                throw new Exception(ex.Message);
            }
        }
    }
}
