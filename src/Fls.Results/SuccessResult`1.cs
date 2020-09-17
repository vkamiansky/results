using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    /// <summary>
    /// Represents a success operation result.
    /// </summary>
    public sealed class SuccessResult<T> : OperationResult<T>
    {
        /// <summary>
        /// Operation result value.
        /// </summary>
        /// <value>Gets the returned value.</value>
        public T Value { get; private set; }

        /// <summary>
        /// Constructs an instance of successful operation result.
        /// </summary>
        /// <param name="value">Result value.</param>
        public SuccessResult(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Matches this instance against result patterns.
        /// </summary>
        /// <param name="matchSuccess">Matches this type of result.</param>
        /// <param name="matchError">Will be ignored in this type of result.</param>
        /// <param name="matchFailure">Will be ignored in this type of result.</param>
        /// <typeparam name="TOut">The type to which the matching binds this instance.</typeparam>
        /// <returns>The result of the matching with <c>matchSuccess</c>.</returns>
        public override TOut Match<TOut>(
            Func<T, TOut> matchSuccess,
            Func<int?, string, TOut> matchError,
            Func<Exception, TOut> matchFailure)
        {
            return matchSuccess(Value);
        }

        /// <summary>
        /// Asynchronously matches this instance against result patterns.
        /// </summary>
        /// <param name="matchSuccessAsync">Matches this type of result.</param>
        /// <param name="matchErrorAsync">Will be ignored in this type of result.</param>
        /// <param name="matchFailureAsync">Will be ignored in this type of result.</param>
        /// <typeparam name="TOut">The type of the task to which the matching binds this instance.</typeparam>
        /// <returns>The result of the matching with <c>matchSuccessAsync</c>.</returns>
        public override async Task<TOut> MatchAsync<TOut>(
            Func<T, Task<TOut>> matchSuccessAsync,
            Func<int?, string, Task<TOut>> matchErrorAsync,
            Func<Exception, Task<TOut>> matchFailureAsync)
        {
            return await matchSuccessAsync(Value);
        }
    }
}
