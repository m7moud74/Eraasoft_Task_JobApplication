using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using MediatR;

namespace JobApplication.Application.Feature.Query.Companies;

public record GetAllCompaniesQuery(CompanyStatus? Status = null) : IRequest<IReadOnlyList<CompanyDto>>;

public class GetAllCompaniesQueryHandler : IRequestHandler<GetAllCompaniesQuery, IReadOnlyList<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAllCompaniesQueryHandler(ICompanyRepository companyRepository, ICurrentUserService currentUserService)
    {
        _companyRepository = companyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<IReadOnlyList<CompanyDto>> Handle(GetAllCompaniesQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAdmin)
        {
            throw new ForbiddenAccessException("Only administrators can view all companies.");
        }

        var companies = request.Status.HasValue
            ? await _companyRepository.GetByStatusAsync(request.Status.Value, cancellationToken)
            : await _companyRepository.GetAllAsync(cancellationToken);

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
