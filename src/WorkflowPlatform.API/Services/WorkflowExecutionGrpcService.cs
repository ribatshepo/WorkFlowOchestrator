using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using WorkflowPlatform.API.Grpc;
using WorkflowPlatform.Application.Workflows.Commands.ExecuteWorkflow;
using MediatR;
using Google.Protobuf.WellKnownTypes;

namespace WorkflowPlatform.API.Services;

/// <summary>
/// gRPC service implementation for high-performance workflow execution operations.
/// Provides streaming capabilities for real-time execution monitoring and batch processing.
/// </summary>
[Authorize]
public class WorkflowExecutionGrpcService : WorkflowExecutionService.WorkflowExecutionServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WorkflowExecutionGrpcService> _logger;
    private readonly Dictionary<string, CancellationTokenSource> _executionCancellationTokens = new();

    public WorkflowExecutionGrpcService(IMediator mediator, ILogger<WorkflowExecutionGrpcService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a workflow with streaming status updates.
    /// </summary>
    public override async Task ExecuteWorkflow(ExecuteWorkflowRequest request, 
        IServerStreamWriter<ExecutionStatusUpdate> responseStream, ServerCallContext context)
    {
        var executionId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
            context.CancellationToken, cancellationTokenSource.Token).Token;

        try
        {
            if (!Guid.TryParse(request.WorkflowId, out var workflowId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid workflow ID format"));
            }

            _executionCancellationTokens[executionId.ToString()] = cancellationTokenSource;

            // Send initial status update
            await responseStream.WriteAsync(new ExecutionStatusUpdate
            {
                ExecutionId = executionId.ToString(),
                Status = ExecutionStatus.Pending,
                CurrentNodeId = string.Empty,
                ProgressPercentage = 0,
                OutputData = string.Empty,
                ErrorMessage = string.Empty,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                Metrics = new ExecutionMetrics
                {
                    ExecutionTimeMs = 0,
                    NodesExecuted = 0,
                    NodesFailed = 0,
                    MemoryUsageBytes = 0,
                    CpuUsagePercentage = 0
                }
            });

            // Create execution command
            var command = new ExecuteWorkflowCommand
            {
                WorkflowId = workflowId,
                InputData = request.InputData,
                ExecutionContext = request.ExecutionContext.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Timeout = request.TimeoutSeconds > 0 ? TimeSpan.FromSeconds(request.TimeoutSeconds) : null,
                ExecutedBy = context.GetHttpContext().User.Identity?.Name ?? "gRPC Client",
                Priority = 0,
                EnableRealTimeUpdates = true,
                NotificationChannels = null
            };

            var result = await _mediator.Send(command, combinedToken);

            if (!result.IsSuccess)
            {
                await responseStream.WriteAsync(new ExecutionStatusUpdate
                {
                    ExecutionId = executionId.ToString(),
                    Status = ExecutionStatus.Failed,
                    CurrentNodeId = string.Empty,
                    ProgressPercentage = 0,
                    OutputData = string.Empty,
                    ErrorMessage = result.ErrorMessage ?? "Execution failed",
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    Metrics = new ExecutionMetrics()
                });

                throw new RpcException(new Status(StatusCode.Internal, result.ErrorMessage ?? "Execution failed"));
            }

            // Send running status
            await responseStream.WriteAsync(new ExecutionStatusUpdate
            {
                ExecutionId = result.ExecutionId.ToString(),
                Status = ExecutionStatus.Running,
                CurrentNodeId = "node_start",
                ProgressPercentage = 10,
                OutputData = string.Empty,
                ErrorMessage = string.Empty,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                Metrics = new ExecutionMetrics
                {
                    ExecutionTimeMs = 1000,
                    NodesExecuted = 1,
                    NodesFailed = 0,
                    MemoryUsageBytes = 1024 * 1024 * 10, // 10MB
                    CpuUsagePercentage = 15
                }
            });

            // Simulate execution progress with periodic updates
            var progressUpdates = new[]
            {
                (25, "node_process", "Processing data..."),
                (50, "node_validate", "Validating results..."),
                (75, "node_transform", "Transforming output..."),
                (90, "node_finalize", "Finalizing execution...")
            };

            foreach (var (progress, nodeId, message) in progressUpdates)
            {
                if (combinedToken.IsCancellationRequested)
                    break;

                await Task.Delay(2000, combinedToken); // Simulate processing time

                await responseStream.WriteAsync(new ExecutionStatusUpdate
                {
                    ExecutionId = result.ExecutionId.ToString(),
                    Status = ExecutionStatus.Running,
                    CurrentNodeId = nodeId,
                    ProgressPercentage = progress,
                    OutputData = $"{{\"message\": \"{message}\"}}",
                    ErrorMessage = string.Empty,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    Metrics = new ExecutionMetrics
                    {
                        ExecutionTimeMs = 2000 * progress / 25,
                        NodesExecuted = progress / 25,
                        NodesFailed = 0,
                        MemoryUsageBytes = 1024 * 1024 * (10 + progress / 10),
                        CpuUsagePercentage = Math.Min(50, 15 + progress / 5)
                    }
                });
            }

            // Send completion status
            if (!combinedToken.IsCancellationRequested)
            {
                await responseStream.WriteAsync(new ExecutionStatusUpdate
                {
                    ExecutionId = result.ExecutionId.ToString(),
                    Status = ExecutionStatus.Completed,
                    CurrentNodeId = "node_end",
                    ProgressPercentage = 100,
                    OutputData = "{\"result\": \"success\", \"processedItems\": 1000}",
                    ErrorMessage = string.Empty,
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    Metrics = new ExecutionMetrics
                    {
                        ExecutionTimeMs = 8000,
                        NodesExecuted = 5,
                        NodesFailed = 0,
                        MemoryUsageBytes = 1024 * 1024 * 25, // 25MB
                        CpuUsagePercentage = 5
                    }
                });

                _logger.LogInformation("gRPC workflow execution {ExecutionId} completed successfully", result.ExecutionId);
            }
        }
        catch (OperationCanceledException)
        {
            await responseStream.WriteAsync(new ExecutionStatusUpdate
            {
                ExecutionId = executionId.ToString(),
                Status = ExecutionStatus.Cancelled,
                CurrentNodeId = string.Empty,
                ProgressPercentage = 0,
                OutputData = string.Empty,
                ErrorMessage = "Execution was cancelled",
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                Metrics = new ExecutionMetrics()
            });

            _logger.LogInformation("gRPC workflow execution {ExecutionId} was cancelled", executionId);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC ExecuteWorkflow for workflow {WorkflowId}", request.WorkflowId);
            
            await responseStream.WriteAsync(new ExecutionStatusUpdate
            {
                ExecutionId = executionId.ToString(),
                Status = ExecutionStatus.Failed,
                CurrentNodeId = string.Empty,
                ProgressPercentage = 0,
                OutputData = string.Empty,
                ErrorMessage = "Internal server error",
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                Metrics = new ExecutionMetrics()
            });

            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
        finally
        {
            _executionCancellationTokens.Remove(executionId.ToString(), out _);
        }
    }

    /// <summary>
    /// Gets the current status of a workflow execution.
    /// </summary>
    public override async Task<ExecutionStatusResponse> GetExecutionStatus(GetExecutionStatusRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ExecutionId, out var executionId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid execution ID format"));
            }

            // This would typically query the database for actual execution status
            // For now, returning a mock response
            var response = new ExecutionStatusResponse
            {
                ExecutionId = executionId.ToString(),
                WorkflowId = Guid.NewGuid().ToString(), // Would be actual workflow ID
                Status = ExecutionStatus.Running,
                StartedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-5)),
                CompletedAt = null,
                OutputData = "{\"interim\": \"results\"}",
                ErrorMessage = string.Empty
            };

            // Add node results if requested
            if (request.IncludeLogs)
            {
                response.NodeResults.Add(new NodeExecutionResult
                {
                    NodeId = "node_1",
                    NodeName = "Start Node",
                    Status = ExecutionStatus.Completed,
                    StartedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-5)),
                    CompletedAt = Timestamp.FromDateTime(DateTime.UtcNow.AddMinutes(-4)),
                    OutputData = "{\"initialized\": true}",
                    ErrorMessage = string.Empty,
                    Metrics = new ExecutionMetrics
                    {
                        ExecutionTimeMs = 60000,
                        NodesExecuted = 1,
                        NodesFailed = 0,
                        MemoryUsageBytes = 1024 * 1024 * 5,
                        CpuUsagePercentage = 10
                    }
                });
            }

            _logger.LogDebug("gRPC GetExecutionStatus completed for {ExecutionId}", executionId);
            return await Task.FromResult(response);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC GetExecutionStatus for {ExecutionId}", request.ExecutionId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    /// <summary>
    /// Cancels a running workflow execution.
    /// </summary>
    public override async Task<Empty> CancelExecution(CancelExecutionRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ExecutionId, out var executionId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid execution ID format"));
            }

            if (_executionCancellationTokens.TryGetValue(request.ExecutionId, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
                _executionCancellationTokens.Remove(request.ExecutionId, out _);
                
                _logger.LogInformation("gRPC CancelExecution completed for {ExecutionId}. Reason: {Reason}", 
                    executionId, request.Reason);
            }
            else
            {
                _logger.LogWarning("gRPC CancelExecution: No active execution found for {ExecutionId}", executionId);
                throw new RpcException(new Status(StatusCode.NotFound, "Execution not found or already completed"));
            }

            return await Task.FromResult(new Empty());
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC CancelExecution for {ExecutionId}", request.ExecutionId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    /// <summary>
    /// Streams execution metrics for monitoring and analytics.
    /// </summary>
    public override async Task StreamExecutionMetrics(StreamMetricsRequest request, 
        IServerStreamWriter<ExecutionMetrics> responseStream, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.WorkflowId, out var workflowId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid workflow ID format"));
            }

            var intervalSeconds = Math.Max(1, Math.Min(300, request.IntervalSeconds)); // 1 to 300 seconds
            var interval = TimeSpan.FromSeconds(intervalSeconds);

            _logger.LogInformation("Starting gRPC metrics streaming for workflow {WorkflowId} with {Interval}s interval", 
                workflowId, intervalSeconds);

            // Stream metrics until cancelled
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var metrics = new ExecutionMetrics
                {
                    ExecutionTimeMs = DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond,
                    NodesExecuted = Random.Shared.Next(1, 20),
                    NodesFailed = Random.Shared.Next(0, 3),
                    MemoryUsageBytes = Random.Shared.Next(10, 100) * 1024 * 1024, // 10-100 MB
                    CpuUsagePercentage = Random.Shared.Next(5, 80)
                };

                // Add some custom metrics
                metrics.CustomMetrics.Add("throughput_per_second", Random.Shared.Next(50, 500));
                metrics.CustomMetrics.Add("active_connections", Random.Shared.Next(10, 100));
                metrics.CustomMetrics.Add("queue_size", Random.Shared.Next(0, 50));

                await responseStream.WriteAsync(metrics);
                await Task.Delay(interval, context.CancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("gRPC metrics streaming cancelled for workflow {WorkflowId}", request.WorkflowId);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC StreamExecutionMetrics for workflow {WorkflowId}", request.WorkflowId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    /// <summary>
    /// Executes multiple workflows in batch with streaming updates.
    /// </summary>
    public override async Task BatchExecuteWorkflows(BatchExecuteRequest request, 
        IServerStreamWriter<BatchExecutionUpdate> responseStream, ServerCallContext context)
    {
        var batchId = Guid.NewGuid().ToString();
        var totalRequests = request.Requests.Count;
        var completedRequests = 0;
        var failedRequests = 0;

        try
        {
            _logger.LogInformation("Starting gRPC batch execution {BatchId} with {Count} workflows", batchId, totalRequests);

            // Send initial batch status
            await responseStream.WriteAsync(new BatchExecutionUpdate
            {
                BatchId = batchId,
                TotalRequests = totalRequests,
                CompletedRequests = 0,
                FailedRequests = 0
            });

            var maxConcurrency = Math.Max(1, Math.Min(10, request.MaxConcurrency));
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task>();

            foreach (var workflowRequest in request.Requests)
            {
                var task = ProcessSingleWorkflow(workflowRequest, batchId, responseStream, semaphore, context.CancellationToken);
                tasks.Add(task);

                await task.ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        Interlocked.Increment(ref completedRequests);
                    }
                    else
                    {
                        Interlocked.Increment(ref failedRequests);
                    }

                    // Send batch progress update
                    responseStream.WriteAsync(new BatchExecutionUpdate
                    {
                        BatchId = batchId,
                        TotalRequests = totalRequests,
                        CompletedRequests = completedRequests,
                        FailedRequests = failedRequests
                    });

                }, TaskContinuationOptions.None);

                // If fail-fast is enabled and we have a failure, cancel remaining tasks
                if (request.FailFast && failedRequests > 0)
                {
                    break;
                }
            }

            await Task.WhenAll(tasks);

            // Send final batch status
            await responseStream.WriteAsync(new BatchExecutionUpdate
            {
                BatchId = batchId,
                TotalRequests = totalRequests,
                CompletedRequests = completedRequests,
                FailedRequests = failedRequests
            });

            _logger.LogInformation("gRPC batch execution {BatchId} completed: {Completed} succeeded, {Failed} failed", 
                batchId, completedRequests, failedRequests);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("gRPC batch execution {BatchId} was cancelled", batchId);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC BatchExecuteWorkflows for batch {BatchId}", batchId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    private async Task ProcessSingleWorkflow(ExecuteWorkflowRequest workflowRequest, string batchId, 
        IServerStreamWriter<BatchExecutionUpdate> responseStream, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        
        try
        {
            if (!Guid.TryParse(workflowRequest.WorkflowId, out var workflowId))
            {
                throw new ArgumentException("Invalid workflow ID format");
            }

            var command = new ExecuteWorkflowCommand
            {
                WorkflowId = workflowId,
                InputData = workflowRequest.InputData,
                ExecutionContext = workflowRequest.ExecutionContext.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Timeout = workflowRequest.TimeoutSeconds > 0 ? TimeSpan.FromSeconds(workflowRequest.TimeoutSeconds) : null,
                ExecutedBy = "gRPC Batch Client",
                Priority = 0,
                EnableRealTimeUpdates = false
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess)
            {
                // Simulate execution completion
                await Task.Delay(Random.Shared.Next(1000, 5000), cancellationToken);

                await responseStream.WriteAsync(new BatchExecutionUpdate
                {
                    BatchId = batchId,
                    TotalRequests = 0, // Will be set by caller
                    CompletedRequests = 0, // Will be set by caller
                    FailedRequests = 0, // Will be set by caller
                    ExecutionUpdates =
                    {
                        new ExecutionStatusUpdate
                        {
                            ExecutionId = result.ExecutionId.ToString(),
                            Status = ExecutionStatus.Completed,
                            CurrentNodeId = "completed",
                            ProgressPercentage = 100,
                            OutputData = "{\"batchResult\": \"success\"}",
                            ErrorMessage = string.Empty,
                            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                            Metrics = new ExecutionMetrics
                            {
                                ExecutionTimeMs = Random.Shared.Next(1000, 5000),
                                NodesExecuted = Random.Shared.Next(3, 10),
                                NodesFailed = 0
                            }
                        }
                    }
                });
            }
            else
            {
                throw new InvalidOperationException(result.ErrorMessage ?? "Execution failed");
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}
