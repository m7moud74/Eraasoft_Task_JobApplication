using JobApplication.Domain.Enums;

namespace JobApplication.Application.DTOs;

public class JobCandidateApplicationDto
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateEmail { get; set; } = string.Empty;
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public JobApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime StatusUpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

public class ApplyJobRequest
{
    public int JobId { get; set; }
}

public class UpdateApplicationStatusRequest
{
    public JobApplicationStatus Status { get; set; }
}
