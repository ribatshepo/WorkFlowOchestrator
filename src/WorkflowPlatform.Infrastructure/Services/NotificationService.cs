using Microsoft.Extensions.Logging;
using WorkflowPlatform.Application.Common.Interfaces;

namespace WorkflowPlatform.Infrastructure.Services;

/// <summary>
/// Implementation of INotificationService for sending email notifications.
/// This is a placeholder implementation that logs notifications instead of sending actual emails.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendEmailAsync(
        string to, 
        string subject, 
        string body, 
        bool isHtml = false, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(to);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);

        // TODO: Implement actual email sending logic using a service like SendGrid, SMTP, etc.
        // For now, we'll log the notification
        
        _logger.LogInformation(
            "Sending email notification to {Recipient} with subject '{Subject}'. Body type: {BodyType}",
            to, subject, isHtml ? "HTML" : "Text");

        // Simulate async email sending operation
        await Task.Delay(100, cancellationToken);

        _logger.LogInformation("Email notification sent successfully to {Recipient}", to);
    }

    public async Task SendWorkflowNotificationAsync(
        Guid workflowId, 
        string executionStatus, 
        string recipientEmail, 
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executionStatus);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);

        var subject = $"Workflow Execution {executionStatus}";
        var body = $"Your workflow (ID: {workflowId}) has {executionStatus.ToLower()}.\n\n" +
                  $"Status: {executionStatus}\n" +
                  $"Workflow ID: {workflowId}\n" +
                  $"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await SendEmailAsync(recipientEmail, subject, body, isHtml: false, cancellationToken);
    }
}
