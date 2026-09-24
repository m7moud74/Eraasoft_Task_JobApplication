using System.Threading;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string action, string entityName, string entityId, string? details = null, CancellationToken cancellationToken = default);
}
