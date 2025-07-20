using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application.Workflows.NodeExecution
{
    /// <summary>
    /// Main execution engine for node processing with 4-phase lifecycle
    /// </summary>
    public class NodeExecutionEngine : INodeExecutionEngine
    {
        private readonly INodeStrategyFactory _strategyFactory;
        private readonly ILogger<NodeExecutionEngine> _logger;
        private readonly IMetricsCollector _metrics;

        public NodeExecutionEngine(
            INodeStrategyFactory strategyFactory,
            ILogger<NodeExecutionEngine> logger,
            IMetricsCollector metrics)
        {
            _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }

        /// <summary>
        /// Execute a node through the complete 4-phase lifecycle
        /// </summary>
        public async Task<NodeExecutionResult> ExecuteNodeAsync(
            NodeExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            var overallStopwatch = Stopwatch.StartNew();
            NodeExecutionResult? finalResult = null;
            INodeExecutionStrategy? strategy = null;

            try
            {
                _logger.LogInformation("Starting node execution for {NodeId} of type {NodeType}", 
                    context.NodeId, context.NodeType);

                // Get the appropriate strategy
                strategy = _strategyFactory.GetStrategy(context.NodeType);
                if (strategy == null)
                {
                    var errorMessage = $"No strategy found for node type '{context.NodeType}'";
                    _logger.LogError(errorMessage);
                    return NodeExecutionResult.Failed(errorMessage);
                }

                _metrics.IncrementNodeExecutionAttempts(context.NodeType);

                // Phase 1: Preprocessing
                var preprocessResult = await ExecutePhaseWithMetricsAsync(
                    "Preprocessing",
                    context,
                    () => strategy.PreprocessAsync(context, cancellationToken));

                if (!preprocessResult.IsSuccess)
                {
                    _logger.LogWarning("Preprocessing failed for node {NodeId}: {Error}", 
                        context.NodeId, preprocessResult.ErrorMessage);
                    finalResult = preprocessResult;
                    return preprocessResult;
                }

                // Phase 2: Execute
                var executeResult = await ExecutePhaseWithMetricsAsync(
                    "Execute",
                    context,
                    () => strategy.ExecuteAsync(context, cancellationToken));

                // Phase 3: Postprocessing (always run, even if execution failed)
                var postprocessResult = await ExecutePhaseWithMetricsAsync(
                    "Postprocessing",
                    context,
                    () => strategy.PostprocessAsync(context, executeResult, cancellationToken));

                // Use postprocess result if execution was successful, otherwise keep execute result
                finalResult = executeResult.IsSuccess ? postprocessResult : executeResult;

                overallStopwatch.Stop();
                _metrics.RecordNodeExecution(context.NodeType, finalResult.Status, overallStopwatch.Elapsed);

                _logger.LogInformation("Node execution completed for {NodeId} with status {Status} in {Duration}ms", 
                    context.NodeId, finalResult.Status, overallStopwatch.ElapsedMilliseconds);

                return finalResult;
            }
            catch (OperationCanceledException)
            {
                overallStopwatch.Stop();
                _logger.LogWarning("Node execution cancelled for {NodeId} after {Duration}ms", 
                    context.NodeId, overallStopwatch.ElapsedMilliseconds);
                
                finalResult = NodeExecutionResult.Cancelled("Node execution was cancelled", overallStopwatch.Elapsed);
                _metrics.RecordNodeExecution(context.NodeType, finalResult.Status, overallStopwatch.Elapsed);
                
                return finalResult;
            }
            catch (Exception ex)
            {
                overallStopwatch.Stop();
                _logger.LogError(ex, "Unexpected error during node execution for {NodeId} after {Duration}ms", 
                    context.NodeId, overallStopwatch.ElapsedMilliseconds);
                
                finalResult = NodeExecutionResult.Failed(ex, overallStopwatch.Elapsed);
                _metrics.RecordNodeExecution(context.NodeType, finalResult.Status, overallStopwatch.Elapsed);
                _metrics.RecordNodeExecutionError(context.NodeType, ex.GetType().Name);
                
                return finalResult;
            }
            finally
            {
                // Phase 4: Finalization (always run, regardless of previous phase outcomes)
                if (strategy != null && finalResult != null)
                {
                    try
                    {
                        await ExecuteFinalizationPhaseAsync(
                            "Finalization",
                            context,
                            () => strategy.FinalizationAsync(context, finalResult, CancellationToken.None)); // Use separate token for cleanup
                    }
                    catch (Exception finalizationEx)
                    {
                        _logger.LogError(finalizationEx, "Finalization failed for node {NodeId}", context.NodeId);
                        // Don't throw - finalization errors shouldn't affect the main result
                    }
                }

                // Dispose the context to clean up any remaining resources
                context.Dispose();
            }
        }

        /// <summary>
        /// Execute a node with retry logic
        /// </summary>
        public async Task<NodeExecutionResult> ExecuteNodeWithRetryAsync(
            NodeExecutionContext context,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            CancellationToken cancellationToken = default)
        {
            var delay = retryDelay ?? TimeSpan.FromSeconds(1);
            var attempt = 0;
            NodeExecutionResult? lastResult = null;

            while (attempt <= maxRetries)
            {
                attempt++;
                
                try
                {
                    _logger.LogInformation("Executing node {NodeId} (attempt {Attempt}/{MaxAttempts})", 
                        context.NodeId, attempt, maxRetries + 1);

                    var result = await ExecuteNodeAsync(context, cancellationToken);
                    
                    if (result.IsSuccess)
                    {
                        if (attempt > 1)
                        {
                            _logger.LogInformation("Node {NodeId} succeeded on attempt {Attempt}", 
                                context.NodeId, attempt);
                        }
                        return result;
                    }

                    lastResult = result;

                    // Don't retry on certain types of failures
                    if (result.Status == NodeExecutionStatus.Cancelled || 
                        result.Status == NodeExecutionStatus.TimedOut ||
                        !ShouldRetry(result))
                    {
                        _logger.LogWarning("Node {NodeId} failed with non-retryable error: {Error}", 
                            context.NodeId, result.ErrorMessage);
                        break;
                    }

                    if (attempt <= maxRetries)
                    {
                        _logger.LogWarning("Node {NodeId} failed on attempt {Attempt}, retrying in {Delay}ms. Error: {Error}", 
                            context.NodeId, attempt, delay.TotalMilliseconds, result.ErrorMessage);
                        
                        _metrics.RecordRetryAttempt(context.NodeType, attempt);
                        
                        await Task.Delay(delay, cancellationToken);
                        
                        // Exponential backoff with jitter
                        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2 + Random.Shared.Next(0, 1000));
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Node execution cancelled for {NodeId} on attempt {Attempt}", 
                        context.NodeId, attempt);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error on attempt {Attempt} for node {NodeId}", 
                        attempt, context.NodeId);
                    
                    lastResult = NodeExecutionResult.Failed(ex);
                    
                    if (attempt <= maxRetries && ShouldRetryException(ex))
                    {
                        await Task.Delay(delay, cancellationToken);
                        delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            _logger.LogError("Node {NodeId} failed after {MaxAttempts} attempts", context.NodeId, maxRetries + 1);
            return lastResult ?? NodeExecutionResult.Failed("Maximum retry attempts exceeded");
        }

        private async Task<NodeExecutionResult> ExecutePhaseWithMetricsAsync(
            string phaseName,
            NodeExecutionContext context,
            Func<Task<NodeExecutionResult>> phaseFunc)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Starting {Phase} phase for node {NodeId}", phaseName, context.NodeId);
                
                var result = await phaseFunc();
                
                stopwatch.Stop();
                _metrics.RecordLifecyclePhaseDuration(context.NodeType, phaseName, stopwatch.Elapsed);
                
                _logger.LogDebug("Completed {Phase} phase for node {NodeId} in {Duration}ms", 
                    phaseName, context.NodeId, stopwatch.ElapsedMilliseconds);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _metrics.RecordLifecyclePhaseDuration(context.NodeType, phaseName, stopwatch.Elapsed);
                
                _logger.LogError(ex, "{Phase} phase failed for node {NodeId} after {Duration}ms", 
                    phaseName, context.NodeId, stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }

        private async Task ExecuteFinalizationPhaseAsync(
            string phaseName,
            NodeExecutionContext context,
            Func<Task> phaseFunc)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogDebug("Starting {Phase} phase for node {NodeId}", phaseName, context.NodeId);
                
                await phaseFunc();
                
                stopwatch.Stop();
                _metrics.RecordLifecyclePhaseDuration(context.NodeType, phaseName, stopwatch.Elapsed);
                
                _logger.LogDebug("Completed {Phase} phase for node {NodeId} in {Duration}ms", 
                    phaseName, context.NodeId, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _metrics.RecordLifecyclePhaseDuration(context.NodeType, phaseName, stopwatch.Elapsed);
                
                _logger.LogError(ex, "{Phase} phase failed for node {NodeId} after {Duration}ms", 
                    phaseName, context.NodeId, stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }

        private static bool ShouldRetry(NodeExecutionResult result)
        {
            // Retry logic based on the type of failure
            return result.Status switch
            {
                NodeExecutionStatus.Failed when result.Exception != null => ShouldRetryException(result.Exception),
                NodeExecutionStatus.TimedOut => true,
                _ => false
            };
        }

        private static bool ShouldRetryException(Exception exception)
        {
            // Define which exceptions are worth retrying
            return exception switch
            {
                TimeoutException => true,
                TaskCanceledException tce when !tce.CancellationToken.IsCancellationRequested => true,
                System.Net.Http.HttpRequestException => true,
                System.Net.Sockets.SocketException => true,
                System.Data.Common.DbException => true,
                _ when exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) => true,
                _ when exception.Message.Contains("connection", StringComparison.OrdinalIgnoreCase) => true,
                _ => false
            };
        }
    }

    /// <summary>
    /// Interface for the node execution engine
    /// </summary>
    public interface INodeExecutionEngine
    {
        /// <summary>
        /// Execute a node through the complete 4-phase lifecycle
        /// </summary>
        Task<NodeExecutionResult> ExecuteNodeAsync(
            NodeExecutionContext context,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute a node with retry logic
        /// </summary>
        Task<NodeExecutionResult> ExecuteNodeWithRetryAsync(
            NodeExecutionContext context,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            CancellationToken cancellationToken = default);
    }
}
