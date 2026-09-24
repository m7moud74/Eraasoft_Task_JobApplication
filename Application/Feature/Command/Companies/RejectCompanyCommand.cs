using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Companies;

public record RejectCompanyCommand(int CompanyId) : IRequest<CompanyDto>;

public class RejectCompanyCommandHandler : IRequestHandler<RejectCompanyCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public RejectCompanyCommandHandler(
        ICompanyRepository companyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _companyRepository = companyRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<CompanyDto> Handle(RejectCompanyCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Only administrators can reject companies.");
        }

        var company = await _companyRepository.GetWithRecruitersByIdAsync(request.CompanyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with ID {request.CompanyId} was not found.");
        }

        company.Reject(_currentUserService.UserId ?? string.Empty);

        _companyRepository.Update(company);
        await _companyRepository.SaveChangesAsync(cancellationToken);

        // Record audit log
        await _auditService.LogAsync("Company rejected", "Company", company.Id.ToString(), $"Company '{company.Name}' rejected.", cancellationToken);

        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Description = company.Description,
            LogoUrl = company.LogoUrl,
            Status = company.Status,
            CreatedAt = company.CreatedAt,
            ApprovedAt = company.ApprovedAt,
            ApprovedByUserId = company.ApprovedByUserId,
            RecruitersCount = company.Recruiters?.Count ?? 0
        };
    }
}
