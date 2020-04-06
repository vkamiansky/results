using System;
using Xunit;
using Moq;
using System.Threading.Tasks;

namespace Fls.Results.Test
{
    public class ResultObjectsTests
    {
        [Fact]
        public void SuccessMatchTest()
        {
            var testValue = 2;
            var successBound = new Mock<IOperationResult<float>>().Object;
            var errorBound = new Mock<IOperationResult<float>>().Object;
            var failureBound = new Mock<IOperationResult<float>>().Object;
            Func<int, IOperationResult<float>> bindSuccess = _ => successBound;
            Func<int?, string, IOperationResult<float>> bindError = (_, str) => errorBound;
            Func<Exception, IOperationResult<float>> bindFailure = _ => failureBound;

            var sut = new OperationResult.SuccessResult<int>(testValue);

            var matchResult = sut.Match(bindSuccess, bindError, bindFailure);
            Assert.Equal(successBound, matchResult);
        }

        [Fact]
        public void ErrorMatchTest()
        {
            var testValue = "Error";
            var successBound = new Mock<IOperationResult<float>>().Object;
            var errorBound = new Mock<IOperationResult<float>>().Object;
            var failureBound = new Mock<IOperationResult<float>>().Object;
            Func<int, IOperationResult<float>> bindSuccess = _ => successBound;
            Func<int?, string, IOperationResult<float>> bindError = (_, str) => errorBound;
            Func<Exception, IOperationResult<float>> bindFailure = _ => failureBound;

            var sut = new OperationResult.ErrorResult<int>(testValue);

            var matchResult = sut.Match(bindSuccess, bindError, bindFailure);
            Assert.Equal(errorBound, matchResult);
        }

        [Fact]
        public void FailureMatchTest()
        {
            var testValue = new Exception("Failure");
            var successBound = new Mock<IOperationResult<float>>().Object;
            var errorBound = new Mock<IOperationResult<float>>().Object;
            var failureBound = new Mock<IOperationResult<float>>().Object;
            Func<int, IOperationResult<float>> bindSuccess = _ => successBound;
            Func<int?, string, IOperationResult<float>> bindError = (_, str) => errorBound;
            Func<Exception, IOperationResult<float>> bindFailure = _ => failureBound;

            var sut = new OperationResult.FailureResult<int>(testValue);

            var matchResult = sut.Match(bindSuccess, bindError, bindFailure);
            Assert.Equal(failureBound, matchResult);
        }

        [Fact]
        public async void SuccessMatchAsyncTest()
        {
            var testValue = 2;
            var successBound = new Mock<IOperationResult<float>>().Object;
            var errorBound = new Mock<IOperationResult<float>>().Object;
            var failureBound = new Mock<IOperationResult<float>>().Object;
            Func<int, Task<IOperationResult<float>>> bindSuccess = _ => Task.FromResult(successBound);
            Func<int?, string, Task<IOperationResult<float>>> bindError = (_, str) => Task.FromResult(errorBound);
            Func<Exception, Task<IOperationResult<float>>> bindFailure = _ => Task.FromResult(failureBound);

            var sut = new OperationResult.SuccessResult<int>(testValue);

            var matchResult = await sut.MatchAsync(bindSuccess, bindError, bindFailure);
            Assert.Equal(successBound, matchResult);
        }

        [Fact]
        public async void ErrorMatchAsyncTest()
        {
            var testValue = "Error";
            var successBound = new Mock<IOperationResult<float>>().Object;
            var errorBound = new Mock<IOperationResult<float>>().Object;
            var failureBound = new Mock<IOperationResult<float>>().Object;
            Func<int, Task<IOperationResult<float>>> bindSuccess = _ => Task.FromResult(successBound);
            Func<int?, string, Task<IOperationResult<float>>> bindError = (_, str) => Task.FromResult(errorBound);
            Func<Exception, Task<IOperationResult<float>>> bindFailure = _ => Task.FromResult(failureBound);

            var sut = new OperationResult.ErrorResult<int>(testValue);

            var matchResult = await sut.MatchAsync(bindSuccess, bindError, bindFailure);
            Assert.Equal(errorBound, matchResult);
        }

        [Fact]
        public async void FailureMatchAsyncTest()
        {
            var testValue = new Exception("Failure");
            var successBound = new Mock<IOperationResult<float>>().Object;
            var errorBound = new Mock<IOperationResult<float>>().Object;
            var failureBound = new Mock<IOperationResult<float>>().Object;
            Func<int, Task<IOperationResult<float>>> bindSuccess = _ => Task.FromResult(successBound);
            Func<int?, string, Task<IOperationResult<float>>> bindError = (_, str) => Task.FromResult(errorBound);
            Func<Exception, Task<IOperationResult<float>>> bindFailure = _ => Task.FromResult(failureBound);

            var sut = new OperationResult.FailureResult<int>(testValue);

            var matchResult = await sut.MatchAsync(bindSuccess, bindError, bindFailure);
            Assert.Equal(failureBound, matchResult);
        }
    }
}
