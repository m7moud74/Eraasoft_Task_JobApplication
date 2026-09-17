using JobApplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobApplication.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<JobCandidateApplication> JobCandidateApplications { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobCandidateApplication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CancelledAt).IsRequired(false);
        });
    }
}
