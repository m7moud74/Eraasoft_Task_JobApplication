using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Companies;

public record GetPendingCompaniesQuery() : IRequest<IReadOnlyList<CompanyDto>>;

public class GetPendingCompaniesQueryHandler : IRequestHandler<GetPendingCompaniesQuery, IReadOnlyList<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetPendingCompaniesQueryHandler(ICompanyRepository companyRepository, ICurrentUserService currentUserService)
    {
        _companyRepository = companyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CompanyDto>> Handle(GetPendingCompaniesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Only administrators can view pending companies.");
        }

        var companies = await _companyRepository.GetPendingCompaniesAsync(cancellationToken);

        return companies.Select(c => new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            LogoUrl = c.LogoUrl,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            ApprovedAt = c.ApprovedAt,
            ApprovedByUserId = c.ApprovedByUserId,
            RecruitersCount = c.Recruiters?.Count ?? 0
        }).ToList();
    }
}
