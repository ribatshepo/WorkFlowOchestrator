using Microsoft.EntityFrameworkCore;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Domain.Workflows.Aggregates;
using WorkflowPlatform.Infrastructure.Persistence;

namespace WorkflowPlatform.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of the workflow repository.
/// Provides data persistence operations for workflow aggregates.
/// </summary>
public class WorkflowRepository : IWorkflowRepository
{
    private readonly WorkflowDbContext _context;

    public WorkflowRepository(WorkflowDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<WorkflowAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .Include(w => w.Nodes)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<WorkflowAggregate> AddAsync(WorkflowAggregate aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        var entry = await _context.Workflows.AddAsync(aggregate, cancellationToken);
        return entry.Entity;
    }

    public Task<WorkflowAggregate> UpdateAsync(WorkflowAggregate aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        _context.Workflows.Update(aggregate);
        return Task.FromResult(aggregate);
    }

    public Task RemoveAsync(WorkflowAggregate aggregate, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        _context.Workflows.Remove(aggregate);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .AnyAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<WorkflowAggregate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        return await _context.Workflows
            .Include(w => w.Nodes)
            .FirstOrDefaultAsync(w => w.Name == name, cancellationToken);
    }

    public async Task<IEnumerable<WorkflowAggregate>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .Where(w => w.Status == Domain.Common.Enumerations.WorkflowStatus.Active)
            .Include(w => w.Nodes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkflowAggregate>> GetByCreatorAsync(Guid createdBy, CancellationToken cancellationToken = default)
    {
        return await _context.Workflows
            .Where(w => w.CreatedBy == createdBy)
            .Include(w => w.Nodes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<WorkflowAggregate> Workflows, int TotalCount)> GetPaginatedAsync(
        int pageSize = 50, 
        int pageNumber = 1, 
        string? searchTerm = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Workflows.AsQueryable();

        // Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(w => 
                w.Name.Contains(searchTerm) || 
                (w.Description != null && w.Description.Contains(searchTerm)) ||
                w.Category.Contains(searchTerm));
        }

        // Get total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and include related data
        var workflows = await query
            .OrderBy(w => w.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(w => w.Nodes)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (workflows, totalCount);
    }
}
