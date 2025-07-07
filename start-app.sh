#!/bin/bash

echo "🏥 PHI Redaction Application - One-Click Startup"
echo "================================================"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

# Function to check if a port is in use
check_port() {
    local port=$1
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        return 0  # Port is in use
    else
        return 1  # Port is free
    fi
}

# Function to kill process on port
kill_port() {
    local port=$1
    local pid=$(lsof -ti:$port 2>/dev/null)
    if [ ! -z "$pid" ]; then
        print_warning "Killing process $pid on port $port"
        kill -9 $pid 2>/dev/null || sudo kill -9 $pid 2>/dev/null
        sleep 2
    fi
}

# Function to wait for service to be ready
wait_for_service() {
    local url=$1
    local name=$2
    local max_attempts=30
    local attempt=1
    
    print_info "Waiting for $name to start..."
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s "$url" >/dev/null 2>&1; then
            print_status "$name is ready!"
            return 0
        fi
        
        printf "."
        sleep 2
        attempt=$((attempt + 1))
    done
    
    print_error "$name failed to start after $max_attempts attempts"
    return 1
}

# Cleanup function
cleanup() {
    print_warning "Shutting down services..."
    
    # Kill background processes
    if [ ! -z "$BACKEND_PID" ]; then
        kill $BACKEND_PID 2>/dev/null
    fi
    
    if [ ! -z "$FRONTEND_PID" ]; then
        kill $FRONTEND_PID 2>/dev/null
    fi
    
    # Kill any remaining processes on our ports
    kill_port 5001
    kill_port 4200
    
    print_status "Cleanup complete"
    exit 0
}

# Set up signal handlers
trap cleanup SIGINT SIGTERM

echo "🔍 Pre-flight checks..."

# Check system architecture and .NET availability
ARCH=$(uname -m)
print_info "System architecture: $ARCH"

# Check if Docker is available and running
DOCKER_AVAILABLE=false
if command -v docker >/dev/null 2>&1 && docker info >/dev/null 2>&1; then
    DOCKER_AVAILABLE=true
    print_status "Docker is available and running"
else
    print_warning "Docker not available"
fi

# Check if .NET is working
DOTNET_WORKING=false
if command -v dotnet >/dev/null 2>&1 && dotnet --version >/dev/null 2>&1; then
    DOTNET_WORKING=true
    print_status ".NET is working: $(dotnet --version)"
else
    print_error ".NET not working (likely architecture mismatch)"
fi

# Check if Node.js is available
NODE_AVAILABLE=false
if command -v node >/dev/null 2>&1 && command -v npm >/dev/null 2>&1; then
    NODE_AVAILABLE=true
    print_status "Node.js available: $(node --version)"
else
    print_warning "Node.js not available"
fi

echo ""
echo "🎯 Startup method selection..."

# Determine best startup method
if [ "$DOCKER_AVAILABLE" = true ]; then
    echo "1️⃣  Docker (Recommended - handles all issues automatically)"
    echo "2️⃣  Local (.NET + Node.js)"
    echo "3️⃣  Auto-detect best method"
    
    read -p "Choose startup method [1-3]: " method
    
    case $method in
        1|"")
            STARTUP_METHOD="docker"
            ;;
        2)
            STARTUP_METHOD="local"
            ;;
        3)
            if [ "$DOTNET_WORKING" = true ] && [ "$NODE_AVAILABLE" = true ]; then
                STARTUP_METHOD="local"
            else
                STARTUP_METHOD="docker"
            fi
            ;;
        *)
            print_error "Invalid choice, using Docker"
            STARTUP_METHOD="docker"
            ;;
    esac
else
    if [ "$DOTNET_WORKING" = true ] && [ "$NODE_AVAILABLE" = true ]; then
        STARTUP_METHOD="local"
        print_info "Using local startup (Docker not available)"
    else
        print_error "Neither Docker nor local .NET/Node.js are working!"
        echo ""
        echo "📋 To fix this:"
        echo "   • Install Docker Desktop, or"
        echo "   • Install .NET 6 ARM64 and Node.js"
        exit 1
    fi
fi

echo ""
echo "🚀 Starting PHI Redaction Application using: $STARTUP_METHOD"
echo ""

if [ "$STARTUP_METHOD" = "docker" ]; then
    # Docker startup
    print_info "Using Docker startup method..."
    
    # Clean up any existing containers
    print_info "Cleaning up existing containers..."
    docker-compose down 2>/dev/null || true
    
    # Kill any processes on our ports
    kill_port 4200
    kill_port 5001
    
    # Start with Docker Compose
    print_info "Building and starting containers..."
    if docker-compose up --build -d; then
        print_status "Docker containers starting..."
        
        # Wait for services
        wait_for_service "http://localhost:5001/health" "Backend API"
        wait_for_service "http://localhost:4200" "Frontend"
        
        echo ""
        print_status "🎉 Application is running!"
        echo ""
        echo "🌐 Frontend: http://localhost:4200"
        echo "🔧 Backend API: http://localhost:5001"
        echo "📊 API Documentation: http://localhost:5001/swagger"
        echo ""
        print_info "Press Ctrl+C to stop all services"
        
        # Keep script running
        while true; do
            sleep 5
            # Check if containers are still running
            if ! docker-compose ps | grep -q "Up"; then
                print_error "Containers stopped unexpectedly"
                break
            fi
        done
        
    else
        print_error "Failed to start Docker containers"
        exit 1
    fi

else
    # Local startup
    print_info "Using local startup method..."
    
    # Kill any existing processes
    kill_port 5001
    kill_port 4200
    
    # Start backend
    print_info "Starting backend (.NET API)..."
    cd PHIRedactionAPI
    
    # Check if we need to restore packages
    if [ ! -d "bin" ]; then
        print_info "Restoring .NET packages..."
        dotnet restore
    fi
    
    # Start backend in background
    dotnet run --urls="http://localhost:5001" > ../backend.log 2>&1 &
    BACKEND_PID=$!
    cd ..
    
    # Wait for backend to start
    wait_for_service "http://localhost:5001/health" "Backend API"
    
    # Start frontend
    print_info "Starting frontend (Angular)..."
    cd phi-redaction-app
    
    # Install npm packages if needed
    if [ ! -d "node_modules" ]; then
        print_info "Installing npm packages..."
        npm install
    fi
    
    # Start frontend in background
    npm start > ../frontend.log 2>&1 &
    FRONTEND_PID=$!
    cd ..
    
    # Wait for frontend to start
    wait_for_service "http://localhost:4200" "Frontend"
    
    echo ""
    print_status "🎉 Application is running!"
    echo ""
    echo "🌐 Frontend: http://localhost:4200"
    echo "🔧 Backend API: http://localhost:5001"
    echo "📊 API Documentation: http://localhost:5001/swagger"
    echo ""
    echo "📋 Logs:"
    echo "   Backend: tail -f backend.log"
    echo "   Frontend: tail -f frontend.log"
    echo ""
    print_info "Press Ctrl+C to stop all services"
    
    # Keep script running and show live logs
    while true; do
        sleep 5
        
        # Check if processes are still running
        if ! kill -0 $BACKEND_PID 2>/dev/null; then
            print_error "Backend process stopped"
            break
        fi
        
        if ! kill -0 $FRONTEND_PID 2>/dev/null; then
            print_error "Frontend process stopped"
            break
        fi
    done
fi

# Cleanup on exit
cleanup
