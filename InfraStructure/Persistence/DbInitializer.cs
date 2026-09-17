using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobApplication.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure roles exist
        string[] roles = ["Admin", "Candidate"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Ensure admin user exists
        const string adminEmail = "admin@trackapplication.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123456");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed candidates and jobs if not present
        if (await context.Candidates.AnyAsync())
        {
            return;
        }

        var candidate1 = new Candidate
        {
            Name = "Ahmed Ali",
            Email = "ahmed@example.com",
            CvUrl = "https://example.com/cvs/ahmed.pdf"
        };

        var candidate2 = new Candidate
        {
            Name = "Sara Mohamed",
            Email = "sara@example.com",
            CvUrl = "https://example.com/cvs/sara.pdf"
        };

        await context.Candidates.AddRangeAsync(candidate1, candidate2);
        await context.SaveChangesAsync();

        // Create identity users for candidate1 and candidate2
        var user1 = new ApplicationUser
        {
            UserName = candidate1.Email,
            Email = candidate1.Email,
            CandidateId = candidate1.Id,
            EmailConfirmed = true
        };
        if ((await userManager.CreateAsync(user1, "Candidate@123456")).Succeeded)
        {
            await userManager.AddToRoleAsync(user1, "Candidate");
        }

        var user2 = new ApplicationUser
        {
            UserName = candidate2.Email,
            Email = candidate2.Email,
            CandidateId = candidate2.Id,
            EmailConfirmed = true
        };
        if ((await userManager.CreateAsync(user2, "Candidate@123456")).Succeeded)
        {
            await userManager.AddToRoleAsync(user2, "Candidate");
        }

        var job1 = new Job
        {
            Title = "Senior .NET Developer",
            Description = "Build scalable web APIs and microservices using ASP.NET Core",
            IsActive = true
        };

        var job2 = new Job
        {
            Title = "Frontend Angular Developer",
            Description = "Develop responsive single-page applications",
            IsActive = true
        };

        await context.Jobs.AddRangeAsync(job1, job2);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;

        var applications = new List<JobCandidateApplication>
        {
            new JobCandidateApplication
            {
                CandidateId = candidate1.Id,
                JobId = job1.Id,
                JobApplicationStatus = JobApplicationStatus.Applied,
                AppliedAt = now.AddDays(-5),
                StatusUpdatedAt = now.AddDays(-5),
                CancelledAt = null
            },
            new JobCandidateApplication
            {
                CandidateId = candidate1.Id,
                JobId = job2.Id,
                JobApplicationStatus = JobApplicationStatus.UnderReview,
                AppliedAt = now.AddDays(-10),
                StatusUpdatedAt = now.AddDays(-3),
                CancelledAt = null
            },
            new JobCandidateApplication
            {
                CandidateId = candidate2.Id,
                JobId = job1.Id,
                JobApplicationStatus = JobApplicationStatus.InterView,
                AppliedAt = now.AddDays(-14),
                StatusUpdatedAt = now.AddDays(-2),
                CancelledAt = null
            },
            new JobCandidateApplication
            {
                CandidateId = candidate2.Id,
                JobId = job2.Id,
                JobApplicationStatus = JobApplicationStatus.Cancelled,
                AppliedAt = now.AddDays(-20),
                StatusUpdatedAt = now.AddDays(-7),
                CancelledAt = now.AddDays(-7)
            }
        };

        await context.JobCandidateApplications.AddRangeAsync(applications);
        await context.SaveChangesAsync();
    }
}
