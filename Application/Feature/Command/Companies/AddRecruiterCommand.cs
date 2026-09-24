using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Feature.Command.Companies;

public record AddRecruiterCommand(AddRecruiterRequest Request) : IRequest<RecruiterDto>;

public class AddRecruiterCommandHandler : IRequestHandler<AddRecruiterCommand, RecruiterDto>
{
    private readonly IAuthService _authService;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public AddRecruiterCommandHandler(
        IAuthService authService,
        ICompanyRepository companyRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _authService = authService;
        _companyRepository = companyRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
    }

    public async Task<RecruiterDto> Handle(AddRecruiterCommand request, CancellationToken cancellationToken)
    {
        int targetCompanyId;

        if (_currentUserService.IsAdmin)
        {
            if (!request.Request.CompanyId.HasValue || request.Request.CompanyId.Value <= 0)
            {
                throw new BadRequestException("CompanyId must be specified when adding a recruiter as an administrator.");
            }
            targetCompanyId = request.Request.CompanyId.Value;
        }
        else
        {
            if (!_currentUserService.CompanyId.HasValue)
            {
                throw new ForbiddenAccessException("You must belong to a company to add recruiters.");
            }

            targetCompanyId = _currentUserService.CompanyId.Value;

            if (request.Request.CompanyId.HasValue && request.Request.CompanyId.Value != targetCompanyId)
            {
                throw new ForbiddenAccessException("You cannot add recruiters to another company.");
            }
        }

        var company = await _companyRepository.GetByIdAsync(targetCompanyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with ID {targetCompanyId} was not found.");
        }

        if (company.Status != CompanyStatus.Approved)
        {
            throw new BadRequestException($"Cannot add recruiters to a company with status '{company.Status}'. The company must be approved first.");
        }

        var recruiter = await _authService.AddRecruiterAsync(request.Request, targetCompanyId, cancellationToken);

        // Record audit log
        await _auditService.LogAsync("Recruiter created", "Recruiter", recruiter.Id.ToString(), $"Recruiter '{recruiter.Name}' created for Company {targetCompanyId}.", cancellationToken);

        return recruiter;
    }
}
