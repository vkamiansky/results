using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    public interface IOperationResult<T>
    {
        TOut Match<TOut>(
            Func<T, TOut> bindSuccess,
            Func<int?, string, TOut> bindError,
            Func<Exception, TOut> bindFailure);

        Task<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> bindSuccess,
            Func<int?, string, Task<TOut>> bindError,
            Func<Exception, Task<TOut>> bindFailure);
    }
}
