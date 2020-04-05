using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    /// <summary>
    /// Class providing functional computing chains, 
    /// another words monad providing class
    /// </summary>
    public static class OperationResult
    {
        /// <summary>
        /// OperationResult member class implementing IOperationResult interface
        /// providing succeeded operation value
        /// </summary>
        public sealed class SuccessResult<T> : IOperationResult<T>
        {
            /// <summary>
            /// Result value of any type
            /// </summary>
            /// <value></value>
            public T Value { get; private set; }

            /// <summary>
            /// Constructor with input parametr of any type
            /// </summary>
            public SuccessResult(T value)
            {
                Value = value;
            }

            /// <inheritdoc/>
            public IOperationResult<TOut> Match<TOut>(
                Func<T, IOperationResult<TOut>> bindSuccess,
                Func<string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindSuccess(Value);
            }

            /// <inheritdoc/>
            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindSuccess(Value);
            }
        }

        /// <summary>
        /// OperationResult member class implementing IOperationResult interface
        /// providing error execution result
        /// </summary>
        public sealed class ErrorResult<T> : IOperationResult<T>
        {
            /// <summary>
            /// Error message text
            /// </summary>
            public string Message { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="message">Error message text</param>
            public ErrorResult(string message)
            {
                Message = message;
            }

            /// <inheritdoc/>
            public IOperationResult<TOut> Match<TOut>(
                Func<T, IOperationResult<TOut>> bindSuccess,
                Func<string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindError(Message);
            }

            /// <inheritdoc/>
            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindError(Message);
            }
        }
        /// <summary>
        /// OperationResult member class implementing IOperationResult interface
        /// providing error execution result
        /// </summary>
        public sealed class FailureResult<T> : IOperationResult<T>
        {
            /// <summary>
            /// Exception value
            /// </summary>
            public Exception Exception { get; private set; }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="exception">Exception value</param>
            public FailureResult(Exception exception)
            {
                Exception = exception;
            }

            /// <inheritdoc/>
            public IOperationResult<TOut> Match<TOut>(
                Func<T, IOperationResult<TOut>> bindSuccess,
                Func<string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindFailure(Exception);
            }

            /// <inheritdoc/>
            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindFailure(Exception);
            }
        }

        /// <summary>
        /// Member function to produce success value type
        /// </summary>
        public static IOperationResult<T> Success<T>(T value)
        {
            return new SuccessResult<T>(value);
        }

        /// <summary>
        /// Member function to produce error value type
        /// </summary>
        public static IOperationResult<T> Error<T>(string message)
        {
            return new ErrorResult<T>(message);
        }

        /// <summary>
        /// Member function to produce failure value type
        /// </summary>
        public static IOperationResult<T> Failure<T>(Exception exception)
        {
            return new FailureResult<T>(exception);
        }

        /// <summary>
        /// Binds the result of previous operation to suitable value
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bind">Delegate executed in case of success of previous operation</param>
        /// <returns>One of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static IOperationResult<TOut> Bind<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return source.Match(
                value => bind(value),
                error => Error<TOut>(error),
                exception => Failure<TOut>(exception)
            );
        }

        /// <summary>
        /// Async version of Bind
        /// </summary>
        /// <returns>Task from one of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await source.MatchAsync(
                async value => await bindAsync(value),
                error => Task.FromResult(Error<TOut>(error)),
                exception => Task.FromResult(Failure<TOut>(exception))
            );
        }

        /// <summary>
        /// Async version of Bind
        /// </summary>
        /// <returns>Task from one of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await (await source).BindAsync(bindAsync);
        }

        /// <summary>
        /// Async version of Bind
        /// </summary>
        /// <returns>Task from one of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return (await source).Bind(bind);
        }
    }
}
