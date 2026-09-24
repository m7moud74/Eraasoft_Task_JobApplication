using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JobApplication.Application.Interfaces;

public interface IHangFrieService
{
    void Enqueue<T>(Expression<Action<T>> methodCall);
    void Enqueue<T>(Expression<Func<T, Task>> methodCall);
    void Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);
    void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
}