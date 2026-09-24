using JobApplication.Application.DTOs;

namespace JobApplication.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<CompanyDto> RegisterCompanyAsync(RegisterCompanyRequest request, CancellationToken cancellationToken = default);
    Task<RecruiterDto> AddRecruiterAsync(AddRecruiterRequest request, int companyId, CancellationToken cancellationToken = default);
}
