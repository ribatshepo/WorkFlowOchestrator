using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using WorkflowPlatform.Application;
using WorkflowPlatform.Infrastructure;
using WorkflowPlatform.API.Services;
using WorkflowPlatform.API.Hubs;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/workflow-platform-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add Application layer services
builder.Services.AddApplication();

// Add Infrastructure layer services
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret is not configured. Please set it in User Secrets or Environment Variables.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] 
                ?? throw new InvalidOperationException("JWT Issuer is not configured."),
            ValidAudience = jwtSettings["Audience"] 
                ?? throw new InvalidOperationException("JWT Audience is not configured."),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };

        // Enable JWT authentication for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// API Versioning
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-Version"),
        new QueryStringApiVersionReader("version")
    );
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add rate limiting policies
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("WorkflowPolicy", context => 
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.AddPolicy("ExecutionPolicy", context =>
        System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("WorkflowPlatformPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:3000", "https://localhost:3000" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

// SignalR configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.StreamBufferCapacity = 10;
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
})
.AddMessagePackProtocol(); // Enable MessagePack for better performance

// gRPC Services
builder.Services.AddGrpc(options =>
{
    options.MaxReceiveMessageSize = 16 * 1024 * 1024; // 16MB
    options.MaxSendMessageSize = 16 * 1024 * 1024; // 16MB
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Custom services
builder.Services.AddScoped<WorkflowExecutionBroadcastService>();

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Workflow Platform API",
        Version = "v1",
        Description = "A comprehensive workflow orchestration platform API with REST, gRPC, and SignalR support",
        Contact = new OpenApiContact
        {
            Name = "Workflow Platform Team",
            Email = "dev-team@workflow-platform.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT"
        }
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Add gRPC documentation
    c.DocumentFilter<GrpcDocumentFilter>();
});

// Health Checks with enhanced monitoring
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"))
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Database connection string not configured"))
    .AddRedis(builder.Configuration.GetConnectionString("Redis") 
        ?? "localhost:6379");

builder.Services.AddHealthChecksUI(options =>
{
    options.SetEvaluationTimeInSeconds(30);
    options.MaximumHistoryEntriesPerEndpoint(50);
    options.AddHealthCheckEndpoint("Workflow Platform API", "/health");
}).AddInMemoryStorage();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow Platform API V1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.DefaultModelExpandDepth(2);
        c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
    });

    // Initialize database in development
    try
    {
        await WorkflowPlatform.Infrastructure.DependencyInjection
            .EnsureDatabaseCreatedAsync(app.Services, app.Configuration);
        Log.Information("Database initialization completed successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while initializing the database");
    }
}

// Security headers
app.UseHsts();
app.UseHttpsRedirection();

// Rate limiting
app.UseIpRateLimiting();
app.UseRateLimiter();

// CORS (must be before routing)
app.UseCors("WorkflowPlatformPolicy");

// gRPC Web configuration
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

// Routing
app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                duration = x.Value.Duration.ToString(),
                description = x.Value.Description
            }),
            totalDuration = report.TotalDuration.ToString()
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

// gRPC Services (EnableGrpcWeb requires the middleware above)
app.MapGrpcService<WorkflowGrpcService>();
app.MapGrpcService<WorkflowExecutionGrpcService>();

// SignalR Hubs
app.MapHub<WorkflowExecutionHub>("/hubs/workflow-execution");

// API Controllers  
app.MapControllers();

// Static proto file serving
app.MapGet("/protos/workflow.proto", async context =>
{
    var protoPath = Path.Combine(AppContext.BaseDirectory, "Protos", "workflow.proto");
    if (File.Exists(protoPath))
    {
        context.Response.ContentType = "text/plain";
        await context.Response.SendFileAsync(protoPath);
    }
    else
    {
        context.Response.StatusCode = 404;
    }
});

// Default route
app.MapGet("/", () => Results.Redirect("/swagger"));

// Proto file information endpoint
app.MapGet("/api/proto-info", () => Results.Ok(new
{
    services = new[]
    {
        new { name = "WorkflowService", description = "High-performance workflow CRUD operations" },
        new { name = "WorkflowExecutionService", description = "Streaming workflow execution with real-time updates" }
    },
    protoFile = "/protos/workflow.proto",
    grpcWebEnabled = true,
    documentation = "Use gRPC-Web client libraries for browser compatibility"
}));

// Global exception handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "An error occurred while processing your request.",
            requestId = Activity.Current?.Id ?? context.TraceIdentifier
        };

        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    });
});

Log.Information("Starting Workflow Platform API...");
Log.Information("Available protocols: REST API (/api), gRPC (/grpc), SignalR (/hubs)");
Log.Information("API Documentation available at: /swagger");
Log.Information("Health monitoring available at: /health-ui");

try
{
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
