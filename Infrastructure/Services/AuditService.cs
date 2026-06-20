using Application.Interfaces.Identity;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class AuditService : IAuditService
    {
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            DbContextOptions<AppDbContext> dbContextOptions,
            ICurrentUserService currentUserService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditService> logger)
        {
            _dbContextOptions = dbContextOptions;
            _currentUserService = currentUserService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAsync(
            string action, 
            string entityName, 
            int? entityId = null,
            string? oldValues = null,
            string? newValues = null)
        {
            try
            {
                var userId = _currentUserService.UserId;

                var ipAddress = _httpContextAccessor.HttpContext?
                    .Connection
                    .RemoteIpAddress?
                    .ToString();

                var auditLog = new AuditLog
                {
                    UserId = userId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    OldValues = oldValues,
                    NewValues = newValues,
                    IpAddress = ipAddress,
                    CreatedAt = DateTime.UtcNow
                };

                await using var context = new AppDbContext(_dbContextOptions);
                await context.AuditLogs.AddAsync(auditLog);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to write audit action {AuditAction} for entity {EntityName} with id {EntityId}",
                    action,
                    entityName,
                    entityId);
            }
        }
    }
}
