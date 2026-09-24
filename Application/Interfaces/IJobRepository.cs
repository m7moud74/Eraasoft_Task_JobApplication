using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces;

public interface IJobRepository : IGenericRepository<Job>
{
    IQueryable<Job> Get();

    Task<(IReadOnlyList<Job> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool? activeOnly,
        string? search,
        string? sortBy,
        string? sortDirection,
        int? companyId,
        CancellationToken cancellationToken = default);
}
