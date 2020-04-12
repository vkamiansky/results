using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    // Implement this interface if you need new result type
    public interface IOperationResult<T>
    {
        // Matches one of the input paremeters with the ValueExecution and returns the result one of input delegates
        IOperationResult<TOut> Match<TOut>(
            Func<T, IOperationResult<TOut>> bindSuccess,
            Func<int?, string, IOperationResult<TOut>> bindError,
            Func<Exception, IOperationResult<TOut>> bindFailure);
    
        // Asynchronously matches one of the input paremeters with the ValueExecution and returns task from the result one of input delegates
        Task<IOperationResult<TOut>> MatchAsync<TOut>(
            Func<T, Task<IOperationResult<TOut>>> bindSuccess,
            Func<int?, string, Task<IOperationResult<TOut>>> bindError,
            Func<Exception, Task<IOperationResult<TOut>>> bindFailure);
    }
}
