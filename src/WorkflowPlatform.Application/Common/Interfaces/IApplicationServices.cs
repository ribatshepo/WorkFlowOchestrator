namespace WorkflowPlatform.Application.Common.Interfaces;

/// <summary>
/// Application service interface for the current user context.
/// Provides access to current user information and claims.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's unique identifier.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's username.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Gets the current user's email address.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Checks if the current user has a specific permission.
    /// </summary>
    /// <param name="permission">The permission to check</param>
    /// <returns>True if the user has the permission, false otherwise</returns>
    bool HasPermission(string permission);

    /// <summary>
    /// Checks if the current user is in a specific role.
    /// </summary>
    /// <param name="role">The role to check</param>
    /// <returns>True if the user is in the role, false otherwise</returns>
    bool IsInRole(string role);
}

/// <summary>
/// Application service interface for date/time operations.
/// Provides testable date/time functionality.
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current local date and time.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the current date (without time component).
    /// </summary>
    DateOnly Today { get; }
}

/// <summary>
/// Application service interface for email notifications.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends an email notification.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="isHtml">Whether the body contains HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification about workflow execution completion.
    /// </summary>
    /// <param name="workflowId">The workflow identifier</param>
    /// <param name="executionStatus">The execution status</param>
    /// <param name="recipientEmail">Recipient email address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendWorkflowNotificationAsync(Guid workflowId, string executionStatus, string recipientEmail, CancellationToken cancellationToken = default);
}
