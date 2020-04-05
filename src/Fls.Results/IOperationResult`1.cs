using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    /// <summary>
    /// Basic interface for implementation of new result types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOperationResult<T>
    {
        /// <summary>
        /// Matches one of the input paremeters with the Value
        /// </summary>
        /// <param name="bindSuccess">Delegate called in case of success</param>
        /// <param name="bindError">Delegate called in case of error</param>
        /// <param name="bindFailure">Delegate called in case of failure</param>
        /// <returns>Execution result one of input delegates</returns>
        IOperationResult<TOut> Match<TOut>(
            Func<T, IOperationResult<TOut>> bindSuccess,
            Func<string, IOperationResult<TOut>> bindError,
            Func<Exception, IOperationResult<TOut>> bindFailure);

        /// <summary>
        /// Async version of Match
        /// </summary>
        /// <param name="bindSuccess">Delegate called in case of success</param>
        /// <param name="bindError">Delegate called in case of error</param>
        /// <param name="bindFailure">Delegate called in case of failure</param>
        /// <returns>Task from execution result one of input delegates</returns>        
        Task<IOperationResult<TOut>> MatchAsync<TOut>(
            Func<T, Task<IOperationResult<TOut>>> bindSuccess,
            Func<string, Task<IOperationResult<TOut>>> bindError,
            Func<Exception, Task<IOperationResult<TOut>>> bindFailure);
    }
}
