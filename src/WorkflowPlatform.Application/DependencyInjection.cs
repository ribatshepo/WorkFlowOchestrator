using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Application.Common.Resilience;
using WorkflowPlatform.Application.Common.Services;
using WorkflowPlatform.Application.Workflows.NodeExecution;
using WorkflowPlatform.Application.Workflows.NodeExecution.Strategies;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application;

/// <summary>
/// Dependency injection configuration for the Application layer.
/// Registers all application services and implementations.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Register MediatR
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(assembly);

        // Register Node Execution Engine and related services
        services.AddNodeExecutionEngine();

        return services;
    }

    /// <summary>
    /// Adds the Node Execution Engine and all related services
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddNodeExecutionEngine(this IServiceCollection services)
    {
        // Register the main execution engine
        services.AddScoped<INodeExecutionEngine, NodeExecutionEngine>();

        // Register metrics collector
        services.AddSingleton<IMetricsCollector, DefaultMetricsCollector>();

        // Register resilience policies
        services.AddScoped<IRetryPolicy, RetryPolicy>();
        services.AddScoped<ICircuitBreaker>(provider =>
        {
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CircuitBreaker>>();
            var metrics = provider.GetRequiredService<IMetricsCollector>();
            return new CircuitBreaker("Default", logger, metrics);
        });

        // Register all node execution strategies
        services.AddScoped<INodeExecutionStrategy, HttpRequestNodeStrategy>();
        services.AddScoped<INodeExecutionStrategy, DatabaseQueryNodeStrategy>();
        services.AddScoped<INodeExecutionStrategy, EmailNotificationNodeStrategy>();

        // Register the strategy factory
        services.AddScoped<INodeStrategyFactory>(provider =>
        {
            var strategies = provider.GetServices<INodeExecutionStrategy>();
            return new NodeStrategyFactory(provider, strategies);
        });

        // Register HTTP client for HTTP request node strategy
        services.AddHttpClient<HttpRequestNodeStrategy>(client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "WorkflowPlatform/1.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}
