using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Application.Common.Interfaces;

namespace WorkflowPlatform.Application.Common.Resilience
{
    /// <summary>
    /// Retry policy implementation using Polly
    /// </summary>
    public class RetryPolicy : IRetryPolicy
    {
        private readonly IAsyncPolicy _retryPolicy;
        private readonly ILogger<RetryPolicy> _logger;
        private readonly IMetricsCollector _metrics;

        public RetryPolicy(ILogger<RetryPolicy> logger, IMetricsCollector metrics)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));

            // Configure exponential backoff with jitter
            _retryPolicy = Policy
                .Handle<Exception>(ex => IsTransientException(ex))
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + 
                                                          TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var nodeType = context.GetValueOrDefault("NodeType", "Unknown").ToString();
                        _logger.LogWarning("Retry attempt {RetryCount} for {NodeType} after {Delay}ms. Exception: {Exception}",
                            retryCount, nodeType, timespan.TotalMilliseconds, outcome.Message);
                        
                        _metrics.RecordRetryAttempt(nodeType!, retryCount);
                    });
        }

        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            return await _retryPolicy.ExecuteAsync(async (ct) => await operation(ct), cancellationToken);
        }

        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            Func<Exception, bool> retryPredicate,
            CancellationToken cancellationToken = default)
        {
            var customRetryPolicy = Policy
                .Handle<Exception>(retryPredicate)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + 
                                                          TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var nodeType = context.GetValueOrDefault("NodeType", "Unknown").ToString();
                        _logger.LogWarning("Custom retry attempt {RetryCount} for {NodeType} after {Delay}ms. Exception: {Exception}",
                            retryCount, nodeType, timespan.TotalMilliseconds, outcome.Message);
                        
                        _metrics.RecordRetryAttempt(nodeType!, retryCount);
                    });

            return await customRetryPolicy.ExecuteAsync(async (ct) => await operation(ct), cancellationToken);
        }

        private static bool IsTransientException(Exception exception)
        {
            // Common transient exceptions that should trigger retry
            return exception switch
            {
                TimeoutException => true,
                TaskCanceledException tce when !tce.CancellationToken.IsCancellationRequested => true,
                HttpRequestException => true,
                _ when exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) => true,
                _ when exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
                _ when exception.Message.Contains("network", StringComparison.OrdinalIgnoreCase) => true,
                _ => false
            };
        }
    }

    /// <summary>
    /// Simple circuit breaker implementation
    /// </summary>
    public class CircuitBreaker : ICircuitBreaker
    {
        private readonly object _lock = new();
        private readonly ILogger<CircuitBreaker> _logger;
        private readonly IMetricsCollector _metrics;
        private readonly string _nodeType;
        private readonly int _failureThreshold;
        private readonly TimeSpan _timeout;

        private int _failureCount;
        private DateTime _lastFailureTime;
        private CircuitBreakerState _state;

        public CircuitBreaker(string nodeType, ILogger<CircuitBreaker> logger, IMetricsCollector metrics)
        {
            _nodeType = nodeType ?? throw new ArgumentNullException(nameof(nodeType));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            _failureThreshold = 5;
            _timeout = TimeSpan.FromMinutes(1);
            _state = CircuitBreakerState.Closed;
        }

        public async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            if (IsOpen)
            {
                throw new InvalidOperationException($"Circuit breaker is open for {_nodeType}");
            }

            try
            {
                var result = await operation(cancellationToken);
                OnSuccess();
                return result;
            }
            catch (Exception ex)
            {
                OnFailure(ex);
                throw;
            }
        }

        public bool IsOpen
        {
            get
            {
                lock (_lock)
                {
                    if (_state == CircuitBreakerState.Open && DateTime.UtcNow - _lastFailureTime >= _timeout)
                    {
                        _state = CircuitBreakerState.HalfOpen;
                        _logger.LogInformation("Circuit breaker half-open for {NodeType}", _nodeType);
                        _metrics.RecordCircuitBreakerStateChange(_nodeType, "HalfOpen");
                    }
                    return _state == CircuitBreakerState.Open;
                }
            }
        }

        public CircuitBreakerState State
        {
            get
            {
                lock (_lock)
                {
                    return _state;
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _failureCount = 0;
                _state = CircuitBreakerState.Closed;
                _logger.LogInformation("Circuit breaker reset for {NodeType}", _nodeType);
                _metrics.RecordCircuitBreakerStateChange(_nodeType, "Closed");
            }
        }

        private void OnSuccess()
        {
            lock (_lock)
            {
                _failureCount = 0;
                if (_state == CircuitBreakerState.HalfOpen)
                {
                    _state = CircuitBreakerState.Closed;
                    _logger.LogInformation("Circuit breaker closed for {NodeType}", _nodeType);
                    _metrics.RecordCircuitBreakerStateChange(_nodeType, "Closed");
                }
            }
        }

        private void OnFailure(Exception exception)
        {
            lock (_lock)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                if (_failureCount >= _failureThreshold && _state == CircuitBreakerState.Closed)
                {
                    _state = CircuitBreakerState.Open;
                    _logger.LogWarning("Circuit breaker opened for {NodeType} due to {Exception}. Failure count: {Count}",
                        _nodeType, exception.GetType().Name, _failureCount);
                    _metrics.RecordCircuitBreakerStateChange(_nodeType, "Open");
                }
                else if (_state == CircuitBreakerState.HalfOpen)
                {
                    _state = CircuitBreakerState.Open;
                    _logger.LogWarning("Circuit breaker reopened for {NodeType} during half-open state", _nodeType);
                    _metrics.RecordCircuitBreakerStateChange(_nodeType, "Open");
                }
            }
        }
    }
}
