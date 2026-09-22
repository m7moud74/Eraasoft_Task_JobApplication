using JobApplication.Application.DTOs;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Jobs;

public record GetAllJobsQuery(bool? ActiveOnly = null) : IRequest<IReadOnlyList<JobDto>>;

public class GetAllJobsQueryHandler : IRequestHandler<GetAllJobsQuery, IReadOnlyList<JobDto>>
{
    private readonly IJobRepository _jobRepository;

    public GetAllJobsQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<IReadOnlyList<JobDto>> Handle(GetAllJobsQuery request, CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.GetAllAsync(cancellationToken);

        if (request.ActiveOnly == true)
        {
            jobs = jobs.Where(j => j.IsActive).ToList();
        }

        return jobs.Select(job => new JobDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            IsActive = job.IsActive,
            CreatedByUserId = job.CreatedByUserId
        }).ToList();
    }
}
