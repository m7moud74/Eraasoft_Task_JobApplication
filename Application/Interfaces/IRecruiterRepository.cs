using JobApplication.Domain.Entities;

namespace JobApplication.Application.Interfaces;

public interface IRecruiterRepository : IGenericRepository<Recruiter>
{
    Task<Recruiter?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Recruiter>> GetByCompanyIdAsync(int companyId, CancellationToken cancellationToken = default);
    Task<Recruiter?> GetDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
}
