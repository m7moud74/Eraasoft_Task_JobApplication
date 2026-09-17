namespace JobApplication.Application.Interfaces;

public interface IJobApplicationService
{
    Task CancelAsync(int applicationId, int currentUserId, CancellationToken cancellationToken = default);
}
