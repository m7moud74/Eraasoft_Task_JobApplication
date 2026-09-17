using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Repositories;

public class JobCandidateApplicationRepository : GenericRepository<JobCandidateApplication>, IJobCandidateApplicationRepository
{
    public JobCandidateApplicationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<JobCandidateApplication?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Candidate)
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<JobCandidateApplication>> GetByCandidateIdAsync(int candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Candidate)
            .Include(a => a.Job)
            .Where(a => a.CandidateId == candidateId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobCandidateApplication>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(a => a.Candidate)
            .Include(a => a.Job)
            .Where(a => a.JobId == jobId)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int jobId, int candidateId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(a => a.JobId == jobId && a.CandidateId == candidateId, cancellationToken);
    }

    public async Task<bool> AnyByJobIdAsync(int jobId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(a => a.JobId == jobId, cancellationToken);
    }
}
