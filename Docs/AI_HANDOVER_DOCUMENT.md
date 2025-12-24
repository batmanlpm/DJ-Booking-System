# ?? AI ASSISTANT HANDOVER DOCUMENT

**Date:** 2025-01-23  
**Session Status:** N8N Docker Container Successfully Deployed  
**Next Phase:** Configure Reverse Proxy + SSL + Import Workflow

---

## ?? **CURRENT STATE - EXACTLY WHERE WE ARE:**

### **? COMPLETED:**
1. ? N8N Docker container deployed on Hostinger VPS
2. ? Container running successfully (port 5678)
3. ? Subdomain created: `n8n.thefallencollective.live`
4. ? VPS accessible via SSH (root@srv1208470)
5. ? Docker installed and working
6. ? N8N image pulled and container started

### **?? CONTAINER DETAILS:**
```
Container Name: n8n
Image: n8nio/n8n:latest
Port: 5678 (internal)
Status: Running
Restart Policy: always
Authentication: admin / YourSecurePassword123
Host: n8n.thefallencollective.live
Protocol: https (configured but not proxied yet)
Volume: n8n_data:/home/node/.n8n
```

### **?? VPS DETAILS:**
```
Server: srv1208470
IP: 72.62.82.62
IPv6: 2a02:4780:2d:e691::1
OS: Ubuntu 24.04.3 LTS
Domain: thefallencollective.live
Subdomain Path: /u833570579/domains/thefallencollective.live/public_html/n8n
SSH: root@72.62.82.62 (active session)
```

---

## ?? **NEXT STEPS - WHAT TO DO NOW:**

### **IMMEDIATE NEXT ACTION:**

**Step 1: Verify N8N is Running**
```sh
docker ps
```
**Expected output:**
```
CONTAINER ID   IMAGE        STATUS      PORTS
abc123...      n8nio/n8n    Up 5 min    0.0.0.0:5678->5678/tcp
```

**Step 2: Test Local Access**
```sh
curl http://localhost:5678
```
**Should return:** HTML content (N8N login page)

---

### **PHASE 2: CONFIGURE REVERSE PROXY (CRITICAL)**

**Goal:** Make N8N accessible at `https://n8n.thefallencollective.live`

**Option A: Use Nginx (Recommended)**

1. **Install Nginx:**
```sh
apt install -y nginx
```

2. **Create N8N configuration:**
```sh
nano /etc/nginx/sites-available/n8n.thefallencollective.live
```

3. **Paste this configuration:**
```nginx
server {
    listen 80;
    server_name n8n.thefallencollective.live;
    
    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name n8n.thefallencollective.live;
    
    # SSL Certificate (install with certbot)
    ssl_certificate /etc/letsencrypt/live/n8n.thefallencollective.live/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/n8n.thefallencollective.live/privkey.pem;
    
    # Proxy to N8N Docker container
    location / {
        proxy_pass http://localhost:5678;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

4. **Enable site:**
```sh
ln -s /etc/nginx/sites-available/n8n.thefallencollective.live /etc/nginx/sites-enabled/
```

5. **Install SSL certificate:**
```sh
apt install -y certbot python3-certbot-nginx
certbot --nginx -d n8n.thefallencollective.live
```

6. **Test and reload Nginx:**
```sh
nginx -t
systemctl reload nginx
```

7. **Open browser:** `https://n8n.thefallencollective.live`
   - Login: admin / YourSecurePassword123

---

### **PHASE 3: CONFIGURE N8N ENVIRONMENT VARIABLES**

**Access N8N Web UI:**
```
URL: https://n8n.thefallencollective.live
Username: admin
Password: YourSecurePassword123
```

**Navigate to:** Settings ? Variables

**Add these variables:**

```env
# Cosmos DB Credentials (FIND IN CODE)
COSMOS_ENDPOINT=https://[account-name].documents.azure.com
COSMOS_KEY=[primary-key-from-azure-portal]
COSMOS_DB=DJBookingDB

# ElevenLabs Credentials (ALREADY KNOWN)
ELEVENLABS_API_KEY=sk_b04ee67b81c033d87b4ec481ccb20a5d63fa3b81c7d62a60
ELEVENLABS_AGENT_ID=agent_2201kacf3j0nfjj9w151tbr3n5e0
```

**Where to find Cosmos DB credentials:**
- Check: `Services/CosmosDbService.cs` in the project
- Or: Azure Portal ? Cosmos DB ? Keys
- Or: `Config/app-settings.json`

---

### **PHASE 4: IMPORT WORKFLOW**

**File location:** `N8N_Workflows/workflow-1-sync-db-to-agent.json`

**Steps:**
1. In N8N web UI, click **Workflows** ? **Import from File**
2. Upload: `workflow-1-sync-db-to-agent.json`
3. Click **Execute Workflow** (test button) to verify
4. Check all nodes turn green ?
5. Toggle **Active** switch (top-right) to enable hourly auto-sync

---

## ?? **KEY DOCUMENTATION FILES:**

### **Primary Reference Documents:**
1. **`Docs/N8N_QUICK_START.md`** - Quick setup guide (CURRENT FILE USER IS VIEWING)
2. **`Docs/N8N_COMPLETE_AUTOMATION_GUIDE.md`** - Full technical documentation
3. **`Docs/N8N_ENVIRONMENT_VARIABLES.md`** - All credentials and variables
4. **`N8N_Workflows/workflow-1-sync-db-to-agent.json`** - Ready-to-import workflow

### **Supporting Documentation:**
5. **`Docs/ELEVENLABS_API_INTEGRATION_COMPLETE.md`** - ElevenLabs API details
6. **`Docs/ELEVENLABS_API_TROUBLESHOOTING.md`** - Troubleshooting guide
7. **`Services/N8NWorkflowClient.cs`** - C# client for N8N webhooks (if needed later)

---

## ?? **IMPORTANT CREDENTIALS:**

### **ElevenLabs (Already Documented):**
```
API Key: sk_b04ee67b81c033d87b4ec481ccb20a5d63fa3b81c7d62a60
Agent ID: agent_2201kacf3j0nfjj9w151tbr3n5e0
```

### **N8N Authentication:**
```
Username: admin
Password: YourSecurePassword123
```

### **Cosmos DB (NEEDS TO BE FOUND):**
```
Endpoint: https://[ACCOUNT].documents.azure.com
Key: [FIND IN AZURE PORTAL OR CODE]
Database: DJBookingDB
Containers: venues, bookings, users
```

**Where to find:**
- File: `Services/CosmosDbService.cs` (constructor parameter or constants)
- Or: Azure Portal ? Cosmos DB ? Keys section
- Or: `Config/app-settings.json`

---

## ?? **THE GOAL - WHAT THIS ACHIEVES:**

### **Current Problem:**
```
User: "What venues are available?"
Agent: "I don't have access to the database."
User: ??
```

### **After N8N Setup:**
```
User: "What venues are available?"
Agent: "We currently have 5 active venues: test venue, Club XYZ, The Underground, Neon Nights, and Bass Station!"
User: ??
```

### **How It Works:**
```
N8N Workflow (every hour):
    ?
1. Query Cosmos DB for venues
    ?
2. Get booking count
    ?
3. Format data: "We have 5 venues: test venue, Club XYZ..."
    ?
4. Build updated system prompt with live data
    ?
5. Send PATCH request to ElevenLabs API
    ?
6. Agent's system prompt updated with current venue list
    ?
Agent now knows current venues! ?
```

---

## ?? **COMMON ISSUES & SOLUTIONS:**

### **Issue 1: "Cannot access N8N at n8n.thefallencollective.live"**
**Solution:**
- DNS not propagated yet (wait 5-10 minutes)
- Nginx not configured (see Phase 2 above)
- Firewall blocking port 443 (check VPS firewall)

### **Issue 2: "Cosmos DB connection fails in workflow"**
**Solution:**
- Verify `COSMOS_ENDPOINT` and `COSMOS_KEY` are correct
- Check firewall allows outbound connections
- Test manually: `curl [COSMOS_ENDPOINT]`

### **Issue 3: "ElevenLabs update fails (401 error)"**
**Solution:**
- API key invalid or expired
- Verify at: https://elevenlabs.io/app/settings
- Check Agent ID is correct

### **Issue 4: "Workflow runs but agent doesn't update"**
**Solution:**
- Check N8N execution logs for errors
- Verify ElevenLabs API response is 200 OK
- Test voice command to confirm: "What venues are available?"
- May need to hang up and reconnect voice chat

---

## ?? **QUICK COMMANDS REFERENCE:**

### **Docker Management:**
```sh
# Check N8N is running
docker ps

# View N8N logs
docker logs n8n

# Restart N8N
docker restart n8n

# Stop N8N
docker stop n8n

# Start N8N
docker start n8n

# Remove N8N (if needed to recreate)
docker rm -f n8n
```

### **Nginx Management:**
```sh
# Test configuration
nginx -t

# Reload (after config changes)
systemctl reload nginx

# Restart Nginx
systemctl restart nginx

# Check status
systemctl status nginx

# View logs
tail -f /var/log/nginx/error.log
```

### **SSL Certificate (Certbot):**
```sh
# Install certificate
certbot --nginx -d n8n.thefallencollective.live

# Renew all certificates (auto-renews every 90 days)
certbot renew

# Check certificate status
certbot certificates
```

---

## ?? **TESTING CHECKLIST:**

After completing setup, verify:

- [ ] N8N accessible at `https://n8n.thefallencollective.live`
- [ ] Login works (admin / YourSecurePassword123)
- [ ] Environment variables are set (Settings ? Variables)
- [ ] Workflow imported successfully
- [ ] Manual workflow execution succeeds (all nodes green)
- [ ] Cosmos DB connection works (check node outputs)
- [ ] ElevenLabs API update succeeds (status 200)
- [ ] Workflow activated (toggle switch on)
- [ ] Test voice command: "What venues are available?"
- [ ] Agent responds with correct venue names
- [ ] Scheduled executions appear in N8N execution history

---

## ?? **IF STUCK:**

### **Can't find Cosmos DB credentials?**
```sh
# SSH into VPS
ssh root@72.62.82.62

# Search project files (if uploaded to VPS)
grep -r "documents.azure.com" /path/to/project

# Or ask user to:
# 1. Check Azure Portal ? Cosmos DB ? Keys
# 2. Check Services/CosmosDbService.cs in Visual Studio
# 3. Check Config/app-settings.json
```

### **Nginx not working?**
```sh
# Check if Nginx is installed
nginx -v

# If not installed:
apt install -y nginx

# Check if running:
systemctl status nginx

# Check error logs:
tail -f /var/log/nginx/error.log
```

### **DNS not resolving?**
```sh
# Check DNS propagation
nslookup n8n.thefallencollective.live

# If not resolving:
# - Wait 5-10 minutes for DNS propagation
# - Verify subdomain created in Hostinger panel
# - Check A record points to: 72.62.82.62
```

---

## ?? **USER PREFERENCES:**

**Communication style:**
- User appreciates **step-by-step** instructions
- Prefers **one small step at a time** (don't overwhelm)
- Wants **exact commands** to copy/paste
- Appreciates **verification steps** after each action
- Values **clear explanations** of what each command does

**Technical level:**
- Comfortable with SSH and terminal commands
- Understands Docker basics
- Familiar with Visual Studio and C# (.NET 8)
- Working on DJ Booking System (Fallen Collective)
- Has Hostinger VPS hosting with Docker support

---

## ?? **PROJECT CONTEXT:**

### **Application:** DJ Booking System for "The Fallen Collective"
### **Voice AI:** Candy-Bot (ElevenLabs agent)
### **Database:** Azure Cosmos DB
### **Current Issue:** Agent doesn't know real-time venue data
### **Solution:** N8N workflow to sync DB ? Agent every hour

### **Tech Stack:**
- Frontend: WPF (.NET 8)
- Database: Azure Cosmos DB
- Voice AI: ElevenLabs Conversational AI
- Automation: N8N (Docker on Hostinger VPS)
- Domain: thefallencollective.live (Hostinger)

---

## ?? **IMMEDIATE NEXT INTERACTION:**

**When user returns, ask:**

1. "I see N8N Docker container is running! Let me verify: Can you run `docker ps` and confirm you see the n8n container?"

2. If confirmed: "Great! Now let's set up the reverse proxy so you can access N8N at https://n8n.thefallencollective.live. First, let me check if Nginx is installed. Run: `nginx -v`"

3. Based on response:
   - **If Nginx installed:** Proceed with configuration (see Phase 2)
   - **If not installed:** Install Nginx first: `apt install -y nginx`

**Keep steps small and wait for confirmation after each!**

---

## ? **SESSION SUMMARY:**

**What we accomplished:**
1. ? Identified user has Hostinger VPS with Docker
2. ? Created subdomain: n8n.thefallencollective.live
3. ? SSH'd into VPS (Ubuntu 24.04.3)
4. ? Updated system packages
5. ? Deployed N8N Docker container successfully
6. ? Container is running with authentication enabled

**What's left:**
1. ? Configure Nginx reverse proxy
2. ? Install SSL certificate (Let's Encrypt)
3. ? Set environment variables in N8N
4. ? Import workflow JSON
5. ? Test and activate automation

**Estimated time remaining:** 15-20 minutes

---

## ?? **USEFUL LINKS:**

- **N8N Documentation:** https://docs.n8n.io/
- **N8N Docker Guide:** https://docs.n8n.io/hosting/installation/docker/
- **ElevenLabs API Docs:** https://elevenlabs.io/docs/api-reference
- **Cosmos DB REST API:** https://docs.microsoft.com/azure/cosmos-db/sql/sql-api-get-started
- **Nginx Configuration:** https://nginx.org/en/docs/
- **Certbot:** https://certbot.eff.org/

---

## ?? **NOTES FOR NEXT AI:**

- User's WSL doesn't work (that's why we're using Hostinger VPS)
- User prefers cloud-hosted solutions over local
- N8N Cloud was mentioned but user chose self-hosted on Hostinger (smart choice - consolidates infrastructure)
- Be patient and clear - user is following along step-by-step
- Always provide verification commands after each step
- Docker command syntax matters (Linux vs PowerShell) - we had one false start with wrong syntax

---

**?? Good luck, next AI! You're picking up right after a successful Docker deployment. The user is ready for the next step!** ??

---

**END OF HANDOVER DOCUMENT**
