using System;
using System.Linq;
using System.Threading.Tasks;
using JobApplication.Application.Interfaces;
using JobApplication.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace InfraStructure.BackGroundJobs;

public class EmailNotificationJob : IEmailNotificationJob
{
    private readonly IEmailService _emailService;
    private readonly IJobCandidateApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ILogger<EmailNotificationJob> _logger;

    public EmailNotificationJob(
        IEmailService emailService,
        IJobCandidateApplicationRepository applicationRepository,
        ICompanyRepository companyRepository,
        ILogger<EmailNotificationJob> logger)
    {
        _emailService = emailService;
        _applicationRepository = applicationRepository;
        _companyRepository = companyRepository;
        _logger = logger;
    }

    public async Task SendApplicationCancelledNotificationAsync(int applicationId)
    {
        _logger.LogInformation("Processing SendApplicationCancelledNotificationAsync for application {ApplicationId}", applicationId);

        var application = await _applicationRepository.GetDetailsByIdAsync(applicationId);
        if (application is null)
        {
            _logger.LogWarning("Application {ApplicationId} not found while preparing cancellation email.", applicationId);
            return;
        }

        var candidateEmail = application.Candidate?.Email;
        if (string.IsNullOrWhiteSpace(candidateEmail))
        {
            _logger.LogWarning("Candidate email for application {ApplicationId} is empty. Email skipped.", applicationId);
            return;
        }

        var candidateName = application.Candidate?.Name ?? "Candidate";
        var jobTitle = application.Job?.Title ?? "the requested position";
        var cancelledAt = application.CancelledAt ?? DateTime.UtcNow;

        var subject = $"Application Cancelled - {jobTitle}";
        var body = EmailTemplateHelper.GetApplicationCancelledTemplate(candidateName, jobTitle, cancelledAt);

        await _emailService.SendEmailAsync(candidateEmail, subject, body);
        _logger.LogInformation("Cancellation email dispatched for application {ApplicationId} to {Email}", applicationId, candidateEmail);
    }

    public async Task SendApplicationStatusChangedNotificationAsync(int applicationId, JobApplicationStatus oldStatus, JobApplicationStatus newStatus)
    {
        _logger.LogInformation("Processing SendApplicationStatusChangedNotificationAsync for application {ApplicationId} (Status: {OldStatus} -> {NewStatus})", applicationId, oldStatus, newStatus);

        var application = await _applicationRepository.GetDetailsByIdAsync(applicationId);
        if (application is null)
        {
            _logger.LogWarning("Application {ApplicationId} not found while preparing status change email.", applicationId);
            return;
        }

        var candidateEmail = application.Candidate?.Email;
        if (string.IsNullOrWhiteSpace(candidateEmail))
        {
            _logger.LogWarning("Candidate email for application {ApplicationId} is empty. Email skipped.", applicationId);
            return;
        }

        var candidateName = application.Candidate?.Name ?? "Candidate";
        var jobTitle = application.Job?.Title ?? "the applied position";
        var updatedAt = application.StatusUpdatedAt;

        var subject = $"Application Status Update: {newStatus} - {jobTitle}";
        var body = EmailTemplateHelper.GetApplicationStatusChangedTemplate(candidateName, jobTitle, oldStatus.ToString(), newStatus.ToString(), updatedAt);

        await _emailService.SendEmailAsync(candidateEmail, subject, body);
        _logger.LogInformation("Status change email dispatched for application {ApplicationId} to {Email}", applicationId, candidateEmail);
    }

    public async Task SendCompanyApprovedNotificationAsync(int companyId)
    {
        _logger.LogInformation("Processing SendCompanyApprovedNotificationAsync for company {CompanyId}", companyId);

        var company = await _companyRepository.GetWithRecruitersByIdAsync(companyId);
        if (company is null)
        {
            _logger.LogWarning("Company {CompanyId} not found while preparing approval email.", companyId);
            return;
        }

        var ownerRecruiter = company.Recruiters?.FirstOrDefault(r => r.IsCompanyOwner)
                             ?? company.Recruiters?.FirstOrDefault();

        var recipientEmail = ownerRecruiter?.Email;
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            _logger.LogWarning("Recipient email for company {CompanyId} is empty. Approval email skipped.", companyId);
            return;
        }

        var ownerName = ownerRecruiter?.Name ?? company.Name;
        var approvedAt = company.ApprovedAt ?? DateTime.UtcNow;

        var subject = $"Company Approved: Welcome {company.Name} to Track Application System";
        var body = EmailTemplateHelper.GetCompanyApprovedTemplate(company.Name, ownerName, approvedAt);

        await _emailService.SendEmailAsync(recipientEmail, subject, body);
        _logger.LogInformation("Approval email dispatched for company {CompanyId} to {Email}", companyId, recipientEmail);
    }
}
