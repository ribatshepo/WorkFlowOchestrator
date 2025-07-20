using System;
using System.Threading;
using System.Threading.Tasks;

namespace WorkflowPlatform.Application.Common.Interfaces
{
    /// <summary>
    /// Retry policy configuration and execution
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// Execute an operation with retry logic
        /// </summary>
        Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute an operation with retry logic and custom retry predicate
        /// </summary>
        Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            Func<Exception, bool> retryPredicate,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Circuit breaker pattern implementation
    /// </summary>
    public interface ICircuitBreaker
    {
        /// <summary>
        /// Execute an operation with circuit breaker protection
        /// </summary>
        Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if the circuit breaker is open
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Get the current state of the circuit breaker
        /// </summary>
        CircuitBreakerState State { get; }

        /// <summary>
        /// Reset the circuit breaker to closed state
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Circuit breaker states
    /// </summary>
    public enum CircuitBreakerState
    {
        Closed = 0,
        Open = 1,
        HalfOpen = 2
    }
}
