#!/bin/bash

# DotNet Ecuador API Deployment Script
# Run this script on your Ubuntu VPS

set -e

# Configuration
APP_NAME="dotnetecuador-api"
APP_PATH="/var/www/$APP_NAME"
SERVICE_FILE="/etc/systemd/system/$APP_NAME.service"
USER="www-data"

echo "🚀 Starting deployment of DotNet Ecuador API..."

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo "❌ Please run as root (use sudo)"
    exit 1
fi

# Install .NET 8 if not installed
if ! command -v dotnet &> /dev/null; then
    echo "📦 Installing .NET 8..."
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    apt-get update
    apt-get install -y dotnet-sdk-8.0
fi

# Install MongoDB if not installed
if ! command -v mongod &> /dev/null; then
    echo "📦 Installing MongoDB..."
    wget -qO - https://www.mongodb.org/static/pgp/server-7.0.asc | gpg --dearmor > /usr/share/keyrings/mongodb-server-7.0.gpg
    echo "deb [ arch=amd64,arm64 signed-by=/usr/share/keyrings/mongodb-server-7.0.gpg ] https://repo.mongodb.org/apt/ubuntu $(lsb_release -cs)/mongodb-org/7.0 multiverse" | tee /etc/apt/sources.list.d/mongodb-org-7.0.list
    apt-get update
    apt-get install -y mongodb-org
    systemctl start mongod
    systemctl enable mongod
fi

# Create app directory
echo "📁 Creating application directory..."
mkdir -p $APP_PATH
chown $USER:$USER $APP_PATH

# Stop existing service if running
if systemctl is-active --quiet $APP_NAME; then
    echo "🛑 Stopping existing service..."
    systemctl stop $APP_NAME
fi

# Build and publish the application
echo "🏗️  Building application..."
dotnet publish DotNetEcuador.API/DotNetEcuador.API.csproj -c Release -o $APP_PATH

# Set permissions
chown -R $USER:$USER $APP_PATH
chmod +x $APP_PATH/DotNetEcuador.API.dll

# Copy and configure systemd service
echo "⚙️  Configuring systemd service..."
cp deployment/dotnetecuador-api.service $SERVICE_FILE

# Prompt for MongoDB connection string
read -p "🔗 Enter your MongoDB connection string (press Enter for localhost default): " MONGO_CONN
if [ -z "$MONGO_CONN" ]; then
    MONGO_CONN="mongodb://localhost:27017/dotnet_ecuador"
fi

# Update service file with connection string
sed -i "s|Environment=MONGO_CONNECTION_STRING=.*|Environment=MONGO_CONNECTION_STRING=$MONGO_CONN|" $SERVICE_FILE
sed -i "s|WorkingDirectory=.*|WorkingDirectory=$APP_PATH|" $SERVICE_FILE

# Reload systemd and start service
systemctl daemon-reload
systemctl enable $APP_NAME
systemctl start $APP_NAME

# Configure nginx reverse proxy
if command -v nginx &> /dev/null; then
    echo "🌐 Configuring Nginx..."
    cat > /etc/nginx/sites-available/$APP_NAME << EOF
server {
    listen 80;
    server_name _;
    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF
    
    ln -sf /etc/nginx/sites-available/$APP_NAME /etc/nginx/sites-enabled/
    rm -f /etc/nginx/sites-enabled/default
    systemctl reload nginx
fi

# Check service status
echo "✅ Deployment completed!"
echo "📊 Service status:"
systemctl status $APP_NAME --no-pager
echo ""
echo "🔗 API should be available at: http://your-vps-ip/swagger"
echo "📝 Check logs with: journalctl -u $APP_NAME -f"