using JobApplication.Domain.Entities;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Interfaces;

public interface ICompanyRepository : IGenericRepository<Company>
{
    Task<Company?> GetWithRecruitersByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> GetPendingCompaniesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> GetByStatusAsync(CompanyStatus status, CancellationToken cancellationToken = default);
}
