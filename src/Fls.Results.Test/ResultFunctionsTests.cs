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
                    It.IsAny<Func<string, IOperationResult<int>>>(),
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
                    It.Is<Func<string, IOperationResult<int>>>(y => (y(testError) as OperationResult.ErrorResult<int>).Message == testError),
                    It.Is<Func<Exception, IOperationResult<int>>>(y => (y(testException) as OperationResult.FailureResult<int>).Exception == testException)
                ), Times.Once);

            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
         public async void SuccessMatchAsyncTest()
         {
             var testValue = 2;
             var successBound = new Mock<IOperationResult<float>>().Object;
             var errorBound = new Mock<IOperationResult<float>>().Object;
             var failureBound = new Mock<IOperationResult<float>>().Object;
             Func<int, Task<IOperationResult<float>>> bindSuccess = _ => Task.FromResult(successBound);
             Func<string, Task<IOperationResult<float>>> bindError = _ => Task.FromResult(errorBound);
             Func<Exception, Task<IOperationResult<float>>> bindFailure = _ =>  Task.FromResult(failureBound);

             var sut = new OperationResult.SuccessResult<int>(testValue);

             var matchResult = await sut.MatchAsync(bindSuccess, bindError, bindFailure);
             Assert.Equal(successBound, matchResult);
         }
    }
}
