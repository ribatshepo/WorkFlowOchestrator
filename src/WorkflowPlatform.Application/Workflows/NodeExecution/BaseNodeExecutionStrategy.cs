using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application.Workflows.NodeExecution
{
    /// <summary>
    /// Base implementation of the node execution strategy with common lifecycle management
    /// </summary>
    public abstract class BaseNodeExecutionStrategy : INodeExecutionStrategy
    {
        protected readonly ILogger _logger;
        protected readonly IMetricsCollector _metrics;

        protected BaseNodeExecutionStrategy(
            ILogger logger, 
            IMetricsCollector metrics)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        /// <summary>
        /// The type of node this strategy handles
        /// </summary>
        public abstract string NodeType { get; }

        /// <summary>
        /// Phase 1: Preprocessing - Validate inputs and setup execution context
        /// </summary>
        public virtual async Task<NodeExecutionResult> PreprocessAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting preprocessing for node {NodeId} of type {NodeType}", 
                    context.NodeId, NodeType);

                // Validate cancellation token
                cancellationToken.ThrowIfCancellationRequested();

                // Validate inputs
                var validationResult = await ValidateInputsAsync(context, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errorMessage = $"Input validation failed: {string.Join(", ", validationResult.Errors)}";
                    _logger.LogWarning("Preprocessing failed for node {NodeId}: {ErrorMessage}", 
                        context.NodeId, errorMessage);
                    return NodeExecutionResult.Failed(errorMessage);
                }

                // Log warnings if any
                if (validationResult.Warnings.Count > 0)
                {
                    _logger.LogWarning("Preprocessing warnings for node {NodeId}: {Warnings}", 
                        context.NodeId, string.Join(", ", validationResult.Warnings));
                }

                // Setup execution context
                await SetupExecutionContextAsync(context, cancellationToken);

                _logger.LogInformation("Preprocessing completed successfully for node {NodeId}", context.NodeId);
                return NodeExecutionResult.Success();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Preprocessing cancelled for node {NodeId}", context.NodeId);
                return NodeExecutionResult.Cancelled("Preprocessing was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Preprocessing failed for node {NodeId}", context.NodeId);
                _metrics.RecordNodeExecutionError(NodeType, "PreprocessingError");
                return NodeExecutionResult.Failed(ex);
            }
            finally
            {
                stopwatch.Stop();
                _metrics.RecordLifecyclePhaseDuration(NodeType, "Preprocessing", stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Phase 2: Execute - Perform the core node operation
        /// </summary>
        public abstract Task<NodeExecutionResult> ExecuteAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Phase 3: Postprocessing - Transform output data and validate results
        /// </summary>
        public virtual async Task<NodeExecutionResult> PostprocessAsync(
            NodeExecutionContext context, 
            NodeExecutionResult executionResult, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting postprocessing for node {NodeId} of type {NodeType}", 
                    context.NodeId, NodeType);

                // Skip postprocessing if execution failed
                if (!executionResult.IsSuccess)
                {
                    _logger.LogInformation("Skipping postprocessing for node {NodeId} due to execution failure", 
                        context.NodeId);
                    return executionResult;
                }

                cancellationToken.ThrowIfCancellationRequested();

                // Transform output data
                var transformedResult = await TransformOutputAsync(context, executionResult, cancellationToken);
                
                // Validate output
                var validationResult = await ValidateOutputAsync(context, transformedResult, cancellationToken);
                if (!validationResult.IsValid)
                {
                    var errorMessage = $"Output validation failed: {string.Join(", ", validationResult.Errors)}";
                    _logger.LogWarning("Postprocessing validation failed for node {NodeId}: {ErrorMessage}", 
                        context.NodeId, errorMessage);
                    return NodeExecutionResult.Failed(errorMessage);
                }

                _logger.LogInformation("Postprocessing completed successfully for node {NodeId}", context.NodeId);
                return transformedResult;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Postprocessing cancelled for node {NodeId}", context.NodeId);
                return NodeExecutionResult.Cancelled("Postprocessing was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Postprocessing failed for node {NodeId}", context.NodeId);
                _metrics.RecordNodeExecutionError(NodeType, "PostprocessingError");
                return NodeExecutionResult.Failed(ex);
            }
            finally
            {
                stopwatch.Stop();
                _metrics.RecordLifecyclePhaseDuration(NodeType, "Postprocessing", stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Phase 4: Finalization - Cleanup resources and persist state
        /// </summary>
        public virtual async Task FinalizationAsync(
            NodeExecutionContext context, 
            NodeExecutionResult executionResult, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting finalization for node {NodeId} of type {NodeType}", 
                    context.NodeId, NodeType);

                cancellationToken.ThrowIfCancellationRequested();

                // Always cleanup resources, even if execution failed
                await CleanupResourcesAsync(context, cancellationToken);

                // Persist execution state
                await PersistExecutionStateAsync(context, executionResult, cancellationToken);

                // Trigger completion events
                await TriggerCompletionEventsAsync(context, executionResult, cancellationToken);

                _logger.LogInformation("Finalization completed successfully for node {NodeId}", context.NodeId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Finalization cancelled for node {NodeId}", context.NodeId);
                // Continue cleanup even if cancelled
                try
                {
                    await CleanupResourcesAsync(context, CancellationToken.None);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Cleanup failed during cancelled finalization for node {NodeId}", 
                        context.NodeId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Finalization failed for node {NodeId}", context.NodeId);
                _metrics.RecordNodeExecutionError(NodeType, "FinalizationError");
                
                // Still attempt cleanup
                try
                {
                    await CleanupResourcesAsync(context, CancellationToken.None);
                }
                catch (Exception cleanupEx)
                {
                    _logger.LogError(cleanupEx, "Cleanup failed during finalization error for node {NodeId}", 
                        context.NodeId);
                }
            }
            finally
            {
                stopwatch.Stop();
                _metrics.RecordLifecyclePhaseDuration(NodeType, "Finalization", stopwatch.Elapsed);
            }
        }

        #region Abstract Methods - Must be implemented by concrete strategies

        /// <summary>
        /// Validate input data and configuration
        /// </summary>
        protected abstract Task<ValidationResult> ValidateInputsAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken);

        /// <summary>
        /// Setup any required resources or context for execution
        /// </summary>
        protected abstract Task SetupExecutionContextAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken);

        /// <summary>
        /// Transform the output data after successful execution
        /// </summary>
        protected abstract Task<NodeExecutionResult> TransformOutputAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken);

        /// <summary>
        /// Validate the transformed output data
        /// </summary>
        protected abstract Task<ValidationResult> ValidateOutputAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken);

        /// <summary>
        /// Cleanup any resources created during execution
        /// </summary>
        protected abstract Task CleanupResourcesAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken);

        /// <summary>
        /// Persist the execution state and results
        /// </summary>
        protected abstract Task PersistExecutionStateAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken);

        /// <summary>
        /// Trigger any completion events or notifications
        /// </summary>
        protected abstract Task TriggerCompletionEventsAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken);

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to check if configuration contains required key
        /// </summary>
        protected bool HasRequiredConfiguration(NodeExecutionContext context, string key)
        {
            return context.Configuration.ContainsKey(key) && context.Configuration[key] != null;
        }

        /// <summary>
        /// Helper method to get required configuration with type checking
        /// </summary>
        protected T GetRequiredConfiguration<T>(NodeExecutionContext context, string key)
        {
            if (!context.Configuration.TryGetValue(key, out var value))
                throw new InvalidOperationException($"Required configuration '{key}' is missing");

            if (value is not T typed)
                throw new InvalidOperationException($"Configuration '{key}' is not of type {typeof(T).Name}");

            return typed;
        }

        /// <summary>
        /// Helper method to create timeout cancellation token
        /// </summary>
        protected CancellationToken CreateTimeoutToken(NodeExecutionContext context, TimeSpan timeout)
        {
            var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);
            timeoutSource.CancelAfter(timeout);
            
            // Store the source for cleanup
            context.AddResource($"TimeoutSource_{Guid.NewGuid()}", timeoutSource);
            
            return timeoutSource.Token;
        }

        #endregion
    }
}
