# Dockerfile for WorkflowPlatform API
# Multi-stage build following production best practices
# Stage 1: Build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Install security updates
RUN apt-get update && apt-get upgrade -y && apt-get clean && rm -rf /var/lib/apt/lists/*

# Copy solution and project files for better layer caching
COPY ["WorkflowPlatform.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["src/WorkflowPlatform.API/WorkflowPlatform.API.csproj", "src/WorkflowPlatform.API/"]
COPY ["src/WorkflowPlatform.Application/WorkflowPlatform.Application.csproj", "src/WorkflowPlatform.Application/"]
COPY ["src/WorkflowPlatform.Domain/WorkflowPlatform.Domain.csproj", "src/WorkflowPlatform.Domain/"]
COPY ["src/WorkflowPlatform.Infrastructure/WorkflowPlatform.Infrastructure.csproj", "src/WorkflowPlatform.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "WorkflowPlatform.sln" --runtime linux-x64

# Copy source code
COPY ["src/", "src/"]

# Build arguments for metadata
ARG BUILD_VERSION=unknown
ARG BUILD_DATE=unknown

# Build the application
RUN dotnet build "WorkflowPlatform.sln" -c Release --no-restore --runtime linux-x64

# Publish the application
RUN dotnet publish "src/WorkflowPlatform.API/WorkflowPlatform.API.csproj" \
    -c Release \
    --no-build \
    --runtime linux-x64 \
    --self-contained false \
    --output /app/publish \
    /p:PublishReadyToRun=true \
    /p:Version=${BUILD_VERSION} \
    /p:InformationalVersion=${BUILD_VERSION}

# Stage 2: Runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install security updates and required packages
RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        curl \
        ca-certificates \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd --gid 1000 appuser \
    && useradd --uid 1000 --gid appuser --shell /bin/bash --create-home appuser

# Set working directory
WORKDIR /app

# Copy published application
COPY --from=build /app/publish .

# Set ownership to appuser
RUN chown -R appuser:appuser /app

# Create directory for logs and set permissions
RUN mkdir -p /app/logs \
    && chown -R appuser:appuser /app/logs

# Switch to non-root user
USER appuser

# Configure ASP.NET Core
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_EnableDiagnostics=0

# Security hardening - remove server header
ENV ASPNETCORE_SERVER_HEADER=""

# Health check configuration
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose port
EXPOSE 8080

# Labels for metadata (following OCI standards)
LABEL org.opencontainers.image.title="WorkflowPlatform API"
LABEL org.opencontainers.image.description="Workflow Orchestration Platform API service"
LABEL org.opencontainers.image.vendor="WorkflowPlatform Team"
LABEL org.opencontainers.image.version="${BUILD_VERSION}"
LABEL org.opencontainers.image.created="${BUILD_DATE}"
LABEL org.opencontainers.image.source="https://github.com/ribatshepo/WorkFlowOchestrator"
LABEL org.opencontainers.image.documentation="https://github.com/ribatshepo/WorkFlowOchestrator/blob/main/README.md"
LABEL org.opencontainers.image.licenses="MIT"

# Security labels
LABEL security.non-root="true"
LABEL security.updates="auto"

# Entry point
ENTRYPOINT ["dotnet", "WorkflowPlatform.API.dll"]
