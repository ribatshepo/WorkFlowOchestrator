using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Application.Workflows.NodeExecution.Configurations;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application.Workflows.NodeExecution.Strategies
{
    /// <summary>
    /// Database Query node execution strategy
    /// </summary>
    public class DatabaseQueryNodeStrategy : BaseNodeExecutionStrategy
    {
        private readonly IRetryPolicy _retryPolicy;
        private readonly ICircuitBreaker _circuitBreaker;

        public DatabaseQueryNodeStrategy(
            IRetryPolicy retryPolicy,
            ICircuitBreaker circuitBreaker,
            ILogger<DatabaseQueryNodeStrategy> logger,
            IMetricsCollector metrics)
            : base(logger, metrics)
        {
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
            _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
        }

        public override string NodeType => "DatabaseQuery";

        public override async Task<NodeExecutionResult> ExecuteAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("Starting database query execution for node {NodeId}", context.NodeId);

                var config = GetRequiredConfiguration<DatabaseQueryNodeConfiguration>(context, "DatabaseConfig");
                var timeoutToken = CreateTimeoutToken(context, config.Timeout);

                // Execute database query with retry and circuit breaker
                var result = await _retryPolicy.ExecuteAsync(async (ct) =>
                {
                    return await _circuitBreaker.ExecuteAsync(async (innerCt) =>
                    {
                        return await ExecuteDatabaseQueryAsync(config, innerCt);
                    }, ct);
                }, timeoutToken);

                stopwatch.Stop();
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.Completed, stopwatch.Elapsed);

                _logger.LogInformation("Database query completed successfully for node {NodeId} in {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);

                return NodeExecutionResult.Success(result, stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                _logger.LogWarning("Database query cancelled for node {NodeId} after {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.Cancelled, stopwatch.Elapsed);
                return NodeExecutionResult.Cancelled("Database query was cancelled", stopwatch.Elapsed);
            }
            catch (TimeoutException)
            {
                stopwatch.Stop();
                _logger.LogWarning("Database query timed out for node {NodeId} after {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.TimedOut, stopwatch.Elapsed);
                return NodeExecutionResult.Timeout(TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds), stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database query failed for node {NodeId} after {Duration}ms", 
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
                // Validate database configuration exists
                if (!HasRequiredConfiguration(context, "DatabaseConfig"))
                {
                    result.AddError("DatabaseConfig is required");
                    return result;
                }

                var config = GetRequiredConfiguration<DatabaseQueryNodeConfiguration>(context, "DatabaseConfig");

                // Validate connection string
                if (string.IsNullOrWhiteSpace(config.ConnectionString))
                {
                    result.AddError("ConnectionString is required");
                }

                // Validate query
                if (string.IsNullOrWhiteSpace(config.Query))
                {
                    result.AddError("Query is required");
                }

                // Validate timeout
                if (config.Timeout <= TimeSpan.Zero || config.Timeout > TimeSpan.FromMinutes(30))
                {
                    result.AddError("Timeout must be between 1 second and 30 minutes");
                }

                // Validate database provider
                var supportedProviders = new[] { "PostgreSQL", "SQLServer", "MySQL", "SQLite" };
                if (!Array.Exists(supportedProviders, provider => 
                    string.Equals(provider, config.DatabaseProvider, StringComparison.OrdinalIgnoreCase)))
                {
                    result.AddError($"Database provider '{config.DatabaseProvider}' is not supported");
                }

                // Security validation - check for potentially dangerous operations
                var query = config.Query.ToUpperInvariant();
                if (!config.IsReadOnly && (query.Contains("DROP") || query.Contains("TRUNCATE") || query.Contains("ALTER")))
                {
                    result.AddWarning("Query contains potentially dangerous operations");
                }

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating database query inputs for node {NodeId}", context.NodeId);
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
                var config = GetRequiredConfiguration<DatabaseQueryNodeConfiguration>(context, "DatabaseConfig");

                // Test connection validity
                using var connection = CreateDatabaseConnection(config);
                await connection.OpenAsync(cancellationToken);
                
                context.SetProperty("ConnectionTested", true);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up database query context for node {NodeId}", context.NodeId);
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
                // Database result is already in the correct format
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming database result for node {NodeId}", context.NodeId);
                return NodeExecutionResult.Failed($"Output transformation failed: {ex.Message}");
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
                    validation.AddWarning("Database query returned no results");
                }

                return await Task.FromResult(validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating database result for node {NodeId}", context.NodeId);
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
                // Database connections are disposed automatically
                _logger.LogDebug("Cleaned up database query resources for node {NodeId}", context.NodeId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up database query resources for node {NodeId}", context.NodeId);
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

                _logger.LogDebug("Persisted database query execution state for node {NodeId}", context.NodeId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error persisting database query state for node {NodeId}", context.NodeId);
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
                _logger.LogInformation("Database query node {NodeId} completed with status {Status}", 
                    context.NodeId, result.Status);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering completion events for node {NodeId}", context.NodeId);
            }
        }

        private async Task<object> ExecuteDatabaseQueryAsync(
            DatabaseQueryNodeConfiguration config, 
            CancellationToken cancellationToken)
        {
            using var connection = CreateDatabaseConnection(config);
            await connection.OpenAsync(cancellationToken);

            using var command = connection.CreateCommand();
            command.CommandText = config.Query;
            command.CommandTimeout = (int)config.Timeout.TotalSeconds;

            // Add parameters
            foreach (var parameter in config.Parameters)
            {
                var dbParameter = command.CreateParameter();
                dbParameter.ParameterName = parameter.Key;
                dbParameter.Value = parameter.Value ?? DBNull.Value;
                command.Parameters.Add(dbParameter);
            }

            // Execute based on query type
            if (IsSelectQuery(config.Query))
            {
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                return await ReadResultsAsync(reader, cancellationToken);
            }
            else
            {
                var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
                return new { RowsAffected = rowsAffected };
            }
        }

        private DbConnection CreateDatabaseConnection(DatabaseQueryNodeConfiguration config)
        {
            return config.DatabaseProvider.ToUpperInvariant() switch
            {
                "POSTGRESQL" => new Npgsql.NpgsqlConnection(config.ConnectionString),
                "SQLSERVER" => new Microsoft.Data.SqlClient.SqlConnection(config.ConnectionString),
                "MYSQL" => new MySqlConnector.MySqlConnection(config.ConnectionString),
                "SQLITE" => new Microsoft.Data.Sqlite.SqliteConnection(config.ConnectionString),
                _ => throw new NotSupportedException($"Database provider '{config.DatabaseProvider}' is not supported")
            };
        }

        private static bool IsSelectQuery(string query)
        {
            var trimmedQuery = query.Trim();
            return trimmedQuery.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
                   trimmedQuery.StartsWith("WITH", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<object> ReadResultsAsync(DbDataReader reader, CancellationToken cancellationToken)
        {
            var results = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var columnName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[columnName] = value!; // Suppressing null warning as we handle DBNull properly
                }
                results.Add(row);
            }

            return results;
        }
    }
}
