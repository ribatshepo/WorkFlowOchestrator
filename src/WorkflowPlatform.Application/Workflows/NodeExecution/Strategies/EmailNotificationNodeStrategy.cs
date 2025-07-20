using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using WorkflowPlatform.Application.Common.Interfaces;
using WorkflowPlatform.Application.Workflows.NodeExecution.Configurations;
using WorkflowPlatform.Domain.Workflows.NodeExecution;

namespace WorkflowPlatform.Application.Workflows.NodeExecution.Strategies
{
    /// <summary>
    /// Email Notification node execution strategy
    /// </summary>
    public class EmailNotificationNodeStrategy : BaseNodeExecutionStrategy
    {
        private readonly IRetryPolicy _retryPolicy;
        private readonly ICircuitBreaker _circuitBreaker;

        public EmailNotificationNodeStrategy(
            IRetryPolicy retryPolicy,
            ICircuitBreaker circuitBreaker,
            ILogger<EmailNotificationNodeStrategy> logger,
            IMetricsCollector metrics)
            : base(logger, metrics)
        {
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
            _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker));
        }

        public override string NodeType => "EmailNotification";

        public override async Task<NodeExecutionResult> ExecuteAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var config = GetRequiredConfiguration<EmailNotificationNodeConfiguration>(context, "EmailConfig");
            
            try
            {
                _logger.LogInformation("Starting email notification execution for node {NodeId}", context.NodeId);

                var timeoutToken = CreateTimeoutToken(context, config.Timeout);

                // Execute email sending with retry and circuit breaker
                var result = await _retryPolicy.ExecuteAsync(async (ct) =>
                {
                    return await _circuitBreaker.ExecuteAsync(async (innerCt) =>
                    {
                        return await SendEmailAsync(config, innerCt);
                    }, ct);
                }, timeoutToken);

                stopwatch.Stop();
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.Completed, stopwatch.Elapsed);

                _logger.LogInformation("Email notification sent successfully for node {NodeId} in {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);

                return NodeExecutionResult.Success(result, stopwatch.Elapsed);
            }
            catch (OperationCanceledException)
            {
                stopwatch.Stop();
                _logger.LogWarning("Email notification cancelled for node {NodeId} after {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.Cancelled, stopwatch.Elapsed);
                return NodeExecutionResult.Cancelled("Email notification was cancelled", stopwatch.Elapsed);
            }
            catch (TimeoutException)
            {
                stopwatch.Stop();
                _logger.LogWarning("Email notification timed out for node {NodeId} after {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.TimedOut, stopwatch.Elapsed);
                return NodeExecutionResult.Timeout(config.Timeout, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Email notification failed for node {NodeId} after {Duration}ms", 
                    context.NodeId, stopwatch.ElapsedMilliseconds);
                _metrics.RecordNodeExecution(NodeType, NodeExecutionStatus.Failed, stopwatch.Elapsed);
                _metrics.RecordNodeExecutionError(NodeType, ex.GetType().Name);
                return NodeExecutionResult.Failed(ex, stopwatch.Elapsed);
            }
        }

        protected override async Task<ValidationResult> ValidateInputsAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken)
        {
            var result = new ValidationResult();

            try
            {
                // Validate email configuration exists
                if (!HasRequiredConfiguration(context, "EmailConfig"))
                {
                    result.AddError("EmailConfig is required");
                    return result;
                }

                var config = GetRequiredConfiguration<EmailNotificationNodeConfiguration>(context, "EmailConfig");

                // Validate SMTP settings
                if (string.IsNullOrWhiteSpace(config.SmtpServer))
                {
                    result.AddError("SMTP server is required");
                }

                if (config.SmtpPort <= 0 || config.SmtpPort > 65535)
                {
                    result.AddError("SMTP port must be between 1 and 65535");
                }

                if (string.IsNullOrWhiteSpace(config.Username))
                {
                    result.AddError("Username is required");
                }

                if (string.IsNullOrWhiteSpace(config.Password))
                {
                    result.AddError("Password is required");
                }

                // Validate sender
                if (string.IsNullOrWhiteSpace(config.FromAddress))
                {
                    result.AddError("From address is required");
                }
                else if (!IsValidEmail(config.FromAddress))
                {
                    result.AddError("From address must be a valid email address");
                }

                // Validate recipients
                if (!config.ToAddresses.Any())
                {
                    result.AddError("At least one recipient is required");
                }

                foreach (var email in config.ToAddresses.Concat(config.CcAddresses).Concat(config.BccAddresses))
                {
                    if (!IsValidEmail(email))
                    {
                        result.AddError($"'{email}' is not a valid email address");
                    }
                }

                // Validate subject and body
                if (string.IsNullOrWhiteSpace(config.Subject))
                {
                    result.AddError("Subject is required");
                }

                if (string.IsNullOrWhiteSpace(config.Body) && string.IsNullOrWhiteSpace(config.TemplateId))
                {
                    result.AddError("Either Body or TemplateId is required");
                }

                // Validate timeout
                if (config.Timeout <= TimeSpan.Zero || config.Timeout > TimeSpan.FromMinutes(5))
                {
                    result.AddError("Timeout must be between 1 second and 5 minutes");
                }

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email notification inputs for node {NodeId}", context.NodeId);
                result.AddError($"Validation error: {ex.Message}");
                return await Task.FromResult(result);
            }
        }

        protected override async Task SetupExecutionContextAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken)
        {
            try
            {
                var config = GetRequiredConfiguration<EmailNotificationNodeConfiguration>(context, "EmailConfig");

                // Process template if specified
                if (!string.IsNullOrWhiteSpace(config.TemplateId))
                {
                    var processedBody = await ProcessEmailTemplateAsync(config.TemplateId, cancellationToken);
                    config.Body = processedBody;
                }

                context.SetProperty("SetupCompleted", true);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting up email notification context for node {NodeId}", context.NodeId);
                throw;
            }
        }

        protected override async Task<NodeExecutionResult> TransformOutputAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Email result is already in the correct format
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming email result for node {NodeId}", context.NodeId);
                return NodeExecutionResult.Failed($"Output transformation failed: {ex.Message}");
            }
        }

        protected override async Task<ValidationResult> ValidateOutputAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken)
        {
            var validation = new ValidationResult();

            try
            {
                // Validate that email was sent successfully
                if (result.IsSuccess && result.OutputData == null)
                {
                    validation.AddWarning("Email sent but no confirmation data returned");
                }

                return await Task.FromResult(validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email result for node {NodeId}", context.NodeId);
                validation.AddError($"Output validation error: {ex.Message}");
                return await Task.FromResult(validation);
            }
        }

        protected override async Task CleanupResourcesAsync(
            NodeExecutionContext context, 
            CancellationToken cancellationToken)
        {
            try
            {
                // SMTP client resources are disposed automatically
                _logger.LogDebug("Cleaned up email notification resources for node {NodeId}", context.NodeId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up email notification resources for node {NodeId}", context.NodeId);
            }
        }

        protected override async Task PersistExecutionStateAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Add execution metadata
                result.Metadata["ExecutedAt"] = DateTime.UtcNow;
                result.Metadata["NodeType"] = NodeType;
                result.Metadata["ExecutionId"] = context.ExecutionId.ToString();

                _logger.LogDebug("Persisted email notification execution state for node {NodeId}", context.NodeId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error persisting email notification state for node {NodeId}", context.NodeId);
            }
        }

        protected override async Task TriggerCompletionEventsAsync(
            NodeExecutionContext context, 
            NodeExecutionResult result, 
            CancellationToken cancellationToken)
        {
            try
            {
                // Trigger any completion events or notifications
                _logger.LogInformation("Email notification node {NodeId} completed with status {Status}", 
                    context.NodeId, result.Status);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering completion events for node {NodeId}", context.NodeId);
            }
        }

        private async Task<object> SendEmailAsync(
            EmailNotificationNodeConfiguration config, 
            CancellationToken cancellationToken)
        {
            using var smtpClient = new SmtpClient(config.SmtpServer, config.SmtpPort)
            {
                EnableSsl = config.EnableSsl,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(config.Username, config.Password),
                Timeout = (int)config.Timeout.TotalMilliseconds
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(config.FromAddress, config.FromDisplayName),
                Subject = config.Subject,
                Body = config.Body,
                IsBodyHtml = config.IsBodyHtml
            };

            // Add recipients
            foreach (var toAddress in config.ToAddresses)
            {
                mailMessage.To.Add(toAddress);
            }

            foreach (var ccAddress in config.CcAddresses)
            {
                mailMessage.CC.Add(ccAddress);
            }

            foreach (var bccAddress in config.BccAddresses)
            {
                mailMessage.Bcc.Add(bccAddress);
            }

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            return new
            {
                MessageId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
                Recipients = config.ToAddresses.Count + config.CcAddresses.Count + config.BccAddresses.Count,
                Subject = config.Subject
            };
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> ProcessEmailTemplateAsync(
            string templateId, 
            CancellationToken cancellationToken)
        {
            // Placeholder for template processing logic
            // In a real implementation, this would:
            // 1. Load template from a template engine (e.g., Razor, Handlebars)
            // 2. Process template with provided data
            // 3. Return the rendered content
            
            await Task.Delay(10, cancellationToken); // Simulate template processing
            return $"Processed template {templateId} with data";
        }
    }
}
