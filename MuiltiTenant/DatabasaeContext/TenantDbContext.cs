using Microsoft.EntityFrameworkCore;
using MuiltiTenant.Models;

namespace MuiltiTenant.DatabasaeContext
{
    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

        public DbSet<Token> Tokens { get; set; }
    }
}
