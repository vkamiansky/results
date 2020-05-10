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

            public TOut Match<TOut>(
                Func<T, TOut> bindSuccess,
                Func<int?, string, TOut> bindError,
                Func<Exception, TOut> bindFailure)
            {
                return bindSuccess(Value);
            }

            public async Task<TOut> MatchAsync<TOut>(
                Func<T, Task<TOut>> bindSuccess,
                Func<int?, string, Task<TOut>> bindError,
                Func<Exception, Task<TOut>> bindFailure)
            {
                return await bindSuccess(Value);
            }
        }

        public sealed class ErrorResult<T> : IOperationResult<T>
        {
            public string Message { get; private set; }
            public int? Code { get; private set; }
            public ErrorResult(string message, int? code = null)
            {
                Message = message;
            }

            public TOut Match<TOut>(
                Func<T, TOut> bindSuccess,
                Func<int?, string, TOut> bindError,
                Func<Exception, TOut> bindFailure)
            {
                return bindError(Code, Message);
            }

            public async Task<TOut> MatchAsync<TOut>(
                Func<T, Task<TOut>> bindSuccess,
                Func<int?, string, Task<TOut>> bindError,
                Func<Exception, Task<TOut>> bindFailure)
            {
                return await bindError(Code, Message);
            }
        }

        public sealed class FailureResult<T> : IOperationResult<T>
        {
            public Exception Exception { get; private set; }
            public FailureResult(Exception exception)
            {
                Exception = exception;
            }

            public TOut Match<TOut>(
                Func<T, TOut> bindSuccess,
                Func<int?, string, TOut> bindError,
                Func<Exception, TOut> bindFailure)
            {
                return bindFailure(Exception);
            }

            public async Task<TOut> MatchAsync<TOut>(
                Func<T, Task<TOut>> bindSuccess,
                Func<int?, string, Task<TOut>> bindError,
                Func<Exception, Task<TOut>> bindFailure)
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

        public static IOperationResult<T> Error<T>(int code, string message)
        {
            return new ErrorResult<T>(message, code);
        }

        public static IOperationResult<T> Failure<T>(Exception exception)
        {
            return new FailureResult<T>(exception);
        }

        public static IOperationResult<TOut> Bind<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return source.Match(
                value => bind(value),
                (_, error) => Error<TOut>(error),
                exception => Failure<TOut>(exception)
            );
        }

        public static IOperationResult<T> BindError<T>(this IOperationResult<T> source, Func<string, IOperationResult<T>> bind, Func<Exception, string> getMessage)
        {
            return source.BindError((_, error) => bind(error), getMessage);
        }

        public static IOperationResult<T> BindError<T>(this IOperationResult<T> source, Func<int?, string, IOperationResult<T>> bind, Func<Exception, string> getErrorMessage, int? exceptionCode = null)
        {
            return source.Match(
                _ => source,
                (code, error) => bind(code, error),
                failure => bind(exceptionCode, getErrorMessage(failure))
            );
        }

        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await source.MatchAsync(
                async value => await bindAsync(value),
                (_, error) => Task.FromResult(Error<TOut>(error)),
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

        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<string, IOperationResult<T>> bind, Func<Exception, string> getMessage)
        {
            return (await source).BindError(bind, getMessage);
        }

        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this IOperationResult<T> source, Func<string, Task<IOperationResult<T>>> bind, Func<Exception, string> getMessage)
        {
            return await source.BindErrorAsync((_, error) => bind(error), getMessage);
        }

        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<string, Task<IOperationResult<T>>> bind, Func<Exception, string> getMessage)
        {
            return await (await source).BindErrorAsync(bind, getMessage);
        }

        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<int?, string, IOperationResult<T>> bind, Func<Exception, string> getMessage, int? exceptionCode = null)
        {
            return (await source).BindError(bind, getMessage, exceptionCode);
        }

        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this IOperationResult<T> source, Func<int?, string, Task<IOperationResult<T>>> bind, Func<Exception, string> getMessage, int? exceptionCode = null)
        {
            return await source.MatchAsync(
                _ => Task.FromResult(source),
                async (code, error) => await bind(code, error),
                async failure => await bind(exceptionCode, getMessage(failure))
            );
        }

        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<int?, string, Task<IOperationResult<T>>> bind, Func<Exception, string> getMessage, int? exceptionCode = null)
        {
            return await (await source).BindErrorAsync(bind, getMessage, exceptionCode);
        }

        public static IOperationResult<T> ToResult<T>(this T source)
        {
            return Success(source);
        }

        public static async Task<IOperationResult<T>> ToResultAsync<T>(this Task<T> source)
        {
            return Success(await source);
        }

        public static IOperationResult<T> ToErrorResult<T>(this T source, string error)
        {
            return Error<T>(error);
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

        public static async Task<IOperationResult<T>> UseErrorAsync<T>(this IOperationResult<T> source, Func<string, Task> use, Func<Exception, string> getMessage)
        {
            return await source.MatchAsync(
                _ => Task.FromResult(source),
                async (code, error) =>
                {
                    await use(error);
                    return source;
                },
                async failure =>
                {
                    await use(getMessage(failure));
                    return source;
                }
            );
        }

        public static IOperationResult<T> UseError<T>(this IOperationResult<T> source, Action<string> use, Func<Exception, string> getMessage)
        {
            return source.Match(
                _ => source,
                (code, error) =>
                {
                    use(error);
                    return source;
                },
                failure =>
                {
                    use(getMessage(failure));
                    return source;
                }
            );
        }

        public static async Task<IOperationResult<T>> UseErrorAsync<T>(this Task<IOperationResult<T>> source, Func<string, Task> use, Func<Exception, string> getMessage)
        {
            return await (await source).UseErrorAsync(use, getMessage);
        }

        public static async Task<IOperationResult<T>> UseErrorAsync<T>(this Task<IOperationResult<T>> source, Action<string> use, Func<Exception, string> getMessage)
        {
            return (await source).UseError(use, getMessage);
        }

        public static IOperationResult<T> Use<T>(this IOperationResult<T> source, Action<T> sideEffect)
        {
            return source.Match(
                success =>
                {
                    sideEffect(success);
                    return source;
                },
                (_, __) => source,
                _ => source);
        }

        public static async Task<IOperationResult<T>> UseAsync<T>(this IOperationResult<T> source, Func<T, Task> sideEffect)
        {
            return await source.MatchAsync(
                async success =>
                {
                    await sideEffect(success);
                    return source;
                },
                (_, __) => Task.FromResult(source),
                _ => Task.FromResult(source)
            );
        }

        public static async Task<IOperationResult<T>> UseAsync<T>(this Task<IOperationResult<T>> source, Func<T, Task> sideEffect)
        {
            return await (await source).UseAsync(sideEffect);
        }

        public static async Task<IOperationResult<T>> UseAsync<T>(this Task<IOperationResult<T>> source, Action<T> sideEffect)
        {
            return (await source).Use(sideEffect);
        }
    }
}
