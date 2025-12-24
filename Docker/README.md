# Quick Deploy N8N on Your VPS

**One command to rule them all!** ??

## ?? Prerequisites:
- VPS with Ubuntu/Debian
- Domain: `n8n.thefallencollective.live` pointing to your server IP
- Cosmos DB credentials from Azure Portal

## ? Deploy Now:

```bash
# SSH into your VPS
ssh root@your-vps-ip

# Run one-command deployment
curl -fsSL https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/deploy.sh | sudo bash
```

## ?? What to do after running the command:

1. **Edit `.env` file** when prompted:
   ```bash
   nano .env
   ```
   
   Update these lines:
   ```env
   COSMOS_ENDPOINT=https://YOUR-ACCOUNT.documents.azure.com
   COSMOS_KEY=YOUR_PRIMARY_KEY_HERE
   SSL_EMAIL=your@email.com
   ```

2. **Get SSL certificate**:
   ```bash
   docker-compose run --rm certbot certonly --webroot --webroot-path=/var/www/certbot -d n8n.thefallencollective.live --email your@email.com --agree-tos --no-eff-email
   ```

3. **Restart Nginx**:
   ```bash
   docker-compose restart nginx
   ```

4. **Access N8N**:
   - URL: https://n8n.thefallencollective.live
   - Username: `admin`
   - Password: `YourSecurePassword123`

5. **Import workflow**:
   - Download `N8N_Workflows/workflow-1-sync-db-to-agent.json`
   - In N8N: Workflows ? Import from File
   - Execute to test
   - Toggle "Active"

## ? Done!

Your agent will now sync with Cosmos DB every hour automatically! ??

**Full documentation:** See `Docs/DOCKER_DEPLOYMENT_COMPLETE.md`
