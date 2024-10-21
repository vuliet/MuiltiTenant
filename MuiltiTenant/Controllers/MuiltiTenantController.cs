using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuiltiTenant.DatabaseContext;
using MuiltiTenant.Models;

namespace MuiltiTenant.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MuiltiTenantController : ControllerBase
    {
        private readonly ILogger<MuiltiTenantController> _logger;

        private readonly IHttpContextAccessor _contextAccessor;

        private readonly TenantDbContext _dbcontext;

        public MuiltiTenantController(
            ILogger<MuiltiTenantController> logger,
            IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _dbcontext = _contextAccessor.HttpContext?.Items["DbContext"] as TenantDbContext
                ?? throw new Exception("DbContext in HttpContext is null");
        }

        [HttpGet]
        [Route("/test-muilti-tenant")]
        public async Task<List<Token>> GetMuiltiTenant()
        {
            var tokenEntities = await _dbcontext.Tokens.AsNoTracking().ToListAsync();

            return tokenEntities;
        }
    }
}