@echo off
setlocal enabledelayedexpansion

:: WorkflowPlatform Development Environment Setup Script (Windows)
:: This script sets up the complete development environment on Windows

title WorkflowPlatform Development Setup

echo.
echo ========================================
echo WorkflowPlatform Development Setup
echo ========================================
echo.

:: Check if running as administrator (optional but recommended)
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo Warning: Running without administrator privileges
    echo Some operations may require elevated permissions
    echo.
)

:: Check system requirements
echo Checking system requirements...
echo.

:: Check .NET SDK
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET SDK not found
    echo Please install .NET 8 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do set dotnet_version=%%i
    echo ✅ .NET SDK found: !dotnet_version!
)

:: Check Docker
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: Docker not found
    echo Please install Docker Desktop from https://docker.com/products/docker-desktop
    pause
    exit /b 1
) else (
    echo ✅ Docker found
)

:: Check Docker Compose
docker-compose --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: Docker Compose not found
    echo Please install Docker Compose or update Docker Desktop
    pause
    exit /b 1
) else (
    echo ✅ Docker Compose found
)

:: Check Git
git --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Warning: Git not found
    echo Git is recommended for version control
) else (
    echo ✅ Git found
)

echo.
echo System requirements check completed
echo.

:: Setup .NET environment
echo ========================================
echo Setting up .NET Environment
echo ========================================
echo.

if exist "WorkflowPlatform.sln" (
    echo Restoring NuGet packages...
    dotnet restore WorkflowPlatform.sln
    if %errorlevel% neq 0 (
        echo Error: Failed to restore NuGet packages
        pause
        exit /b 1
    )
    echo ✅ NuGet packages restored
    echo.
    
    echo Building solution...
    dotnet build WorkflowPlatform.sln --configuration Debug
    if %errorlevel% neq 0 (
        echo Error: Failed to build solution
        pause
        exit /b 1
    )
    echo ✅ Solution built successfully
) else (
    echo Warning: WorkflowPlatform.sln not found
)

echo.

:: Setup Docker environment
echo ========================================
echo Setting up Docker Environment
echo ========================================
echo.

:: Check if Docker daemon is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: Docker daemon is not running
    echo Please start Docker Desktop and try again
    pause
    exit /b 1
)
echo ✅ Docker daemon is running

:: Create necessary directories
if not exist "logs" mkdir logs
if not exist "docker\postgres\data" mkdir docker\postgres\data
if not exist "docker\redis\data" mkdir docker\redis\data
if not exist "docker\grafana\data" mkdir docker\grafana\data
if not exist "docker\prometheus\data" mkdir docker\prometheus\data

echo ✅ Docker directories created
echo.

:: Setup secrets
echo ========================================
echo Setting up Development Secrets
echo ========================================
echo.

if exist "src\WorkflowPlatform.API\WorkflowPlatform.API.csproj" (
    cd src\WorkflowPlatform.API
    
    echo Initializing user secrets...
    dotnet user-secrets init >nul 2>&1
    
    echo Configuring development secrets...
    
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=WorkflowPlatformDev;Username=workflow_user;Password=YOUR_SECURE_POSTGRES_PASSWORD;Port=5432"
    dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379,password=YOUR_SECURE_REDIS_PASSWORD"
    dotnet user-secrets set "ConnectionStrings:RabbitMQ" "amqp://workflow_user:YOUR_SECURE_RABBITMQ_PASSWORD@localhost:5672/workflow_dev"
    
    dotnet user-secrets set "Jwt:Secret" "YOUR_SECURE_JWT_SECRET_MINIMUM_256_BITS_CHANGE_THIS_IN_PRODUCTION"
    dotnet user-secrets set "Jwt:Issuer" "WorkflowPlatform-Development"
    dotnet user-secrets set "Jwt:Audience" "WorkflowPlatform-Development-Users"
    dotnet user-secrets set "Jwt:ExpirationHours" "24"
    
    cd ..\..
    echo ✅ Development secrets configured
) else (
    echo Warning: API project not found, skipping secret setup
)

echo.

:: Start services
echo ========================================
echo Starting Development Services
echo ========================================
echo.

echo Stopping any existing services...
docker-compose down --remove-orphans >nul 2>&1

echo Starting infrastructure services...
docker-compose up -d postgres redis rabbitmq
if %errorlevel% neq 0 (
    echo Error: Failed to start infrastructure services
    pause
    exit /b 1
)

echo Waiting for services to be ready...
echo.

:: Wait for PostgreSQL
echo Waiting for PostgreSQL...
set timeout=30
:wait_postgres
docker-compose exec -T postgres pg_isready -U workflow_user -d WorkflowPlatformDev >nul 2>&1
if %errorlevel% equ 0 goto postgres_ready
set /a timeout=%timeout%-1
if %timeout% leq 0 (
    echo Error: PostgreSQL failed to start within 30 seconds
    pause
    exit /b 1
)
timeout /t 1 /nobreak >nul
goto wait_postgres
:postgres_ready
echo ✅ PostgreSQL is ready

:: Wait for Redis
echo Waiting for Redis...
set timeout=30
:wait_redis
docker-compose exec -T redis redis-cli ping >nul 2>&1
if %errorlevel% equ 0 goto redis_ready
set /a timeout=%timeout%-1
if %timeout% leq 0 (
    echo Error: Redis failed to start within 30 seconds
    pause
    exit /b 1
)
timeout /t 1 /nobreak >nul
goto wait_redis
:redis_ready
echo ✅ Redis is ready

:: Wait for RabbitMQ
echo Waiting for RabbitMQ...
set timeout=60
:wait_rabbitmq
docker-compose exec -T rabbitmq rabbitmq-diagnostics -q ping >nul 2>&1
if %errorlevel% equ 0 goto rabbitmq_ready
set /a timeout=%timeout%-1
if %timeout% leq 0 (
    echo Error: RabbitMQ failed to start within 60 seconds
    pause
    exit /b 1
)
timeout /t 1 /nobreak >nul
goto wait_rabbitmq
:rabbitmq_ready
echo ✅ RabbitMQ is ready

echo.
echo ✅ All infrastructure services are running

echo.

:: Display environment information
echo ========================================
echo Development Environment Information
echo ========================================
echo.
echo 🌐 Service URLs:
echo   • API: http://localhost:5000
echo   • Frontend: http://localhost:3000 (when started)
echo   • PostgreSQL: localhost:5432
echo   • Redis: localhost:6379
echo   • RabbitMQ Management: http://localhost:15672
echo   • Prometheus: http://localhost:9090
echo   • Grafana: http://localhost:3001
echo.
echo 🔑 Default Credentials:
echo   • Database: workflow_user / [FROM .ENV FILE]
echo   • RabbitMQ: workflow_user / [FROM .ENV FILE]  
echo   • Redis: [FROM .ENV FILE]
echo   • Grafana: admin / [FROM .ENV FILE]
echo.
echo 🚀 Next Steps:
echo   1. Start the API: cd src\WorkflowPlatform.API ^&^& dotnet run
echo   2. Start the frontend: cd workflow-platform-frontend ^&^& npm install ^&^& npm run dev
echo   3. Start monitoring: docker-compose up -d prometheus grafana
echo   4. Access the application at http://localhost:3000
echo.
echo 🛠  Useful Commands:
echo   • View logs: docker-compose logs -f [service-name]
echo   • Stop services: docker-compose down
echo   • Reset database: docker-compose down -v ^&^& setup-dev.bat
echo   • Build API: dotnet build WorkflowPlatform.sln
echo   • Run tests: dotnet test WorkflowPlatform.sln
echo.

echo ========================================
echo ✅ Development environment setup completed!
echo ========================================
echo.
pause
