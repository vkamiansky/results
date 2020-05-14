using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    /// <summary>
    /// Defines generalized result matching methods.
    /// </summary>
    /// <typeparam name="T">The type of success result.</typeparam>
    public interface IOperationResult<T>
    {
        /// <summary>
        /// Matches this instance against result patterns. 
        /// </summary>
        /// <param name="matchSuccess">Will be used if this instance represents a successful result.</param>
        /// <param name="matchError">Will be used if this instance represents a user error result.</param>
        /// <param name="matchFailure">Will be used if this instance represents a system failure result.</param>
        /// <typeparam name="TOut">The type to which the matching binds this instance.</typeparam>
        /// <returns>The result of the matching.</returns>
        TOut Match<TOut>(
            Func<T, TOut> matchSuccess,
            Func<int?, string, TOut> matchError,
            Func<Exception, TOut> matchFailure);

        /// <summary>
        /// Asynchronously matches this instance against result patterns. 
        /// </summary>
        /// <param name="matchSuccessAsync">Will be used if this instance represents a successful result.</param>
        /// <param name="matchErrorAsync">Will be used if this instance represents a user error result.</param>
        /// <param name="matchFailureAsync">Will be used if this instance represents a system failure result.</param>
        /// <typeparam name="TOut">The type of the task to which the matching binds this instance.</typeparam>
        /// <returns>A task producing the result of the matching.</returns>
        Task<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> matchSuccessAsync,
            Func<int?, string, Task<TOut>> matchErrorAsync,
            Func<Exception, Task<TOut>> matchFailureAsync);
    }
}
