using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Application.Workflows.NodeExecution.Configurations;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application.Workflows.NodeExecution.Strategies
{
    /// <summary>
    /// HTTP Request node execution strategy
    /// </summary>
    public class HttpRequestNodeStrategy : BaseNodeExecutionStrategy
    {
        private readonly HttpClient _httpClient;
        private readonly IRetryPolicy _retryPolicy;
        private readonly ICircuitBreaker _circuitBreaker;

        public HttpRequestNodeStrategy(
            HttpClient httpClient,
            IRetryPolicy retryPolicy,
            ICircuitBreaker circuitBreaker,
            ILogger<HttpRequestNodeStrategy> logger,
            IMetricsCollector metrics)
            : base(logger, metrics)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
            _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
        }

        public override string NodeType => "HttpRequest";

        public override async Task<NodeExecutionResult> ExecuteAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting HTTP request execution for node {NodeId}", context.NodeId);

                var config = GetRequiredConfiguration<HttpRequestNodeConfiguration>(context, "HttpConfig");
                var timeoutToken = CreateTimeoutToken(context, config.Timeout);

                // Execute HTTP request with retry and circuit breaker
                var result = await _retryPolicy.ExecuteAsync(async (ct) =>
                {
                    return await _circuitBreaker.ExecuteAsync(async (innerCt) =>
                    {
                        return await ExecuteHttpRequestAsync(config, innerCt);
                    }, ct);
                }, timeoutToken);

                stopwatch.Stop();
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.Completed, stopwatch.Elapsed);

                _logger.LogInformation("HTTP request completed successfully for node {NodeId} in {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);

                return NodeExecutionResult.Success(result, stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                _logger.LogWarning("HTTP request cancelled for node {NodeId} after {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.Cancelled, stopwatch.Elapsed);
                return NodeExecutionResult.Cancelled("HTTP request was cancelled", stopwatch.Elapsed);
            }
            catch (TimeoutException)
            {
                stopwatch.Stop();
                _logger.LogWarning("HTTP request timed out for node {NodeId} after {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.TimedOut, stopwatch.Elapsed);
                return NodeExecutionResult.Timeout(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds), stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "HTTP request failed for node {NodeId} after {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.Failed, stopwatch.Elapsed);
                _metrics.RecordNodeExecutionError(NodeType, ex.GetType().Name);
                return NodeExecutionResult.Failed(ex, stopwatch.Elapsed);
            }
        }

        protected override async Task<ValidationResult> ValidateInputsAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken)
        {
            var result = new ValidationResult();

            try
            {
                // Validate HTTP configuration exists
                if (!HasRequiredConfiguration(context, "HttpConfig"))
                {
                    result.AddError("HttpConfig is required");
                    return result;
                }

                var config = GetRequiredConfiguration<HttpRequestNodeConfiguration>(context, "HttpConfig");

                // Validate URL
                if (string.IsNullOrWhiteSpace(config.Url))
                {
                    result.AddError("URL is required");
                }
                else if (!Uri.IsWellFormedUriString(config.Url, UriKind.Absolute))
                {
                    result.AddError("URL must be a valid absolute URI");
                }

                // Validate HTTP method
                var allowedMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };
                if (!Array.Exists(allowedMethods, method => 
                    string.Equals(method, config.Method, StringComparison.OrdinalIgnoreCase)))
                {
                    result.AddError($"HTTP method '{config.Method}' is not supported");
                }

                // Validate timeout
                if (config.Timeout <= TimeSpan.Zero || config.Timeout > TimeSpan.FromMinutes(10))
                {
                    result.AddError("Timeout must be between 1 second and 10 minutes");
                }

                // Validate content type for requests with body
                if (!string.IsNullOrWhiteSpace(config.Body) && string.IsNullOrWhiteSpace(config.ContentType))
                {
                    result.AddWarning("ContentType not specified for request with body");
                }

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating HTTP request inputs for node {NodeId}", context.NodeId);
                result.AddError($"Validation error: {ex.Message}");
                return await Task.FromResult(result);
            }
        }

        protected override async Task SetupExecutionContextAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken)
        {
            try
            {
                var config = GetRequiredConfiguration<HttpRequestNodeConfiguration>(context, "HttpConfig");

                // Setup HTTP client timeout
                _httpClient.Timeout = config.Timeout;

                // Add any additional setup logic here
                context.SetProperty("SetupCompleted", true);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up HTTP request context for node {NodeId}", context.NodeId);
                throw;
            }
        }

        protected override async Task<NodeExecutionResult> TransformOutputAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken)
        {
            try
            {
                if (result.OutputData is string responseContent)
                {
                    // Try to parse as JSON if possible
                    try
                    {
                        var jsonObject = JsonSerializer.Deserialize<object>(responseContent);
                        return await Task.FromResult(result.WithOutputData(jsonObject));
                    }
                    catch
                    {
                        // If not valid JSON, return as string
                        return await Task.FromResult(result);
                    }
                }

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming HTTP response for node {NodeId}", context.NodeId);
                return await Task.FromResult(NodeExecutionResult.Failed($"Output transformation failed: {ex.Message}"));
            }
        }

        protected override async Task<ValidationResult> ValidateOutputAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken)
        {
            var validation = new ValidationResult();

            try
            {
                // Basic output validation
                if (result.OutputData == null)
                {
                    validation.AddWarning("HTTP response is empty");
                }

                return await Task.FromResult(validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating HTTP response for node {NodeId}", context.NodeId);
                validation.AddError($"Output validation error: {ex.Message}");
                return await Task.FromResult(validation);
            }
        }

        protected override async Task CleanupResourcesAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken)
        {
            try
            {
                // HTTP client is managed by DI, no cleanup needed
                _logger.LogDebug("Cleaned up HTTP request resources for node {NodeId}", context.NodeId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up HTTP request resources for node {NodeId}", context.NodeId);
            }
        }

        protected override async Task PersistExecutionStateAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Add execution metadata
                result.Metadata["ExecutedAt"] = DateTime.UtcNow;
                result.Metadata["NodeType"] = NodeType;
                result.Metadata["ExecutionId"] = context.ExecutionId.ToString();

                _logger.LogDebug("Persisted HTTP request execution state for node {NodeId}", context.NodeId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error persisting HTTP request state for node {NodeId}", context.NodeId);
            }
        }

        protected override async Task TriggerCompletionEventsAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Trigger any completion events or notifications
                _logger.LogInformation("HTTP request node {NodeId} completed with status {Status}", 
                    context.NodeId, result.Status);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering completion events for node {NodeId}", context.NodeId);
            }
        }

        private async Task<string> ExecuteHttpRequestAsync(
            HttpRequestNodeConfiguration config, 
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(new HttpMethod(config.Method), config.Url);

            // Add headers
            foreach (var header in config.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Add body if specified
            if (!string.IsNullOrWhiteSpace(config.Body))
            {
                request.Content = new StringContent(config.Body, Encoding.UTF8, config.ContentType ?? "application/json");
            }

            // Add authentication if specified
            await AddAuthenticationAsync(request, config.Authentication);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            // Check for successful status code
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return content;
        }

        private Task AddAuthenticationAsync(
            HttpRequestMessage request, 
            Dictionary<string, object> authConfig)
        {
            if (authConfig.TryGetValue("Type", out var authType))
            {
                switch (authType.ToString()?.ToLowerInvariant())
                {
                    case "bearer":
                        if (authConfig.TryGetValue("Token", out var token))
                        {
                            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.ToString());
                        }
                        break;
                    case "basic":
                        if (authConfig.TryGetValue("Username", out var username) && 
                            authConfig.TryGetValue("Password", out var password))
                        {
                            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                        }
                        break;
                    case "apikey":
                        if (authConfig.TryGetValue("HeaderName", out var headerName) && 
                            authConfig.TryGetValue("ApiKey", out var apiKey))
                        {
                            request.Headers.TryAddWithoutValidation(headerName.ToString()!, apiKey.ToString());
                        }
                        break;
                }
            }

            return Task.CompletedTask;
        }
    }
}
