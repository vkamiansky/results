using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    /// <summary>
    /// Represents a system failure operation result. This type of result is used to show that the system has failed.
    /// </summary>
    public sealed class FailureResult<T> : OperationResult<T>
    {
        /// <summary>
        /// Exception associated with the failure
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Constructs an instance of system failure operation result.
        /// </summary>
        /// <param name="exception">An exception associated with the failure.</param>
        public FailureResult(Exception exception)
        {
            Exception = exception;
        }

        /// <summary>
        /// Matches this instance against result patterns.
        /// </summary>
        /// <param name="matchSuccess">Will be ignored in this type of result.</param>
        /// <param name="matchError">Will be ignored in this type of result.</param>
        /// <param name="matchFailure">Matches this type of result.</param>
        /// <typeparam name="TOut">The type to which the matching binds this instance.</typeparam>
        /// <returns>The result of the matching with <c>matchFailure</c>.</returns>
        public override TOut Match<TOut>(
            Func<T, TOut> matchSuccess,
            Func<int?, string, TOut> matchError,
            Func<Exception, TOut> matchFailure)
        {
            return matchFailure(Exception);
        }

        /// <summary>
        /// Asynchronously matches this instance against result patterns.
        /// </summary>
        /// <param name="matchSuccessAsync">Will be ignored in this type of result.</param>
        /// <param name="matchErrorAsync">Will be ignored in this type of result.</param>
        /// <param name="matchFailureAsync">Matches this type of result.</param>
        /// <typeparam name="TOut">The type of the task to which the matching binds this instance.</typeparam>
        /// <returns>The result of the matching with <c>matchFailureAsync</c>.</returns>
        public override async Task<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> matchSuccessAsync,
            Func<int?, string, Task<TOut>> matchErrorAsync,
            Func<Exception, Task<TOut>> matchFailureAsync)
        {
            return await matchFailureAsync(Exception);
        }
    }
}
