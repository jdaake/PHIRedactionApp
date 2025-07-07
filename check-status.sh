#!/bin/bash

echo "📊 PHI Redaction App - Status Check"
echo "=================================="

check_service() {
    local url=$1
    local name=$2
    
    if curl -s "$url" >/dev/null 2>&1; then
        echo "✅ $name: Running"
    else
        echo "❌ $name: Not responding"
    fi
}

echo "🔍 Checking services..."
check_service "http://localhost:4200" "Frontend"
check_service "http://localhost:5001/health" "Backend API"
check_service "http://localhost:5001/swagger" "API Documentation"

echo ""
echo "🔌 Port usage:"
lsof -i:4200,5001 2>/dev/null | grep LISTEN || echo "No services listening on ports 4200, 5001"

echo ""
echo "🐳 Docker containers:"
docker-compose ps 2>/dev/null || echo "No Docker containers running"
