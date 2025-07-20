using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Application;
using WorkflowPlatform.Application.Workflows.NodeExecution;
using WorkflowPlatform.Application.Workflows.NodeExecution.Configurations;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Examples
{
    /// <summary>
    /// Example demonstrating how to use the Node Execution Engine
    /// </summary>
    public class NodeExecutionEngineExample
    {
        private readonly INodeExecutionEngine _executionEngine;
        private readonly ILogger<NodeExecutionEngineExample> _logger;

        public NodeExecutionEngineExample(
            INodeExecutionEngine executionEngine,
            ILogger<NodeExecutionEngineExample> logger)
        {
            _executionEngine = executionEngine;
            _logger = logger;
        }

        /// <summary>
        /// Example of executing an HTTP Request node
        /// </summary>
        public async Task<NodeExecutionResult> ExecuteHttpRequestNodeExample()
        {
            _logger.LogInformation("Starting HTTP Request node execution example");

            // Create HTTP Request configuration
            var httpConfig = new HttpRequestNodeConfiguration
            {
                Url = "https://jsonplaceholder.typicode.com/posts/1",
                Method = "GET",
                Timeout = TimeSpan.FromSeconds(30),
                Headers = new Dictionary<string, string>
                {
                    { "Accept", "application/json" },
                    { "User-Agent", "WorkflowPlatform/1.0" }
                }
            };

            // Create execution context
            var context = new NodeExecutionContext(
                nodeId: Guid.NewGuid(),
                nodeType: "HttpRequest",
                workflowId: Guid.NewGuid(),
                executionId: Guid.NewGuid(),
                inputData: null,
                configuration: new Dictionary<string, object>
                {
                    { "HttpConfig", httpConfig }
                });

            try
            {
                // Execute the node through all 4 phases
                var result = await _executionEngine.ExecuteNodeAsync(context, CancellationToken.None);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("HTTP Request executed successfully. Output: {Output}", 
                        result.OutputData?.ToString());
                }
                else
                {
                    _logger.LogError("HTTP Request failed: {Error}", result.ErrorMessage);
                }

                return result;
            }
            finally
            {
                // Context is automatically disposed by the engine
            }
        }

        /// <summary>
        /// Example of executing a Database Query node
        /// </summary>
        public async Task<NodeExecutionResult> ExecuteDatabaseQueryNodeExample()
        {
            _logger.LogInformation("Starting Database Query node execution example");

            // Create Database Query configuration
            var dbConfig = new DatabaseQueryNodeConfiguration
            {
                ConnectionString = "Host=localhost;Database=workflow_dev;Username=workflow_user;Password=secret",
                Query = "SELECT id, name, email FROM users WHERE active = @active LIMIT 10",
                Parameters = new Dictionary<string, object>
                {
                    { "@active", true }
                },
                Timeout = TimeSpan.FromSeconds(30),
                IsReadOnly = true,
                DatabaseProvider = "PostgreSQL"
            };

            var context = new NodeExecutionContext(
                nodeId: Guid.NewGuid(),
                nodeType: "DatabaseQuery",
                workflowId: Guid.NewGuid(),
                executionId: Guid.NewGuid(),
                inputData: null,
                configuration: new Dictionary<string, object>
                {
                    { "DatabaseConfig", dbConfig }
                });

            var result = await _executionEngine.ExecuteNodeAsync(context, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Database Query executed successfully. Retrieved {Count} rows", 
                    ((List<Dictionary<string, object>>)result.OutputData!).Count);
            }
            else
            {
                _logger.LogError("Database Query failed: {Error}", result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Example of executing an Email Notification node
        /// </summary>
        public async Task<NodeExecutionResult> ExecuteEmailNotificationNodeExample()
        {
            _logger.LogInformation("Starting Email Notification node execution example");

            // Create Email Notification configuration
            var emailConfig = new EmailNotificationNodeConfiguration
            {
                SmtpServer = "smtp.gmail.com",
                SmtpPort = 587,
                EnableSsl = true,
                Username = "your-email@gmail.com",
                Password = "your-app-password",
                FromAddress = "your-email@gmail.com",
                FromDisplayName = "Workflow Platform",
                ToAddresses = new List<string> { "recipient@example.com" },
                Subject = "Workflow Execution Notification",
                Body = "This is a test notification from the Workflow Platform.",
                IsBodyHtml = false,
                Timeout = TimeSpan.FromMinutes(2)
            };

            var context = new NodeExecutionContext(
                nodeId: Guid.NewGuid(),
                nodeType: "EmailNotification",
                workflowId: Guid.NewGuid(),
                executionId: Guid.NewGuid(),
                inputData: null,
                configuration: new Dictionary<string, object>
                {
                    { "EmailConfig", emailConfig }
                });

            var result = await _executionEngine.ExecuteNodeAsync(context, CancellationToken.None);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Email notification sent successfully");
            }
            else
            {
                _logger.LogError("Email notification failed: {Error}", result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Example of executing a node with retry logic
        /// </summary>
        public async Task<NodeExecutionResult> ExecuteNodeWithRetryExample()
        {
            _logger.LogInformation("Starting node execution with retry example");

            var httpConfig = new HttpRequestNodeConfiguration
            {
                Url = "https://httpstat.us/503", // This will return 503 Service Unavailable
                Method = "GET",
                Timeout = TimeSpan.FromSeconds(10)
            };

            var context = new NodeExecutionContext(
                nodeId: Guid.NewGuid(),
                nodeType: "HttpRequest",
                workflowId: Guid.NewGuid(),
                executionId: Guid.NewGuid(),
                inputData: null,
                configuration: new Dictionary<string, object>
                {
                    { "HttpConfig", httpConfig }
                });

            // Execute with retry logic (3 retries with exponential backoff)
            var result = await _executionEngine.ExecuteNodeWithRetryAsync(
                context,
                maxRetries: 3,
                retryDelay: TimeSpan.FromSeconds(1));

            if (result.IsSuccess)
            {
                _logger.LogInformation("Node executed successfully after retries");
            }
            else
            {
                _logger.LogError("Node execution failed after all retry attempts: {Error}", result.ErrorMessage);
            }

            return result;
        }

        /// <summary>
        /// Example of a complete workflow execution with multiple nodes
        /// </summary>
        public async Task<List<NodeExecutionResult>> ExecuteWorkflowExample()
        {
            _logger.LogInformation("Starting complete workflow execution example");

            var results = new List<NodeExecutionResult>();

            try
            {
                // Step 1: Execute HTTP Request to fetch data
                var httpResult = await ExecuteHttpRequestNodeExample();
                results.Add(httpResult);

                if (!httpResult.IsSuccess)
                {
                    _logger.LogError("Workflow failed at HTTP Request step");
                    return results;
                }

                // Step 2: Process the data (would typically involve data transformation node)
                _logger.LogInformation("Processing HTTP response data...");

                // Step 3: Store results in database (example with dummy data)
                var dbResult = await ExecuteDatabaseQueryNodeExample();
                results.Add(dbResult);

                if (!dbResult.IsSuccess)
                {
                    _logger.LogError("Workflow failed at Database Query step");
                    return results;
                }

                // Step 4: Send notification email
                var emailResult = await ExecuteEmailNotificationNodeExample();
                results.Add(emailResult);

                if (emailResult.IsSuccess)
                {
                    _logger.LogInformation("Workflow completed successfully!");
                }
                else
                {
                    _logger.LogWarning("Workflow completed but email notification failed");
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during workflow execution");
                results.Add(NodeExecutionResult.Failed(ex));
                return results;
            }
        }
    }

    /// <summary>
    /// Console application to demonstrate the Node Execution Engine
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup dependency injection
            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add application services (includes Node Execution Engine)
            services.AddApplication();

            // Add example class
            services.AddScoped<NodeExecutionEngineExample>();

            var serviceProvider = services.BuildServiceProvider();

            try
            {
                var example = serviceProvider.GetRequiredService<NodeExecutionEngineExample>();

                Console.WriteLine("=== Node Execution Engine Demo ===\n");

                // Run individual node examples
                Console.WriteLine("1. HTTP Request Node Example:");
                await example.ExecuteHttpRequestNodeExample();
                Console.WriteLine();

                Console.WriteLine("2. Node with Retry Example:");
                await example.ExecuteNodeWithRetryExample();
                Console.WriteLine();

                Console.WriteLine("3. Complete Workflow Example:");
                var workflowResults = await example.ExecuteWorkflowExample();
                Console.WriteLine($"Workflow executed {workflowResults.Count} nodes");

                Console.WriteLine("\n=== Demo Complete ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                serviceProvider.Dispose();
            }
        }
    }
}
