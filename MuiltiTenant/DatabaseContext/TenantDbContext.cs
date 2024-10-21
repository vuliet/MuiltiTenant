using Microsoft.EntityFrameworkCore;
using MuiltiTenant.Models;

namespace MuiltiTenant.DatabaseContext
{
    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

        public DbSet<Token> Tokens { get; set; }
    }
}
