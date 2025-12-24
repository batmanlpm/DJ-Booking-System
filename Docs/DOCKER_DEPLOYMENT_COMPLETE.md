# ?? N8N DOCKER DEPLOYMENT - COMPLETE GUIDE

**Date:** 2025-01-23  
**Status:** ? **PRODUCTION READY**

---

## ?? **WHAT THIS IS:**

A **complete Docker Compose setup** for N8N that includes:
- ? N8N Workflow Automation
- ? Nginx Reverse Proxy
- ? Automatic SSL (Let's Encrypt)
- ? Pre-configured with Cosmos DB + ElevenLabs
- ? One-command deployment
- ? Auto-restart on server reboot
- ? Health checks and monitoring

---

## ?? **FILES CREATED:**

```
Docker/
??? docker-compose.yml          # Main Docker Compose configuration
??? .env.example                # Environment variables template
??? deploy.sh                   # Automated deployment script
??? nginx/
    ??? conf.d/
        ??? n8n.conf            # Nginx reverse proxy configuration
```

---

## ?? **ONE-COMMAND DEPLOYMENT:**

### **Option 1: Deploy from URL (GitHub)**

```sh
# Download and run deployment
curl -fsSL https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/deploy.sh | sudo bash
```

---

### **Option 2: Deploy from Local Files**

```sh
# Navigate to Docker directory
cd /path/to/Docker

# Make deploy script executable
chmod +x deploy.sh

# Run deployment
sudo ./deploy.sh
```

---

## ?? **STEP-BY-STEP DEPLOYMENT:**

### **Step 1: Prepare Environment**

```sh
# SSH into your VPS
ssh root@72.62.82.62

# Create deployment directory
mkdir -p /opt/n8n
cd /opt/n8n

# Download Docker Compose files
wget https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/docker-compose.yml
wget https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/.env.example
wget https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/deploy.sh

# Create Nginx config directory
mkdir -p nginx/conf.d
wget -O nginx/conf.d/n8n.conf https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/nginx/conf.d/n8n.conf
```

---

### **Step 2: Configure Environment Variables**

```sh
# Copy environment template
cp .env.example .env

# Edit with your credentials
nano .env
```

**Required changes in `.env`:**
```env
# Cosmos DB (Get from Azure Portal ? Cosmos DB ? Keys)
COSMOS_ENDPOINT=https://YOUR-ACCOUNT.documents.azure.com
COSMOS_KEY=YOUR_PRIMARY_KEY_HERE

# Email for SSL certificate
SSL_EMAIL=admin@thefallencollective.live

# Optional: Change password
N8N_BASIC_AUTH_PASSWORD=YourSecurePassword123
```

**Save:** `Ctrl+O`, `Enter`, `Ctrl+X`

---

### **Step 3: Deploy**

```sh
# Make deploy script executable
chmod +x deploy.sh

# Run deployment
./deploy.sh
```

**This will:**
1. ? Check/install Docker and Docker Compose
2. ? Validate environment variables
3. ? Pull latest N8N and Nginx images
4. ? Start all containers
5. ? Configure networking
6. ? Display next steps

---

### **Step 4: Get SSL Certificate**

```sh
# Run Certbot to get SSL certificate
docker-compose run --rm certbot certonly \
  --webroot \
  --webroot-path=/var/www/certbot \
  -d n8n.thefallencollective.live \
  --email admin@thefallencollective.live \
  --agree-tos \
  --no-eff-email
```

**Expected output:**
```
Successfully received certificate.
Certificate is saved at: /etc/letsencrypt/live/n8n.thefallencollective.live/fullchain.pem
```

---

### **Step 5: Restart Nginx**

```sh
docker-compose restart nginx
```

---

### **Step 6: Verify Deployment**

```sh
# Check all containers are running
docker-compose ps
```

**Expected output:**
```
NAME      IMAGE            STATUS        PORTS
n8n       n8nio/n8n        Up 2 min      0.0.0.0:5678->5678/tcp
nginx     nginx:alpine     Up 2 min      0.0.0.0:80->80/tcp, 0.0.0.0:443->443/tcp
certbot   certbot/certbot  Up 2 min
```

---

## ?? **ACCESS N8N:**

### **URL:** https://n8n.thefallencollective.live

### **Login:**
- **Username:** `admin`
- **Password:** `YourSecurePassword123` (or your custom password from `.env`)

---

## ?? **IMPORT WORKFLOW:**

### **Step 1: Download Workflow**

The workflow file is already in your project:
```
N8N_Workflows/workflow-1-sync-db-to-agent.json
```

### **Step 2: Import to N8N**

1. **Open N8N:** https://n8n.thefallencollective.live
2. **Login** with credentials
3. **Click** "Workflows" (left sidebar)
4. **Click** "Add Workflow" ? "Import from File"
5. **Select:** `workflow-1-sync-db-to-agent.json`
6. **Click** "Import"

### **Step 3: Verify Workflow**

1. **Open imported workflow**
2. **Click** "Execute Workflow" (test button)
3. **Check** all nodes turn green ?
4. **Toggle** "Active" switch (top-right)

---

## ? **VERIFY IT WORKS:**

### **Test 1: Check Containers**

```sh
docker-compose ps
```

**All should show "Up"**

---

### **Test 2: Check N8N Logs**

```sh
docker-compose logs -f n8n
```

**Should see:**
```
n8n ready on 0.0.0.0, port 5678
```

---

### **Test 3: Access Web UI**

**Open browser:** https://n8n.thefallencollective.live

**Should see:** N8N login page

---

### **Test 4: Check SSL Certificate**

```sh
curl -I https://n8n.thefallencollective.live
```

**Should see:** `HTTP/2 200` and `strict-transport-security` header

---

### **Test 5: Test Workflow Execution**

1. **In N8N, open workflow**
2. **Click "Execute Workflow"**
3. **All nodes should turn green ?**
4. **Check "Build System Prompt" node output**
5. **Should see venue list**

---

## ?? **DOCKER COMPOSE COMMANDS:**

### **View Logs**
```sh
# All containers
docker-compose logs -f

# N8N only
docker-compose logs -f n8n

# Nginx only
docker-compose logs -f nginx
```

### **Restart Services**
```sh
# All containers
docker-compose restart

# N8N only
docker-compose restart n8n

# Nginx only
docker-compose restart nginx
```

### **Stop Services**
```sh
docker-compose down
```

### **Start Services**
```sh
docker-compose up -d
```

### **Update Images**
```sh
# Pull latest images
docker-compose pull

# Recreate containers
docker-compose up -d --force-recreate
```

### **Check Status**
```sh
docker-compose ps
```

---

## ?? **ENVIRONMENT VARIABLES:**

### **Already Configured in Docker Compose:**

```env
# N8N Configuration
N8N_HOST=n8n.thefallencollective.live
N8N_PROTOCOL=https
N8N_BASIC_AUTH_USER=admin
N8N_BASIC_AUTH_PASSWORD=YourSecurePassword123

# Cosmos DB (from .env file)
COSMOS_ENDPOINT=https://your-account.documents.azure.com
COSMOS_KEY=your_key_here
COSMOS_DB=DJBookingDB

# ElevenLabs (pre-configured)
ELEVENLABS_API_KEY=sk_b04ee67b81c033d87b4ec481ccb20a5d63fa3b81c7d62a60
ELEVENLABS_AGENT_ID=agent_2201kacf3j0nfjj9w151tbr3n5e0
```

**No need to set these manually in N8N!** They're automatically available to all workflows.

---

## ?? **WHAT'S INCLUDED:**

### **1. N8N Container**
- **Image:** `n8nio/n8n:latest`
- **Port:** 5678 (internal)
- **Volumes:** Persistent storage for workflows
- **Health Check:** Monitors container health
- **Auto-Restart:** Always restarts on failure

### **2. Nginx Container**
- **Image:** `nginx:alpine`
- **Ports:** 80 (HTTP), 443 (HTTPS)
- **SSL:** Automatic certificate management
- **WebSocket:** Full support for N8N
- **Security Headers:** HSTS, X-Frame-Options, etc.

### **3. Certbot Container**
- **Image:** `certbot/certbot:latest`
- **Function:** Automatic SSL certificate renewal
- **Schedule:** Checks every 12 hours
- **Auto-Renewal:** 30 days before expiry

---

## ?? **AUTO-RENEWAL & HEALTH CHECKS:**

### **SSL Certificate Auto-Renewal**
- Certbot runs every 12 hours
- Automatically renews certificates 30 days before expiry
- Nginx reloads every 6 hours to pick up new certificates

### **Container Health Checks**
- N8N: Checks `/healthz` endpoint every 30 seconds
- Auto-restart if unhealthy after 3 failures
- Logs health check failures

---

## ?? **POST-DEPLOYMENT CHECKLIST:**

- [ ] Containers running (`docker-compose ps`)
- [ ] SSL certificate obtained
- [ ] Nginx restarted after SSL
- [ ] N8N accessible at https://n8n.thefallencollective.live
- [ ] Login works (admin / YourSecurePassword123)
- [ ] Workflow imported
- [ ] Workflow test execution successful
- [ ] All nodes green ?
- [ ] Workflow activated (toggle on)
- [ ] Scheduled execution appears in history

---

## ?? **TROUBLESHOOTING:**

### **Issue: Can't access N8N at URL**

**Check DNS:**
```sh
nslookup n8n.thefallencollective.live
```

**Should point to:** `72.62.82.62`

**Check containers:**
```sh
docker-compose ps
```

**Check Nginx logs:**
```sh
docker-compose logs nginx
```

---

### **Issue: SSL certificate error**

**Check certificate exists:**
```sh
docker-compose exec nginx ls /etc/letsencrypt/live/n8n.thefallencollective.live/
```

**Re-run Certbot:**
```sh
docker-compose run --rm certbot certonly --webroot --webroot-path=/var/www/certbot -d n8n.thefallencollective.live --email admin@thefallencollective.live --agree-tos --force-renewal
```

---

### **Issue: Workflow fails to execute**

**Check environment variables:**
```sh
docker-compose exec n8n env | grep COSMOS
docker-compose exec n8n env | grep ELEVENLABS
```

**Check N8N logs:**
```sh
docker-compose logs n8n | tail -100
```

---

## ?? **MONITORING:**

### **Container Stats**
```sh
docker stats
```

### **N8N Execution History**
**In N8N Web UI:**
- Click "Executions" (left sidebar)
- See all workflow runs
- Click any execution to see details

### **Disk Usage**
```sh
docker system df
```

---

## ?? **SECURITY FEATURES:**

? **SSL/TLS Encryption** (Let's Encrypt)  
? **HTTP to HTTPS Redirect**  
? **Basic Authentication** (username/password)  
? **Security Headers** (HSTS, X-Frame-Options)  
? **WebSocket Support** (secure wss://)  
? **Container Isolation** (Docker network)  
? **Automatic Updates** (via `docker-compose pull`)  

---

## ?? **PRODUCTION READY:**

This setup is **production-ready** with:
- ? Automatic SSL certificate renewal
- ? Container health monitoring
- ? Auto-restart on failure
- ? Persistent data storage
- ? Security headers
- ? Logging and monitoring
- ? Webhook support
- ? WebSocket support

---

## ?? **RELATED DOCUMENTATION:**

- `N8N_Workflows/workflow-1-sync-db-to-agent.json` - Workflow to import
- `Docs/N8N_COMPLETE_AUTOMATION_GUIDE.md` - Technical details
- `Docs/N8N_ENVIRONMENT_VARIABLES.md` - Variable reference
- `Docs/AI_HANDOVER_DOCUMENT.md` - Setup context

---

## ? **SUMMARY:**

**You now have:**
1. ? Complete Docker Compose setup
2. ? Nginx reverse proxy with SSL
3. ? Automatic certificate renewal
4. ? Pre-configured with all credentials
5. ? One-command deployment script
6. ? Production-ready monitoring
7. ? Complete documentation

**To deploy:**
```sh
curl -fsSL https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/Docker/deploy.sh | sudo bash
```

**That's it!** N8N will be running at `https://n8n.thefallencollective.live` ??

---

**?? DOCKER COMPOSE DEPLOYMENT - COMPLETE AND READY!** ?
