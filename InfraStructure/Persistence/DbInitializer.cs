using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.EnsureCreatedAsync();

        if (await context.Candidates.AnyAsync())
        {
            return; // DB has been seeded
        }

        var candidate1 = new Candidate
        {
            Id = 1,
            Name = "Ahmed Ali",
            CvUrl = "https://example.com/cvs/ahmed.pdf"
        };

        var candidate2 = new Candidate
        {
            Id = 2,
            Name = "Sara Mohamed",
            CvUrl = "https://example.com/cvs/sara.pdf"
        };

        var job1 = new Job
        {
            Id = 1,
            Title = "Senior .NET Developer",
            Description = "Build scalable web APIs and microservices using ASP.NET Core",
            IsActive = true
        };

        var job2 = new Job
        {
            Id = 2,
            Title = "Frontend Angular Developer",
            Description = "Develop responsive single-page applications",
            IsActive = true
        };

        var now = DateTime.UtcNow;

        var applications = new List<JobCandidateApplication>
        {
            // Application 1: Candidate 1, Job 1, Status: Applied (Eligible for cancellation by Candidate 1)
            new JobCandidateApplication
            {
                Id = 1,
                CandidateId = 1,
                JobId = 1,
                JobApplicationStatus = JobApplicationStatus.Applied,
                AppliedAt = now.AddDays(-5),
                StatusUpdatedAt = now.AddDays(-5),
                CancelledAt = null
            },
            // Application 2: Candidate 1, Job 2, Status: UnderReview (Eligible for cancellation by Candidate 1)
            new JobCandidateApplication
            {
                Id = 2,
                CandidateId = 1,
                JobId = 2,
                JobApplicationStatus = JobApplicationStatus.UnderReview,
                AppliedAt = now.AddDays(-10),
                StatusUpdatedAt = now.AddDays(-3),
                CancelledAt = null
            },
            // Application 3: Candidate 2, Job 1, Status: InterView (Cannot be cancelled - reaches Interview)
            new JobCandidateApplication
            {
                Id = 3,
                CandidateId = 2,
                JobId = 1,
                JobApplicationStatus = JobApplicationStatus.InterView,
                AppliedAt = now.AddDays(-14),
                StatusUpdatedAt = now.AddDays(-2),
                CancelledAt = null
            },
            // Application 4: Candidate 2, Job 2, Status: Cancelled (Already cancelled)
            new JobCandidateApplication
            {
                Id = 4,
                CandidateId = 2,
                JobId = 2,
                JobApplicationStatus = JobApplicationStatus.Cancelled,
                AppliedAt = now.AddDays(-20),
                StatusUpdatedAt = now.AddDays(-7),
                CancelledAt = now.AddDays(-7)
            }
        };

        await context.Candidates.AddRangeAsync(candidate1, candidate2);
        await context.Jobs.AddRangeAsync(job1, job2);
        await context.JobCandidateApplications.AddRangeAsync(applications);

        await context.SaveChangesAsync();
    }
}
