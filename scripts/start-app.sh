#!/bin/bash

# Script to start the DotNet Ecuador API application

SERVICE_NAME="dotnet-ecuador-api"

echo "▶️ Starting $SERVICE_NAME service..."

# Start the systemd service
sudo systemctl start $SERVICE_NAME

# Wait for service to initialize
sleep 5

# Check if service started successfully
if sudo systemctl is-active --quiet $SERVICE_NAME; then
    echo "✅ Service started successfully"
    
    # Show service status
    sudo systemctl status $SERVICE_NAME --no-pager -l
    
    # Wait a bit more for the web server to be ready
    echo "⏳ Waiting for web server to be ready..."
    sleep 10
    
    # Test HTTP endpoint
    echo "🔍 Testing health endpoint..."
    for i in {1..10}; do
        if curl -s http://localhost:5000/health > /dev/null; then
            echo "✅ Application is responding on http://localhost:5000"
            break
        else
            if [ $i -eq 10 ]; then
                echo "⚠️ Application may not be fully ready yet"
            else
                echo "⏳ Waiting for application to respond... ($i/10)"
                sleep 3
            fi
        fi
    done
else
    echo "❌ Failed to start service"
    echo "📋 Service logs:"
    sudo journalctl -u $SERVICE_NAME --no-pager -l --since "1 minute ago"
    exit 1
fi

echo "🎉 Application started successfully!"