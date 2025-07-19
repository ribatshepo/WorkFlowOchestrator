using WorkflowPlatform.Application.Common.Interfaces;

namespace WorkflowPlatform.Infrastructure.Services;

/// <summary>
/// Implementation of IDateTimeService that provides testable date/time functionality.
/// </summary>
public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime Now => DateTime.Now;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.Today);
}
