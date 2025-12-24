#!/bin/bash
# Complete N8N Deployment Script for VPS
# Run this directly on your VPS: bash complete-deploy.sh

set -e

echo "?? Starting N8N Complete Deployment"
echo "===================================="
echo ""

# Step 1: Clean up existing containers
echo "?? Cleaning up old containers..."
docker stop certbot 2>/dev/null && docker rm certbot 2>/dev/null || true
docker ps -a | grep -E "n8n|nginx|certbot" || echo "? No conflicting containers found"
echo ""

# Step 2: Create deployment directory
echo "?? Setting up deployment directory..."
mkdir -p /opt/n8n-deploy
cd /opt/n8n-deploy
echo "? Working directory: $(pwd)"
echo ""

# Step 3: Download files
echo "?? Downloading configuration files..."
curl -sSL -O https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/docker-compose.yml
curl -sSL -O https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/.env.example
mkdir -p nginx/conf.d
curl -sSL -o nginx/conf.d/n8n.conf https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/nginx/conf.d/n8n.conf
echo "? Files downloaded"
echo ""

# Step 4: Setup environment
echo "?? Setting up environment..."
if [ ! -f .env ]; then
    cp .env.example .env
    echo "??  IMPORTANT: You need to edit .env with your Cosmos DB credentials!"
    echo "   Run: nano .env"
    echo ""
    read -p "Press Enter to edit .env now, or Ctrl+C to do it later..."
    nano .env
else
    echo "? .env file already exists"
fi
echo ""

# Step 5: Deploy containers
echo "?? Starting Docker containers..."
docker-compose down 2>/dev/null || true
docker-compose up -d
echo ""

# Step 6: Wait for containers to start
echo "? Waiting for containers to start (10 seconds)..."
sleep 10
echo ""

# Step 7: Check status
echo "?? Container status:"
docker-compose ps
echo ""

# Step 8: Next steps
echo "? Deployment complete!"
echo ""
echo "?? Next Steps:"
echo ""
echo "1. Get SSL certificate:"
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
