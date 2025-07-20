using System;

namespace WorkflowPlatform.Domain.Common
{
    /// <summary>
    /// Base result class for all operations
    /// </summary>
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Value { get; private set; }
        public string? ErrorMessage { get; private set; }
        public Exception? Exception { get; private set; }

        protected Result(bool isSuccess, T? value, string? errorMessage, Exception? exception = null)
        {
            IsSuccess = isSuccess;
            Value = value;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public static Result<T> Success(T value) => new(true, value, null);
        public static Result<T> Success() => new(true, default, null);
        public static Result<T> Failure(string error) => new(false, default, error);
        public static Result<T> Failure(Exception exception) => new(false, default, exception.Message, exception);
        public static Result<T> Failure(string error, Exception exception) => new(false, default, error, exception);
    }

    /// <summary>
    /// Result class without value
    /// </summary>
    public class Result : Result<object>
    {
        private Result(bool isSuccess, string? errorMessage, Exception? exception = null)
            : base(isSuccess, null, errorMessage, exception) { }

        public new static Result Success() => new(true, null);
        public new static Result Failure(string error) => new(false, error);
        public new static Result Failure(Exception exception) => new(false, exception.Message, exception);
        public new static Result Failure(string error, Exception exception) => new(false, error, exception);
    }
}
