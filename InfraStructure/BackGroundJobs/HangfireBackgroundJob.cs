using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hangfire;
using JobApplication.Application.Interfaces;

namespace InfraStructure.BackGroundJobs;

public class HangfireBackgroundJob : IHangFrieService
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public HangfireBackgroundJob(IBackgroundJobClient hangfire)
    {
        _backgroundJobClient = hangfire;
    }

    public void Enqueue<T>(Expression<Action<T>> methodCall)
    {
        _backgroundJobClient.Enqueue<T>(methodCall);
    }

    public void Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        _backgroundJobClient.Enqueue<T>(methodCall);
    }

    public void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay)
    {
        _backgroundJobClient.Schedule<T>(methodCall, delay);
    }

    public void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
    {
        _backgroundJobClient.Schedule<T>(methodCall, delay);
    }
}
