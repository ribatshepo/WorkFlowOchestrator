using System;
using System.Collections.Generic;

namespace WorkflowPlatform.Application.Workflows.NodeExecution.Configurations
{
    /// <summary>
    /// Configuration for HTTP Request node
    /// </summary>
    public class HttpRequestNodeConfiguration
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";
        public Dictionary<string, string> Headers { get; set; } = new();
        public string? Body { get; set; }
        public string? ContentType { get; set; } = "application/json";
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public bool FollowRedirects { get; set; } = true;
        public int MaxRedirects { get; set; } = 10;
        public Dictionary<string, object> Authentication { get; set; } = new();
    }

    /// <summary>
    /// Configuration for Database Query node
    /// </summary>
    public class DatabaseQueryNodeConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public bool IsReadOnly { get; set; } = true;
        public int MaxRetries { get; set; } = 3;
        public string DatabaseProvider { get; set; } = "PostgreSQL";
    }

    /// <summary>
    /// Configuration for Email Notification node
    /// </summary>
    public class EmailNotificationNodeConfiguration
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromDisplayName { get; set; } = string.Empty;
        public List<string> ToAddresses { get; set; } = new();
        public List<string> CcAddresses { get; set; } = new();
        public List<string> BccAddresses { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; } = true;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public string? TemplateId { get; set; }
        public Dictionary<string, object> TemplateData { get; set; } = new();
    }
}
