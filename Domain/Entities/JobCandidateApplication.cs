using JobApplication.Domain.Enums;
using JobApplication.Domain.Exceptions;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobApplication.Domain.Entities;

public class JobCandidateApplication
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    [ForeignKey(nameof(CandidateId))]
    public Candidate Candidate { get; set; } = null!;
    public int JobId { get; set; }
    [ForeignKey(nameof(JobId))]
    public Job Job { get; set; } = null!;
    public JobApplicationStatus JobApplicationStatus { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime StatusUpdatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public void Cancel()
    {
        if (JobApplicationStatus == JobApplicationStatus.Cancelled)
        {
            throw new DomainException("Application has already been cancelled.");
        }

        if (JobApplicationStatus != JobApplicationStatus.Applied && JobApplicationStatus != JobApplicationStatus.UnderReview)
        {
            throw new DomainException($"Cannot cancel application with status '{JobApplicationStatus}'. Only Applied or UnderReview applications can be cancelled.");
        }

        JobApplicationStatus = JobApplicationStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        StatusUpdatedAt = DateTime.UtcNow;
    }
}
