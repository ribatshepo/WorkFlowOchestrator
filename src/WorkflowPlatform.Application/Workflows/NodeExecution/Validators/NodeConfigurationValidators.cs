using FluentValidation;
using WorkflowPlatform.Application.Workflows.NodeExecution.Configurations;

namespace WorkflowPlatform.Application.Workflows.NodeExecution.Validators
{
    /// <summary>
    /// Validator for HTTP Request node configuration
    /// </summary>
    public class HttpRequestNodeConfigurationValidator : AbstractValidator<HttpRequestNodeConfiguration>
    {
        public HttpRequestNodeConfigurationValidator()
        {
            RuleFor(x => x.Url)
                .NotEmpty()
                .WithMessage("URL is required")
                .Must(BeValidUrl)
                .WithMessage("URL must be a valid absolute URI");

            RuleFor(x => x.Method)
                .NotEmpty()
                .WithMessage("HTTP method is required")
                .Must(BeValidHttpMethod)
                .WithMessage("HTTP method must be one of: GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS");

            RuleFor(x => x.Timeout)
                .GreaterThan(TimeSpan.Zero)
                .WithMessage("Timeout must be greater than zero")
                .LessThanOrEqualTo(TimeSpan.FromMinutes(10))
                .WithMessage("Timeout cannot exceed 10 minutes");

            RuleFor(x => x.ContentType)
                .NotEmpty()
                .When(x => !string.IsNullOrWhiteSpace(x.Body))
                .WithMessage("ContentType is required when Body is specified");

            RuleForEach(x => x.Headers)
                .Must(header => !string.IsNullOrWhiteSpace(header.Key))
                .WithMessage("Header name cannot be empty");
        }

        private static bool BeValidUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        private static bool BeValidHttpMethod(string method)
        {
            var allowedMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };
            return Array.Exists(allowedMethods, m => 
                string.Equals(m, method, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Validator for Database Query node configuration
    /// </summary>
    public class DatabaseQueryNodeConfigurationValidator : AbstractValidator<DatabaseQueryNodeConfiguration>
    {
        public DatabaseQueryNodeConfigurationValidator()
        {
            RuleFor(x => x.ConnectionString)
                .NotEmpty()
                .WithMessage("ConnectionString is required");

            RuleFor(x => x.Query)
                .NotEmpty()
                .WithMessage("Query is required")
                .Must(BeValidQuery)
                .WithMessage("Query contains potentially dangerous operations");

            RuleFor(x => x.Timeout)
                .GreaterThan(TimeSpan.Zero)
                .WithMessage("Timeout must be greater than zero")
                .LessThanOrEqualTo(TimeSpan.FromMinutes(30))
                .WithMessage("Timeout cannot exceed 30 minutes");

            RuleFor(x => x.DatabaseProvider)
                .NotEmpty()
                .WithMessage("DatabaseProvider is required")
                .Must(BeValidDatabaseProvider)
                .WithMessage("DatabaseProvider must be one of: PostgreSQL, SQLServer, MySQL, SQLite");

            RuleFor(x => x.MaxRetries)
                .GreaterThanOrEqualTo(0)
                .WithMessage("MaxRetries cannot be negative")
                .LessThanOrEqualTo(10)
                .WithMessage("MaxRetries cannot exceed 10");
        }

        private static bool BeValidDatabaseProvider(string provider)
        {
            var supportedProviders = new[] { "PostgreSQL", "SQLServer", "MySQL", "SQLite" };
            return Array.Exists(supportedProviders, p => 
                string.Equals(p, provider, StringComparison.OrdinalIgnoreCase));
        }

        private static bool BeValidQuery(string query)
        {
            // Basic validation to prevent obviously dangerous operations
            var upperQuery = query.ToUpperInvariant();
            var dangerousKeywords = new[] { "DROP TABLE", "DROP DATABASE", "TRUNCATE", "DELETE FROM", "ALTER TABLE" };
            
            return !Array.Exists(dangerousKeywords, keyword => upperQuery.Contains(keyword));
        }
    }

    /// <summary>
    /// Validator for Email Notification node configuration
    /// </summary>
    public class EmailNotificationNodeConfigurationValidator : AbstractValidator<EmailNotificationNodeConfiguration>
    {
        public EmailNotificationNodeConfigurationValidator()
        {
            RuleFor(x => x.SmtpServer)
                .NotEmpty()
                .WithMessage("SMTP server is required");

            RuleFor(x => x.SmtpPort)
                .GreaterThan(0)
                .WithMessage("SMTP port must be greater than 0")
                .LessThanOrEqualTo(65535)
                .WithMessage("SMTP port must be less than or equal to 65535");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required");

            RuleFor(x => x.FromAddress)
                .NotEmpty()
                .WithMessage("From address is required")
                .EmailAddress()
                .WithMessage("From address must be a valid email address");

            RuleFor(x => x.ToAddresses)
                .NotEmpty()
                .WithMessage("At least one recipient is required");

            RuleForEach(x => x.ToAddresses)
                .EmailAddress()
                .WithMessage("All recipients must have valid email addresses");

            RuleForEach(x => x.CcAddresses)
                .EmailAddress()
                .WithMessage("All CC recipients must have valid email addresses");

            RuleForEach(x => x.BccAddresses)
                .EmailAddress()
                .WithMessage("All BCC recipients must have valid email addresses");

            RuleFor(x => x.Subject)
                .NotEmpty()
                .WithMessage("Subject is required");

            RuleFor(x => x.Body)
                .NotEmpty()
                .When(x => string.IsNullOrWhiteSpace(x.TemplateId))
                .WithMessage("Body is required when TemplateId is not specified");

            RuleFor(x => x.Timeout)
                .GreaterThan(TimeSpan.Zero)
                .WithMessage("Timeout must be greater than zero")
                .LessThanOrEqualTo(TimeSpan.FromMinutes(5))
                .WithMessage("Timeout cannot exceed 5 minutes");
        }
    }
}
