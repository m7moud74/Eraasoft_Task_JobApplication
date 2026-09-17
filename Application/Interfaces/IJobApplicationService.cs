using JobApplication.Application.DTOs;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Interfaces;

public interface IJobApplicationService
{
    Task<JobCandidateApplicationDto> ApplyAsync(int jobId, int candidateId, CancellationToken cancellationToken = default);
    Task<JobCandidateApplicationDto> GetByIdAsync(int applicationId, int currentCandidateId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobCandidateApplicationDto>> GetMyApplicationsAsync(int candidateId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JobCandidateApplicationDto>> GetJobApplicationsAsync(int jobId, CancellationToken cancellationToken = default);
    Task<JobCandidateApplicationDto> UpdateStatusAsync(int applicationId, JobApplicationStatus status, CancellationToken cancellationToken = default);
    Task CancelAsync(int applicationId, int currentUserId, CancellationToken cancellationToken = default);
}
