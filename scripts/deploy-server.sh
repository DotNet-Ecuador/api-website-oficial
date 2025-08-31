#!/bin/bash

# Deploy script for DotNet Ecuador API
# This script runs on the VPS server during deployment

set -e

echo "🚀 Starting deployment process..."

# Configuration
APP_PATH=${APP_PATH:-/opt/dotnet-ecuador-api}
SERVICE_NAME="dotnet-ecuador-api"
TEMP_DIR="/tmp/dotnet-ecuador-deployment"
USER_NAME="www-data"

# Create application directory if it doesn't exist
if [ ! -d "$APP_PATH" ]; then
    echo "📁 Creating application directory: $APP_PATH"
    sudo mkdir -p "$APP_PATH"
    sudo chown -R $USER_NAME:$USER_NAME "$APP_PATH"
fi

# Create logs directory
sudo mkdir -p "$APP_PATH/logs"
sudo chown -R $USER_NAME:$USER_NAME "$APP_PATH/logs"

# Stop the application service
echo "⏹️ Stopping application service..."
./scripts/stop-app.sh

# Backup current version (if exists)
if [ -d "$APP_PATH/app" ]; then
    echo "💾 Creating backup of current version..."
    sudo cp -r "$APP_PATH/app" "$APP_PATH/app.backup.$(date +%Y%m%d_%H%M%S)" || true
    sudo rm -rf "$APP_PATH/app"
fi

# Copy new application files
echo "📦 Deploying new version..."
sudo mkdir -p "$APP_PATH/app"
sudo cp -r $TEMP_DIR/* "$APP_PATH/app/"
sudo chown -R $USER_NAME:$USER_NAME "$APP_PATH/app"

# Set executable permissions
sudo chmod +x "$APP_PATH/app/DotNetEcuador.API"

# Install/Update systemd service
if [ -f "$APP_PATH/app/systemd/dotnet-ecuador-api.service" ]; then
    echo "🔧 Installing systemd service..."
    sudo cp "$APP_PATH/app/systemd/dotnet-ecuador-api.service" /etc/systemd/system/
    sudo systemctl daemon-reload
    sudo systemctl enable $SERVICE_NAME
fi

# Create environment file for systemd
echo "🔐 Setting up environment variables..."
sudo tee /etc/systemd/system/$SERVICE_NAME.env > /dev/null <<EOF
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://localhost:5000
MONGO_CONNECTION_STRING=$MONGO_CONNECTION_STRING
JWT_SECRET_KEY=$JWT_SECRET_KEY
JWT_ISSUER=$JWT_ISSUER
JWT_AUDIENCE=$JWT_AUDIENCE
JWT_ACCESS_TOKEN_EXPIRATION=$JWT_ACCESS_TOKEN_EXPIRATION
JWT_REFRESH_TOKEN_EXPIRATION=$JWT_REFRESH_TOKEN_EXPIRATION
EOF

sudo chmod 600 /etc/systemd/system/$SERVICE_NAME.env
sudo chown root:root /etc/systemd/system/$SERVICE_NAME.env

# Start the application
echo "▶️ Starting application service..."
./scripts/start-app.sh

# Clean up old backups (keep last 3)
echo "🧹 Cleaning up old backups..."
cd "$APP_PATH"
sudo ls -t app.backup.* 2>/dev/null | tail -n +4 | sudo xargs rm -rf || true

echo "✅ Deployment completed successfully!"
echo "📊 Application is running at: http://localhost:5000"
echo "🔍 Check status with: systemctl status $SERVICE_NAME"