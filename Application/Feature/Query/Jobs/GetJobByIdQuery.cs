using JobApplication.Application.DTOs;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using MediatR;

namespace JobApplication.Application.Feature.Query.Jobs;

public record GetJobByIdQuery(int Id) : IRequest<JobDto>;

public class GetJobByIdQueryHandler : IRequestHandler<GetJobByIdQuery, JobDto>
{
    private readonly IJobRepository _jobRepository;

    public GetJobByIdQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<JobDto> Handle(GetJobByIdQuery request, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(request.Id, cancellationToken);
        if (job is null)
        {
            throw new NotFoundException($"Job with ID {request.Id} was not found.");
        }

        return new JobDto
        {
            Id = job.Id,
            Title = job.Title,
            Description = job.Description,
            IsActive = job.IsActive,
            CreatedByUserId = job.CreatedByUserId,
            CompanyId = job.CompanyId,
            CompanyName = job.Company?.Name,
            RecruiterId = job.RecruiterId,
            RecruiterName = job.Recruiter?.Name
        };
    }
}
