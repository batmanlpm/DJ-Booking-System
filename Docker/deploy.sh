#!/bin/bash

# N8N Docker Deployment Script
# Deploys N8N with Nginx reverse proxy and automatic SSL

set -e

echo "?? N8N Docker Deployment Script"
echo "================================"
echo ""

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo "? Please run as root (use sudo)"
    exit 1
fi

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "? Docker is not installed"
    echo "Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    systemctl enable docker
    systemctl start docker
    echo "? Docker installed"
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "? Docker Compose is not installed"
    echo "Installing Docker Compose..."
    curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
    echo "? Docker Compose installed"
fi

# Create .env file if it doesn't exist
if [ ! -f .env ]; then
    echo "?? Creating .env file from template..."
    cp .env.example .env
    echo "??  IMPORTANT: Edit .env file with your Cosmos DB credentials!"
    echo "   Run: nano .env"
    echo ""
    read -p "Press Enter after you've edited .env file..."
fi

# Load environment variables
source .env

# Validate required variables
if [ "$COSMOS_ENDPOINT" = "https://your-account.documents.azure.com" ]; then
    echo "? Please configure COSMOS_ENDPOINT in .env file"
    exit 1
fi

if [ "$COSMOS_KEY" = "your_cosmos_primary_key_here" ]; then
    echo "? Please configure COSMOS_KEY in .env file"
    exit 1
fi

echo "? Environment variables configured"
echo ""

# Stop existing containers
echo "?? Stopping existing containers..."
docker-compose down 2>/dev/null || true

# Pull latest images
echo "?? Pulling latest images..."
docker-compose pull

# Start containers
echo "?? Starting containers..."
docker-compose up -d

# Wait for N8N to be ready
echo "? Waiting for N8N to start..."
sleep 10

# Check if N8N is running
if docker ps | grep -q n8n; then
    echo "? N8N container is running"
else
    echo "? N8N container failed to start"
    echo "Checking logs:"
    docker-compose logs n8n
    exit 1
fi

echo ""
echo "?? Deployment Complete!"
echo ""
echo "?? Next Steps:"
echo "1. Obtain SSL certificate:"
echo "   docker-compose run --rm certbot certonly --webroot --webroot-path=/var/www/certbot -d $N8N_DOMAIN --email $SSL_EMAIL --agree-tos --no-eff-email"
echo ""
echo "2. Restart Nginx:"
echo "   docker-compose restart nginx"
echo ""
echo "3. Access N8N:"
echo "   https://$N8N_DOMAIN"
echo "   Username: $N8N_BASIC_AUTH_USER"
echo "   Password: $N8N_BASIC_AUTH_PASSWORD"
echo ""
echo "4. Import workflow:"
echo "   - Download: N8N_Workflows/workflow-1-sync-db-to-agent.json"
echo "   - In N8N: Workflows ? Import from File"
echo ""
echo "?? Useful Commands:"
echo "   docker-compose logs -f n8n    # View N8N logs"
echo "   docker-compose ps             # Check container status"
echo "   docker-compose down           # Stop all containers"
echo "   docker-compose up -d          # Start all containers"
echo ""
