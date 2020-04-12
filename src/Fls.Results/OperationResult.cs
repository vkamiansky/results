using System;
using System.Threading.Tasks;

namespace Fls.Results
{
    public static class OperationResult
    {
        public sealed class SuccessResult<T> : IOperationResult<T>
        {
            public T Value { get; private set; }
            public SuccessResult(T value)
            {
                Value = value;
            }

            public IOperationResult<TOut> Match<TOut>(
                Func<T, IOperationResult<TOut>> bindSuccess,
                Func<string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindSuccess(Value);
            }

            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindSuccess(Value);
            }
        }

        public sealed class ErrorResult<T> : IOperationResult<T>
        {
            public string Message { get; private set; }
            public ErrorResult(string message)
            {
                Message = message;
            }

            public IOperationResult<TOut> Match<TOut>(
                Func<T, IOperationResult<TOut>> bindSuccess,
                Func<string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindError(Message);
            }

            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindError(Message);
            }
        }

        public sealed class FailureResult<T> : IOperationResult<T>
        {
            public Exception Exception { get; private set; }
            public FailureResult(Exception exception)
            {
                Exception = exception;
            }

            public IOperationResult<TOut> Match<TOut>(
                Func<T, IOperationResult<TOut>> bindSuccess,
                Func<string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindFailure(Exception);
            }

            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindFailure(Exception);
            }
        }

        public static IOperationResult<T> Success<T>(T value)
        {
            return new SuccessResult<T>(value);
        }

        public static IOperationResult<T> Error<T>(string message)
        {
            return new ErrorResult<T>(message);
        }

        public static IOperationResult<T> Failure<T>(Exception exception)
        {
            return new FailureResult<T>(exception);
        }

        public static IOperationResult<TOut> Bind<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return source.Match(
                value => bind(value),
                error => Error<TOut>(error),
                exception => Failure<TOut>(exception)
            );
        }

        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await source.MatchAsync(
                async value => await bindAsync(value),
                error => Task.FromResult(Error<TOut>(error)),
                exception => Task.FromResult(Failure<TOut>(exception))
            );
        }

        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await (await source).BindAsync(bindAsync);
        }

        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return (await source).Bind(bind);
        }

        public static IOperationResult<T> ToResult<T>(this T source)
        {
            return Success(source);
        }

        public static IOperationResult<T> Try<T>(Func<T> foo)
        {
            try
            {
                return foo().ToResult();
            }
            catch (Exception ex)
            {
                return Failure<T>(ex);
            }
        }

        public static async Task<IOperationResult<T>> TryAsync<T>(Func<T> foo)
        {
            return await Try(foo);
        }

        public static async Task<IOperationResult<T>> TryAsync<T>(Func<Task<T>> foo)
        {
            try
            {
                return (await foo()).ToResult();
            }
            catch (Exception ex)
            {
                return Failure<T>(ex);
            }
        }
    }
}
