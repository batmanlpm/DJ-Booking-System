#!/bin/bash

# N8N Automated Deployment Script
# This script will deploy N8N with all configurations automatically

set -e

echo "?? Starting N8N Automated Deployment"
echo "===================================="
echo ""

# Step 1: Clean up old containers
echo "?? Cleaning up old N8N containers..."
docker stop n8n nginx certbot 2>/dev/null || true
docker rm n8n nginx certbot 2>/dev/null || true
echo "? Cleanup complete"
echo ""

# Step 2: Create deployment directory
echo "?? Creating deployment directory..."
mkdir -p /opt/n8n-deploy
cd /opt/n8n-deploy
echo "? Working in: $(pwd)"
echo ""

# Step 3: Download files
echo "?? Downloading configuration files..."
curl -sSL -O https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/docker-compose.yml
mkdir -p nginx/conf.d
curl -sSL -o nginx/conf.d/n8n.conf https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/nginx/conf.d/n8n.conf
echo "? Files downloaded"
echo ""

# Step 4: Create .env file with credentials
echo "?? Creating .env file with Cosmos DB credentials..."
cat > .env << 'EOF'
# Cosmos DB Credentials
COSMOS_ENDPOINT=https://fallen-collective.documents.azure.com:443/
COSMOS_KEY=EpxIq3hV8kXQ7kNY1KKJQmL5dkX0uZeW4GMUinPf6hNqRApx84Co5Ffve0bAktpyzH2xho5swBV5ACDbeunr5Q==

# N8N Authentication
N8N_BASIC_AUTH_USER=admin
N8N_BASIC_AUTH_PASSWORD=YourSecurePassword123

# Domain Configuration
N8N_DOMAIN=n8n.thefallencollective.live

# Email for SSL Certificate
SSL_EMAIL=admin@thefallencollective.live

# Timezone
TZ=UTC
EOF
echo "? .env file created with Cosmos DB credentials"
echo ""

# Step 5: Deploy containers
echo "?? Starting Docker containers..."
docker-compose down 2>/dev/null || true
docker-compose up -d
echo "? Containers started"
echo ""

# Step 6: Wait for N8N to be ready
echo "? Waiting for N8N to start (30 seconds)..."
sleep 30
echo ""

# Step 7: Check status
echo "?? Container status:"
docker-compose ps
echo ""

echo "? Deployment complete!"
echo ""
echo "?? Next Steps:"
echo ""
echo "1. Get SSL certificate:"
echo "   cd /opt/n8n-deploy"
echo "   docker-compose run --rm certbot certonly --webroot --webroot-path=/var/www/certbot -d n8n.thefallencollective.live --email admin@thefallencollective.live --agree-tos --no-eff-email"
echo ""
echo "2. Restart Nginx:"
echo "   docker-compose restart nginx"
echo ""
echo "3. Access N8N:"
echo "   https://n8n.thefallencollective.live"
echo "   Username: admin"
echo "   Password: YourSecurePassword123"
echo ""
echo "4. Import workflow:"
echo "   Download: https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/N8N_Workflows/workflow-1-sync-db-to-agent.json"
echo "   In N8N: Workflows ? Import from File"
echo ""
echo "?? Done!"
