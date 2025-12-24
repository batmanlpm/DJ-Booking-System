# ?? N8N ENVIRONMENT VARIABLES - COMPLETE CONFIGURATION

**Date:** 2025-01-23  
**Status:** ? **READY TO USE**

---

## ?? **COPY & PASTE THESE EXACT VALUES:**

### **FOR N8N ? Settings ? Environment Variables:**

```env
# ============================================
# COSMOS DB CONFIGURATION
# ============================================

# Replace with your actual values from Azure Portal
COSMOS_ENDPOINT=https://YOUR_ACCOUNT_NAME.documents.azure.com
COSMOS_KEY=YOUR_PRIMARY_KEY_HERE
COSMOS_DB=DJBookingDB

# Container names (these are the actual container names in your database)
COSMOS_USERS_CONTAINER=users
COSMOS_BOOKINGS_CONTAINER=bookings
COSMOS_VENUES_CONTAINER=venues
COSMOS_CHAT_CONTAINER=chat-messages
COSMOS_SETTINGS_CONTAINER=settings
COSMOS_RADIO_CONTAINER=radio-stations
COSMOS_REPORTS_CONTAINER=reports

# ============================================
# ELEVENLABS CONFIGURATION
# ============================================

ELEVENLABS_API_KEY=sk_b04ee67b81c033d87b4ec481ccb20a5d63fa3b81c7d62a60
ELEVENLABS_AGENT_ID=agent_2201kacf3j0nfjj9w151tbr3n5e0

# ============================================
# OPTIONAL: WEBHOOK CONFIGURATION
# ============================================

# If you want to expose N8N webhooks publicly (via ngrok/cloudflare)
N8N_WEBHOOK_URL=http://localhost:5678/webhook
N8N_WEBHOOK_TEST_URL=http://localhost:5678/webhook-test

# ============================================
# OPTIONAL: NOTIFICATION SERVICES
# ============================================

# Discord webhook for notifications (optional)
DISCORD_WEBHOOK_URL=https://discord.com/api/webhooks/YOUR_WEBHOOK_HERE

# Email for notifications (optional)
NOTIFICATION_EMAIL=admin@example.com
```

---

## ?? **HOW TO GET YOUR COSMOS DB CREDENTIALS:**

### **Method 1: From Azure Portal**

1. **Go to Azure Portal:** https://portal.azure.com
2. **Find your Cosmos DB account**
3. **Click "Keys" in left menu**
4. **Copy these values:**
   - **URI** ? Use as `COSMOS_ENDPOINT`
   - **PRIMARY KEY** ? Use as `COSMOS_KEY`
   - **Database name** ? Should be `DJBookingDB`

---

### **Method 2: From Your Code**

**Check `Services/CosmosDbService.cs`:**
```csharp
// Look for the constructor call in MainWindow.xaml.cs or App.xaml.cs
var cosmosService = new CosmosDbService(connectionString, "DJBookingDB");
```

**The connection string format:**
```
AccountEndpoint=https://YOUR_ACCOUNT.documents.azure.com;AccountKey=YOUR_KEY_HERE;
```

---

### **Method 3: From Configuration Files**

**Check `Config/app-settings.json`:**
```json
{
  "CosmosDb": {
    "AccountEndpoint": "https://YOUR_ACCOUNT.documents.azure.com",
    "AccountKey": "YOUR_PRIMARY_KEY_HERE"
  }
}
```

---

## ? **VERIFY YOUR CONFIGURATION:**

### **Test 1: Check Endpoint Format**
```
? Correct: https://fallen-collective.documents.azure.com
? Wrong: fallen-collective.documents.azure.com (missing https://)
? Wrong: https://fallen-collective.documents.azure.com/ (extra slash)
```

### **Test 2: Check Key Length**
```
? Cosmos Key is typically 88 characters long
? Should end with "==" or "="
? If shorter, it might be truncated
```

---

## ?? **SETTING ENVIRONMENT VARIABLES IN N8N:**

### **Docker (Command Line):**
```powershell
docker run -it --rm `
  --name n8n `
  -p 5678:5678 `
  -e COSMOS_ENDPOINT="https://your-account.documents.azure.com" `
  -e COSMOS_KEY="your_key_here" `
  -e COSMOS_DB="DJBookingDB" `
  -e ELEVENLABS_API_KEY="sk_b04ee67b81c033d87b4ec481ccb20a5d63fa3b81c7d62a60" `
  -e ELEVENLABS_AGENT_ID="agent_2201kacf3j0nfjj9w151tbr3n5e0" `
  -v n8n_data:/home/node/.n8n `
  n8nio/n8n
```

### **Docker Compose (.env file):**
```env
# Create a file named .env in same directory as docker-compose.yml
COSMOS_ENDPOINT=https://your-account.documents.azure.com
COSMOS_KEY=your_key_here
COSMOS_DB=DJBookingDB
ELEVENLABS_API_KEY=sk_b04ee67b81c033d87b4ec481ccb20a5d63fa3b81c7d62a60
ELEVENLABS_AGENT_ID=agent_2201kacf3j0nfjj9w151tbr3n5e0
```

**docker-compose.yml:**
```yaml
version: '3'
services:
  n8n:
    image: n8nio/n8n
    ports:
      - "5678:5678"
    env_file:
      - .env
    volumes:
      - n8n_data:/home/node/.n8n
volumes:
  n8n_data:
```

---

### **N8N Desktop App:**

1. **Open N8N Desktop**
2. **Click Settings (gear icon)**
3. **Click "Credentials"**
4. **Add "Environment Variable" credential**
5. **Paste variables**

---

### **N8N Web UI (Self-Hosted):**

1. **Open N8N:** http://localhost:5678
2. **Click Settings ? Environment**
3. **Add each variable:**
   - Name: `COSMOS_ENDPOINT`
   - Value: `https://your-account.documents.azure.com`
   - Click "Add"
4. **Repeat for all variables**

---

## ?? **TEST YOUR CONFIGURATION:**

### **PowerShell Test Script:**

```powershell
# Test Cosmos DB Connection
$endpoint = "https://your-account.documents.azure.com"
$key = "your_key_here"
$db = "DJBookingDB"

$headers = @{
    "Authorization" = $key
    "x-ms-version" = "2018-12-31"
}

try {
    $response = Invoke-RestMethod -Uri "$endpoint/dbs/$db" -Method Get -Headers $headers
    Write-Host "? Cosmos DB connection successful!" -ForegroundColor Green
    Write-Host "Database: $($response.id)" -ForegroundColor Cyan
} catch {
    Write-Host "? Connection failed: $($_.Exception.Message)" -ForegroundColor Red
}
```

---

## ?? **EXAMPLE N8N WORKFLOW USING THESE VARIABLES:**

### **Node: Get Venues from Cosmos DB**

**URL:**
```
={{$env.COSMOS_ENDPOINT}}/dbs/{{$env.COSMOS_DB}}/colls/venues/docs
```

**Headers:**
```json
{
  "Authorization": "={{$env.COSMOS_KEY}}",
  "x-ms-version": "2018-12-31",
  "x-ms-documentdb-isquery": "true",
  "Content-Type": "application/query+json"
}
```

**Body:**
```json
{
  "query": "SELECT * FROM c WHERE c.isActive = true",
  "parameters": []
}
```

---

### **Node: Update ElevenLabs Agent**

**URL:**
```
=https://api.elevenlabs.io/v1/convai/agents/{{$env.ELEVENLABS_AGENT_ID}}
```

**Headers:**
```json
{
  "xi-api-key": "={{$env.ELEVENLABS_API_KEY}}",
  "Content-Type": "application/json"
}
```

---

## ?? **SECURITY NOTES:**

### **DO NOT:**
? Commit .env files to Git  
? Share API keys publicly  
? Hardcode credentials in workflow JSON  
? Expose credentials in logs

### **DO:**
? Use environment variables  
? Restrict N8N access (use authentication)  
? Rotate keys periodically  
? Use read-only keys where possible  
? Add .env to .gitignore

**Add to .gitignore:**
```
.env
.env.local
.env.*.local
docker-compose.override.yml
n8n_data/
```

---

## ?? **UPDATE EXISTING WORKFLOWS:**

If you already imported workflows, update them to use environment variables:

**Before (hardcoded):**
```
https://myaccount.documents.azure.com/dbs/DJBookingDB/colls/venues/docs
```

**After (using env vars):**
```
={{$env.COSMOS_ENDPOINT}}/dbs/{{$env.COSMOS_DB}}/colls/venues/docs
```

---

## ? **VERIFICATION CHECKLIST:**

- [ ] COSMOS_ENDPOINT includes `https://`
- [ ] COSMOS_ENDPOINT does NOT have trailing slash
- [ ] COSMOS_KEY is ~88 characters long
- [ ] COSMOS_DB is exactly `DJBookingDB`
- [ ] ELEVENLABS_API_KEY starts with `sk_`
- [ ] ELEVENLABS_AGENT_ID starts with `agent_`
- [ ] Container names match your database
- [ ] All variables set in N8N
- [ ] Test workflow executes successfully

---

## ?? **RELATED DOCUMENTATION:**

- `N8N_Workflows/workflow-1-sync-db-to-agent.json` - Uses these variables
- `Docs/N8N_QUICK_START.md` - Setup guide
- `Docs/N8N_COMPLETE_AUTOMATION_GUIDE.md` - Full documentation

---

## ?? **SUMMARY:**

**You need 5 main variables:**
1. ? `COSMOS_ENDPOINT` - Your Cosmos DB URL
2. ? `COSMOS_KEY` - Your Cosmos DB primary key
3. ? `COSMOS_DB` - Database name (DJBookingDB)
4. ? `ELEVENLABS_API_KEY` - Already documented
5. ? `ELEVENLABS_AGENT_ID` - Already documented

**Time to configure:** ~2 minutes  
**Required for:** All N8N workflows  
**Security:** Keep these values secret!

---

**?? YOUR CREDENTIALS ARE READY TO USE IN N8N!** ?
