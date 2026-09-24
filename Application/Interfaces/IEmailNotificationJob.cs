using System.Threading.Tasks;
using JobApplication.Domain.Enums;

namespace JobApplication.Application.Interfaces;

public interface IEmailNotificationJob
{
    Task SendApplicationCancelledNotificationAsync(int applicationId);
    Task SendApplicationStatusChangedNotificationAsync(int applicationId, JobApplicationStatus oldStatus, JobApplicationStatus newStatus);
    Task SendCompanyApprovedNotificationAsync(int companyId);
}
