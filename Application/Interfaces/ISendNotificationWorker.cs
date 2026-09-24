using System;

namespace Application.Interfaces;

public interface ISendNotificationWorker
{
    Task SendntoificationCanditat(int Applicationid,int currentUserId ,CancellationToken cancellationToken);
}
