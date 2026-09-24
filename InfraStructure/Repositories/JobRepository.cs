using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Repositories;

public class JobRepository : GenericRepository<Job>, IJobRepository
{
    public JobRepository(ApplicationDbContext context) : base(context)
    {
    }

    public IQueryable<Job> Get()
    {
        return _dbSet.AsQueryable();
    }

    public override async Task<Job?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(j => j.Company)
            .Include(j => j.Recruiter)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyList<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(j => j.Company)
            .Include(j => j.Recruiter)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Job> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        bool? activeOnly,
        string? search,
        string? sortBy,
        string? sortDirection,
        int? companyId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(j => j.Company)
            .Include(j => j.Recruiter)
            .AsQueryable();

        if (activeOnly.HasValue)
        {
            query = query.Where(j => j.IsActive == activeOnly.Value);
        }

        if (companyId.HasValue && companyId.Value > 0)
        {
            query = query.Where(j => j.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();
            query = query.Where(j => j.Title.Contains(trimmedSearch));
        }

        var isAscending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = isAscending
            ? query.OrderBy(j => j.CreatedAt)
            : query.OrderByDescending(j => j.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
