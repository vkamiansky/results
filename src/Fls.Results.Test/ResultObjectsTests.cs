using System;
using Xunit;
using Moq;

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
            Func<string, IOperationResult<float>> bindError = _ => errorBound;
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
             Func<string, IOperationResult<float>> bindError = _ => errorBound;
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
             Func<string, IOperationResult<float>> bindError = _ => errorBound;
             Func<Exception, IOperationResult<float>> bindFailure = _ => failureBound;

             var sut = new OperationResult.FailureResult<int>(testValue);

             var matchResult = sut.Match(bindSuccess, bindError, bindFailure);
             Assert.Equal(failureBound, matchResult);
         }
    } 
}
