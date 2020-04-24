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
                Func<int?, string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindSuccess(Value);
            }

            /// <summary>
            /// Asynchronously matches bindSuccess delegate to the Value.
            /// </summary>
            /// <param name="bindSuccess">Delegate called in case of success</param>
            /// <param name="bindError">Delegate called in case of error</param>
            /// <param name="bindFailure">Delegate called in case of failure</param>
            /// <returns>Execution result of bindSuccess</returns>
            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<int?, string, Task<IOperationResult<TOut>>> bindError,
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
            /// Error code of operaion
            /// </summary>
            public int? Code { get; private set; }

            /// <summary>
            /// Constructor with input error message text and optional error code. Providing error code can have a positive effect 
            /// on performance
            /// </summary>
            public ErrorResult(string message, int? code = null)
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
                Func<int?, string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindError(Code, Message);
            }

            /// <summary>
            /// Asynchronously matches bindError delegate to the Value.
            /// </summary>
            /// <param name="bindSuccess">Delegate called in case of success</param>
            /// <param name="bindError">Delegate called in case of error</param>
            /// <param name="bindFailure">Delegate called in case of failure</param>
            /// <returns>Execution result of bindError</returns>
            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<int?, string, Task<IOperationResult<TOut>>> bindError,
                Func<Exception, Task<IOperationResult<TOut>>> bindFailure)
            {
                return await bindError(Code, Message);
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
                Func<int?, string, IOperationResult<TOut>> bindError,
                Func<Exception, IOperationResult<TOut>> bindFailure)
            {
                return bindFailure(Exception);
            }

            /// <summary>
            /// Asynchronously matches bindFailure delegate to the Value.
            /// </summary>
            /// <param name="bindSuccess">Delegate called in case of success</param>
            /// <param name="bindError">Delegate called in case of error</param>
            /// <param name="bindFailure">Delegate called in case of failure</param>
            /// <returns>Execution result of bindFailure</returns>
            public async Task<IOperationResult<TOut>> MatchAsync<TOut>(
                Func<T, Task<IOperationResult<TOut>>> bindSuccess,
                Func<int?, string, Task<IOperationResult<TOut>>> bindError,
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
        /// Binds the result of previous operation to suitable value. If the success result passes as input, the bind function is called 
        /// and it's result passes on. If the error/failure one, output put result would be ErrorResult/FailureResult type.
        /// </summary>
        ///  <param name="source"></param>
        /// <param name="bind">Delegate executed in case of success of previous operation</param>
        /// <returns>One of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static IOperationResult<TOut> Bind<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return source.Match(
                value => bind(value),
                (_, error) => Error<TOut>(error),
                exception => Failure<TOut>(exception)
            );
        }

        /// <summary>
        /// Bindes exactly errors and failures. In case of error of failure calls bind, other ways passes previous result forward
        /// </summary>
        /// /// <remarks>
        /// If source matches as a success it is passed on, if not the error message pair is bound with bind. If source 
        /// matches as a failure, the bind function is called with an error message extracted from the attached Exception using 
        /// getMessage.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="bind">Delegate to bind error of failure</param>
        /// <param name="getMessage">Delegetate to find out error message for given exception</param>
        /// <returns>Binned error/failure or result of previous operation if is was succeeded</returns>
        public static IOperationResult<T> BindError<T>(this IOperationResult<T> source, Func<string, IOperationResult<T>> bind, Func<Exception, string> getMessage)
        {
            return source.BindError((_, error) => bind(error), getMessage);
        }

        /// <summary>
        /// Bindes exactly errors and failures. In case of error of failure calls bind, other ways passes previous result forward. 
        /// If the error result is provided with error code it also might be used in binding
        /// </summary>
        /// <remarks>
        /// If source matches as a success it is passed on, if not the error message/code pair is bound with bind. If source 
        /// matches as a failure, the bind function is called with an error message extracted from the attached Exception using 
        /// getErrorMessage and the one-for-all critical failure code exceptionCode.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="bind">Delegate to bind error of failure</param>
        /// <param name="getErrorMessage">Delegetate to find out error message for given exception</param>
        /// <param name="exceptionCode">The common code to bind for exception</param>
        /// <returns></returns>
        public static IOperationResult<T> BindError<T>(this IOperationResult<T> source, Func<int?, string, IOperationResult<T>> bind, Func<Exception, string> getErrorMessage, int? exceptionCode = null)
        {
            return source.Match(
                _ => source,
                (code, error) => bind(code, error),
                failure => bind(exceptionCode, getErrorMessage(failure))
            );
                }

        /// <summary>
        /// Asynchronously binds the result of previous operation to suitable value. If the success result passes as input, the bind function is called 
        /// and it's result passes on. If the error/failure one, output put result would be task from ErrorResult/FailureResult type.
        /// It's async version of Bind, to support functional pipline with IOperationResult/<T/> as output of previous operaion
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bind">Delegate executed in case of success of previous operation</param>
        /// <returns>Task from one of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this IOperationResult<TIn> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await source.MatchAsync(
                async value => await bindAsync(value),
                (_, error) => Task.FromResult(Error<TOut>(error)),
                exception => Task.FromResult(Failure<TOut>(exception))
            );
        }

        /// <summary>
        /// Asynchronously binds the result of previous operation to suitable value. If the task from success result passes as input, the bind function is called 
        /// and it's result passes on. If the error/failure one, output put result would be task from ErrorResult/FailureResult type.
        /// It's async version of Bind, to support functional pipline with IOperationResult/<T/> as output of previous operaion
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bind">Delegate executed in case of success of previous operation</param>
        /// <returns>Task from one of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, Task<IOperationResult<TOut>>> bindAsync)
        {
            return await (await source).BindAsync(bindAsync);
        }

       /// <summary>
        /// Asynchronously binds the result of previous operation to suitable value. If the task success result passes as input, the bind function is called 
        /// and it's result passes on. If the error/failure one, output put result would be task from ErrorResult/FailureResult type.
        /// It's async version of Bind, to support functional pipline with IOperationResult/<T/> as output of previous operaion
        /// </summary>
        /// <param name="source"></param>
        /// <param name="bind">Delegate executed in case of success of previous operation</param>
        /// <returns>Task from one of value type SuccessResult, ErrorResult or FailureResult</returns>
        public static async Task<IOperationResult<TOut>> BindAsync<TIn, TOut>(this Task<IOperationResult<TIn>> source, Func<TIn, IOperationResult<TOut>> bind)
        {
            return (await source).Bind(bind);
        }

        /// <summary>
        /// Asynchronously bindes exactly errors and failures. In case of error of failure calls bind, other ways passes previous result forward. 
        /// If the error result is provided with error code it also might be used in binding
        /// </summary>
        /// <remarks>
        /// If source matches as a success it is passed on, if not the error message pair is bound with bind. If source 
        /// matches as a failure, the bind function is called with an error message extracted from the attached Exception using 
        /// getErrorMessage.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="bind">Delegate to bind error of failure</param>
        /// <param name="getErrorMessage">Delegetate to find out error message for given exception</param>
        /// <param name="exceptionCode">The common code to bind for exception</param>
        /// <returns>Task from binned error/failure or result of previous operation if is was succeeded</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<string, IOperationResult<T>> bind, Func<Exception, string> getMessage)
        {
            return (await source).BindError(bind, getMessage);
        }

        /// <summary>
        /// Asynchronously bindes exactly errors and failures. In case of error of failure calls bind, other ways passes previous result forward. 
        /// If the error result is provided with error code it also might be used in binding
        /// </summary>
        /// <remarks>
        /// If source matches as a success it is passed on, if not the error message pair is bound with bind. If source 
        /// matches as a failure, the bind function is called with an error message extracted from the attached Exception using 
        /// getErrorMessage.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="bind">Delegate to bind error of failure</param>
        /// <param name="getErrorMessage">Delegetate to find out error message for given exception</param>
        /// <param name="exceptionCode">The common code to bind for exception</param>
        /// <returns>Task from binned error/failure or result of previous operation if is was succeeded</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this IOperationResult<T> source, Func<string, Task<IOperationResult<T>>> bind, Func<Exception, string> getMessage)
        {
            return await source.BindErrorAsync((_, error) => bind(error), getMessage);
        }

        /// <summary>
        /// Asynchronously bindes exactly errors and failures. In case of error of failure calls bind, other ways passes previous result forward. 
        /// If the error result is provided with error code it also might be used in binding
        /// </summary>
        /// <remarks>
        /// If source matches as a success it is passed on, if not the error message pair is bound with bind. If source 
        /// matches as a failure, the bind function is called with an error message extracted from the attached Exception using 
        /// getErrorMessage.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="bind">Delegate to bind error of failure</param>
        /// <param name="getErrorMessage">Delegetate to find out error message for given exception</param>
        /// <param name="exceptionCode">The common code to bind for exception</param>
        /// <returns>Task from binned error/failure or result of previous operation if is was succeeded</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<string, Task<IOperationResult<T>>> bind, Func<Exception, string> getMessage)
        {
            return await (await source).BindErrorAsync(bind, getMessage);
        }

        /// <summary>
        /// Asynchronously bindes exactly errors and failures. In case of error of failure calls bind, other ways passes previous result forward. 
        /// If the error result is provided with error code it also might be used in binding
        /// </summary>
        /// <remarks>
        /// If source matches as a success it is passed on, if not the error message/code pair is bound with bind. If source 
        /// matches as a failure, the bind function is called with an error message extracted from the attached Exception using 
        /// getErrorMessage and the one-for-all critical failure code exceptionCode.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="bind">Delegate to bind error of failure</param>
        /// <param name="getErrorMessage">Delegetate to find out error message for given exception</param>
        /// <param name="exceptionCode">The common code to bind for exception</param>
        /// <returns>Task from binned error/failure or result of previous operation if is was succeeded</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<int?, string, IOperationResult<T>> bind, Func<Exception, string> getMessage, int? exceptionCode = null)
        {
            return (await source).BindError(bind, getMessage, exceptionCode);
        }

        /// <summary>
        /// Asynchronously bindes exactly errors and failures. In case of error of failure calls bind, other ways passes previous result forward. 
        /// If the error result is provided with error code it also might be used in binding
        /// </summary>
        /// <remarks>
        /// If source matches as a success it is passed on, if not the error message/code pair is bound with bind. If source 
        /// matches as a failure, the bind function is called with an error message extracted from the attached Exception using 
        /// getErrorMessage and the one-for-all critical failure code exceptionCode.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="bind">Delegate to bind error of failure</param>
        /// <param name="getErrorMessage">Delegetate to find out error message for given exception</param>
        /// <param name="exceptionCode">The common code to bind for exception</param>
        /// <returns>Task from binned error/failure or result of previous operation if is was succeeded</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this IOperationResult<T> source, Func<int?, string, Task<IOperationResult<T>>> bind, Func<Exception, string> getMessage, int? exceptionCode = null)
        {
            return await source.MatchAsync(
                _ => Task.FromResult(source),
                async (code, error) => await bind(code, error),
                async failure => await bind(exceptionCode, getMessage(failure))
            );
        }

        /// <summary>
        /// Asynchronously bindes exactly errors and failures. In case of error of failure calls bind, other ways passes previous result forward. 
        /// If the error result is provided with error code it also might be used in binding
        /// </summary>
        /// <remarks>
        /// If source matches as a success it is passed on, if not the error message/code pair is bound with bind. If source 
        /// matches as a failure, the bind function is called with an error message extracted from the attached Exception using 
        /// getErrorMessage and the one-for-all critical failure code exceptionCode.
        /// </remarks>
        /// <param name="source"></param>
        /// <param name="bind">Delegate to bind error of failure</param>
        /// <param name="getErrorMessage">Delegetate to find out error message for given exception</param>
        /// <param name="exceptionCode">The common code to bind for exception</param>
        /// <returns>Task from binned error/failure or result of previous operation if is was succeeded</returns>
        public static async Task<IOperationResult<T>> BindErrorAsync<T>(this Task<IOperationResult<T>> source, Func<int?, string, Task<IOperationResult<T>>> bind, Func<Exception, string> getMessage, int? exceptionCode = null)
        {
            return await (await source).BindErrorAsync(bind, getMessage, exceptionCode);
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
