#!/bin/bash

echo "🚀 PHI Redaction App - Quick Start"
echo "=================================="

# Kill any existing processes
echo "🧹 Cleaning up..."
sudo lsof -ti:4200,5001 | xargs -r kill -9 2>/dev/null || true
docker-compose down 2>/dev/null || true

# Auto-detect best method
if command -v docker >/dev/null 2>&1 && docker info >/dev/null 2>&1; then
    echo "🐳 Using Docker..."
    docker-compose up --build -d
    
    echo "⏳ Waiting for services..."
    sleep 30
    
    echo "✅ Application ready!"
    echo "🌐 Frontend: http://localhost:4200"
    echo "🔧 Backend: http://localhost:5001"
    
elif command -v dotnet >/dev/null 2>&1 && dotnet --version >/dev/null 2>&1; then
    echo "💻 Using local .NET..."
    
    # Start backend
    cd PHIRedactionAPI
    dotnet run --urls="http://localhost:5001" &
    BACKEND_PID=$!
    cd ..
    
    # Start frontend
    cd phi-redaction-app
    npm install >/dev/null 2>&1
    npm start &
    FRONTEND_PID=$!
    cd ..
    
    echo "⏳ Waiting for services..."
    sleep 20
    
    echo "✅ Application ready!"
    echo "🌐 Frontend: http://localhost:4200"
    echo "🔧 Backend: http://localhost:5001"
    echo ""
    echo "🛑 To stop: kill $BACKEND_PID $FRONTEND_PID"
    
else
    echo "❌ Neither Docker nor .NET are working"
    echo "📋 Install Docker Desktop or fix .NET architecture"
    exit 1
fi
