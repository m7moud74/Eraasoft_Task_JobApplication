using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Companies;

public record GetCompanyByIdQuery(int Id) : IRequest<CompanyDto>;

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCompanyByIdQueryHandler(ICompanyRepository companyRepository, ICurrentUserService currentUserService)
    {
        _companyRepository = companyRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetWithRecruitersByIdAsync(request.Id, cancellationToken);
        if (company is null)
        {
            throw new NotFoundException($"Company with ID {request.Id} was not found.");
        }

        if (!_currentUserService.IsAdmin && _currentUserService.CompanyId != request.Id)
        {
            throw new ForbiddenAccessException("You are not authorized to view details for this company.");
        }

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
