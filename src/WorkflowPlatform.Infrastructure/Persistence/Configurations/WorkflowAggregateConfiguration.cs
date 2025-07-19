using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowPlatform.Domain.Workflows.Aggregates;
using System.Text.Json;

namespace WorkflowPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for WorkflowAggregate.
/// Defines the database mapping and relationships for workflow entities.
/// </summary>
public class WorkflowAggregateConfiguration : IEntityTypeConfiguration<WorkflowAggregate>
{
    public void Configure(EntityTypeBuilder<WorkflowAggregate> builder)
    {
        // Table configuration
        builder.ToTable("Workflows");
        builder.HasKey(w => w.Id);

        // Primary properties
        builder.Property(w => w.Id)
            .ValueGeneratedNever(); // We generate GUIDs in domain logic

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(w => w.Description)
            .HasMaxLength(2000);

        builder.Property(w => w.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Status)
            .IsRequired()
            .HasConversion<string>(); // Store enum as string

        builder.Property(w => w.Priority)
            .IsRequired()
            .HasConversion<string>(); // Store enum as string

        builder.Property(w => w.TriggerConfiguration)
            .HasMaxLength(4000);

        builder.Property(w => w.DefaultTimeout)
            .HasConversion(
                v => v.HasValue ? v.Value.TotalMilliseconds : (double?)null,
                v => v.HasValue ? TimeSpan.FromMilliseconds(v.Value) : null);

        builder.Property(w => w.MaxConcurrentExecutions)
            .IsRequired()
            .HasDefaultValue(10);

        builder.Property(w => w.IsTemplate)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(w => w.LastExecutedAt);

        builder.Property(w => w.TotalExecutions)
            .IsRequired()
            .HasDefaultValue(0);

        // Complex properties
        builder.Property(w => w.GlobalVariables)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
            .HasColumnType("jsonb"); // PostgreSQL-specific JSON type

        // Base entity properties (inherited)
        builder.Property(w => w.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(w => w.ModifiedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(w => w.CreatedBy)
            .IsRequired();

        builder.Property(w => w.ModifiedBy);

        // Relationships
        builder.HasMany(w => w.Nodes)
            .WithOne()
            .HasForeignKey(n => n.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Value objects as owned entities
        builder.OwnsMany(w => w.Connections, connection =>
        {
            connection.ToTable("WorkflowConnections");
            connection.WithOwner().HasForeignKey("WorkflowId");
            connection.HasKey("Id");

            connection.Property<int>("Id").ValueGeneratedOnAdd();
            connection.Property(c => c.SourceNodeId).IsRequired();
            connection.Property(c => c.TargetNodeId).IsRequired();
            connection.Property(c => c.SourcePort).IsRequired().HasMaxLength(100);
            connection.Property(c => c.TargetPort).IsRequired().HasMaxLength(100);
            connection.Property(c => c.Condition).HasMaxLength(1000);
            
            connection.Property(c => c.DataMapping)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                .HasColumnType("jsonb");

            // Indexes for performance
            connection.HasIndex(c => c.SourceNodeId);
            connection.HasIndex(c => c.TargetNodeId);
        });

        // Domain events (ignore for now, will be handled by event store later)
        builder.Ignore(w => w.DomainEvents);
    }
}
