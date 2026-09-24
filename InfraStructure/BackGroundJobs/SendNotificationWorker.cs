using System;
using Application.Interfaces;
using JobApplication.Application.Exceptions;
using JobApplication.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace InfraStructure.BackGroundJobs;

public class SendNotificationWorker : ISendNotificationWorker
{
    private readonly ILogger<SendNotificationWorker> _logger;
     private readonly IJobCandidateApplicationRepository _repository;

    public SendNotificationWorker(ILogger<SendNotificationWorker> logger, IJobCandidateApplicationRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task SendntoificationCanditat(int Applicationid,int currentUserId ,CancellationToken cancellationToken)
    {
         var application = await _repository.GetByIdAsync(Applicationid, cancellationToken);
        if (application is null)
        {
           _logger.LogError($"Job application with ID {Applicationid} was not found.");
           return;
        }

        if (application.CandidateId != currentUserId)
        {
            _logger.LogError("Only the candidate who submitted this application can cancel it.");
        }


        _logger.LogInformation($"SendEmail to {currentUserId} about cancel his application ");
    }
}
