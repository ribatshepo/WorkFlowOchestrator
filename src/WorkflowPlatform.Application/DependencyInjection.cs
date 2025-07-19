using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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

        // Register application services
        // Additional application services will be registered here as they are created

        return services;
    }
}
