﻿using Microsoft.EntityFrameworkCore;
using MuiltiTenant.Models;

namespace MuiltiTenant
{
    public class TenantDbContext : DbContext
    {
        public TenantDbContext(DbContextOptions<TenantDbContext> options) : base(options) { }

        public DbSet<TokenEntity> Tokens { get; set; }
    }
}
