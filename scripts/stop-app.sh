#!/bin/bash

# Script to stop the DotNet Ecuador API application

SERVICE_NAME="dotnet-ecuador-api"

echo "⏹️ Stopping $SERVICE_NAME service..."

# Stop the systemd service
if sudo systemctl is-active --quiet $SERVICE_NAME; then
    sudo systemctl stop $SERVICE_NAME
    echo "✅ Service stopped successfully"
else
    echo "ℹ️ Service was not running"
fi

# Wait a moment to ensure clean shutdown
sleep 2

# Kill any remaining processes (fallback)
PIDS=$(pgrep -f "DotNetEcuador.API" || true)
if [ ! -z "$PIDS" ]; then
    echo "🔄 Cleaning up remaining processes..."
    echo $PIDS | sudo xargs kill -TERM || true
    sleep 3
    
    # Force kill if still running
    PIDS=$(pgrep -f "DotNetEcuador.API" || true)
    if [ ! -z "$PIDS" ]; then
        echo "💥 Force killing remaining processes..."
        echo $PIDS | sudo xargs kill -KILL || true
    fi
fi

echo "✅ Application stopped"