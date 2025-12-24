#!/bin/bash
# Complete SSL Setup for N8N

set -e

echo "?? Setting up SSL for N8N"
echo "=========================="
echo ""

cd /opt/n8n-deploy

# Step 1: Update DNS check
echo "?? Checking DNS for n8n.thefallencollective.live..."
CURRENT_IP=$(curl -s ifconfig.me)
DNS_IP=$(dig +short n8n.thefallencollective.live | head -n1)

echo "   VPS IP: $CURRENT_IP"
echo "   DNS IP: $DNS_IP"
echo ""

if [ "$CURRENT_IP" != "$DNS_IP" ]; then
    echo "??  WARNING: DNS not pointing to this server!"
    echo "   Please update your DNS A record:"
    echo "   Type: A"
    echo "   Name: n8n"
    echo "   Value: $CURRENT_IP"
    echo "   TTL: 300"
    echo ""
    echo "   Then wait 5-10 minutes and run this script again."
    exit 1
fi

echo "? DNS correctly configured!"
echo ""

# Step 2: Get SSL certificate
echo "?? Requesting SSL certificate from Let's Encrypt..."
docker compose run --rm certbot certonly \
    --webroot \
    --webroot-path=/var/www/certbot \
    -d n8n.thefallencollective.live \
    --email admin@thefallencollective.live \
    --agree-tos \
    --no-eff-email \
    --force-renewal

if [ $? -ne 0 ]; then
    echo "? Failed to get SSL certificate"
    echo "   Make sure nginx is running and port 80 is accessible"
    exit 1
fi

echo "? SSL certificate obtained!"
echo ""

# Step 3: Update nginx config for HTTPS
echo "?? Updating nginx configuration for HTTPS..."
cat > nginx/conf.d/n8n.conf << 'EOF'
# HTTP - Redirect to HTTPS
server {
    listen 80;
    server_name n8n.thefallencollective.live;
    
    # Let's Encrypt challenge
    location /.well-known/acme-challenge/ {
        root /var/www/certbot;
    }
    
    # Redirect all other traffic to HTTPS
    location / {
        return 301 https://$host$request_uri;
    }
}

# HTTPS - N8N
server {
    listen 443 ssl http2;
    server_name n8n.thefallencollective.live;
    
    # SSL Certificate
    ssl_certificate /etc/letsencrypt/live/n8n.thefallencollective.live/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/n8n.thefallencollective.live/privkey.pem;
    
    # SSL Configuration
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers off;
    
    # Security Headers
    add_header Strict-Transport-Security "max-age=31536000" always;
    
    # Proxy to N8N
    location / {
        proxy_pass http://n8n:5678;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_connect_timeout 300s;
        proxy_send_timeout 300s;
        proxy_read_timeout 300s;
    }
}
EOF

echo "? Nginx config updated"
echo ""

# Step 4: Update N8N environment for HTTPS
echo "?? Updating N8N configuration for HTTPS..."
sed -i 's/N8N_PROTOCOL=http/N8N_PROTOCOL=https/' .env
sed -i 's/N8N_SECURE_COOKIE=false/N8N_SECURE_COOKIE=true/' .env
sed -i 's|WEBHOOK_URL=http://|WEBHOOK_URL=https://|' .env

echo "? N8N config updated"
echo ""

# Step 5: Restart containers
echo "?? Restarting containers..."
docker compose restart nginx
docker compose restart n8n

echo "? Containers restarted"
echo ""

# Wait for containers to be ready
echo "? Waiting for services to start..."
sleep 10

# Step 6: Verify SSL
echo "?? Verifying SSL certificate..."
if curl -sSf https://n8n.thefallencollective.live > /dev/null 2>&1; then
    echo "? SSL is working!"
else
    echo "??  SSL verification failed, but certificate is installed"
    echo "   Check logs: docker compose logs nginx"
fi

echo ""
echo "=========================="
echo "? SSL SETUP COMPLETE!"
echo "=========================="
echo ""
echo "?? Access N8N at: https://n8n.thefallencollective.live"
echo ""
echo "?? Next steps:"
echo "   1. Clear your browser cache"
echo "   2. Access https://n8n.thefallencollective.live"
echo "   3. Import workflow: N8N_Workflows/workflow-1-sync-db-to-agent.json"
echo "   4. Activate workflow"
echo ""
echo "?? Done!"
