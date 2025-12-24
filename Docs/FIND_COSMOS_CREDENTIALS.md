# ?? N8N DEPLOYMENT - FIND YOUR COSMOS DB CREDENTIALS

## ?? WHERE TO FIND YOUR COSMOS DB CREDENTIALS:

### Option 1: Azure Portal (Most Reliable)
1. Go to: https://portal.azure.com
2. Search for "Cosmos DB"
3. Click your database: **fallen-collective** or **DJBookingDB**
4. Click **Keys** in the left menu
5. Copy:
   - URI (Endpoint): `https://[account-name].documents.azure.com:443/`
   - PRIMARY KEY or SECONDARY KEY

### Option 2: Check Your App Configuration

Your app loads Cosmos DB credentials from one of these locations:

1. **App.xaml.cs** - Check the connection string initialization
2. **appsettings.json** or **app.config** - Configuration file
3. **Environment Variables** - System environment variables
4. **User Secrets** - Visual Studio user secrets

### Option 3: Check App.xaml.cs

The connection string is likely set up in `App.xaml.cs` in the `OnStartup` method where `CosmosDbService` is initialized.

Look for something like:
```csharp
string connectionString = "AccountEndpoint=https://...;AccountKey=...;";
cosmosDbService = new CosmosDbService(connectionString, "DJBookingDB");
```

---

## ?? WHAT YOU NEED FOR N8N:

```env
COSMOS_ENDPOINT=https://[YOUR-ACCOUNT].documents.azure.com
COSMOS_KEY=[YOUR_PRIMARY_KEY_HERE]
```

**Database Name:** `DJBookingDB` (already known)
**Containers:** `users`, `Bookings`, `Venues`, `Chat`, `Settings`, `RadioStations`, `Reports`

---

## ?? ONCE YOU HAVE THE CREDENTIALS:

Run this in your VPS:

```bash
cd /opt/n8n-deploy

# Edit .env file
nano .env

# Add these lines:
COSMOS_ENDPOINT=https://your-account.documents.azure.com
COSMOS_KEY=your_primary_key_here
SSL_EMAIL=admin@thefallencollective.live

# Save and exit: Ctrl+O, Enter, Ctrl+X

# Deploy
docker-compose up -d

# Get SSL certificate
docker-compose run --rm certbot certonly --webroot --webroot-path=/var/www/certbot -d n8n.thefallencollective.live --email admin@thefallencollective.live --agree-tos --no-eff-email

# Restart Nginx
docker-compose restart nginx
```

---

## ? ACCESS N8N:

**URL:** https://n8n.thefallencollective.live  
**Username:** admin  
**Password:** YourSecurePassword123

---

## ?? IMPORT WORKFLOW:

1. Download: https://raw.githubusercontent.com/batmanlpm/DJ-Booking-System/main/N8N_Workflows/workflow-1-sync-db-to-agent.json
2. In N8N: Workflows ? Import from File
3. Execute to test
4. Toggle "Active"

**Done!** Your agent will sync with Cosmos DB every hour! ??
