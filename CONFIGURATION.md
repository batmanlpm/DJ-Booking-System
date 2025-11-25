# ?? Configuration Guide for DJ Booking System

This guide explains how to configure the application with your own credentials.

---

## ?? IMPORTANT SECURITY NOTICE

**NEVER commit real credentials to GitHub!**

This project uses a **template system** to protect your credentials:

- **`App.xaml.cs`** - Your LOCAL file with REAL credentials (ignored by Git)
- **`App.xaml.cs.template`** - Template file uploaded to GitHub (no real credentials)

The real `App.xaml.cs` is in `.gitignore` so it will NEVER be uploaded to GitHub.

---

## ?? Quick Setup (First Time)

### Step 1: Clone Repository

```bash
git clone https://github.com/YOUR_USERNAME/DJ-Booking-System.git
cd DJ-Booking-System
```

### Step 2: Create App.xaml.cs from Template

The repository includes `App.xaml.cs.template` but NOT `App.xaml.cs`.

**Copy the template:**

```powershell
# Windows PowerShell
Copy-Item "App.xaml.cs.template" "App.xaml.cs"
```

```bash
# Linux/Mac
cp App.xaml.cs.template App.xaml.cs
```

### Step 3: Add Your Credentials

Open `App.xaml.cs` and replace these lines (around line 118-120):

**BEFORE:**
```csharp
string endpoint = Environment.GetEnvironmentVariable("COSMOS_DB_ENDPOINT") 
    ?? "https://YOUR-COSMOS-ACCOUNT.documents.azure.com:443/";
string accountKey = Environment.GetEnvironmentVariable("COSMOS_DB_KEY") 
    ?? "YOUR_COSMOS_DB_PRIMARY_KEY_HERE";
```

**AFTER:**
```csharp
string endpoint = "https://your-actual-account.documents.azure.com:443/";
string accountKey = "your_actual_primary_key_here";
```

### Step 4: Build and Run

```bash
dotnet restore
dotnet build --configuration Release
dotnet run
```

? **Your credentials are safe!** The modified `App.xaml.cs` will NOT be uploaded to Git.

---

## ?? Required Credentials

### 1. Azure Cosmos DB

**What you need:**
- Cosmos DB Account Endpoint
- Cosmos DB Primary Key

**Where to configure:**

Edit `App.xaml.cs` (lines 118-120):

```csharp
string endpoint = "https://fallen-collective.documents.azure.com:443/";
string accountKey = "EpxIq3hV8kXQ7kNY1KKJQmL5dkX0uZeW4GMUinPf6hNqRApx84Co5Ffve0bAktpyzH2xho5swBV5ACDbeunr5Q==";
```

?? **This file is in .gitignore - it will NOT be uploaded!**

---

### 2. Discord Bot Token (Optional)

**What you need:**
- Discord Bot Token from Discord Developer Portal

**Where to configure:**

Create a file `DiscordBotToken.txt` in the project root:

```
your_discord_bot_token_here
```

This file is automatically ignored by `.gitignore`

---

### 3. FTP/Hostinger Credentials (Optional - for auto-deployment)

**What you need:**
- FTP Server
- FTP Username
- FTP Password

**Where to configure:**

These are only needed if you're using the auto-upload scripts. They are already in `.gitignore` and won't be committed.

---

## ?? How the Template System Works

### For Repository Owner (You):

1. ? Work with `App.xaml.cs` containing REAL credentials
2. ? File is ignored by Git - never uploaded
3. ? You can commit code changes normally
4. ? Credentials stay safe on your machine

### For Other Developers:

1. Clone repository (gets `App.xaml.cs.template`)
2. Copy template to `App.xaml.cs`
3. Add their own credentials
4. Run the app

### What Gets Uploaded to GitHub:

? `App.xaml.cs.template` - Safe template with placeholders  
? `App.xaml.cs` - Your real file (protected by .gitignore)  
? All other code files  
? Documentation  

---

## ??? Database Setup

### Required Cosmos DB Containers

Create these containers in your Azure Cosmos DB account:

| Container Name | Partition Key | Description |
|---|---|---|
| `users` | `/username` | User accounts |
| `Bookings` | `/id` | DJ bookings |
| `Venues` | `/id` | Venue listings |
| `Chat` | `/channel` | Chat messages |
| `FriendRequests` | `/toUsername` | Friend requests |
| `Friendships` | `/user1` | Friend relationships |
| `OnlineStatus` | `/username` | User online status |
| `Settings` | `/id` | App settings |
| `RadioStations` | `/id` | Radio stations |
| `Reports` | `/id` | User reports |

### Database Name
```
DJBookingDB
```

---

## ?? Security Best Practices

### ? DO:
- Work with the real `App.xaml.cs` locally
- Keep credentials in ignored files
- Use `.gitignore` to protect sensitive data
- Rotate credentials regularly
- Use different credentials for dev/prod

### ? DON'T:
- Commit `App.xaml.cs` to Git
- Share credentials in chat/email
- Remove files from `.gitignore`
- Use same credentials across environments
- Hardcode credentials in uploaded files

---

## ?? Troubleshooting

### "App.xaml.cs not found" Error

**Solution:** Copy the template:
```powershell
Copy-Item "App.xaml.cs.template" "App.xaml.cs"
```

Then add your credentials.

### "Connection to Cosmos DB failed"

**Check:**
1. `App.xaml.cs` exists (not just the template)
2. Endpoint URL is correct
3. Primary key is valid
4. Database and containers exist
5. Firewall allows your IP

### "Credentials not configured" Message

**Solution:** You're running the template file. Create `App.xaml.cs` from template and add real credentials.

---

## ?? Support

If you need help with configuration:

- **GitHub Issues:** https://github.com/YOUR_USERNAME/DJ-Booking-System/issues
- **Email:** support@fallencollective.com
- **Discord:** https://discord.gg/fallencollective

---

## ?? Configuration Checklist

Before running the app:

- [ ] Repository cloned
- [ ] `App.xaml.cs` created from template
- [ ] Azure Cosmos DB credentials added to `App.xaml.cs`
- [ ] Database `DJBookingDB` created
- [ ] All required containers created
- [ ] Discord bot token configured (optional)
- [ ] `.gitignore` includes `App.xaml.cs`
- [ ] Verified `App.xaml.cs` is NOT tracked by Git

---

**The Fallen Collective**  
Last Updated: January 25, 2025
