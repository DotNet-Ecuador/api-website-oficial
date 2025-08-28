#!/bin/bash

# DotNet Ecuador API Update Script
# Use this for quick updates after initial deployment

set -e

APP_NAME="dotnetecuador-api"
APP_PATH="/var/www/$APP_NAME"

echo "🔄 Updating DotNet Ecuador API..."

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo "❌ Please run as root (use sudo)"
    exit 1
fi

# Stop service
echo "🛑 Stopping service..."
systemctl stop $APP_NAME

# Build and publish
echo "🏗️  Building application..."
dotnet publish DotNetEcuador.API/DotNetEcuador.API.csproj -c Release -o $APP_PATH

# Set permissions
chown -R www-data:www-data $APP_PATH

# Start service
echo "▶️  Starting service..."
systemctl start $APP_NAME

echo "✅ Update completed!"
systemctl status $APP_NAME --no-pager