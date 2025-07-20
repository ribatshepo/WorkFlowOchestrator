using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using WorkflowPlatform.API.Grpc;
using WorkflowPlatform.Application.Workflows.Commands.CreateWorkflow;
using WorkflowPlatform.Application.Workflows.Commands.DeleteWorkflow;
using WorkflowPlatform.Application.Workflows.Commands.UpdateWorkflow;
using WorkflowPlatform.Application.Workflows.Commands.ValidateWorkflow;
using WorkflowPlatform.Application.Workflows.Queries.GetWorkflow;
using WorkflowPlatform.Application.Workflows.Queries.ListWorkflows;
using MediatR;
using Google.Protobuf.WellKnownTypes;

namespace WorkflowPlatform.API.Services;

/// <summary>
/// gRPC service implementation for high-performance workflow operations.
/// Provides streaming capabilities and optimized binary communication.
/// </summary>
[Authorize]
public class WorkflowGrpcService : WorkflowService.WorkflowServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WorkflowGrpcService> _logger;

    public WorkflowGrpcService(IMediator mediator, ILogger<WorkflowGrpcService> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a workflow by ID with optional detailed information.
    /// </summary>
    public override async Task<WorkflowResponse> GetWorkflow(GetWorkflowRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.WorkflowId, out var workflowId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid workflow ID format"));
            }

            var query = new GetWorkflowQuery
            {
                Id = workflowId,
                IncludeDefinition = request.IncludeDefinition,
                IncludeExecutionHistory = false,
                IncludeMetrics = false
            };

            var result = await _mediator.Send(query, context.CancellationToken);

            if (!result.IsSuccess)
            {
                var status = result.ErrorMessage?.Contains("not found") == true 
                    ? StatusCode.NotFound 
                    : StatusCode.Internal;
                throw new RpcException(new Status(status, result.ErrorMessage ?? "Unknown error"));
            }

            var response = new WorkflowResponse
            {
                WorkflowId = result.WorkflowId.ToString(),
                Name = result.Name,
                Description = result.Description,
                Status = MapToGrpcWorkflowStatus(result.Status),
                CreatedAt = Timestamp.FromDateTime(result.CreatedAt.ToUniversalTime()),
                UpdatedAt = Timestamp.FromDateTime(result.UpdatedAt.ToUniversalTime())
            };

            // Add metadata
            foreach (var metadata in result.Metadata)
            {
                response.Metadata.Add(metadata.Key, metadata.Value);
            }

            // Add definition if requested
            if (result.Definition != null && request.IncludeDefinition)
            {
                response.Definition = MapToGrpcDefinition(result.Definition);
            }

            _logger.LogDebug("gRPC GetWorkflow completed for {WorkflowId}", workflowId);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC GetWorkflow for {WorkflowId}", request.WorkflowId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    /// <summary>
    /// Creates a new workflow definition.
    /// </summary>
    public override async Task<WorkflowPlatform.API.Grpc.CreateWorkflowResponse> CreateWorkflow(WorkflowPlatform.API.Grpc.CreateWorkflowRequest request, ServerCallContext context)
    {
        try
        {
            var command = new CreateWorkflowCommand
            {
                Name = request.Name,
                Description = request.Description,
                Category = "General", // Default category for gRPC
                Priority = WorkflowPlatform.Domain.Common.Enumerations.WorkflowPriority.Normal,
                DefaultTimeout = null,
                MaxConcurrentExecutions = 10,
                IsTemplate = false,
                GlobalVariables = request.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
            };

            var result = await _mediator.Send(command, context.CancellationToken);

            var response = new WorkflowPlatform.API.Grpc.CreateWorkflowResponse
            {
                WorkflowId = result.WorkflowId.ToString(),
                Success = result.IsSuccess
            };

            if (!result.IsSuccess)
            {
                response.ErrorMessage = result.ErrorMessage ?? string.Join(", ", result.ValidationErrors);
            }

            _logger.LogInformation("gRPC CreateWorkflow completed: {Success} for '{Name}'", result.IsSuccess, request.Name);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC CreateWorkflow for '{Name}'", request.Name);
            return new WorkflowPlatform.API.Grpc.CreateWorkflowResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    /// <summary>
    /// Updates an existing workflow definition.
    /// </summary>
    public override async Task<WorkflowResponse> UpdateWorkflow(UpdateWorkflowRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.WorkflowId, out var workflowId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid workflow ID format"));
            }

            var command = new UpdateWorkflowCommand
            {
                WorkflowId = workflowId,
                Name = request.Name,
                Description = request.Description,
                Category = "General",
                Definition = request.Definition != null ? MapFromGrpcDefinition(request.Definition) : null,
                GlobalVariables = request.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                Metadata = request.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                LastModifiedBy = context.GetHttpContext().User.Identity?.Name ?? "gRPC Client",
                IncrementVersion = true
            };

            var result = await _mediator.Send(command, context.CancellationToken);

            if (!result.IsSuccess)
            {
                var status = result.ErrorMessage?.Contains("not found") == true 
                    ? StatusCode.NotFound 
                    : StatusCode.InvalidArgument;
                throw new RpcException(new Status(status, result.ErrorMessage ?? "Update failed"));
            }

            // Get the updated workflow to return full details
            var getQuery = new GetWorkflowQuery { Id = workflowId, IncludeDefinition = true };
            var getResult = await _mediator.Send(getQuery, context.CancellationToken);

            if (!getResult.IsSuccess)
            {
                throw new RpcException(new Status(StatusCode.Internal, "Failed to retrieve updated workflow"));
            }

            var response = new WorkflowResponse
            {
                WorkflowId = getResult.WorkflowId.ToString(),
                Name = getResult.Name,
                Description = getResult.Description,
                Status = MapToGrpcWorkflowStatus(getResult.Status),
                CreatedAt = Timestamp.FromDateTime(getResult.CreatedAt.ToUniversalTime()),
                UpdatedAt = Timestamp.FromDateTime(getResult.UpdatedAt.ToUniversalTime())
            };

            foreach (var metadata in getResult.Metadata)
            {
                response.Metadata.Add(metadata.Key, metadata.Value);
            }

            if (getResult.Definition != null)
            {
                response.Definition = MapToGrpcDefinition(getResult.Definition);
            }

            _logger.LogInformation("gRPC UpdateWorkflow completed for {WorkflowId} version {Version}", 
                workflowId, result.NewVersion);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC UpdateWorkflow for {WorkflowId}", request.WorkflowId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    /// <summary>
    /// Deletes a workflow and optionally related data.
    /// </summary>
    public override async Task<Empty> DeleteWorkflow(DeleteWorkflowRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.WorkflowId, out var workflowId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid workflow ID format"));
            }

            var command = new DeleteWorkflowCommand
            {
                WorkflowId = workflowId,
                ForceDelete = request.ForceDelete,
                DeleteExecutionHistory = false,
                Reason = "Deleted via gRPC",
                DeletedBy = context.GetHttpContext().User.Identity?.Name ?? "gRPC Client"
            };

            var result = await _mediator.Send(command, context.CancellationToken);

            if (!result.IsSuccess)
            {
                var status = result.ErrorMessage?.Contains("not found") == true 
                    ? StatusCode.NotFound 
                    : StatusCode.FailedPrecondition;
                throw new RpcException(new Status(status, result.ErrorMessage ?? "Delete failed"));
            }

            _logger.LogInformation("gRPC DeleteWorkflow completed for {WorkflowId}", workflowId);
            return new Empty();
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC DeleteWorkflow for {WorkflowId}", request.WorkflowId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    /// <summary>
    /// Lists workflows with pagination and filtering.
    /// </summary>
    public override async Task<WorkflowPlatform.API.Grpc.ListWorkflowsResponse> ListWorkflows(ListWorkflowsRequest request, ServerCallContext context)
    {
        try
        {
            var query = new ListWorkflowsQuery
            {
                PageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100),
                Page = 1, // gRPC uses token-based pagination
                SearchTerm = request.Filter?.NameContains,
                Category = null,
                SortBy = request.OrderBy ?? "UpdatedAt",
                SortDescending = true,
                IncludeDefinition = false
            };

            var result = await _mediator.Send(query, context.CancellationToken);

            // PaginatedResult doesn't have IsSuccess, it's always successful if no exception
            var response = new WorkflowPlatform.API.Grpc.ListWorkflowsResponse
            {
                TotalCount = result.TotalCount,
                NextPageToken = result.HasNextPage ? $"page_{result.Page + 1}" : string.Empty
            };

            foreach (var workflow in result.Items)
            {
                var workflowResponse = new WorkflowResponse
                {
                    WorkflowId = workflow.Id.ToString(),
                    Name = workflow.Name,
                    Description = workflow.Description ?? string.Empty,
                    Status = MapToGrpcWorkflowStatus(workflow.IsActive ? "Active" : "Inactive"),
                    CreatedAt = Timestamp.FromDateTime(workflow.CreatedAt.ToUniversalTime()),
                    UpdatedAt = Timestamp.FromDateTime(workflow.UpdatedAt.ToUniversalTime())
                };

                if (workflow.Tags != null)
                {
                    foreach (var tag in workflow.Tags)
                    {
                        workflowResponse.Metadata.Add("tag", tag);
                    }
                }

                response.Workflows.Add(workflowResponse);
            }

            _logger.LogDebug("gRPC ListWorkflows returned {Count} workflows", result.Items.Count);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC ListWorkflows");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    /// <summary>
    /// Validates a workflow definition without saving it.
    /// </summary>
    public override async Task<ValidateWorkflowResponse> ValidateWorkflow(ValidateWorkflowRequest request, ServerCallContext context)
    {
        try
        {
            var command = new ValidateWorkflowCommand
            {
                Definition = MapFromGrpcDefinition(request.Definition),
                StrictValidation = request.StrictValidation,
                CheckNodeCompatibility = true,
                ValidateConnections = true,
                CheckCircularDependencies = true,
                TestInputData = null
            };

            var result = await _mediator.Send(command, context.CancellationToken);

            if (!result.IsSuccess)
            {
                throw new RpcException(new Status(StatusCode.Internal, result.ErrorMessage ?? "Validation failed"));
            }

            var response = new ValidateWorkflowResponse
            {
                IsValid = result.IsValid
            };

            foreach (var error in result.Errors)
            {
                response.Errors.Add(new WorkflowPlatform.API.Grpc.ValidationError
                {
                    Field = error.PropertyPath ?? error.NodeId ?? "Unknown",
                    Message = error.Message,
                    ErrorCode = error.Code
                });

                if (!string.IsNullOrEmpty(error.NodeId))
                {
                    response.Errors[response.Errors.Count - 1].Context.Add("nodeId", error.NodeId);
                }
            }

            foreach (var warning in result.Warnings)
            {
                response.Warnings.Add(warning.Message);
            }

            _logger.LogDebug("gRPC ValidateWorkflow completed: {IsValid} with {ErrorCount} errors", 
                result.IsValid, result.Errors.Count);
            return response;
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in gRPC ValidateWorkflow");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    // Helper methods for mapping between DTOs and gRPC messages
    private static WorkflowStatus MapToGrpcWorkflowStatus(string status)
    {
        return status.ToLower() switch
        {
            "draft" => WorkflowStatus.Draft,
            "active" => WorkflowStatus.Active,
            "inactive" => WorkflowStatus.Inactive,
            "archived" => WorkflowStatus.Archived,
            "error" => WorkflowStatus.Error,
            _ => WorkflowStatus.Unspecified
        };
    }

    private static WorkflowDefinition MapToGrpcDefinition(WorkflowDefinitionDto definition)
    {
        var grpcDefinition = new WorkflowDefinition();

        foreach (var node in definition.Nodes)
        {
            var grpcNode = new WorkflowNode
            {
                NodeId = node.NodeId,
                NodeType = node.NodeType,
                Name = node.Name,
                Description = node.Description,
                Position = new Position { X = node.Position.X, Y = node.Position.Y },
                Configuration = new NodeConfiguration
                {
                    ConfigJson = System.Text.Json.JsonSerializer.Serialize(node.Configuration),
                    TimeoutSeconds = 300,
                    RetryCount = 0,
                    ParallelExecution = false
                }
            };

            foreach (var metadata in node.Metadata)
            {
                grpcNode.Metadata.Add(metadata.Key, metadata.Value);
            }

            grpcDefinition.Nodes.Add(grpcNode);
        }

        foreach (var edge in definition.Edges)
        {
            var grpcEdge = new WorkflowEdge
            {
                EdgeId = edge.EdgeId,
                SourceNodeId = edge.SourceNodeId,
                TargetNodeId = edge.TargetNodeId,
                SourceHandle = edge.SourceHandle,
                TargetHandle = edge.TargetHandle
            };

            if (edge.Condition != null)
            {
                grpcEdge.Condition = new EdgeCondition
                {
                    ConditionType = edge.Condition.ConditionType,
                    Expression = edge.Condition.Expression
                };

                foreach (var param in edge.Condition.Parameters)
                {
                    grpcEdge.Condition.Parameters.Add(param.Key, param.Value);
                }
            }

            grpcDefinition.Edges.Add(grpcEdge);
        }

        grpcDefinition.Settings = new WorkflowSettings
        {
            MaxExecutionTimeSeconds = (int)definition.Settings.MaxExecutionTime.TotalSeconds,
            MaxParallelNodes = definition.Settings.MaxParallelNodes,
            EnableLogging = definition.Settings.EnableLogging,
            LogLevel = definition.Settings.LogLevel
        };

        foreach (var envVar in definition.Settings.EnvironmentVariables)
        {
            grpcDefinition.Settings.EnvironmentVariables.Add(envVar.Key, envVar.Value);
        }

        return grpcDefinition;
    }

    private static WorkflowDefinitionDto MapFromGrpcDefinition(WorkflowDefinition definition)
    {
        var nodes = definition.Nodes.Select(node => new WorkflowNodeDto
        {
            NodeId = node.NodeId,
            NodeType = node.NodeType,
            Name = node.Name,
            Description = node.Description,
            Configuration = string.IsNullOrEmpty(node.Configuration.ConfigJson) 
                ? new Dictionary<string, object>()
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(node.Configuration.ConfigJson) ?? new Dictionary<string, object>(),
            Position = new PositionDto { X = node.Position.X, Y = node.Position.Y },
            Metadata = node.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        }).ToList();

        var edges = definition.Edges.Select(edge => new WorkflowEdgeDto
        {
            EdgeId = edge.EdgeId,
            SourceNodeId = edge.SourceNodeId,
            TargetNodeId = edge.TargetNodeId,
            SourceHandle = edge.SourceHandle,
            TargetHandle = edge.TargetHandle,
            Condition = edge.Condition != null ? new EdgeConditionDto
            {
                ConditionType = edge.Condition.ConditionType,
                Expression = edge.Condition.Expression,
                Parameters = edge.Condition.Parameters.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            } : null
        }).ToList();

        var settings = new WorkflowSettingsDto
        {
            MaxExecutionTime = TimeSpan.FromSeconds(definition.Settings.MaxExecutionTimeSeconds),
            MaxParallelNodes = definition.Settings.MaxParallelNodes,
            EnableLogging = definition.Settings.EnableLogging,
            LogLevel = definition.Settings.LogLevel,
            EnvironmentVariables = definition.Settings.EnvironmentVariables.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };

        return new WorkflowDefinitionDto
        {
            Nodes = nodes,
            Edges = edges,
            Settings = settings
        };
    }
}
