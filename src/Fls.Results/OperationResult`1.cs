using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    /// <summary>
    /// Base class for operation result types.
    /// </summary>
    /// <typeparam name="T">The type of success result.</typeparam>
    public abstract class OperationResult<T> : IOperationResult<T>
    {
        /// <summary>
        /// Matches this instance against result patterns. 
        /// </summary>
        /// <param name="matchSuccess">Will be used if this instance represents a successful result.</param>
        /// <param name="matchError">Will be used if this instance represents a user error result.</param>
        /// <param name="matchFailure">Will be used if this instance represents a system failure result.</param>
        /// <typeparam name="TOut">The type to which the matching binds this instance.</typeparam>
        /// <returns>The result of the matching.</returns>
        public abstract TOut Match<TOut>(
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
        public abstract Task<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> matchSuccessAsync,
            Func<int?, string, Task<TOut>> matchErrorAsync,
            Func<Exception, Task<TOut>> matchFailureAsync);

        /// <summary>
        /// Implicitly converts a value into a SuccessResult comprising it. 
        /// </summary>
        /// <param name="value">The value implicitly convertible to a SuccessResult.</param>
        public static implicit operator OperationResult<T>(T value)
        {
            return new SuccessResult<T>(value);
        }

        /// <summary>
        /// Implicitly converts an error data set into an ErrorResult comprising it. 
        /// </summary>
        /// <param name="error">The error data set implicitly convertible to an ErrorResult.</param>
        public static implicit operator OperationResult<T>(Error error)
        {
            return new ErrorResult<T>(error.Message, error.Code);
        }

        /// <summary>
        /// Implicitly converts an exception into an FailureResult comprising it. 
        /// </summary>
        /// <param name="exception">The exception implicitly convertible to a FailureResult.</param>
        public static implicit operator OperationResult<T>(Exception exception)
        {
            return new FailureResult<T>(exception);
        }
    }
}
