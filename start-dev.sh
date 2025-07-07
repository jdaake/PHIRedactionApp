#!/bin/bash

echo "🛠️  PHI Redaction App - Development Mode"
echo "========================================"

# Function to handle cleanup
cleanup() {
    echo ""
    echo "🛑 Stopping development servers..."
    
    if [ ! -z "$BACKEND_PID" ]; then
        kill $BACKEND_PID 2>/dev/null
    fi
    
    if [ ! -z "$FRONTEND_PID" ]; then
        kill $FRONTEND_PID 2>/dev/null
    fi
    
    # Kill any remaining processes
    sudo lsof -ti:5001,4200 | xargs -r kill -9 2>/dev/null || true
    
    echo "✅ Cleanup complete"
    exit 0
}

trap cleanup SIGINT SIGTERM

# Kill existing processes
echo "🧹 Cleaning up existing processes..."
sudo lsof -ti:4200,5001 | xargs -r kill -9 2>/dev/null || true

# Start backend with hot reload
echo "🔧 Starting backend with hot reload..."
cd PHIRedactionAPI
dotnet restore >/dev/null 2>&1
dotnet run --urls="http://localhost:5001" &
BACKEND_PID=$!
cd ..

# Wait a bit for backend to start
sleep 5

# Start frontend with hot reload
echo "🌐 Starting frontend with hot reload..."
cd phi-redaction-app
npm install >/dev/null 2>&1
ng serve --host 0.0.0.0 --port 4200 --disable-host-check &
FRONTEND_PID=$!
cd ..

echo ""
echo "🛠️  Development servers starting..."
echo "⏳ Please wait 20-30 seconds for full startup..."
echo ""
echo "📍 Services will be available at:"
echo "   🌐 Frontend: http://localhost:4200"
echo "   🔧 Backend: http://localhost:5001"
echo "   📊 API Docs: http://localhost:5001/swagger"
echo ""
echo "🔄 Hot reload enabled:"
echo "   • Backend: Changes auto-reload"
echo "   • Frontend: Changes auto-refresh browser"
echo ""
echo "🛑 Press Ctrl+C to stop all services"

# Keep script running
while true; do
    sleep 5
    
    # Check if both processes are still running
    if ! kill -0 $BACKEND_PID 2>/dev/null || ! kill -0 $FRONTEND_PID 2>/dev/null; then
        echo "❌ One or more services stopped"
        cleanup
    fi
done
