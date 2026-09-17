using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces;

public interface IJobRepository : IGenericRepository<Job>
{
    IQueryable<Job> Get();
}
