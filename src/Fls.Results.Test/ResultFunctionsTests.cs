using System;
using Xunit;
using Moq;
using System.Threading.Tasks;

namespace Fls.Results.Test
{
    public class ResultFunctionsTests
    {
        [Fact]
        public void SuccessConstructTest()
        {
            var testValue = 2;
            var returnResult = OperationResult.Success(testValue);

            Assert.IsType<SuccessResult<int>>(returnResult);
            Assert.Equal(testValue, (returnResult as SuccessResult<int>).Value);
        }

        [Fact]
        public void BindTest()
        {
            var expectedResultMock = new Mock<IOperationResult<int>>();
            var expectedResult = expectedResultMock.Object;

            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.Match(
                    It.IsAny<Func<int, IOperationResult<int>>>(),
                    It.IsAny<Func<int?, string, IOperationResult<int>>>(),
                    It.IsAny<Func<Exception, IOperationResult<int>>>()
                )).Returns(expectedResult);

            var source = sourceMock.Object;

            var actualResult = sourceMock.Object.Bind(
                // This function is supposed to be passed as the matchSuccess case
                _ =>
                {
                    return expectedResult;
                }
            );

            var testError = "testError";
            var testException = new InvalidCastException("test");

            // Verify that the right functions have been created by the Bind function and passed to Match
            sourceMock.Verify(x =>
                x.Match(
                    It.Is<Func<int, IOperationResult<int>>>(y => y(default(int)) == expectedResult),
                    It.Is<Func<int?, string, IOperationResult<int>>>(y => (y(null, testError) as ErrorResult<int>).Message == testError),
                    It.Is<Func<Exception, IOperationResult<int>>>(y => (y(testException) as FailureResult<int>).Exception == testException)
                ), Times.Once);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void BindErrorTestWithoutErrorCode()
        {
            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.Match(
                    It.IsAny<Func<int, IOperationResult<int>>>(),
                    It.IsAny<Func<int?, string, IOperationResult<int>>>(),
                    It.IsAny<Func<Exception, IOperationResult<int>>>()
                )).Returns(sourceMock.Object);

            var expectedResultSuccess = sourceMock.Object;
            var expectedResultOther = new Mock<IOperationResult<int>>().Object;

            var actualResult = sourceMock.Object.BindError(
                _ =>
                {
                    return expectedResultOther;
                },

                _ =>
                {
                    return default(string);
                }
            );

            sourceMock.Verify(x =>
                x.Match(
                    It.Is<Func<int, IOperationResult<int>>>(y => y(default(int)) == expectedResultSuccess),
                    It.Is<Func<int?, string, IOperationResult<int>>>(y => y(default(int), default(string)) == expectedResultOther),
                    It.Is<Func<Exception, IOperationResult<int>>>(y => y(default(Exception)) == expectedResultOther)
                ), Times.Once);

            Assert.Equal(expectedResultSuccess, actualResult);
        }

        [Fact]
        public void BindErrorTestWithErrorCode()
        {
            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.Match(
                    It.IsAny<Func<int, IOperationResult<int>>>(),
                    It.IsAny<Func<int?, string, IOperationResult<int>>>(),
                    It.IsAny<Func<Exception, IOperationResult<int>>>()
                )).Returns(sourceMock.Object);

            var expectedResultSuccess = sourceMock.Object;
            var expectedResultOther = new Mock<IOperationResult<int>>().Object;

            var actualResult = sourceMock.Object.BindError(
                (code, str) =>
                {
                    return expectedResultOther;
                },

                _ =>
                {
                    return default(string);
                },

                default(int?)
            );

            sourceMock.Verify(x =>
                x.Match(
                    It.Is<Func<int, IOperationResult<int>>>(y => y(default(int)) == expectedResultSuccess),
                    It.Is<Func<int?, string, IOperationResult<int>>>(y => y(default(int), default(string)) == expectedResultOther),
                    It.Is<Func<Exception, IOperationResult<int>>>(y => y(default(Exception)) == expectedResultOther)
                ), Times.Once);

            Assert.Equal(expectedResultSuccess, actualResult);
        }

        [Fact]
        public async void BindAsyncTestIOperationResulToFuncTaskIOperationResult()
        {
            var expectedResultMock = new Mock<IOperationResult<int>>();
            var expectedResult = expectedResultMock.Object;

            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.MatchAsync(
                    It.IsAny<Func<int, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<int?, string, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<Exception, Task<IOperationResult<int>>>>()
                )).Returns(Task.FromResult(expectedResult));

            var source = sourceMock.Object;

            var actualResult = await sourceMock.Object.BindAsync(
                // This function is supposed to be passed as the matchSuccess case
                _ =>
                {
                    return Task.FromResult(expectedResult);
                }
            );

            var testError = "testError";
            var testException = new InvalidCastException("test");

            // Verify that the right functions have been created by the Bind function and passed to Match
            sourceMock.Verify(x =>
                x.MatchAsync(
                    It.Is<Func<int, Task<IOperationResult<int>>>>(y => y(default(int)).Result == expectedResult),
                    It.Is<Func<int?, string, Task<IOperationResult<int>>>>(y => (y(null, testError).Result as ErrorResult<int>).Message == testError),
                    It.Is<Func<Exception, Task<IOperationResult<int>>>>(y => (y(testException).Result as FailureResult<int>).Exception == testException)
                ), Times.Once);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async void BindAsyncTestTaskIOperationResultToFuncTaskIOperationResult()
        {
            var expectedResultMock = new Mock<IOperationResult<int>>();
            var expectedResult = expectedResultMock.Object;

            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.MatchAsync(
                    It.IsAny<Func<int, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<int?, string, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<Exception, Task<IOperationResult<int>>>>()
                )).Returns(Task.FromResult(expectedResult));

            var source = Task.FromResult(sourceMock.Object);

            var actualResult = await source.BindAsync(
                // This function is supposed to be passed as the matchSuccess case
                _ =>
                {
                    return Task.FromResult(expectedResult);
                }
            );

            var testError = "testError";
            var testException = new InvalidCastException("test");

            // Verify that the right functions have been created by the Bind function and passed to Match
            sourceMock.Verify(x =>
                x.MatchAsync(
                    It.Is<Func<int, Task<IOperationResult<int>>>>(y => y(default(int)).Result == expectedResult),
                    It.Is<Func<int?, string, Task<IOperationResult<int>>>>(y => (y(null, testError).Result as ErrorResult<int>).Message == testError),
                    It.Is<Func<Exception, Task<IOperationResult<int>>>>(y => (y(testException).Result as FailureResult<int>).Exception == testException)
                ), Times.Once);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async void BindAsyncTestTaskIOperationResultToFuncIOperationResult()
        {
            var expectedResultMock = new Mock<IOperationResult<int>>();
            var expectedResult = expectedResultMock.Object;

            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.Match(
                    It.IsAny<Func<int, IOperationResult<int>>>(),
                    It.IsAny<Func<int?, string, IOperationResult<int>>>(),
                    It.IsAny<Func<Exception, IOperationResult<int>>>()
                )).Returns(expectedResult);

            var source = Task.FromResult(sourceMock.Object);

            var actualResult = await source.BindAsync(
                // This function is supposed to be passed as the matchSuccess case
                _ =>
                {
                    return expectedResult;
                }
            );

            var testError = "testError";
            var testException = new InvalidCastException("test");

            // Verify that the right functions have been created by the Bind function and passed to Match
            sourceMock.Verify(x =>
                x.Match(
                    It.Is<Func<int, IOperationResult<int>>>(y => y(default(int)) == expectedResult),
                    It.Is<Func<int?, string, IOperationResult<int>>>(y => (y(null, testError) as ErrorResult<int>).Message == testError),
                    It.Is<Func<Exception, IOperationResult<int>>>(y => (y(testException) as FailureResult<int>).Exception == testException)
                ), Times.Once);

            Assert.Equal(expectedResult, actualResult);
        }


        [Fact]
        public async void BindErrorAsyncTestWithoutErrorCodeTaskIOperationResultFuncIOperationResult()
        {
            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.Match(
                    It.IsAny<Func<int, IOperationResult<int>>>(),
                    It.IsAny<Func<int?, string, IOperationResult<int>>>(),
                    It.IsAny<Func<Exception, IOperationResult<int>>>()
                )).Returns(sourceMock.Object);

            var expectedResultSuccess = sourceMock.Object;
            var expectedResultOther = new Mock<IOperationResult<int>>().Object;

            var source = Task.FromResult(sourceMock.Object);
            var actualResult = await source.BindErrorAsync(
                _ =>
                {
                    return expectedResultOther;
                },

                _ =>
                {
                    return default(string);
                }
            );

            sourceMock.Verify(x =>
                x.Match(
                    It.Is<Func<int, IOperationResult<int>>>(y => y(default(int)) == expectedResultSuccess),
                    It.Is<Func<int?, string, IOperationResult<int>>>(y => y(default(int), default(string)) == expectedResultOther),
                    It.Is<Func<Exception, IOperationResult<int>>>(y => y(default(Exception)) == expectedResultOther)
                ), Times.Once);

            Assert.Equal(expectedResultSuccess, actualResult);
        }

        [Fact]
        public async void BindErrorAsyncTestWithoutErrorCodeIOperationResultFuncIOperationResult()
        {
            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.MatchAsync(
                    It.IsAny<Func<int, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<int?, string, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<Exception, Task<IOperationResult<int>>>>()
                )).Returns(Task.FromResult(sourceMock.Object));

            var expectedResultSuccess = sourceMock.Object;
            var expectedResultOther = new Mock<IOperationResult<int>>().Object;

            var source = sourceMock.Object;
            var actualResult = await source.BindErrorAsync(
                _ =>
                {
                    return Task.FromResult(expectedResultOther);
                },

                _ =>
                {
                    return default(string);
                }
            );

            sourceMock.Verify(x =>
                x.MatchAsync(
                    It.Is<Func<int, Task<IOperationResult<int>>>>(y => y(default(int)).Result == expectedResultSuccess),
                    It.Is<Func<int?, string, Task<IOperationResult<int>>>>(y => y(default(int), default(string)).Result == expectedResultOther),
                    It.Is<Func<Exception, Task<IOperationResult<int>>>>(y => y(default(Exception)).Result == expectedResultOther)
                ), Times.Once);

            Assert.Equal(expectedResultSuccess, actualResult);
        }

        [Fact]
        public async void BindErrorAsyncTestWithoutErrorCodeTaskIOperationResultFuncTaskIOperationResult()
        {
            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.MatchAsync(
                    It.IsAny<Func<int, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<int?, string, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<Exception, Task<IOperationResult<int>>>>()
                )).Returns(Task.FromResult(sourceMock.Object));

            var expectedResultSuccess = sourceMock.Object;
            var expectedResultOther = new Mock<IOperationResult<int>>().Object;

            var source = Task.FromResult(sourceMock.Object);
            var actualResult = await source.BindErrorAsync(
                _ =>
                {
                    return Task.FromResult(expectedResultOther);
                },

                _ =>
                {
                    return default(string);
                }
            );

            sourceMock.Verify(x =>
                x.MatchAsync(
                    It.Is<Func<int, Task<IOperationResult<int>>>>(y => y(default(int)).Result == expectedResultSuccess),
                    It.Is<Func<int?, string, Task<IOperationResult<int>>>>(y => y(default(int), default(string)).Result == expectedResultOther),
                    It.Is<Func<Exception, Task<IOperationResult<int>>>>(y => y(default(Exception)).Result == expectedResultOther)
                ), Times.Once);

            Assert.Equal(expectedResultSuccess, actualResult);
        }

        [Fact]
        public async void BindErrorAsyncTestWithErrorCodeTaskIOperationResultFuncIOperationResult()
        {
            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.Match(
                    It.IsAny<Func<int, IOperationResult<int>>>(),
                    It.IsAny<Func<int?, string, IOperationResult<int>>>(),
                    It.IsAny<Func<Exception, IOperationResult<int>>>()
                )).Returns(sourceMock.Object);

            var expectedResultSuccess = sourceMock.Object;
            var expectedResultOther = new Mock<IOperationResult<int>>().Object;

            var source = Task.FromResult(sourceMock.Object);
            var actualResult = await source.BindErrorAsync(
                (code, str) =>
                {
                    return expectedResultOther;
                },

                _ =>
                {
                    return default(string);
                },

                default(int?)
            );

            sourceMock.Verify(x =>
                x.Match(
                    It.Is<Func<int, IOperationResult<int>>>(y => y(default(int)) == expectedResultSuccess),
                    It.Is<Func<int?, string, IOperationResult<int>>>(y => y(default(int), default(string)) == expectedResultOther),
                    It.Is<Func<Exception, IOperationResult<int>>>(y => y(default(Exception)) == expectedResultOther)
                ), Times.Once);

            Assert.Equal(expectedResultSuccess, actualResult);
        }

        [Fact]
        public async void BindErrorAsyncTestWithErrorCodeIOperationResultFuncIOperationResult()
        {
            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.MatchAsync(
                    It.IsAny<Func<int, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<int?, string, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<Exception, Task<IOperationResult<int>>>>()
                )).Returns(Task.FromResult(sourceMock.Object));

            var expectedResultSuccess = sourceMock.Object;
            var expectedResultOther = new Mock<IOperationResult<int>>().Object;

            var source = sourceMock.Object;
            var actualResult = await source.BindErrorAsync(
                (code, str) =>
                {
                    return Task.FromResult(expectedResultOther);
                },

                _ =>
                {
                    return default(string);
                },

                default(int?)
            );

            sourceMock.Verify(x =>
                x.MatchAsync(
                    It.Is<Func<int, Task<IOperationResult<int>>>>(y => y(default(int)).Result == expectedResultSuccess),
                    It.Is<Func<int?, string, Task<IOperationResult<int>>>>(y => y(default(int), default(string)).Result == expectedResultOther),
                    It.Is<Func<Exception, Task<IOperationResult<int>>>>(y => y(default(Exception)).Result == expectedResultOther)
                ), Times.Once);

            Assert.Equal(expectedResultSuccess, actualResult);
        }

        [Fact]
        public async void BindErrorAsyncTestWithErrorCodeTaskIOperationResultFuncTaskIOperationResult()
        {
            var sourceMock = new Mock<IOperationResult<int>>();
            sourceMock.Setup(x =>
                x.MatchAsync(
                    It.IsAny<Func<int, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<int?, string, Task<IOperationResult<int>>>>(),
                    It.IsAny<Func<Exception, Task<IOperationResult<int>>>>()
                )).Returns(Task.FromResult(sourceMock.Object));

            var expectedResultSuccess = sourceMock.Object;
            var expectedResultOther = new Mock<IOperationResult<int>>().Object;

            var source = Task.FromResult(sourceMock.Object);
            var actualResult = await source.BindErrorAsync(
                (code, str) =>
                {
                    return Task.FromResult(expectedResultOther);
                },

                _ =>
                {
                    return default(string);
                },

                default(int?)
            );

            sourceMock.Verify(x =>
                x.MatchAsync(
                    It.Is<Func<int, Task<IOperationResult<int>>>>(y => y(default(int)).Result == expectedResultSuccess),
                    It.Is<Func<int?, string, Task<IOperationResult<int>>>>(y => y(default(int), default(string)).Result == expectedResultOther),
                    It.Is<Func<Exception, Task<IOperationResult<int>>>>(y => y(default(Exception)).Result == expectedResultOther)
                ), Times.Once);

            Assert.Equal(expectedResultSuccess, actualResult);
        }

        [Fact]
        public void TryTest()
        {
            // If operation succeeded
            var testValue = default(int);
            var returnResult = OperationResult.Try(() => testValue);
            Assert.IsType<SuccessResult<int>>(returnResult);
            Assert.Equal(testValue, (returnResult as SuccessResult<int>).Value);

            // If operation fails
            var testException = default(Exception);
            var returnException = OperationResult.Try<int>(() => { throw testException; });
            Assert.IsType<FailureResult<int>>(returnException);
        }

        [Fact]
        public async void TryAsyncTest()
        {
            // If operation succeeded
            var testValue = default(int);
            var returnResult = await OperationResult.TryAsync(() => Task.FromResult(testValue));
            Assert.IsType<SuccessResult<int>>(returnResult);
            Assert.Equal(testValue, (returnResult as SuccessResult<int>).Value);

            // If operation fails
            var testException = default(Exception);
            var returnException = await OperationResult.TryAsync<int>(() => { throw testException; });
            Assert.IsType<FailureResult<int>>(returnException);
        }

        [Fact]
        public void AnyErrorTest()
        {
            var message1 = "Message1";
            var message2 = "Message2";
            var errorCode2 = 555;
            var resultFuncs = new Func<IOperationResult<int>>[]
            {
                () => OperationResult.Error<int>(message1),
                () => OperationResult.Error<int>(errorCode2, message2)
            };

            var res = resultFuncs.Any();

            res.Match(_ =>
            {
                Assert.False(true);
                return 0;
            }, (code, error) =>
            {
                Assert.Equal(errorCode2, code);
                Assert.Equal(message2, error);
                return 0;
            }, _ =>
            {
                Assert.False(true);
                return 0;
            });
        }

        [Fact]
        public void AnyFailureTest()
        {
            var message = "Message";
            var exception = new Exception("Exception");
            var resultFuncs = new Func<IOperationResult<int>>[]
            {
                () => OperationResult.Error<int>(message),
                () => OperationResult.Failure<int>(exception)
            };

            var res = resultFuncs.Any();

            res.Match(_ =>
            {
                Assert.False(true);
                return 0;
            }, (_, __) =>
            {
                Assert.False(true);
                return 0;
            }, ex =>
            {
                Assert.Equal(exception, ex);
                return 0;
            });
        }

        [Fact]
        public void AnySuccessTest()
        {
            var message = "Message";
            var successValue = 123;
            var resultFuncs = new Func<IOperationResult<int>>[]
            {
                () => OperationResult.Error<int>(message),
                () => OperationResult.Success<int>(successValue)
            };

            var res = resultFuncs.Any();

            res.Match(value =>
            {
                Assert.Equal(successValue, value);
                return 0;
            }, (_, __) =>
            {
                Assert.False(true);
                return 0;
            }, ex =>
            {
                Assert.False(true);
                return 0;
            });
        }

        [Fact]
        public void AllErrorTest()
        {
            var value1 = 123;
            var message2 = "Message2";
            var errorCode2 = 555;
            var message3 = "Message3";
            var resultFuncs = new Func<IOperationResult<int>>[]
            {
                () => OperationResult.Success<int>(value1),
                () => OperationResult.Error<int>(errorCode2, message2),
                () => OperationResult.Error<int>(message3),
            };

            var res = resultFuncs.All();

            res.Match(_ =>
            {
                Assert.False(true);
                return 0;
            }, (code, error) =>
            {
                Assert.Equal(errorCode2, code);
                Assert.Equal(message2, error);
                return 0;
            }, _ =>
            {
                Assert.False(true);
                return 0;
            });
        }
    }
}
