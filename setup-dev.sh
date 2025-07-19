#!/bin/bash

# WorkflowPlatform Development Environment Setup Script
# This script sets up the complete development environment

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Helper functions
print_header() {
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Main setup function
main() {
    print_header "WorkflowPlatform Development Environment Setup"
    
    # Check system requirements
    check_requirements
    
    # Setup .NET environment
    setup_dotnet_environment
    
    # Setup Docker environment
    setup_docker_environment
    
    # Setup secrets
    setup_secrets
    
    # Start services
    start_services
    
    # Run initial setup
    run_initial_setup
    
    print_success "Development environment setup completed!"
    print_environment_info
}

check_requirements() {
    print_header "Checking System Requirements"
    
    local missing_deps=()
    
    if ! command_exists "dotnet"; then
        missing_deps+=("dotnet (.NET 8 SDK)")
    else
        dotnet_version=$(dotnet --version)
        if [[ ! "$dotnet_version" =~ ^8\. ]]; then
            print_warning "Found .NET version $dotnet_version, but .NET 8 is recommended"
        else
            print_success ".NET 8 SDK found: $dotnet_version"
        fi
    fi
    
    if ! command_exists "docker"; then
        missing_deps+=("docker")
    else
        print_success "Docker found: $(docker --version)"
    fi
    
    if ! command_exists "docker-compose"; then
        missing_deps+=("docker-compose")
    else
        print_success "Docker Compose found: $(docker-compose --version)"
    fi
    
    if ! command_exists "git"; then
        missing_deps+=("git")
    else
        print_success "Git found: $(git --version)"
    fi
    
    if ! command_exists "curl"; then
        missing_deps+=("curl")
    else
        print_success "curl found"
    fi
    
    if [ ${#missing_deps[@]} -ne 0 ]; then
        print_error "Missing required dependencies:"
        for dep in "${missing_deps[@]}"; do
            echo "  - $dep"
        done
        echo ""
        echo "Please install the missing dependencies and run this script again."
        exit 1
    fi
    
    print_success "All system requirements are met"
}

setup_dotnet_environment() {
    print_header "Setting up .NET Environment"
    
    # Restore NuGet packages
    if [ -f "WorkflowPlatform.sln" ]; then
        print_info "Restoring NuGet packages..."
        dotnet restore WorkflowPlatform.sln
        print_success "NuGet packages restored"
    else
        print_warning "WorkflowPlatform.sln not found, skipping package restoration"
    fi
    
    # Build solution
    print_info "Building solution..."
    if dotnet build WorkflowPlatform.sln --configuration Debug; then
        print_success "Solution built successfully"
    else
        print_error "Failed to build solution"
        exit 1
    fi
}

setup_docker_environment() {
    print_header "Setting up Docker Environment"
    
    # Check if Docker daemon is running
    if ! docker info >/dev/null 2>&1; then
        print_error "Docker daemon is not running. Please start Docker and try again."
        exit 1
    fi
    
    print_success "Docker daemon is running"
    
    # Create necessary directories
    mkdir -p logs
    mkdir -p docker/postgres/data
    mkdir -p docker/redis/data
    mkdir -p docker/grafana/data
    mkdir -p docker/prometheus/data
    
    print_success "Docker directories created"
}

setup_secrets() {
    print_header "Setting up Development Secrets"
    
    # Check if user secrets are initialized
    if [ -f "src/WorkflowPlatform.API/WorkflowPlatform.API.csproj" ]; then
        cd src/WorkflowPlatform.API
        
        # Initialize user secrets if not already done
        if ! dotnet user-secrets list >/dev/null 2>&1; then
            print_info "Initializing user secrets..."
            dotnet user-secrets init
        fi
        
        # Set development secrets (use secure values in production)
        print_info "Configuring development secrets..."
        
        dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=WorkflowPlatformDev;Username=workflow_user;Password=YOUR_SECURE_POSTGRES_PASSWORD;Port=5432"
        dotnet user-secrets set "ConnectionStrings:Redis" "localhost:6379,password=YOUR_SECURE_REDIS_PASSWORD"
        dotnet user-secrets set "ConnectionStrings:RabbitMQ" "amqp://workflow_user:YOUR_SECURE_RABBITMQ_PASSWORD@localhost:5672/workflow_dev"
        
        dotnet user-secrets set "Jwt:Secret" "YOUR_SECURE_JWT_SECRET_MINIMUM_256_BITS_CHANGE_THIS_IN_PRODUCTION"
        dotnet user-secrets set "Jwt:Issuer" "WorkflowPlatform-Development"
        dotnet user-secrets set "Jwt:Audience" "WorkflowPlatform-Development-Users"
        dotnet user-secrets set "Jwt:ExpirationHours" "24"
        
        cd ../..
        print_success "Development secrets configured"
    else
        print_warning "API project not found, skipping secret setup"
    fi
}

start_services() {
    print_header "Starting Development Services"
    
    # Stop any existing services
    print_info "Stopping any existing services..."
    docker-compose down --remove-orphans >/dev/null 2>&1 || true
    
    # Start infrastructure services
    print_info "Starting infrastructure services..."
    docker-compose up -d postgres redis rabbitmq
    
    # Wait for services to be ready
    print_info "Waiting for services to be ready..."
    
    # Wait for PostgreSQL
    print_info "Waiting for PostgreSQL..."
    timeout=30
    while ! docker-compose exec -T postgres pg_isready -U workflow_user -d WorkflowPlatformDev >/dev/null 2>&1; do
        if [ $timeout -le 0 ]; then
            print_error "PostgreSQL failed to start within 30 seconds"
            exit 1
        fi
        sleep 1
        timeout=$((timeout-1))
    done
    print_success "PostgreSQL is ready"
    
    # Wait for Redis
    print_info "Waiting for Redis..."
    timeout=30
    while ! docker-compose exec -T redis redis-cli ping >/dev/null 2>&1; do
        if [ $timeout -le 0 ]; then
            print_error "Redis failed to start within 30 seconds"
            exit 1
        fi
        sleep 1
        timeout=$((timeout-1))
    done
    print_success "Redis is ready"
    
    # Wait for RabbitMQ
    print_info "Waiting for RabbitMQ..."
    timeout=60
    while ! docker-compose exec -T rabbitmq rabbitmq-diagnostics -q ping >/dev/null 2>&1; do
        if [ $timeout -le 0 ]; then
            print_error "RabbitMQ failed to start within 60 seconds"
            exit 1
        fi
        sleep 1
        timeout=$((timeout-1))
    done
    print_success "RabbitMQ is ready"
    
    print_success "All infrastructure services are running"
}

run_initial_setup() {
    print_header "Running Initial Setup"
    
    # Run database migrations (when implemented)
    print_info "Database migrations will be run when implemented..."
    
    # Seed initial data (when implemented)
    print_info "Initial data seeding will be run when implemented..."
    
    print_success "Initial setup completed"
}

print_environment_info() {
    print_header "Development Environment Information"
    
    echo "🌐 Service URLs:"
    echo "  • API: http://localhost:5000"
    echo "  • Frontend: http://localhost:3000 (when started)"
    echo "  • PostgreSQL: localhost:5432"
    echo "  • Redis: localhost:6379"
    echo "  • RabbitMQ Management: http://localhost:15672"
    echo "  • Prometheus: http://localhost:9090"
    echo "  • Grafana: http://localhost:3001"
    echo ""
    
    echo "🔑 Default Credentials:"
    echo "  • Database: workflow_user / [FROM .ENV FILE]"
    echo "  • RabbitMQ: workflow_user / [FROM .ENV FILE]"
    echo "  • Redis: [FROM .ENV FILE]"
    echo "  • Grafana: admin / [FROM .ENV FILE]"
    echo ""
    
    echo "🚀 Next Steps:"
    echo "  1. Start the API: cd src/WorkflowPlatform.API && dotnet run"
    echo "  2. Start the frontend: cd workflow-platform-frontend && npm install && npm run dev"
    echo "  3. Start monitoring: docker-compose up -d prometheus grafana"
    echo "  4. Access the application at http://localhost:3000"
    echo ""
    
    echo "🛠  Useful Commands:"
    echo "  • View logs: docker-compose logs -f [service-name]"
    echo "  • Stop services: docker-compose down"
    echo "  • Reset database: docker-compose down -v && ./setup-dev.sh"
    echo "  • Build API: dotnet build WorkflowPlatform.sln"
    echo "  • Run tests: dotnet test WorkflowPlatform.sln"
}

# Handle script interruption
trap 'print_error "Setup interrupted"; exit 1' INT TERM

# Run main function
main "$@"
