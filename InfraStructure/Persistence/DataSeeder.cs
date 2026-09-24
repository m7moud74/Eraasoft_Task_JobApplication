using System;
using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;
using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Persistence;

public static class DataSeeder
{
    public const string AdminRoleId = "11111111-1111-1111-1111-111111111111";
    public const string CandidateRoleId = "22222222-2222-2222-2222-222222222222";
    public const string RecruiterRoleId = "33333333-3333-3333-3333-333333333333";
    public const string CompanyRoleId = "44444444-4444-4444-4444-444444444444";

    public const string AdminUserId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    public const string RecruiterUserId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";
    public const string Candidate1UserId = "cccccccc-cccc-cccc-cccc-cccccccccccc";
    public const string Candidate2UserId = "dddddddd-dddd-dddd-dddd-dddddddddddd";

    public static void SeedData(this ModelBuilder modelBuilder)
    {
        // 1. Seed Roles
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = AdminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "admin-role-stamp"
            },
            new IdentityRole
            {
                Id = CandidateRoleId,
                Name = "Candidate",
                NormalizedName = "CANDIDATE",
                ConcurrencyStamp = "candidate-role-stamp"
            },
            new IdentityRole
            {
                Id = RecruiterRoleId,
                Name = "Recruiter",
                NormalizedName = "RECRUITER",
                ConcurrencyStamp = "recruiter-role-stamp"
            },
            new IdentityRole
            {
                Id = CompanyRoleId,
                Name = "Company",
                NormalizedName = "COMPANY",
                ConcurrencyStamp = "company-role-stamp"
            }
        );

        // 2. Seed Companies
        modelBuilder.Entity<Company>().HasData(
            new Company
            {
                Id = 1,
                Name = "Tech Solutions Ltd",
                Description = "Leading software engineering consultancy",
                LogoUrl = "https://example.com/logos/techsolutions.png",
                Status = CompanyStatus.Approved,
                CreatedAt = new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                ApprovedAt = new DateTime(2026, 9, 1, 12, 0, 0, 0, DateTimeKind.Utc),
                ApprovedByUserId = AdminUserId
            },
            new Company
            {
                Id = 2,
                Name = "Innovate Inc",
                Description = "Next-gen AI and cloud startup",
                LogoUrl = "https://example.com/logos/innovate.png",
                Status = CompanyStatus.Pending,
                CreatedAt = new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc),
                ApprovedAt = null,
                ApprovedByUserId = null
            }
        );

        // 3. Seed Recruiters
        modelBuilder.Entity<Recruiter>().HasData(
            new Recruiter
            {
                Id = 1,
                Name = "Tech Solutions Recruiter",
                Email = "recruiter@trackapplication.com",
                CompanyId = 1,
                UserId = RecruiterUserId,
                IsCompanyOwner = true,
                CreatedAt = new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 4. Seed Candidates
        modelBuilder.Entity<Candidate>().HasData(
            new Candidate
            {
                Id = 1,
                Name = "Ahmed Ali",
                Email = "ahmed@example.com",
                CvUrl = "https://example.com/cvs/ahmed.pdf",
                CvPublicId = null
            },
            new Candidate
            {
                Id = 2,
                Name = "Sara Mohamed",
                Email = "sara@example.com",
                CvUrl = "https://example.com/cvs/sara.pdf",
                CvPublicId = null
            }
        );

        // 5. Seed ApplicationUsers
        var adminUser = new ApplicationUser
        {
            Id = AdminUserId,
            UserName = "admin@trackapplication.com",
            NormalizedUserName = "ADMIN@TRACKAPPLICATION.COM",
            Email = "admin@trackapplication.com",
            NormalizedEmail = "ADMIN@TRACKAPPLICATION.COM",
            EmailConfirmed = true,
            SecurityStamp = "admin-sec-stamp",
            ConcurrencyStamp = "admin-con-stamp",
            CandidateId = null,
            CompanyId = null,
            RecruiterId = null,
            PasswordHash = "AQAAAAIAAYagAAAAEP8UZUlR0DWLRbTRXSQVaoukedY3JGlSv7Inqz2M99Z83fpD2lZCKDdQ/hFpf3CA/g=="
        };

        var recruiterUser = new ApplicationUser
        {
            Id = RecruiterUserId,
            UserName = "recruiter@trackapplication.com",
            NormalizedUserName = "RECRUITER@TRACKAPPLICATION.COM",
            Email = "recruiter@trackapplication.com",
            NormalizedEmail = "RECRUITER@TRACKAPPLICATION.COM",
            EmailConfirmed = true,
            SecurityStamp = "recruiter-sec-stamp",
            ConcurrencyStamp = "recruiter-con-stamp",
            CandidateId = null,
            CompanyId = 1,
            RecruiterId = 1,
            PasswordHash = "AQAAAAIAAYagAAAAEGQ+uwT28xXzpK1jkjEdy9/6cQnw5EYi5eSrIvNbkL1bs6ZXFfvVSKIyaUbVtn89CA=="
        };

        var candidate1User = new ApplicationUser
        {
            Id = Candidate1UserId,
            UserName = "ahmed@example.com",
            NormalizedUserName = "AHMED@EXAMPLE.COM",
            Email = "ahmed@example.com",
            NormalizedEmail = "AHMED@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = "candidate1-sec-stamp",
            ConcurrencyStamp = "candidate1-con-stamp",
            CandidateId = 1,
            CompanyId = null,
            RecruiterId = null,
            PasswordHash = "AQAAAAIAAYagAAAAEEL0j8uzjSBk/PwElVXAvhwnqJD4KkEXThsjSqEdgQHEZkgVIJl6KRxV7QoK15t3mQ=="
        };

        var candidate2User = new ApplicationUser
        {
            Id = Candidate2UserId,
            UserName = "sara@example.com",
            NormalizedUserName = "SARA@EXAMPLE.COM",
            Email = "sara@example.com",
            NormalizedEmail = "SARA@EXAMPLE.COM",
            EmailConfirmed = true,
            SecurityStamp = "candidate2-sec-stamp",
            ConcurrencyStamp = "candidate2-con-stamp",
            CandidateId = 2,
            CompanyId = null,
            RecruiterId = null,
            PasswordHash = "AQAAAAIAAYagAAAAELOdWxLfCLqfsNoxSh6YuOea+qy5gCtZWleyxSC/OpP09V3A6c6RA738LX5V4FnGsg=="
        };

        modelBuilder.Entity<ApplicationUser>().HasData(adminUser, recruiterUser, candidate1User, candidate2User);

        // 6. Seed UserRoles
        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string>
            {
                UserId = AdminUserId,
                RoleId = AdminRoleId
            },
            new IdentityUserRole<string>
            {
                UserId = RecruiterUserId,
                RoleId = RecruiterRoleId
            },
            new IdentityUserRole<string>
            {
                UserId = Candidate1UserId,
                RoleId = CandidateRoleId
            },
            new IdentityUserRole<string>
            {
                UserId = Candidate2UserId,
                RoleId = CandidateRoleId
            }
        );

        // 7. Seed Jobs
        modelBuilder.Entity<Job>().HasData(
            new Job
            {
                Id = 1,
                Title = "Senior .NET Developer",
                Description = "Build scalable web APIs and microservices using ASP.NET Core",
                IsActive = true,
                CreatedByUserId = RecruiterUserId,
                CompanyId = 1,
                RecruiterId = 1,
                CreatedAt = new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc)
            },
            new Job
            {
                Id = 2,
                Title = "Frontend Angular Developer",
                Description = "Develop responsive single-page applications",
                IsActive = true,
                CreatedByUserId = RecruiterUserId,
                CompanyId = 1,
                RecruiterId = 1,
                CreatedAt = new DateTime(2026, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 8. Seed JobCandidateApplications
        modelBuilder.Entity<JobCandidateApplication>().HasData(
            new JobCandidateApplication
            {
                Id = 1,
                CandidateId = 1,
                JobId = 1,
                JobApplicationStatus = JobApplicationStatus.Applied,
                AppliedAt = new DateTime(2026, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc),
                StatusUpdatedAt = new DateTime(2026, 9, 6, 0, 0, 0, 0, DateTimeKind.Utc),
                CancelledAt = null
            },
            new JobCandidateApplication
            {
                Id = 2,
                CandidateId = 1,
                JobId = 2,
                JobApplicationStatus = JobApplicationStatus.UnderReview,
                AppliedAt = new DateTime(2026, 9, 2, 0, 0, 0, 0, DateTimeKind.Utc),
                StatusUpdatedAt = new DateTime(2026, 9, 9, 0, 0, 0, 0, DateTimeKind.Utc),
                CancelledAt = null
            },
            new JobCandidateApplication
            {
                Id = 3,
                CandidateId = 2,
                JobId = 1,
                JobApplicationStatus = JobApplicationStatus.InterView,
                AppliedAt = new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc),
                StatusUpdatedAt = new DateTime(2026, 9, 13, 0, 0, 0, 0, DateTimeKind.Utc),
                CancelledAt = null
            },
            new JobCandidateApplication
            {
                Id = 4,
                CandidateId = 2,
                JobId = 2,
                JobApplicationStatus = JobApplicationStatus.Cancelled,
                AppliedAt = new DateTime(2026, 8, 27, 0, 0, 0, 0, DateTimeKind.Utc),
                StatusUpdatedAt = new DateTime(2026, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc),
                CancelledAt = new DateTime(2026, 9, 8, 0, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
