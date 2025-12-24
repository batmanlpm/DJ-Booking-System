# ?? ONE-COMMAND N8N DEPLOYMENT

## ? **COSMOS DB CREDENTIALS FOUND!**

Your Cosmos DB credentials from `App.xaml.cs`:
```
Endpoint: https://fallen-collective.documents.azure.com:443/
Key: EpxIq3hV8kXQ7kNY1KKJQmL5dkX0uZeW4GMUinPf6hNqRApx84Co5Ffve0bAktpyzH2xho5swBV5ACDbeunr5Q==
```

---

## ?? **DEPLOY NOW ON YOUR VPS:**

**In your VPS terminal (you're already SSH'd in as root@srv1208470), run:**

```bash
curl -sSL https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/auto-deploy.sh | bash
```

**That's it! This one command will:**
1. ? Clean up old containers
2. ? Download docker-compose.yml and nginx config
3. ? Create .env file with YOUR Cosmos DB credentials
4. ? Start all Docker containers (N8N + Nginx + Certbot)
5. ? Display next steps

---

## ?? **AFTER THE SCRIPT COMPLETES:**

### **Step 1: Get SSL Certificate**
```bash
cd /opt/n8n-deploy
docker-compose run --rm certbot certonly --webroot --webroot-path=/var/www/certbot -d n8n.thefallencollective.live --email admin@thefallencollective.live --agree-tos --no-eff-email
```

### **Step 2: Restart Nginx**
```bash
docker-compose restart nginx
```

### **Step 3: Access N8N**
```
URL: https://n8n.thefallencollective.live
Username: admin
Password: YourSecurePassword123
```

### **Step 4: Import Workflow**
1. Download workflow: https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/N8N_Workflows/workflow-1-sync-db-to-agent.json
2. In N8N: Workflows ? Import from File
3. Execute to test
4. Toggle "Active"

---

## ? **WHAT'S INCLUDED:**

- ? **Cosmos DB credentials** (automatically configured)
- ? **ElevenLabs API** (pre-configured)
- ? **N8N authentication** (admin/YourSecurePassword123)
- ? **Nginx reverse proxy** (with SSL support)
- ? **Certbot** (automatic SSL renewal)

---

## ?? **READY!**

Just run the one command above and your N8N will be deployed with all credentials ready to go!
