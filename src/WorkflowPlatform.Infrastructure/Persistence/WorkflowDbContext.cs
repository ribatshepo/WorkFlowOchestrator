using Microsoft.EntityFrameworkCore;
using WorkflowPlatform.Domain.Workflows.Aggregates;
using WorkflowPlatform.Domain.Workflows.Entities;
using WorkflowPlatform.Infrastructure.Persistence.Configurations;

namespace WorkflowPlatform.Infrastructure.Persistence;

/// <summary>
/// Entity Framework DbContext for the Workflow Platform.
/// Configures entity mappings and database relationships.
/// </summary>
public class WorkflowDbContext : DbContext
{
    public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Workflow definitions and their configuration.
    /// </summary>
    public DbSet<WorkflowAggregate> Workflows { get; set; } = null!;

    /// <summary>
    /// Individual nodes within workflow definitions.
    /// </summary>
    public DbSet<WorkflowNode> WorkflowNodes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from separate configuration classes
        modelBuilder.ApplyConfiguration(new WorkflowAggregateConfiguration());
        modelBuilder.ApplyConfiguration(new WorkflowNodeConfiguration());

        // Configure database schema
        modelBuilder.HasDefaultSchema("workflow");

        // Add database-level constraints and indexes
        ConfigureIndexes(modelBuilder);
    }

    /// <summary>
    /// Saves changes to the database and publishes domain events.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of state entries written to the database</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        UpdateTimestamps();

        // Save changes to database
        var result = await base.SaveChangesAsync(cancellationToken);

        // TODO: Publish domain events after successful save
        // This will be implemented when we add the domain event dispatcher

        return result;
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Domain.Common.Primitives.Entity<Guid> entity)
            {
                if (entry.State == EntityState.Added)
                {
                    // CreatedAt is set in the entity constructor
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.GetType()
                        .GetProperty("UpdatedAt")?
                        .SetValue(entity, DateTime.UtcNow);
                }
            }
        }
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // Workflow indexes for performance
        modelBuilder.Entity<WorkflowAggregate>()
            .HasIndex(w => w.Name)
            .IsUnique()
            .HasDatabaseName("IX_Workflows_Name");

        modelBuilder.Entity<WorkflowAggregate>()
            .HasIndex(w => w.Status)
            .HasDatabaseName("IX_Workflows_Status");

        modelBuilder.Entity<WorkflowAggregate>()
            .HasIndex(w => w.Category)
            .HasDatabaseName("IX_Workflows_Category");

        modelBuilder.Entity<WorkflowAggregate>()
            .HasIndex(w => w.CreatedBy)
            .HasDatabaseName("IX_Workflows_CreatedBy");

        // WorkflowNode indexes
        modelBuilder.Entity<WorkflowNode>()
            .HasIndex(n => n.WorkflowId)
            .HasDatabaseName("IX_WorkflowNodes_WorkflowId");

        modelBuilder.Entity<WorkflowNode>()
            .HasIndex(n => new { n.WorkflowId, n.DisplayName })
            .HasDatabaseName("IX_WorkflowNodes_WorkflowId_DisplayName");
    }
}
