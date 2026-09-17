using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces;

public interface IJobCandidateApplicationRepository : IGenericRepository<JobCandidateApplication>
{
    Task<JobCandidateApplication?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobCandidateApplication>> GetByCandidateIdAsync(int candidateId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobCandidateApplication>> GetByJobIdAsync(int jobId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int jobId, int candidateId, CancellationToken cancellationToken = default);
    Task<bool> AnyByJobIdAsync(int jobId, CancellationToken cancellationToken = default);
}
