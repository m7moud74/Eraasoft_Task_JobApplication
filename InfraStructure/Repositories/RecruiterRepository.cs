using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Repositories;

public class RecruiterRepository : GenericRepository<Recruiter>, IRecruiterRepository
{
    public RecruiterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Recruiter?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Company)
            .FirstOrDefaultAsync(r => r.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Recruiter>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Company)
            .Where(r => r.CompanyId == companyId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Recruiter?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Company)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public override async Task<Recruiter?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.Company)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
