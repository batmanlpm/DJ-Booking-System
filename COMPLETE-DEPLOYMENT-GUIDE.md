# ?? DJ BOOKING SYSTEM - COMPLETE DEPLOYMENT GUIDE
## For AI Assistants & Developers - Everything You Need to Know

**Project:** CandyBot DJ Booking System  
**Owner:** The Fallen Collective & Mega Byte I.T Services  
**Current Version:** 1.2.5  
**Last Updated:** 2025-01-23  

---

## ?? TABLE OF CONTENTS

1. [Project Overview](#project-overview)
2. [System Architecture](#system-architecture)
3. [Credentials & Access](#credentials--access)
4. [File Locations](#file-locations)
5. [Build & Deployment Process](#build--deployment-process)
6. [Upload Procedures](#upload-procedures)
7. [Version Management](#version-management)
8. [Common Commands](#common-commands)
9. [Troubleshooting](#troubleshooting)
10. [AI Agent Commands](#ai-agent-commands)

---

## 1. PROJECT OVERVIEW

### Application Details
- **Name:** CandyBot DJ Booking System
- **Type:** WPF Desktop Application (.NET 8)
- **Target:** Windows 10/11
- **Deployment:** Self-contained executable
- **Auto-Update:** Hourly checks via version.json

### Key Features
- DJ & Venue Booking Management
- User Management (17 permission system)
- RadioBOSS Integration (3 stations)
- Discord Chat Integration
- Azure Cosmos DB Backend
- Candy-Bot AI Assistant
- Real-time Online Status
- Auto-Update System

---

## 2. SYSTEM ARCHITECTURE

### Technology Stack
```
Frontend:     WPF (Windows Presentation Foundation)
Backend:      Azure Cosmos DB
Framework:    .NET 8
Language:     C# 12.0
Web Hosting:  Hostinger
Radio:        RadioBOSS Cloud (c19, c40)
Chat:         Discord WebView2
Audio:        NAudio
Updates:      HTTPS/SSL
```

### Project Structure
```
K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\
??? DJBookingSystem.csproj          ? Main project file
??? MainWindow.xaml                 ? Main application window
??? App.xaml.cs                     ? Application entry point
??? Services\                       ? Backend services
?   ??? CosmosDbService.cs          ? Database access
?   ??? SecureUpdateClient.cs       ? Auto-updater
?   ??? OnlineUserStatusService.cs  ? Real-time status
??? Views\                          ? UI Views
?   ??? Bookings\
?   ??? Radio\
?   ??? UsersView.xaml
?   ??? SettingsView.xaml
??? Models\                         ? Data models
?   ??? User.cs
?   ??? Booking.cs
?   ??? Venue.cs
??? Installer\Output\               ? Build output
?   ??? DJBookingSystem-Setup.exe   ? STATIC installer (upload this)
?   ??? DJBookingSystem-Setup-v1.2.5.exe  ? Version backup
?   ??? version.json                ? Version metadata
??? Website\                        ? Static website
?   ??? index.html                  ? Landing + registration
??? Build-Installer-v1.2.5.ps1      ? Build script
??? CHANGELOG.md                    ? Version history
```

---

## 3. CREDENTIALS & ACCESS

### ?? HOSTINGER WEB HOSTING

**Domain:** https://djbookupdates.com

**FTP Access:**
```
Protocol:   FTP (Port 21)
Host:       ftp://153.92.10.234
Port:       21
Username:   u833570579.Upload
Password:   [Contact owner for password]
Directory:  /home/u833570579/domains/djbookupdates.com/public_html
```

**File Manager (Alternative):**
- Login: https://hpanel.hostinger.com
- Username: u833570579.djbookupdates.com
- Navigate to: File Manager ? public_html

**FTP Clients:**
- FileZilla (Recommended): https://filezilla-project.org/
- SmartFTP: https://www.smartftp.com/

---

### ?? AZURE COSMOS DB

**Database:** DJ Booking System Database

**Connection String:**
```
Located in: Services\CosmosDbService.cs (line 15-20)
Endpoint:   https://[endpoint].documents.azure.com:443/
Key:        [Primary Key - encrypted in code]
Database:   DJBookingDB
```

**Containers:**
- `Users` - User accounts (Partition Key: /Username)
- `Bookings` - DJ bookings
- `Venues` - Venue information
- `OnlineUsers` - Real-time online status

**Access:**
- Portal: https://portal.azure.com
- Account: [Contact owner]

---

### ?? RADIOBOSS CLOUD

**Station 1: LivePartyMusic.fm (C40)**
```
URL:      https://c40.radioboss.fm/
Username: Remote
Password: R3m0t3
```

**Station 2: Candy-Bot Relay (C19)**
```
URL:      https://c19.radioboss.fm/
Username: Remote
Password: R3m0t3
```

**Auto-Login:**
- Implemented in `RadioBossCloudView.xaml.cs` (line 200-250)
- Implemented in `RadioBossStreamView.xaml.cs` (line 200-250)

---

### ?? DISCORD INTEGRATION

**Server:** The Fallen Collective
**Bot Token:** [Stored in Services\DiscordService.cs]
**Webhook:** [Configure in VenueManagementWindow]

---

## 4. FILE LOCATIONS

### Local Development
```
Project Root:  K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\
Solution File: DJBookingSystem.sln
Build Output:  bin\Release\net8.0-windows\win-x64\publish\
Installer:     Installer\Output\DJBookingSystem-Setup.exe
```

### Hostinger Server
```
Root:          /home/u833570579/domains/djbookupdates.com/public_html/
Landing Page:  public_html/index.html
Version File:  public_html/version.json
Installer:     public_html/downloads/DJBookingSystem-Setup.exe
Changelog:     public_html/changelog.html
```

### Important Files
| File | Local Path | Server Path | Purpose |
|------|------------|-------------|---------|
| Installer | `Installer\Output\DJBookingSystem-Setup.exe` | `public_html/downloads/DJBookingSystem-Setup.exe` | Main installer (STATIC) |
| Version | `Installer\Output\version.json` | `public_html/version.json` | Auto-update metadata |
| Landing Page | `Website\index.html` | `public_html/index.html` | Download + Registration |
| Changelog | `CHANGELOG.md` | `public_html/changelog.html` | Version history |

---

## 5. BUILD & DEPLOYMENT PROCESS

### Step-by-Step Build Process

#### **Step 1: Update Version Numbers**

Edit these files:
1. **DJBookingSystem.csproj** (lines 13-16)
```xml
<Version>1.2.5</Version>
<AssemblyVersion>1.2.5.0</AssemblyVersion>
<FileVersion>1.2.5.0</FileVersion>
<ProductVersion>1.2.5</ProductVersion>
```

2. **SplashScreen.xaml** (line 127)
```xml
<TextBlock Text="Version 1.2.5" .../>
```

3. **Installer\Output\version.json** (lines 3-5)
```json
"currentVersion": "1.2.4",
"latestVersion": "1.2.5",
"releaseDate": "2025-01-23",
```

4. **CHANGELOG.md** (add new version section at top)

---

#### **Step 2: Build the Application**

**Option A: Using PowerShell Script (Recommended)**
```powershell
# Navigate to project folder
cd "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"

# Run build script
.\Build-Installer-v1.2.5.ps1
```

**Option B: Manual Build**
```powershell
# Clean previous builds
dotnet clean DJBookingSystem.csproj --configuration Release

# Build and publish
dotnet publish DJBookingSystem.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false

# Copy to Installer\Output
Copy-Item "bin\Release\net8.0-windows\win-x64\publish\DJBookingSystem.exe" `
          "Installer\Output\DJBookingSystem-Setup.exe" -Force

# Create version backup
Copy-Item "Installer\Output\DJBookingSystem-Setup.exe" `
          "Installer\Output\DJBookingSystem-Setup-v1.2.5.exe" -Force
```

---

#### **Step 3: Verify Build**

```powershell
# Check files exist
Get-ChildItem "Installer\Output" -Filter "*.exe"

# Expected output:
# DJBookingSystem-Setup.exe        (STATIC - for upload)
# DJBookingSystem-Setup-v1.2.5.exe (Backup)
```

**Build Checklist:**
- [ ] No compilation errors
- [ ] No warnings (should be 0)
- [ ] Installer file exists (~690 MB)
- [ ] Version numbers updated everywhere
- [ ] CHANGELOG.md updated

---

## 6. UPLOAD PROCEDURES

### ?? Upload to Hostinger

#### **Option A: FTP Upload (FileZilla)**

1. **Connect:**
   - Open FileZilla
   - Host: `153.92.10.234`
   - Username: `u833570579.Upload`
   - Password: [Your password]
   - Port: `21`
   - Click "Quickconnect"

2. **Navigate:**
   - Remote site: `/home/u833570579/domains/djbookupdates.com/public_html`

3. **Upload Files:**
   ```
   Local ? Remote
   ??????????????????????????????????????????????????????????
   Installer\Output\DJBookingSystem-Setup.exe 
       ? public_html/downloads/DJBookingSystem-Setup.exe
   
   Installer\Output\version.json 
       ? public_html/version.json
   
   Website\index.html 
       ? public_html/index.html
   
   CHANGELOG.md (optional)
       ? public_html/changelog.html
   ```

4. **Verify Upload:**
   - Check file sizes match
   - Visit: https://djbookupdates.com/
   - Test download link
   - Check version.json: https://djbookupdates.com/version.json

---

#### **Option B: Hostinger File Manager**

1. Login: https://hpanel.hostinger.com
2. Navigate: File Manager ? `public_html`
3. Upload files using drag-and-drop
4. Overwrite when prompted

---

### ?? Upload Checklist

Before Upload:
- [ ] Built successfully with 0 errors
- [ ] Version numbers updated
- [ ] CHANGELOG.md updated
- [ ] Tested installer locally
- [ ] Backed up version-specific installer

Upload:
- [ ] Connected to FTP successfully
- [ ] Uploaded `DJBookingSystem-Setup.exe` to `downloads/`
- [ ] Uploaded `version.json` to root
- [ ] Uploaded `index.html` to root
- [ ] Uploaded `changelog.html` (optional)

After Upload:
- [ ] Tested website: https://djbookupdates.com/
- [ ] Download link works
- [ ] version.json accessible
- [ ] Auto-updater detects new version
- [ ] Registration form works

---

## 7. VERSION MANAGEMENT

### Version Numbering Scheme
```
MAJOR.MINOR.PATCH
  ?     ?     ?
  ?     ?     ???? Bug fixes (1.2.4 ? 1.2.5)
  ?     ?????????? New features (1.2.0 ? 1.3.0)
  ???????????????? Breaking changes (1.0.0 ? 2.0.0)
```

### Current Versions
- **1.2.5** - Latest (Permission system, Radio Control Center)
- **1.2.4** - Previous (Online status, UI fixes)
- **1.2.0** - Major update (Discord, WebView2, Auto-updates)
- **1.0.0** - Initial release

### Files to Update for New Version

| File | What to Change |
|------|----------------|
| `DJBookingSystem.csproj` | Lines 13-16: Version, AssemblyVersion, FileVersion |
| `SplashScreen.xaml` | Line 127: Version text |
| `version.json` | Lines 3-5: currentVersion, latestVersion, releaseDate |
| `CHANGELOG.md` | Add new version section at top |
| `Website\index.html` | Line 306: Version badge text |
| Build script filename | Rename to `Build-Installer-v1.2.X.ps1` |

---

## 8. COMMON COMMANDS

### Build Commands
```powershell
# Clean build
dotnet clean

# Build debug
dotnet build

# Build release
dotnet build --configuration Release

# Publish self-contained
dotnet publish --configuration Release --runtime win-x64 --self-contained true
```

### File Operations
```powershell
# List installer files
Get-ChildItem "Installer\Output" -Filter "*.exe"

# Check file size
(Get-Item "Installer\Output\DJBookingSystem-Setup.exe").Length / 1MB

# Copy to static filename
Copy-Item "Installer\Output\DJBookingSystem-Setup-v1.2.5.exe" `
          "Installer\Output\DJBookingSystem-Setup.exe" -Force
```

### Git Commands (if using version control)
```bash
git status
git add .
git commit -m "Release v1.2.5 - Permission system complete"
git tag v1.2.5
git push origin main --tags
```

---

## 9. TROUBLESHOOTING

### Build Errors

**"CS1998: Async method lacks 'await'"**
- Fixed in v1.2.5
- Remove `async` keyword or add `await Task.CompletedTask`

**"CS8629: Nullable value type may be null"**
- Fixed in v1.2.5
- Use null-forgiving operator `!` or null checks

**"WebView2 runtime not found"**
- Install: https://developer.microsoft.com/microsoft-edge/webview2/
- Or ensure `Microsoft.Web.WebView2` NuGet package is installed

---

### Upload Issues

**"530 Login authentication failed"**
- Check FTP credentials
- Verify port is 21
- Check firewall settings

**"File too large to upload"**
- Use FTP instead of File Manager
- Split into smaller chunks
- Compress with 7-Zip first

**"404 Not Found" after upload**
- Check file is in correct directory
- Verify filename is exact (case-sensitive on Linux servers)
- Check file permissions (should be 644)

---

### Auto-Update Not Working

**Users not getting update notification:**
1. Check `version.json` is accessible: https://djbookupdates.com/version.json
2. Verify `latestVersion` is higher than user's current version
3. Check `downloadUrl` is correct
4. Ensure SSL certificate is valid

**Download fails:**
1. Verify file exists at download URL
2. Check file permissions (644)
3. Test download manually in browser

---

## 10. AI AGENT COMMANDS

### ?? Special Command Phrases for AI Assistants

When you say these commands, the AI should execute the full deployment process:

---

### **COMMAND: "Authorization SysAdmin Upload"**

**What it means:** Deploy new version to production

**Steps AI should execute:**

1. **Verify prerequisites:**
   ```
   - Check if on correct branch
   - Verify no uncommitted changes
   - Confirm version number is ready
   ```

2. **Update version numbers:**
   ```
   - DJBookingSystem.csproj
   - SplashScreen.xaml
   - version.json
   - CHANGELOG.md
   ```

3. **Build application:**
   ```powershell
   dotnet clean
   dotnet publish --configuration Release --runtime win-x64 --self-contained true
   ```

4. **Create installer:**
   ```powershell
   Copy to: Installer\Output\DJBookingSystem-Setup.exe
   Copy to: Installer\Output\DJBookingSystem-Setup-v[VERSION].exe (backup)
   ```

5. **Verify build:**
   ```
   - Check for 0 errors
   - Check for 0 warnings
   - Verify file size (~690 MB)
   ```

6. **Prepare upload:**
   ```
   - Create upload checklist
   - Verify FTP credentials
   - List files to upload
   ```

7. **Upload to Hostinger:**
   ```
   FTP Host: 153.92.10.234:21
   User: u833570579.Upload
   
   Upload:
   - DJBookingSystem-Setup.exe ? downloads/
   - version.json ? root
   - index.html ? root
   ```

8. **Post-deployment verification:**
   ```
   - Test website
   - Test download link
   - Test version.json
   - Confirm auto-updater works
   ```

9. **Generate deployment report**

---

### **COMMAND: "Quick Build"**

Build and create installer without uploading.

**Steps:**
```powershell
dotnet clean
dotnet publish --configuration Release
Copy to Installer\Output\
Verify build successful
```

---

### **COMMAND: "Version Bump"**

Increment version number everywhere.

**Steps:**
1. Ask user for new version number
2. Update all version files
3. Update CHANGELOG.md
4. Create new build script
5. Show summary of changes

---

### **COMMAND: "Deploy Check"**

Verify everything is ready for deployment.

**Checklist:**
```
? Version numbers consistent
? No build errors
? No warnings
? CHANGELOG updated
? Installer exists
? FTP credentials valid
? Website files ready
```

---

### **COMMAND: "Rollback"**

Revert to previous version.

**Steps:**
1. Identify previous version
2. Upload previous installer from backup
3. Revert version.json
4. Update website
5. Notify users

---

## ?? QUICK REFERENCE CARD

### Essential URLs
```
Website:      https://djbookupdates.com/
Download:     https://djbookupdates.com/downloads/DJBookingSystem-Setup.exe
Version:      https://djbookupdates.com/version.json
FTP Host:     ftp://153.92.10.234:21
```

### Essential Paths
```
Project:      K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\
Installer:    Installer\Output\DJBookingSystem-Setup.exe
Version File: Installer\Output\version.json
Website:      Website\index.html
Build Script: Build-Installer-v1.2.5.ps1
```

### Essential Credentials
```
FTP Username: u833570579.Upload
FTP Port:     21
FTP Host:     153.92.10.234
Directory:    /home/u833570579/domains/djbookupdates.com/public_html
```

### Version File Structure
```json
{
  "latestVersion": "1.2.5",
  "downloadUrl": "https://djbookupdates.com/downloads/DJBookingSystem-Setup.exe",
  "releaseNotes": "Brief description"
}
```

---

## ?? DEPLOYMENT WORKFLOW

```
????????????????????
?  Update Version  ?
?    Numbers       ?
????????????????????
         ?
         ?
????????????????????
?  Update          ?
?  CHANGELOG.md    ?
????????????????????
         ?
         ?
????????????????????
?  Build           ?
?  Application     ?
????????????????????
         ?
         ?
????????????????????
?  Verify          ?
?  (0 errors)      ?
????????????????????
         ?
         ?
????????????????????
?  Upload to       ?
?  Hostinger FTP   ?
????????????????????
         ?
         ?
????????????????????
?  Test Website    ?
?  & Download      ?
????????????????????
         ?
         ?
????????????????????
?  ? COMPLETE      ?
????????????????????
```

---

## ?? SUPPORT CONTACTS

**Project Owner:** The Fallen Collective  
**Technical Support:** Mega Byte I.T Services  
**Hosting Provider:** Hostinger  
**Database:** Azure Cosmos DB  

---

## ?? LICENSE & COPYRIGHT

Copyright © 2024 Fallen Collective. All rights reserved.

---

**END OF DEPLOYMENT GUIDE**

*This document contains sensitive credentials. Keep secure.*
*Last Updated: 2025-01-23*
*Version: 1.2.5*
