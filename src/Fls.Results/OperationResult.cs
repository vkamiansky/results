using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    /// <summary>
    /// Provides functional computing chains, 
    /// another words monad providing class
    /// </summary>
    public static class OperationResult
    {
        /// <summary>
        /// Provides succeeded operation value, wrapping the real result value to make it 
        /// honest 
        /// </summary>
        public sealed class SuccessResult<T> : IOperationResult<T>
        {
            /// <summary>
            /// Wrapped result value of any type
            /// </summary>
            /// <value></value>
            public T Value { get; private set; }

            /// <summary>
            /// Constructor with input value of any type to wrap
            /// </summary>
            public SuccessResult(T value)
            {
                Value = value;
            }

            /// <summary>
            /// Matches bindSuccess delegate to the Value.
            /// </summary>
            /// <param name="bindSuccess">Delegate called in case of success</param>
            /// <param name="bindError">Delegate called in case of error</param>
            /// <param name="bindFailure">Delegate called in case of failure</param>
            /// <returns>Execution result of bindSuccess</returns>
            public IOperationResult<TOut> Match<TOut>(
                Func<T, IOperationResult<TOut>> bindSuccess,
                Func<string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindSuccess(Value);
            }

            /// <summary>
            /// Async version of Match
            /// </summary>
            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindSuccess(Value);
            }
        }

        /// <summary>
        /// Provides error execution result. Wraps the real result value, which is supposed to be an error message.
        /// </summary>
        public sealed class ErrorResult<T> : IOperationResult<T>
        {
            /// <summary>
            /// Error message text
            /// </summary>
            public string Message { get; private set; }

            /// <summary>
            /// Constructor with input error message text
            /// </summary>
            public ErrorResult(string message)
            {
                Message = message;
            }

                        /// <summary>
            /// Matches bindError delegate to the Value.
            /// </summary>
            /// <param name="bindSuccess">Delegate called in case of success</param>
            /// <param name="bindError">Delegate called in case of error</param>
            /// <param name="bindFailure">Delegate called in case of failure</param>
            /// <returns>Execution result of bindError</returns>
            public IOperationResult<TOut> Match<TOut>(
                Func<T, IOperationResult<TOut>> bindSuccess,
                Func<string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindError(Message);
            }

            /// <summary>
            /// Async version of Match
            /// </summary>
            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindError(Message);
            }
        }
        /// <summary>
        /// Provides failure execution result. Wraps the real result value, which is supposed to be an exception.
        /// </summary>
        public sealed class FailureResult<T> : IOperationResult<T>
        {
            /// <summary>
            /// Exception value of executed operation
            /// </summary>
            public Exception Exception { get; private set; }

            /// <summary>
            /// Constructed with exception value type, extrated from previously executed operation
            /// </summary>
            public FailureResult(Exception exception)
            {
                Exception = exception;
            }

                        /// <summary>
            /// Matches bindFailure delegate to the Value.
            /// </summary>
            /// <param name="bindSuccess">Delegate called in case of success</param>
            /// <param name="bindError">Delegate called in case of error</param>
            /// <param name="bindFailure">Delegate called in case of failure</param>
            /// <returns>Execution result of bindFailure</returns>
            public IOperationResult<TOut> Match<TOut>(
                Func<T, IOperationResult<TOut>> bindSuccess,
                Func<string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindFailure(Exception);
            }

            /// <summary>
            /// Async version of Match
            /// </summary>
            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindFailure(Exception);
            }
        }

        /// <summary>
        /// Produces success value type for given value
        /// </summary>
        public static IOperationResult<T> Success<T>(T value)
        {
            return new SuccessResult<T>(value);
        }

        /// <summary>
        /// Produces error value type for given error message text
        /// </summary>
        public static IOperationResult<T> Error<T>(string message)
        {
            return new ErrorResult<T>(message);
        }

        /// <summary>
        /// Produces failure value type for given exception
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
        /// Async version of Bind, to support functional pipline with IOperationResult/<T/> as output of previous operaion
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
        /// Async version of Bind, to suport functional pipeline with async Task as output of previous operation
        /// </summary>
        /// <returns>Task from one of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await (await source).BindAsync(bindAsync);
        }

        /// <summary>
        /// Async version of Bind, to suport functional pipeline with async Task as output of previous operation
        /// </summary>
        /// <returns>Task from one of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return (await source).Bind(bind);
        }
    }
}
