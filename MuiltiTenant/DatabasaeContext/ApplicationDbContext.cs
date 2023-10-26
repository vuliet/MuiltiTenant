using Microsoft.EntityFrameworkCore;
using MuiltiTenant.Models;

namespace MuiltiTenant.DatabasaeContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Role> Roles { get; set; }
    }
}
