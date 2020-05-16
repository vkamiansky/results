# Results

[![Build status](https://ci.appveyor.com/api/projects/status/ol33akfttn7sl176?svg=true)](https://ci.appveyor.com/project/vkamiansky/results)

## Description
A C# framework for building railroad-oriented code with results.

This library is meant to help you:

* Discriminate unambiguously between successfully-performed operations, operations failed due to improper use of the system, and system failures.
* Implement your code in [honest](http://functionalprogrammingcsharp.com/honest-functions) functions combining them seamlessly.
* Avoid using `try`/`catch` in multiple layers of your application. No more custom exceptions handling slowing down your code.
* Make your code easier to extend and refactor, thinking of it as a sequence of stages with clear input and output types.

## How to use it

1) Add the package.

```
dotnet add package Fls.Results
```

2) Add a using directive.

```c#
using Fls.Results;
```

3) Create an instance of `IOperationResult<T>`. We can create results of any type we want.

```c#
// Our custom class
public class ApiResponse<T>
{
    public string OriginalString { get; set; }

    public T ParsedObject { get; set; }
}
```
...
```c#
// A success result with a value of our response type.
IOperationResult<ApiResponse<int>> success = OperationResult.Success(new ApiResponse<int>() { OriginalString = "2", ParsedObject = 2 });
// An error result with a custom error code of 323 and an error message.
IOperationResult<ApiResponse<int>> error = OperationResult.Error<ApiResponse<int>>(323, "Wrong input from user.");
// A system failure result with the associated exception.
IOperationResult<ApiResponse<int>> failure = OperationResult.Failure<ApiResponse<int>>(new InvalidOperationException());
```

4) `Bind` results of one type to another. Convert `T` into a successful `IOperationResult<T>` using `.ToResult()`.

```c#
public static IOperationResult<int> Divide(int x, int y)
{
    return y == 0
    ? OperationResult.Error<int>(1, "Can't divide by zero.")
    : OperationResult.Success(x / y);
}
```
...
```c#
IOperationResult<string> stringResult = 
            Divide(3, 4)
            .Bind(x => x.ToString().ToResult());
```

5) `Use` successful result to execute a side effect. `UseError` does the thing for unsuccessful results.

```c#
IOperationResult<int> stringResult = 
            Divide(x, y)
            .Use(x => Console.WriteLine(x))
            .UseError(error => Console.WriteLine(error), ex => ex.Message);
```

6) 'IfError' redirects execution to another option of getting result if the previous one fails.

```c#
IOperationResult<string> stringResult = 
            Divide(3, 0)
            .Bind(x => x.ToString().ToResult())
            .IfError(() => OperationResult.Success("You can't do it"))
            .Use(x => Console.WriteLine(x));
 ```
 
7) `Bind`, `BindError`, `Try`, `Use`, `UseError`, `IfError`, `ToResult` have asynchronous versions `BindAsync`, `BindErrorAsync`, `TryAsync`, `UseAsync`, `UseErrorAsync`, `IfErrorAsync`, `ToResultAsync`. Use them to create asynchronous code flow.

```c#
public static async Task<IOperationResult<string>> ToOutputString<T>(Task<IOperationResult<T>> source)
{
    return await source.BindAsync(x => $"The {x.GetType().Name} value is {x.ToString()}".ToResult());
}
```
...
```c#
IOperationResult<string> stringResult = await ToOutputString(Task.FromResult(OperationResult.Success(1)))
            .UseAsync(x => Console.WriteLine(x));
```

8) Find the complete set of supported methods in class [`OperationResult`](https://github.com/vkamiansky/results/blob/master/src/Fls.Results/OperationResult.cs).
