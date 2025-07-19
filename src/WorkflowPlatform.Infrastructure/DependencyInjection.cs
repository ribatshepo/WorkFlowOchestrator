using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Infrastructure.Persistence;
using WorkflowPlatform.Infrastructure.Repositories;
using WorkflowPlatform.Infrastructure.Services;

namespace WorkflowPlatform.Infrastructure;

/// <summary>
/// Dependency injection configuration for the Infrastructure layer.
/// Registers all infrastructure services and implementations.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database configuration
        services.AddDbContext<WorkflowDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection connection string is not configured");

            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(WorkflowDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });

            // Enable detailed errors in development
            if (configuration.GetValue<string>("Environment") == "Development")
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        // Repository registrations
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();

        // Application service implementations
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<INotificationService, NotificationService>();

        // Health checks
        services.AddHealthChecks()
            .AddDbContextCheck<WorkflowDbContext>("database");

        return services;
    }

    /// <summary>
    /// Ensures the database is created and migrated.
    /// Use this method during application startup for development scenarios.
    /// </summary>
    /// <param name="services">The service provider</param>
    /// <param name="configuration">The application configuration</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task EnsureDatabaseCreatedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkflowDbContext>();

        // Only auto-migrate in development
        if (configuration.GetValue<string>("Environment") == "Development")
        {
            await context.Database.EnsureCreatedAsync();
            
            // Apply any pending migrations
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }
        }
    }
}
