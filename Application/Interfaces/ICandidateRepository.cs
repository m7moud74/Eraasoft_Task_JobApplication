using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces;

public interface ICandidateRepository : IGenericRepository<Candidate>
{
    Task<Candidate?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
