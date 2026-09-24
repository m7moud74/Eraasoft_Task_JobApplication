using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Companies;

public record GetCompanyRecruitersQuery(int? CompanyId = null) : IRequest<IReadOnlyList<RecruiterDto>>;

public class GetCompanyRecruitersQueryHandler : IRequestHandler<GetCompanyRecruitersQuery, IReadOnlyList<RecruiterDto>>
{
    private readonly IRecruiterRepository _recruiterRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCompanyRecruitersQueryHandler(
        IRecruiterRepository recruiterRepository,
        ICompanyRepository companyRepository,
        ICurrentUserService currentUserService)
    {
        _recruiterRepository = recruiterRepository;
        _companyRepository = companyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<RecruiterDto>> Handle(GetCompanyRecruitersQuery request, CancellationToken cancellationToken)
    {
        int targetCompanyId;

        if (_currentUserService.IsAdmin)
        {
            if (request.CompanyId.HasValue && request.CompanyId.Value > 0)
            {
                targetCompanyId = request.CompanyId.Value;
            }
            else if (_currentUserService.CompanyId.HasValue)
            {
                targetCompanyId = _currentUserService.CompanyId.Value;
            }
            else
            {
                throw new BadRequestException("CompanyId must be specified to view recruiters as an administrator.");
            }
        }
        else
        {
            if (!_currentUserService.CompanyId.HasValue)
            {
                throw new ForbiddenAccessException("You must belong to a company to view recruiters.");
            }

            targetCompanyId = _currentUserService.CompanyId.Value;

            if (request.CompanyId.HasValue && request.CompanyId.Value != targetCompanyId)
            {
                throw new ForbiddenAccessException("You are not authorized to view recruiters from another company.");
            }
        }

        var company = await _companyRepository.GetByIdAsync(targetCompanyId, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with ID {targetCompanyId} was not found.");
        }

        var recruiters = await _recruiterRepository.GetByCompanyIdAsync(targetCompanyId, cancellationToken);

        return recruiters.Select(r => new RecruiterDto
        {
            Id = r.Id,
            Name = r.Name,
            Email = r.Email,
            CompanyId = r.CompanyId,
            CompanyName = r.Company?.Name ?? company.Name,
            UserId = r.UserId,
            IsCompanyOwner = r.IsCompanyOwner,
            CreatedAt = r.CreatedAt
        }).ToList();
    }
}
