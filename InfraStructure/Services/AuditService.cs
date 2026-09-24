using System;
using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace JobApplication.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AuditService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task LogAsync(string action, string entityName, string entityId, string? details = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = _currentUserService.UserId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await _context.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("AuditLog recorded: Action='{Action}', Entity='{EntityName}', Id='{EntityId}', User='{UserId}'",
                action, entityName, entityId, auditLog.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record AuditLog for action '{Action}' on entity '{EntityName}' (Id: {EntityId}).",
                action, entityName, entityId);
            // We do not fail the parent business transaction if audit writing encounters an error
        }
    }
}
