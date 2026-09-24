using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Command.Companies;

public record ApproveCompanyCommand(int CompanyId) : IRequest<CompanyDto>;

public class ApproveCompanyCommandHandler : IRequestHandler<ApproveCompanyCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IHangFrieService _hangfireService;
    private readonly IAuditService _auditService;

    public ApproveCompanyCommandHandler(
        ICompanyRepository companyRepository,
        ICurrentUserService currentUserService,
        IHangFrieService hangfireService,
        IAuditService auditService)
    {
        _companyRepository = companyRepository;
        _currentUserService = currentUserService;
        _hangfireService = hangfireService;
        _auditService = auditService;
    }

    public async Task<CompanyDto> Handle(ApproveCompanyCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Only administrators can approve companies.");
        }

        var company = await _companyRepository.GetWithRecruitersByIdAsync(request.CompanyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with ID {request.CompanyId} was not found.");
        }

        company.Approve(_currentUserService.UserId ?? string.Empty);

        _companyRepository.Update(company);
        await _companyRepository.SaveChangesAsync(cancellationToken);

        // Record audit log
        await _auditService.LogAsync("Company approved", "Company", company.Id.ToString(), $"Company '{company.Name}' approved.", cancellationToken);

        // Enqueue background email notification strictly after successful persistence
        _hangfireService.Enqueue<IEmailNotificationJob>(job =>
            job.SendCompanyApprovedNotificationAsync(company.Id));

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
