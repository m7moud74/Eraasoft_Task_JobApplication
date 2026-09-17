using JobApplication.Application.Interfaces;
using JobApplication.Domain.Entities;
using JobApplication.Infrastructure.Persistence;

namespace JobApplication.Infrastructure.Repositories;

public class JobCandidateApplicationRepository : GenericRepository<JobCandidateApplication>, IJobCandidateApplicationRepository
{
    public JobCandidateApplicationRepository(ApplicationDbContext context) : base(context)
    {
    }
}
