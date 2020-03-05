using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    public interface IOperationResult<T>
    {
        IOperationResult<TOut> Match<TOut>(
            Func<T, IOperationResult<TOut>> bindSuccess,
            Func<string, IOperationResult<TOut>> bindError,
            Func<Exception, IOperationResult<TOut>> bindFailure);

        Task<IOperationResult<TOut>> MatchAsync<TOut>(
            Func<T, Task<IOperationResult<TOut>>> bindSuccess,
            Func<string, Task<IOperationResult<TOut>>> bindError,
            Func<Exception, Task<IOperationResult<TOut>>> bindFailure);
    }
}
