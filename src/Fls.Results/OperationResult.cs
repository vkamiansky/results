using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace Fls.Results
{
    /// <summary>
    /// Provides a set of static methods for processing objects implementing <c>IOperationResult&lt;T&gt;</c>
    /// </summary>
    public static class OperationResult
    {
        /// <summary>
        /// Produces a new success result.
        /// </summary>
        /// <param name="value">Result value.</param>
        /// <typeparam name="T">The type of expected operation result.</typeparam>
        /// <returns>A new success result instance.</returns>
        public static IOperationResult<T> Success<T>(T value)
        {
            return new SuccessResult<T>(value);
        }

        /// <summary>
        /// Produces a new user error result.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <typeparam name="T">The type of expected operation result.</typeparam>
        /// <returns>A new user error result instance.</returns>
        public static IOperationResult<T> Error<T>(string message)
        {
            return new ErrorResult<T>(message);
        }

        /// <summary>
        /// Produces a new user error result.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <param name="message">Error message.</param>
        /// <typeparam name="T">The type of expected operation result.</typeparam>
        /// <returns>A new user error result instance.</returns>
        public static IOperationResult<T> Error<T>(int code, string message)
        {
            return new ErrorResult<T>(message, code);
        }

        /// <summary>
        /// Produces a new system failure result.
        /// </summary>
        /// <param name="exception">Exception caught.</param>
        /// <typeparam name="T">The type of expected operation result.</typeparam>
        /// <returns>A new system failure result instance.</returns>
        public static IOperationResult<T> Failure<T>(Exception exception)
        {
            return new FailureResult<T>(exception);
        }

        /// <summary>
        /// Converts the given source object into a success operation result with the respective value.
        /// </summary>
        /// <param name="source">The object to be converted.</param>
        /// <typeparam name="T">The type of the source object.</typeparam>
        /// <returns>A new success operation result.</returns>
        public static IOperationResult<T> ToResult<T>(this T source)
        {
            return Success(source);
        }

        /// <summary>
        /// Asynchronously converts the given produced source object into a success operation result with the respective value.
        /// </summary>
        /// <param name="source">The task producing the object to be converted.</param>
        /// <typeparam name="T">The type of the produced source object.</typeparam>
        /// <returns>A task producing a new success operation result.</returns>
        public static async Task<IOperationResult<T>> ToResultAsync<T>(this Task<T> source)
        {
            return Success(await source);
        }

        /// <summary>
        /// Executes the given function in an honest manner. If the function throws an exception it is interpreted as a failure result.async
        /// Otherwise the function result is returned as a success.
        /// </summary>
        /// <param name="getResult">The function to be run in an honest manner.</param>
        /// <typeparam name="T">The type of the result produced by the function.</typeparam>
        /// <returns>A new operation result.</returns>
        public static IOperationResult<T> Try<T>(Func<T> getResult)
        {
            try
            {
                return getResult().ToResult();
            }
            catch (Exception ex)
            {
                return Failure<T>(ex);
            }
        }

        /// <summary>
        /// Executes the given asynchronous function in an honest manner. If the function throws an exception it is interpreted as a failure result.async
        /// Otherwise the function result is returned as a success.
        /// </summary>
        /// <param name="getResultAsync">The asynchronous function to be run in an honest manner.</param>
        /// <typeparam name="T">The type of the result produced by the function.</typeparam>
        /// <returns>A task producing a new operation result.</returns>
        public static async Task<IOperationResult<T>> TryAsync<T>(Func<Task<T>> getResultAsync)
        {
            try
            {
                return (await getResultAsync()).ToResult();
            }
            catch (Exception ex)
            {
                return Failure<T>(ex);
            }
        }

        /// <summary>
        /// Applies the <c>bind</c> function to the value of the given operation result if it matches as a success.
        /// Otherwise the given operation result is passed on with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="bind">The function to be applied to the source result value if it matches as a success.</param>
        /// <typeparam name="TIn">The expected type of the source result value.</typeparam>
        /// <typeparam name="TOut">The expected type of the returned result value.</typeparam>
        /// <returns>A new operation result calculated based on the matching of the source result.</returns>
        public static IOperationResult<TOut> Bind<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return source.Match(
                value => bind(value),
                (code, error) => code.HasValue
                ? Error<TOut>(code.Value, error)
                : Error<TOut>(error),
                exception => Failure<TOut>(exception)
            );
        }

        /// <summary>
        /// Applies the <c>bindAsync</c> async function to the value of the given operation result if it matches as a success.
        /// Otherwise the given operation result is passed on as a task result with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="bindAsync">The async function to be applied to the source result value if it matches as a success.</param>
        /// <typeparam name="TIn">The expected type of the source result value.</typeparam>
        /// <typeparam name="TOut">The expected type of the produced result value.</typeparam>
        /// <returns>A task producing a new operation result calculated based on the matching of the source result.</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await source.MatchAsync(
                async value => await bindAsync(value),
                (code, error) => Task.FromResult(
                    code.HasValue
                    ? Error<TOut>(code.Value, error)
                    : Error<TOut>(error)),
                exception => Task.FromResult(Failure<TOut>(exception))
            );
        }

        /// <summary>
        /// Applies the <c>bindAsync</c> async function to the value of the operation result produced by the given task if it matches as a success.
        /// Otherwise the operation result is passed on as a task result with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="bindAsync">The async function to be applied to the produced source result value if it matches as a success.</param>
        /// <typeparam name="TIn">The expected type of the produced source result value.</typeparam>
        /// <typeparam name="TOut">The expected type of the produced result value.</typeparam>
        /// <returns>A task producing a new operation result calculated based on the matching of the source result.</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await (await source).BindAsync(bindAsync);
        }

        /// <summary>
        /// Applies the <c>bind</c> function to the value of the operation result produced by the given task if it matches as a success.
        /// Otherwise the operation result is passed on as a task result with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="bind">The function to be applied to the produced source result value if it matches as a success.</param>
        /// <typeparam name="TIn">The expected type of the produced source result value.</typeparam>
        /// <typeparam name="TOut">The expected type of the produced result value.</typeparam>
        /// <returns>A task producing a new operation result calculated based on the matching of the source result.</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return (await source).Bind(bind);
        }

        /// <summary>
        /// Applies the <c>bind</c> function to the message of the given operation result if it doesn't match as a success.
        /// Otherwise the given operation result is passed on with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="bind">The function to be applied to the source result error message if it matches as an error or to the exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A new operation result calculated based on the matching of the source result.</returns>
        public static IOperationResult<T> BindError<T>(this IOperationResult<T> source, Func<string, IOperationResult<T>> bind, Func<Exception, string> getFailureMessage)
        {
            return source.BindError((_, error) => bind(error), getFailureMessage);
        }

        /// <summary>
        /// Applies the <c>bind</c> function to the message of the operation result produced by the given task if it doesn't match as a success.
        /// Otherwise the operation result is passed on with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="bind">The function to be applied to the source result error message if it matches as an error or to the exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the produced source result value.</typeparam>
        /// <returns>A task producing a new operation result calculated based on the matching of the source result.</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<string, IOperationResult<T>> bind, Func<Exception, string> getFailureMessage)
        {
            return (await source).BindError(bind, getFailureMessage);
        }

        /// <summary>
        /// Applies the <c>bindAsync</c> asynchronous function to the message of the operation result produced by the given task if it doesn't match as a success.
        /// Otherwise the operation result is passed on with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="bindAsync">The asynchronous function to be applied to the source result error message if it matches as an error or to the exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task producing a new operation result calculated based on the matching of the source result.</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this IOperationResult<T> source, Func<string, Task<IOperationResult<T>>> bindAsync, Func<Exception, string> getFailureMessage)
        {
            return await source.BindErrorAsync((_, error) => bindAsync(error), getFailureMessage);
        }

        /// <summary>
        /// Applies the <c>bindAsync</c> asynchronous function to the message of the operation result produced by the given task if it doesn't match as a success.
        /// Otherwise the operation result is passed on with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="bindAsync">The asynchronous function to be applied to the source result error message if it matches as an error or to the exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task producing a new operation result calculated based on the matching of the source result.</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<string, Task<IOperationResult<T>>> bindAsync, Func<Exception, string> getFailureMessage)
        {
            return await (await source).BindErrorAsync(bindAsync, getFailureMessage);
        }

        /// <summary>
        /// Applies the <c>bind</c> function to the message of the given operation result if it doesn't match as a success.
        /// Otherwise the given operation result is passed on with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="bind">The function to be applied to the source result error message and code if it matches as an error or to the exception message and the standard failure code if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <param name="systemFailureCode">The standard code used for system failures.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A new operation result calculated based on the matching of the source result.</returns>
        public static IOperationResult<T> BindError<T>(this IOperationResult<T> source, Func<int?, string, IOperationResult<T>> bind, Func<Exception, string> getFailureMessage, int? systemFailureCode = null)
        {
            return source.Match(
                _ => source,
                (code, error) => bind(code, error),
                failure => bind(systemFailureCode, getFailureMessage(failure))
            );
        }

        /// <summary>
        /// Applies the <c>bind</c> function to the error message of the given operation result if it doesn't match as a success.
        /// Otherwise the given operation result is passed on with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="bind">The function to be applied to the produced source result error message and code if it matches as an error or to the exception message and the standard failure code if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <param name="systemFailureCode">The standard code used for system failures.</param>
        /// <typeparam name="T">The expected type of the produced source result value.</typeparam>
        /// <returns>A task producing a new operation result calculated based on the matching of the source result.</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<int?, string, IOperationResult<T>> bind, Func<Exception, string> getFailureMessage, int? systemFailureCode = null)
        {
            return (await source).BindError(bind, getFailureMessage, systemFailureCode);
        }

        /// <summary>
        /// Applies the <c>bindAsync</c> asynchronous function to the error message of the given operation result if it doesn't match as a success.
        /// Otherwise the given operation result is passed on with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="bindAsync">The asynchronous function to be applied to the produced source result error message and code if it matches as an error or to the exception message and the standard failure code if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <param name="systemFailureCode">The standard code used for system failures.</param>
        /// <typeparam name="T">The expected type of the produced source result value.</typeparam>
        /// <returns>A task producing a new operation result calculated based on the matching of the source result.</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this IOperationResult<T> source, Func<int?, string, Task<IOperationResult<T>>> bindAsync, Func<Exception, string> getFailureMessage, int? systemFailureCode = null)
        {
            return await source.MatchAsync(
                _ => Task.FromResult(source),
                async (code, error) => await bindAsync(code, error),
                async failure => await bindAsync(systemFailureCode, getFailureMessage(failure))
            );
        }

        /// <summary>
        /// Applies the <c>bindAsync</c> asynchronous function to the error message of the produced source operation result if it doesn't match as a success.
        /// Otherwise the operation result is passed on with the appropriate expected value type.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="bindAsync">The asynchronous function to be applied to the produced source result error message and code if it matches as an error or to the exception message and the standard failure code if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <param name="systemFailureCode">The standard code used for system failures.</param>
        /// <typeparam name="T">The expected type of the produced source result value.</typeparam>
        /// <returns>A task producing a new operation result calculated based on the matching of the source result.</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<int?, string, Task<IOperationResult<T>>> bindAsync, Func<Exception, string> getFailureMessage, int? systemFailureCode = null)
        {
            return await (await source).BindErrorAsync(bindAsync, getFailureMessage, systemFailureCode);
        }

        /// <summary>
        /// Returns the new operation result using the <c>returnResult</c> function if the source operation result doesn't match as a success.
        /// Otherwise the operation result is passed on unchanged.
        /// </summary>
        /// <param name="source">The operation result to be matched.</param>
        /// <param name="returnResult">The function used as an alternative way of getting a success if the source result doesn't match as one.</param>
        /// <typeparam name="T">The type of the expected result value.</typeparam>
        /// <returns>A new operation result calculated based on the matching of the source result.</returns>
        public static IOperationResult<T> IfError<T>(this IOperationResult<T> source, Func<IOperationResult<T>> returnResult)
        {
            return source.Match(
                _ => source,
                (_, __) => returnResult(),
                _ => returnResult()
            );
        }

        /// <summary>
        /// Returns a task producing a new operation result using the <c>returnResultAsync</c> asynchronous function if the source operation result produced by the given task doesn't match as a success.
        /// Otherwise the operation result is passed on as a task result.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="returnResultAsync">The asynchronous function used as an alternative way of getting a success if the source result doesn't match as one.</param>
        /// <typeparam name="T">The type of the expected result value.</typeparam>
        /// <returns>A task producing a new operation result based on the matching of the source result.</returns>
        public static async Task<IOperationResult<T>> IfErrorAsync<T>(this IOperationResult<T> source, Func<Task<IOperationResult<T>>> returnResultAsync)
        {
            return await source.MatchAsync(
                _ => Task.FromResult(source),
                async (_, __) => await returnResultAsync(),
                async _ => await returnResultAsync()
            );
        }

        /// <summary>
        /// Returns the new operation result using the <c>returnResult</c> function if the source operation result produced by the given task doesn't match as a success.
        /// Otherwise the operation result is passed on as a task result.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="returnResult">The function used as an alternative way of getting a success if the source result doesn't match as one.</param>
        /// <typeparam name="T">The type of the expected result value.</typeparam>
        /// <returns>A task producing a new operation result based on the matching of the source result.</returns>
        public static async Task<IOperationResult<T>> IfErrorAsync<T>(this Task<IOperationResult<T>> source, Func<IOperationResult<T>> returnResult)
        {
            return (await source).IfError(returnResult);
        }

        /// <summary>
        /// Returns the new operation result using the <c>returnResultAsync</c> asynchronous function if the source operation result produced by the given task doesn't match as a success.
        /// Otherwise the operation result is passed on as a task result.
        /// </summary>
        /// <param name="source">The task producing the operation result to be processed.</param>
        /// <param name="returnResultAsync">The asynchronous function used as an alternative way of getting a success if the source result doesn't match as one.</param>
        /// <typeparam name="T">The type of the expected result value.</typeparam>
        /// <returns>A task producing a new operation result based on the matching of the source result.</returns>
        public static async Task<IOperationResult<T>> IfErrorAsync<T>(this Task<IOperationResult<T>> source, Func<Task<IOperationResult<T>>> returnResultAsync)
        {
            return await (await source).IfErrorAsync(returnResultAsync);
        }

        /// <summary>
        /// Uses the message of the given operation result if it doesn't match as a success to execute the <c>use</c> side effect.
        /// The source operation result is passed on unchanged.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="use">The side effect to be executed for the source result error message if it matches as an error or for the exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>The source operation result.</returns>
        public static IOperationResult<T> UseError<T>(this IOperationResult<T> source, Action<string> use, Func<Exception, string> getFailureMessage)
        {
            return source.UseError((_, error) => use(error), getFailureMessage);
        }

        /// <summary>
        /// Uses the message of the given operation result if it doesn't match as a success to execute the asynchronous <c>useAsync</c> side effect.
        /// The source operation result is passed on as a task result.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="useAsync">The asynchronous side effect to be executed for the source result error message if it matches as an error or for the exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task returning the source operation result.</returns>
        public static async Task<IOperationResult<T>> UseErrorAsync<T>(this IOperationResult<T> source, Func<string, Task> useAsync, Func<Exception, string> getFailureMessage)
        {
            return await source.UseErrorAsync(async (_, error) => await useAsync(error), getFailureMessage);
        }

        /// <summary>
        /// Uses the message of the operation result returned from the given task if the result doesn't match as a success to execute the asynchronous <c>useAsync</c> side effect.
        /// The operation result returned from the source task is passed on as a task result.
        /// </summary>
        /// <param name="source">A task producing the operation result to be processed.</param>
        /// <param name="useAsync">The asynchronous side effect to be executed for the source result error message if it matches as an error or for the exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task returning the operation result returned from the source task.</returns>
        public static async Task<IOperationResult<T>> UseErrorAsync<T>(this Task<IOperationResult<T>> source, Func<string, Task> useAsync, Func<Exception, string> getFailureMessage)
        {
            return await (await source).UseErrorAsync(useAsync, getFailureMessage);
        }

        /// <summary>
        /// Uses the message of the operation result returned from the given task if the result doesn't match as a success to execute the <c>use</c> side effect.
        /// The operation result returned from the source task is passed on as a task result.
        /// </summary>
        /// <param name="source">A task producing the operation result to be processed.</param>
        /// <param name="use">The side effect to be executed for the source result error message if it matches as an error or for the exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task returning the operation result returned from the source task.</returns>
        public static async Task<IOperationResult<T>> UseErrorAsync<T>(this Task<IOperationResult<T>> source, Action<string> use, Func<Exception, string> getFailureMessage)
        {
            return (await source).UseError(use, getFailureMessage);
        }

        /// <summary>
        /// Uses the message of the given operation result if it doesn't match as a success to execute the <c>use</c> side effect.
        /// The source operation result is passed on unchanged.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="use">The side effect to be executed for the source result error code and message if it matches as an error or for the exception message and system failure code if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <param name="systemFailureCode">The standard code used for system failures.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>The source operation result.</returns>
        public static IOperationResult<T> UseError<T>(this IOperationResult<T> source, Action<int?, string> use, Func<Exception, string> getFailureMessage, int? systemFailureCode = null)
        {
            return source.Match(
                _ => source,
                (code, error) =>
                {
                    use(code, error);
                    return source;
                },
                failure =>
                {
                    use(systemFailureCode, getFailureMessage(failure));
                    return source;
                }
            );
        }

        /// <summary>
        /// Uses the message of the given operation result if it doesn't match as a success to execute the asynchronous <c>useAsync</c> side effect.
        /// The source operation result is passed on as a task result.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="useAsync">The asynchronous side effect to be executed for the source result error code and message if it matches as an error or for the standard failure code and exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <param name="systemFailureCode">The standard code used for system failures.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task returning the source operation result.</returns>
        public static async Task<IOperationResult<T>> UseErrorAsync<T>(this IOperationResult<T> source, Func<int?, string, Task> useAsync, Func<Exception, string> getFailureMessage, int? systemFailureCode = null)
        {
            return await source.MatchAsync(
                _ => Task.FromResult(source),
                async (code, error) =>
                {
                    await useAsync(code, error);
                    return source;
                },
                async failure =>
                {
                    await useAsync(systemFailureCode, getFailureMessage(failure));
                    return source;
                }
            );
        }

        /// <summary>
        /// Uses the message of the operation result returned from the given task if the result doesn't match as a success to execute the asynchronous <c>useAsync</c> side effect.
        /// The operation result returned from the source task is passed on as a task result.
        /// </summary>
        /// <param name="source">A task producing the operation result to be processed.</param>
        /// <param name="useAsync">The asynchronous side effect to be executed for the source result error code and message if it matches as an error or for the standard failure code and exception message if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <param name="systemFailureCode">The standard code used for system failures.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task returning the operation result returned from the source task.</returns>
        public static async Task<IOperationResult<T>> UseErrorAsync<T>(this Task<IOperationResult<T>> source, Func<int?, string, Task> useAsync, Func<Exception, string> getFailureMessage, int? systemFailureCode = null)
        {
            return await (await source).UseErrorAsync(useAsync, getFailureMessage);
        }

        /// <summary>
        /// Uses the message of the operation result returned from the given task if the result doesn't match as a success to execute the <c>use</c> side effect.
        /// The operation result returned from the source task is passed on as a task result.
        /// </summary>
        /// <param name="source">A task producing the operation result to be processed.</param>
        /// <param name="use">The side effect to be executed for the source result error code and message if it matches as an error or for the exception message and system failure code if it matches as a failure.</param>
        /// <param name="getFailureMessage">A function used to extract the exception message from the result if it matches as a failure.</param>
        /// <param name="systemFailureCode">The standard code used for system failures.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task returning the operation result returned from the source task.</returns>
        public static async Task<IOperationResult<T>> UseErrorAsync<T>(this Task<IOperationResult<T>> source, Action<int?, string> use, Func<Exception, string> getFailureMessage, int? systemFailureCode = null)
        {
            return (await source).UseError(use, getFailureMessage);
        }

        /// <summary>
        /// Uses the value of the given operation result if it matches as a success to execute the <c>use</c> side effect.
        /// The source operation result is passed on unchanged.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="use">The side effect to be executed for the source result value if it matches as a success.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>The source operation result.</returns>
        public static IOperationResult<T> Use<T>(this IOperationResult<T> source, Action<T> use)
        {
            return source.Match(
                success =>
                {
                    use(success);
                    return source;
                },
                (_, __) => source,
                _ => source);
        }

        /// <summary>
        /// Uses the value of the given operation result if it matches as a success to execute the asynchronous <c>useAsync</c> side effect.
        /// The source operation result is passed on as a task result.
        /// </summary>
        /// <param name="source">The operation result to be processed.</param>
        /// <param name="useAsync">The asynchronous side effect to be executed for the source result error message if it matches as an error or for the exception message if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task returning the source result.</returns>
        public static async Task<IOperationResult<T>> UseAsync<T>(this IOperationResult<T> source, Func<T, Task> useAsync)
        {
            return await source.MatchAsync(
                async success =>
                {
                    await useAsync(success);
                    return source;
                },
                (_, __) => Task.FromResult(source),
                _ => Task.FromResult(source)
            );
        }

        /// <summary>
        /// Uses the value of the operation result returned from the given task if it matches as a success to execute the asynchronous <c>useAsync</c> side effect.
        /// The operation result returned from the source task is passed on as a task result.
        /// </summary>
        /// <param name="source">A task producing the operation result to be processed.</param>
        /// <param name="useAsync">The asynchronous side effect to be executed for the source result error message if it matches as an error or for the exception message if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task returning the operation result returned from the source task.</returns>
        public static async Task<IOperationResult<T>> UseAsync<T>(this Task<IOperationResult<T>> source, Func<T, Task> useAsync)
        {
            return await (await source).UseAsync(useAsync);
        }

        /// <summary>
        /// Uses the value of the operation result returned from the given task if it matches as a success to execute the <c>use</c> side effect.
        /// The operation result returned from the source task is passed on as a task result.
        /// </summary>
        /// <param name="source">A task producing the operation result to be processed.</param>
        /// <param name="use">The side effect to be executed for the source result error message if it matches as an error or for the exception message if it matches as a failure.</param>
        /// <typeparam name="T">The expected type of the source result value.</typeparam>
        /// <returns>A task returning the operation result returned from the source task.</returns>
        public static async Task<IOperationResult<T>> UseAsync<T>(this Task<IOperationResult<T>> source, Action<T> use)
        {
            return (await source).Use(use);
        }

        /// <summary>
        /// Produces a success result if all functions in the source sequence yield successful results.
        /// Otherwise returns the first unsuccessful result it encounters with the appropriate result type.
        /// </summary>
        /// <param name="source">A sequence of result-producing functions.</param>
        /// <typeparam name="T">The type of expected values of the results produced by the functions in the source sequence.</typeparam>
        /// <returns>A new array-typed operation result calculated based on matching the results produced by the source sequence.</returns>
        public static IOperationResult<T[]> All<T>(this IEnumerable<Func<IOperationResult<T>>> source)
        {
            return source.Aggregate(new List<T>().ToResult(), (a, f) => a.Bind(aList => f().Bind(newRes =>
            {
                aList.Add(newRes);
                return aList.ToResult();
            }))).Bind(x => x.ToArray().ToResult());
        }

        /// <summary>
        /// Produces a success result if any function in the source sequence yields a successful result.
        /// Otherwise returns the last unsuccessful result it encounters.
        /// </summary>
        /// <param name="source">A sequence of result-producing functions.</param>
        /// <typeparam name="T">The type of expected result value.</typeparam>
        /// <returns>A new operation result calculated based on matching the results produced by the source sequence.</returns>
        public static IOperationResult<T> Any<T>(this IEnumerable<Func<IOperationResult<T>>> source)
        {
            return source.Aggregate(Error<T>(""), (a, f) => a.IfError(f));
        }
    }
}
