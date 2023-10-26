using Microsoft.EntityFrameworkCore;
using MuiltiTenant.Resolver;
using System.Net;

namespace MuiltiTenant.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
        {
            var tenant = tenantResolver.Resolve(context.Request.Host.Host);

            if (tenant is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            var optionsBuilder = new DbContextOptionsBuilder<TenantDbContext>();
            optionsBuilder.UseMySql(tenant.ConnectionString, new MySqlServerVersion(new Version(8, 0, 29)));

            using var dbContext = new TenantDbContext(optionsBuilder.Options);

            context.Items["DbContext"] = dbContext;

            await _next(context);
        }
    }

}
