using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MuiltiTenant.DatabaseContext;
using MuiltiTenant.Models;
using MuiltiTenant.Services;

namespace MuiltiTenant.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MuiltiTenantController : ControllerBase
    {
        private readonly ILogger<MuiltiTenantController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ITenantService _tenantService;

        public MuiltiTenantController(
            ILogger<MuiltiTenantController> logger,
            IHttpContextAccessor contextAccessor,
            ITenantService tenantService)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _tenantService = tenantService;
        }

        private TenantDbContext GetTenantDbContext()
        {
            var dbContext = _contextAccessor.HttpContext?.Items["DbContext"] as TenantDbContext;
            if (dbContext == null)
            {
                _logger.LogError("TenantDbContext not found in HttpContext");
                throw new InvalidOperationException("Tenant database context is not available");
            }
            return dbContext;
        }

        private Tenant GetCurrentTenant()
        {
            var tenant = _contextAccessor.HttpContext?.Items["Tenant"] as Tenant;
            if (tenant == null)
            {
                _logger.LogError("Current tenant not found in HttpContext");
                throw new InvalidOperationException("Current tenant information is not available");
            }
            return tenant;
        }

        /// <summary>
        /// Get all tokens for the current tenant
        /// </summary>
        [HttpGet("tokens")]
        public async Task<ActionResult<IEnumerable<Token>>> GetTokens()
        {
            try
            {
                var dbContext = GetTenantDbContext();
                var currentTenant = GetCurrentTenant();

                _logger.LogInformation("Fetching tokens for tenant: {TenantName}", currentTenant.Name);

                var tokens = await dbContext.Tokens
                    .AsNoTracking()
                    .OrderByDescending(t => t.CreationDate)
                    .Take(100) // Limit results for performance
                    .ToListAsync();

                return Ok(tokens);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tokens");
                return StatusCode(500, "An error occurred while fetching tokens");
            }
        }

        /// <summary>
        /// Get a specific token by ID
        /// </summary>
        [HttpGet("tokens/{id}")]
        public async Task<ActionResult<Token>> GetToken(long id)
        {
            try
            {
                var dbContext = GetTenantDbContext();
                var token = await dbContext.Tokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (token == null)
                {
                    return NotFound($"Token with ID {id} not found");
                }

                return Ok(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching token with ID: {TokenId}", id);
                return StatusCode(500, "An error occurred while fetching the token");
            }
        }

        /// <summary>
        /// Create a new token
        /// </summary>
        [HttpPost("tokens")]
        public async Task<ActionResult<Token>> CreateToken([FromBody] CreateTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Hash) || string.IsNullOrWhiteSpace(request.Data))
                {
                    return BadRequest("Hash and Data are required");
                }

                var dbContext = GetTenantDbContext();
                var currentTenant = GetCurrentTenant();

                // Check if token with same hash already exists
                var existingToken = await dbContext.Tokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Hash == request.Hash);

                if (existingToken != null)
                {
                    return Conflict("Token with the same hash already exists");
                }

                var token = new Token
                {
                    Hash = request.Hash,
                    Data = request.Data,
                    CreationDate = DateTime.UtcNow
                };

                dbContext.Tokens.Add(token);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation("Token created for tenant: {TenantName}, TokenId: {TokenId}", 
                    currentTenant.Name, token.Id);

                return CreatedAtAction(nameof(GetToken), new { id = token.Id }, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token");
                return StatusCode(500, "An error occurred while creating the token");
            }
        }

        /// <summary>
        /// Get current tenant information
        /// </summary>
        [HttpGet("tenant-info")]
        public ActionResult<object> GetTenantInfo()
        {
            try
            {
                var currentTenant = GetCurrentTenant();
                
                return Ok(new
                {
                    Id = currentTenant.Id,
                    Name = currentTenant.Name,
                    Domain = currentTenant.Domain,
                    // Don't expose connection string for security
                    ServerInfo = new
                    {
                        RequestHost = HttpContext.Request.Host.Host,
                        RequestTime = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tenant info");
                return StatusCode(500, "An error occurred while fetching tenant information");
            }
        }

        /// <summary>
        /// Health check endpoint for the current tenant
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult> HealthCheck()
        {
            try
            {
                var dbContext = GetTenantDbContext();
                var currentTenant = GetCurrentTenant();

                // Test database connectivity
                var canConnect = await dbContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return StatusCode(503, "Database connection failed");
                }

                // Get basic statistics
                var tokenCount = await dbContext.Tokens.CountAsync();

                return Ok(new
                {
                    Status = "Healthy",
                    Tenant = currentTenant.Name,
                    Domain = currentTenant.Domain,
                    DatabaseStatus = "Connected",
                    TokenCount = tokenCount,
                    CheckTime = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    Status = "Unhealthy",
                    Error = "Health check failed",
                    CheckTime = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Legacy endpoint for backward compatibility
        /// </summary>
        [HttpGet]
        [Route("/test-muilti-tenant")]
        public async Task<ActionResult<List<Token>>> GetMuiltiTenant()
        {
            _logger.LogWarning("Legacy endpoint '/test-muilti-tenant' used. Consider using '/api/MuiltiTenant/tokens' instead.");
            
            var result = await GetTokens();
            if (result.Result is OkObjectResult okResult)
            {
                return Ok(okResult.Value);
            }
            
            return result.Result ?? StatusCode(500);
        }
    }

    public class CreateTokenRequest
    {
        public string Hash { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
