using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WorkflowPlatform.API.Services;

/// <summary>
/// Swagger document filter to include gRPC service documentation.
/// </summary>
public class GrpcDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Add gRPC services section to the OpenAPI documentation
        swaggerDoc.Info.Description += "\n\n## gRPC Services\n\n" +
            "This API also provides high-performance gRPC services for:\n\n" +
            "- **WorkflowService**: High-performance workflow CRUD operations\n" +
            "- **WorkflowExecutionService**: Streaming workflow execution with real-time updates\n\n" +
            "gRPC endpoints are available at the same base URL with `/grpc/` prefix.\n\n" +
            "### gRPC Web Support\n\n" +
            "gRPC-Web is enabled for browser compatibility. Use gRPC-Web client libraries to consume services from web applications.\n\n" +
            "### Available Proto Files\n\n" +
            "- `/protos/workflow.proto` - Workflow service definitions\n\n" +
            "## SignalR Hubs\n\n" +
            "Real-time communication is provided via SignalR hubs:\n\n" +
            "- **/hubs/workflow-execution** - Real-time workflow execution updates\n\n" +
            "### SignalR Features\n\n" +
            "- Real-time execution progress updates\n" +
            "- Collaborative workflow editing\n" +
            "- System notifications\n" +
            "- Metrics streaming\n\n" +
            "Use SignalR client libraries to connect and subscribe to real-time updates.";

        // Add custom tags for better organization
        swaggerDoc.Tags ??= new List<OpenApiTag>();
        
        swaggerDoc.Tags.Add(new OpenApiTag
        {
            Name = "Workflows",
            Description = "Workflow definition management (REST API)"
        });

        swaggerDoc.Tags.Add(new OpenApiTag
        {
            Name = "Executions", 
            Description = "Workflow execution operations (REST API)"
        });

        swaggerDoc.Tags.Add(new OpenApiTag
        {
            Name = "gRPC Services",
            Description = "High-performance gRPC services for workflows and executions"
        });

        swaggerDoc.Tags.Add(new OpenApiTag
        {
            Name = "SignalR Hubs",
            Description = "Real-time communication hubs"
        });
    }
}
