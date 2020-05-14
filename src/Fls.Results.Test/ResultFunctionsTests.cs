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

            Assert.IsType<OperationResult.SuccessResult<int>>(returnResult);
            Assert.Equal(testValue, (returnResult as OperationResult.SuccessResult<int>).Value);
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
                    It.Is<Func<int?, string, IOperationResult<int>>>(y => (y(null, testError) as OperationResult.ErrorResult<int>).Message == testError),
                    It.Is<Func<Exception, IOperationResult<int>>>(y => (y(testException) as OperationResult.FailureResult<int>).Exception == testException)
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
                    It.Is<Func<int?, string, Task<IOperationResult<int>>>>(y => (y(null, testError).Result as OperationResult.ErrorResult<int>).Message == testError),
                    It.Is<Func<Exception, Task<IOperationResult<int>>>>(y => (y(testException).Result as OperationResult.FailureResult<int>).Exception == testException)
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
                    It.Is<Func<int?, string, Task<IOperationResult<int>>>>(y => (y(null, testError).Result as OperationResult.ErrorResult<int>).Message == testError),
                    It.Is<Func<Exception, Task<IOperationResult<int>>>>(y => (y(testException).Result as OperationResult.FailureResult<int>).Exception == testException)
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
                    It.Is<Func<int?, string, IOperationResult<int>>>(y => (y(null, testError) as OperationResult.ErrorResult<int>).Message == testError),
                    It.Is<Func<Exception, IOperationResult<int>>>(y => (y(testException) as OperationResult.FailureResult<int>).Exception == testException)
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
            Assert.IsType<OperationResult.SuccessResult<int>>(returnResult);
            Assert.Equal(testValue, (returnResult as OperationResult.SuccessResult<int>).Value);

            // If operation fails
            var testException = default(Exception);
            var returnException = OperationResult.Try<int>(() => { throw testException; });
            Assert.IsType<OperationResult.FailureResult<int>>(returnException);
        }

        [Fact]
        public async void TryAsyncTest()
        {
            // If operation succeeded
            var testValue = default(int);
            var returnResult = await OperationResult.TryAsync(() => Task.FromResult(testValue));
            Assert.IsType<OperationResult.SuccessResult<int>>(returnResult);
            Assert.Equal(testValue, (returnResult as OperationResult.SuccessResult<int>).Value);

            // If operation fails
            var testException = default(Exception);
            var returnException = await OperationResult.TryAsync<int>(() => { throw testException; });
            Assert.IsType<OperationResult.FailureResult<int>>(returnException);
        }
    }
}
