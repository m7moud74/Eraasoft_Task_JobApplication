using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Jobs;

public record GetAllJobsQuery(
    int Page = 1,
    int PageSize = 10,
    bool? ActiveOnly = null,
    string? Search = null,
    string? SortBy = "createdAt",
    string? SortDirection = "desc",
    int? CompanyId = null) : IRequest<PagedResult<JobDto>>;

public class GetAllJobsQueryHandler : IRequestHandler<GetAllJobsQuery, PagedResult<JobDto>>
{
    private const int MinPageNumber = 1;
    private const int MinPageSize = 1;
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly IJobRepository _jobRepository;
    private readonly ICacheService _cacheService;

    public GetAllJobsQueryHandler(IJobRepository jobRepository, ICacheService cacheService)
    {
        _jobRepository = jobRepository;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<JobDto>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
    {
        // 1. Validate & normalize pagination parameters
        var page = request.Page < MinPageNumber ? MinPageNumber : request.Page;
        var pageSize = request.PageSize switch
        {
            < MinPageSize => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => request.PageSize
        };

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "createdAt" : request.SortBy.Trim().ToLowerInvariant();
        var sortDirection = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        var search = string.IsNullOrWhiteSpace(request.Search) ? string.Empty : request.Search.Trim().ToLowerInvariant();
        var activeOnlyStr = request.ActiveOnly.HasValue ? request.ActiveOnly.Value.ToString().ToLowerInvariant() : "all";
        var companyIdStr = request.CompanyId.HasValue && request.CompanyId.Value > 0 ? request.CompanyId.Value.ToString() : "all";

        // 2. Build unique cache key including all query parameters
        var cacheKey = $"jobs:page={page}:size={pageSize}:active={activeOnlyStr}:search={search}:sort={sortBy}_{sortDirection}:company={companyIdStr}";

        // 3. Check Redis cache
        var cachedResult = await _cacheService.GetAsync<PagedResult<JobDto>>(cacheKey, cancellationToken);
        if (cachedResult is not null)
        {
            return cachedResult;
        }

        // 4. Cache miss -> Query SQL Server with pagination, filtering & sorting
        var (items, totalCount) = await _jobRepository.GetPagedAsync(
            page,
            pageSize,
            request.ActiveOnly,
            request.Search,
            sortBy,
            sortDirection,
            request.CompanyId,
            cancellationToken);

        var dtos = items.Select(job => new JobDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            IsActive = job.IsActive,
            CreatedByUserId = job.CreatedByUserId,
            CompanyId = job.CompanyId,
            CompanyName = job.Company?.Name,
            RecruiterId = job.RecruiterId,
            RecruiterName = job.Recruiter?.Name,
            CreatedAt = job.CreatedAt
        }).ToList();

        var result = new PagedResult<JobDto>(dtos, page, pageSize, totalCount);

        // 5. Store in Redis cache with short TTL
        await _cacheService.SetAsync(cacheKey, result, CacheDuration, cancellationToken);

        return result;
    }
}
