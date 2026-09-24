using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<JobCandidateApplication> JobCandidateApplications { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Recruiter> Recruiters { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.UserId).HasMaxLength(450).IsRequired(false);
            entity.Property(a => a.Action).IsRequired().HasMaxLength(100);
            entity.Property(a => a.EntityName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.EntityId).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Details).HasMaxLength(1000).IsRequired(false);
            entity.Property(a => a.CreatedAt).IsRequired();

            entity.HasIndex(a => new { a.EntityName, a.EntityId });
            entity.HasIndex(a => a.CreatedAt);
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.LogoUrl).HasMaxLength(500);
            entity.Property(c => c.ApprovedByUserId).IsRequired(false);
            entity.Property(c => c.Status).HasConversion<int>();
        });

        modelBuilder.Entity<Recruiter>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.Property(r => r.Email).IsRequired().HasMaxLength(200);
            entity.Property(r => r.UserId).IsRequired(false);

            entity.HasOne(r => r.Company)
                  .WithMany(c => c.Recruiters)
                  .HasForeignKey(r => r.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Candidate>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
            entity.Property(c => c.CvUrl).HasMaxLength(1000);
            entity.Property(c => c.CvPublicId).HasMaxLength(500).IsRequired(false);
        });

        modelBuilder.Entity<JobCandidateApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CancelledAt).IsRequired(false);

            // Unique index preventing duplicate applications for the same candidate and job
            entity.HasIndex(e => new { e.CandidateId, e.JobId }).IsUnique();

            // Indexes for frequent lookups and status filtering
            entity.HasIndex(e => e.JobId);
            entity.HasIndex(e => e.JobApplicationStatus);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedByUserId).IsRequired(false);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Performance indexes
            entity.HasIndex(e => e.CompanyId);
            entity.HasIndex(e => e.CreatedByUserId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(j => j.Company)
                  .WithMany(c => c.Jobs)
                  .HasForeignKey(j => j.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(j => j.Recruiter)
                  .WithMany()
                  .HasForeignKey(j => j.RecruiterId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Candidate)
                  .WithOne()
                  .HasForeignKey<ApplicationUser>(u => u.CandidateId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(u => u.Company)
                  .WithMany()
                  .HasForeignKey(u => u.CompanyId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(u => u.Recruiter)
                  .WithOne()
                  .HasForeignKey<ApplicationUser>(u => u.RecruiterId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.SeedData();
    }
}
