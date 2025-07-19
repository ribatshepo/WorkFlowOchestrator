using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkflowPlatform.Domain.Workflows.Entities;
using System.Text.Json;

namespace WorkflowPlatform.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for WorkflowNode.
/// Defines the database mapping for workflow node entities.
/// </summary>
public class WorkflowNodeConfiguration : IEntityTypeConfiguration<WorkflowNode>
{
    public void Configure(EntityTypeBuilder<WorkflowNode> builder)
    {
        // Table configuration
        builder.ToTable("WorkflowNodes");
        builder.HasKey(n => n.Id);

        // Primary properties
        builder.Property(n => n.Id)
            .ValueGeneratedNever(); // We generate GUIDs in domain logic

        builder.Property(n => n.WorkflowId)
            .IsRequired();

        builder.Property(n => n.DisplayName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(n => n.PositionX)
            .IsRequired();

        builder.Property(n => n.PositionY)
            .IsRequired();

        builder.Property(n => n.IsEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        // Complex properties stored as JSON
        builder.Property(n => n.RuntimeContext)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
            .HasColumnType("jsonb"); // PostgreSQL-specific JSON type

        // Base entity properties (inherited)
        builder.Property(n => n.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(n => n.ModifiedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(n => n.CreatedBy)
            .IsRequired();

        builder.Property(n => n.ModifiedBy);

        // Value object - NodeConfiguration as owned entity
        builder.OwnsOne(n => n.Configuration, config =>
        {
            config.Property(c => c.NodeType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("NodeType");

            config.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ConfigName");

            config.Property(c => c.Description)
                .HasMaxLength(1000)
                .HasColumnName("ConfigDescription");

            config.Property(c => c.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                .HasColumnType("jsonb")
                .HasColumnName("Properties");

            config.Property(c => c.Metadata)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, string>())
                .HasColumnType("jsonb")
                .HasColumnName("Metadata");

            config.Property(c => c.Timeout)
                .HasColumnName("TimeoutMilliseconds")
                .HasConversion(
                    v => v.HasValue ? (int)v.Value.TotalMilliseconds : (int?)null,
                    v => v.HasValue ? TimeSpan.FromMilliseconds(v.Value) : null);

            config.Property(c => c.MaxRetryAttempts)
                .HasDefaultValue(3)
                .HasColumnName("MaxRetryAttempts");

            config.Property(c => c.ContinueOnFailure)
                .HasDefaultValue(false)
                .HasColumnName("ContinueOnFailure");
        });

        // Navigation properties - ignoring collections for now as they are handled by the aggregate
        builder.Ignore(n => n.IncomingConnections);
        builder.Ignore(n => n.OutgoingConnections);
    }
}
