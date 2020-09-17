using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    /// <summary>
    /// Represents a user error operation result. This type of result is used to show that no system failure has happened, just bad input received by the system.
    /// </summary>
    public sealed class ErrorResult<T> : OperationResult<T>
    {
        /// <summary>
        /// Error message text
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Error code
        /// </summary>
        public int? Code { get; private set; }

        /// <summary>
        /// Constructs an instance of user error operation result.
        /// </summary>
        /// <param name="message">The error message text associated with the result.</param>
        /// <param name="code">The error code associated with the result.</param>
        public ErrorResult(string message, int? code = null)
        {
            Message = message;
            Code = code;
        }

        /// <summary>
        /// Matches this instance against result patterns.
        /// </summary>
        /// <param name="matchSuccess">Will be ignored in this type of result.</param>
        /// <param name="matchError">Matches this type of result.</param>
        /// <param name="matchFailure">Will be ignored in this type of result.</param>
        /// <typeparam name="TOut">The type to which the matching binds this instance.</typeparam>
        /// <returns>The result of the matching with <c>matchError</c>.</returns>
        public override TOut Match<TOut>(
            Func<T, TOut> matchSuccess,
            Func<int?, string, TOut> matchError,
            Func<Exception, TOut> matchFailure)
        {
            return matchError(Code, Message);
        }

        /// <summary>
        /// Asynchronously matches this instance against result patterns.
        /// </summary>
        /// <param name="matchSuccessAsync">Will be ignored in this type of result.</param>
        /// <param name="matchErrorAsync">Matches this type of result.</param>
        /// <param name="matchFailureAsync">Will be ignored in this type of result.</param>
        /// <typeparam name="TOut">The type of the task to which the matching binds this instance.</typeparam>
        /// <returns>The result of the matching with <c>matchErrorAsync</c>.</returns>
        public override async Task<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> matchSuccessAsync,
            Func<int?, string, Task<TOut>> matchErrorAsync,
            Func<Exception, Task<TOut>> matchFailureAsync)
        {
            return await matchErrorAsync(Code, Message);
        }
    }
}
