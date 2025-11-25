# ?? LET'S START! Complete Setup Guide

## ?? Overview

We're going to set up:
1. ? Professional fancy installer with graphics
2. ? Hostinger server for hosting updates
3. ? Hourly automatic forced updates
4. ? Complete deployment workflow

**Estimated Time**: 30-45 minutes  
**Difficulty**: Easy (step-by-step)

---

## ?? Phase 1: Prerequisites Check (5 minutes)

### **Step 1.1: Verify Your Environment**

Open PowerShell in your project directory and run:

```powershell
# Check .NET SDK
dotnet --version
# Should show: 8.x.x

# Check project builds
dotnet build
# Should succeed

# Check current location
pwd
# Should be: K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\
```

? **All good? Continue!**  
? **Errors?** Let me know which step failed.

---

### **Step 1.2: Download Inno Setup**

1. **Go to**: https://jrsoftware.org/isdl.php
2. **Download**: Inno Setup 6 (latest version)
3. **Install**: Run installer, click Next ? Install ? Finish
4. **Verify**: Should be installed at `C:\Program Files (x86)\Inno Setup 6\`

? **Installed? Great!**

---

### **Step 1.3: Verify Hostinger Access**

1. **Open**: https://hpanel.hostinger.com
2. **Login**: With your credentials
3. **Verify**: You can see your domain/hosting plan
4. **Note**: Your domain name (we'll need this)

? **Can login? Perfect!**

---

## ?? Phase 2: Create Graphics (10-15 minutes)

You have **2 options**:

### **Option A: Quick & Simple (Recommended for now)**
Skip graphics creation, use the installer without custom images first. You can add graphics later!

**Choose this if**: You want to get everything working ASAP

### **Option B: Create Professional Graphics**
Use Canva to create custom space-themed graphics.

**Choose this if**: You have 15 extra minutes and want the installer to look amazing

---

### **For Option A (Skip Graphics):**

```powershell
# Create placeholder directories
New-Item -Path "Assets" -ItemType Directory -Force
New-Item -Path "Prerequisites" -ItemType Directory -Force

Write-Host "? Directories created. Skipping graphics for now."
```

**Continue to Phase 3 ?**

---

### **For Option B (Create Graphics):**

#### **Step 2.1: Open Canva**
1. Go to: https://canva.com
2. Sign up/Login (FREE account)

#### **Step 2.2: Create WizardImage.bmp**

```
1. Click: Create a design ? Custom size
2. Enter: Width: 164 pixels, Height: 314 pixels
3. Click: Create new design

4. Add background:
   - Search: "space stars dark"
   - Choose a dark space image
   - Adjust to fit

5. Add text:
   - Add text box: "THE FALLEN COLLECTIVE"
   - Font: Orbitron or Exo (futuristic)
   - Color: #00FF00 (neon green)
   - Size: Large, bold
   
   - Add another text: "DJ BOOKING SYSTEM"
   - Color: #00BFFF (light blue)
   - Size: Medium

6. Add icon (optional):
   - Search: "DJ turntable icon"
   - Add to center
   - Adjust size

7. Download:
   - Click: Share ? Download
   - Format: PNG
   - Click: Download
```

#### **Step 2.3: Convert PNG to BMP**

1. Go to: https://convertio.co/png-bmp/
2. Upload your PNG file
3. Click: Convert
4. Download: Save as `WizardImage.bmp`
5. Move to: `K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Assets\`

#### **Step 2.4: Create WizardSmallImage.bmp**

```
1. Canva ? Custom size: 55 x 55 pixels
2. Add simple logo or DJ icon
3. Dark background + neon green accent
4. Download as PNG
5. Convert to BMP (same as above)
6. Save as: WizardSmallImage.bmp
7. Move to Assets folder
```

#### **Step 2.5: Create Icon**

```
1. Canva ? 256 x 256 pixels
2. Simple DJ logo/icon
3. Download as PNG
4. Go to: https://convertio.co/png-ico/
5. Upload ? Convert ? Download
6. Save as: SetupIcon.ico
7. Move to Assets folder
```

? **Graphics created!**

---

## ?? Phase 3: Build Installer (5 minutes)

### **Step 3.1: Update installer.iss**

Open `installer.iss` and update these lines:

**Line 4-6** (Update your info):
```iss
#define MyAppVersion "1.2.0"
#define MyAppPublisher "The Fallen Collective & Mega Byte I.T Services"
#define MyAppURL "https://YOUR-DOMAIN.com"
```

**Replace** `YOUR-DOMAIN.com` with your actual domain!

### **Step 3.2: Run Build Script**

```powershell
# Run the automated build
.\Build-Installer.ps1
```

**Expected Output:**
```
========================================
 DJ BOOKING SYSTEM INSTALLER BUILDER
========================================

Step 1: Checking prerequisites...
  Inno Setup found!

Step 2: Building application...
  Publishing .NET application...
  Application built successfully!

Step 3: Creating directories...
  Directories ready!

Step 4: Checking WebView2 Runtime...
  Downloading WebView2 Runtime...
  WebView2 Runtime downloaded!

Step 5: Checking graphics assets...
  [Graphics status]

Step 7: Building installer...
  Running Inno Setup Compiler...
  Installer created successfully!
  File: DJBookingSystem-Setup-v1.2.0.exe
  Size: ~125 MB

Step 9: Calculating file hash...
  SHA256: [HASH]

========================================
 BUILD COMPLETE!
========================================
```

? **Installer created!** Look in: `Installer\Output\`

? **Error?** Tell me the error message.

---

## ?? Phase 4: Hostinger Setup (10 minutes)

### **Step 4.1: Enable SSL Certificate**

**IMPORTANT: Do this first!**

```
1. Login: https://hpanel.hostinger.com
2. Go to: Hosting ? [Your Domain]
3. Click: Security ? SSL
4. Click: Install SSL
5. Choose: Free (Let's Encrypt SSL)
6. Click: Install

? Wait: 10-15 minutes for SSL to activate
```

**Verify SSL works:**
- Visit: `https://YOUR-DOMAIN.com`
- See green padlock? ? SSL active!
- Still see warning? ? Wait a few more minutes

---

### **Step 4.2: Create Folder Structure**

**Option 1: File Manager (Easy)**

```
1. Hostinger ? Files ? File Manager
2. Navigate to: public_html
3. Create new folder: "updates"
4. Enter "updates" folder
5. Create new folder: "installers"
```

**Option 2: FTP (Recommended for large files)**

```
1. Download FileZilla: https://filezilla-project.org/
2. Get FTP credentials:
   - Hostinger ? Files ? FTP Accounts
   - Note: Host, Username, Password
3. FileZilla ? Connect:
   - Host: ftp.yourdomain.com
   - Username: [your username]
   - Password: [your password]
   - Port: 21
4. Navigate to: /public_html/
5. Right-click ? Create directory: "updates"
6. Enter "updates" ? Create directory: "installers"
```

**Result:**
```
public_html/
??? updates/
    ??? installers/
```

? **Folders created!**

---

### **Step 4.3: Upload Installer**

**Using FileZilla (Recommended):**

```
1. Local site (left): Navigate to:
   K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Installer\Output\

2. Remote site (right): Navigate to:
   /public_html/updates/installers/

3. Drag & drop:
   DJBookingSystem-Setup-v1.2.0.exe ? Right side

? Wait: 5-15 minutes (depending on connection)
```

**Using File Manager:**

```
1. Go to: public_html/updates/installers/
2. Click: Upload
3. Select: DJBookingSystem-Setup-v1.2.0.exe
4. Wait for upload to complete
```

? **Installer uploaded!**

**Verify**: Visit `https://yourdomain.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe`  
Should download the file!

---

### **Step 4.4: Create version.json**

**Using File Manager:**

```
1. Go to: public_html/updates/
2. Click: New File
3. Name: version.json
4. Click: Edit
5. Paste this content:
```

```json
{
  "updateAvailable": true,
  "currentVersion": "1.0.0",
  "latestVersion": "1.2.0",
  "releaseDate": "2025-01-15T10:00:00Z",
  "downloadUrl": "https://YOUR-DOMAIN.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe",
  "changelogUrl": "https://YOUR-DOMAIN.com/updates/changelog.html",
  "releaseNotes": "Major update with Discord integration and hourly auto-updates",
  "features": [
    "Discord WebView2 integration with auto-login",
    "Hourly automatic forced updates",
    "Improved UI performance",
    "Chat mode selection dialog"
  ],
  "bugFixes": [
    "Fixed WebView2 initialization error",
    "Fixed online status tracking",
    "Fixed auto-login issues"
  ],
  "isCritical": true,
  "minimumVersion": "1.0.0",
  "isSecureConnection": true
}
```

**?? IMPORTANT**: Replace `YOUR-DOMAIN.com` with your actual domain!

```
6. Click: Save Changes
```

? **version.json created!**

**Verify**: Visit `https://yourdomain.com/updates/version.json`  
Should show the JSON content!

---

## ?? Phase 5: Update Client Code (5 minutes)

### **Step 5.1: Get SSL Certificate Fingerprint**

**In your browser:**

```
1. Visit: https://YOUR-DOMAIN.com
2. Click: Padlock icon (left of URL)
3. Click: Connection is secure
4. Click: Certificate is valid
5. Go to: Details tab
6. Find: SHA-256 Fingerprint
7. Copy the value (looks like: AA:BB:CC:DD:EE:...)
```

**Or use PowerShell:**

```powershell
# Quick method (Windows)
$url = "YOUR-DOMAIN.com"
$tcpClient = New-Object System.Net.Sockets.TcpClient($url, 443)
$sslStream = New-Object System.Net.Security.SslStream($tcpClient.GetStream())
$sslStream.AuthenticateAsClient($url)
$cert = $sslStream.RemoteCertificate
$hash = $cert.GetCertHashString("SHA256")
$formatted = ($hash -replace '(.{2})', '$1:').TrimEnd(':')
Write-Host "SHA-256: $formatted"
$sslStream.Close()
$tcpClient.Close()
```

---

### **Step 5.2: Update SecureUpdateClient.cs**

Open: `Services\SecureUpdateClient.cs`

**Find line ~25:**
```csharp
private const string UPDATE_SERVER_URL = "https://djbookupdates.com";
```

**Change to:**
```csharp
private const string UPDATE_SERVER_URL = "https://YOUR-DOMAIN.com";
```

**Find line ~27:**
```csharp
private const string UPDATE_CHECK_ENDPOINT = "/api/updates/check";
```

**Change to:**
```csharp
private const string UPDATE_CHECK_ENDPOINT = "/updates/version.json";
```

**Find lines ~30-36:**
```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "YOUR_CERTIFICATE_SHA256_FINGERPRINT_HERE",
    "BACKUP_CERTIFICATE_SHA256_FINGERPRINT_HERE"
};
```

**Replace with your actual fingerprint:**
```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99"
    // Your actual SHA-256 fingerprint here!
};
```

**Save the file!**

---

### **Step 5.3: Build and Test**

```powershell
# Build the application
dotnet build

# Should succeed!
```

? **Build successful!**

---

## ?? Phase 6: Test Everything (5 minutes)

### **Step 6.1: Test Server URLs**

**In your browser, test these URLs:**

```
1. https://YOUR-DOMAIN.com/updates/version.json
   ? Should show JSON with version info

2. https://YOUR-DOMAIN.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe
   ? Should download the installer

Both must work for updates to work!
```

---

### **Step 6.2: Test Update Check**

**Run your application:**

```powershell
# Start the app
.\bin\Debug\net8.0-windows\DJBookingSystem.exe
```

**Check Output window in Visual Studio:**

```
Expected output after 3 seconds:
-----------------------------------
[UpdateManager] Checking for updates on startup (secure connection)...
[SecureUpdateClient] Connecting to: https://YOUR-DOMAIN.com
[SecureUpdateClient] Certificate validated
[UpdateManager] Update available: 1.2.0
[UpdateManager] Showing notification dialog
```

**Dialog should appear!**

? **Update dialog appeared? SUCCESS!**  
? **No dialog? Check the debug output for errors**

---

### **Step 6.3: Test Hourly Check**

**Check Output window:**

```
Expected:
---------
Next update check scheduled for: [NEXT HOUR]:00:00
Time until next check: [X] minutes
Automatic update checks enabled (every hour on the hour, secure connection, forced download)
```

? **Timer scheduled? Perfect!**

---

### **Step 6.4: Test Forced Download (Optional)**

**In UpdateManager.cs, add a test method:**

```csharp
public static async Task TestForcedUpdate()
{
    await CheckForUpdatesOnStartupAsync(
        showNotifications: true, 
        forceDownload: true
    );
}
```

**Call it from somewhere to test:**
- Dialog appears
- Cannot close
- Download starts automatically after 2 seconds
- Progress bar shows

---

## ? Phase 7: Verify Complete Setup

### **Final Checklist:**

- [ ] Inno Setup installed
- [ ] Installer built successfully
- [ ] Hostinger SSL enabled (https works)
- [ ] Folders created: `/updates/` and `/installers/`
- [ ] Installer uploaded to Hostinger
- [ ] `version.json` created and accessible
- [ ] SSL fingerprint obtained
- [ ] `SecureUpdateClient.cs` updated with:
  - [ ] Your domain URL
  - [ ] Correct endpoint (`/updates/version.json`)
  - [ ] SSL fingerprint
- [ ] Application builds successfully
- [ ] Update check works (dialog appears)
- [ ] Hourly timer is scheduled

---

## ?? SUCCESS INDICATORS

### **You'll know everything works when:**

1. ? You can download installer from: `https://yourdomain.com/updates/installers/[filename].exe`
2. ? You can view JSON from: `https://yourdomain.com/updates/version.json`
3. ? App shows: "Checking for updates..." in debug output
4. ? Update dialog appears after 3 seconds
5. ? Dialog shows version 1.2.0
6. ? "Next update check scheduled" appears in output
7. ? Forced download mode works (cannot close dialog)

---

## ?? Common Issues & Solutions

### **Issue: "Update check failed"**
```
? Solution:
1. Verify SSL works: https://yourdomain.com
2. Check version.json is accessible
3. Verify UPDATE_SERVER_URL matches exactly
4. Check firewall isn't blocking
```

### **Issue: "Certificate validation failed"**
```
? Solution:
1. Get correct fingerprint from browser
2. Make sure you copied the SHA-256, not SHA-1
3. Format should be: AA:BB:CC:DD:...
4. Update TRUSTED_FINGERPRINTS array
5. Rebuild application
```

### **Issue: "Installer file not found (404)"**
```
? Solution:
1. Verify upload completed successfully
2. Check file is in: /public_html/updates/installers/
3. Verify filename matches exactly in version.json
4. Test download URL directly in browser
```

### **Issue: "Hourly checks not running"**
```
? Solution:
1. Check debug output for timer creation
2. Verify "Next update check scheduled" appears
3. Wait until the top of the hour
4. Check output again for "Hourly update check triggered"
```

---

## ?? What Happens Next

### **For You (Developer):**
```
When you deploy a new version:
1. Build new installer
2. Upload to Hostinger
3. Update version.json
4. Within 1 hour ? All users get forced update!
```

### **For Your Users:**
```
Every hour on the hour:
1. App checks for updates
2. Finds new version
3. Dialog appears (cannot close)
4. Download starts automatically
5. Installation completes
6. App restarts with new version
No user interaction needed!
```

---

## ?? Quick Commands Reference

```powershell
# Build installer
.\Build-Installer.ps1

# Build app
dotnet build

# Run app
.\bin\Debug\net8.0-windows\DJBookingSystem.exe

# Get SSL fingerprint
# [Use browser method above]

# Upload via FTP (FileZilla)
# [Connect ? Drag & Drop]

# Test URLs
Start-Process "https://yourdomain.com/updates/version.json"
Start-Process "https://yourdomain.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe"
```

---

## ?? Need Help?

**If you get stuck:**

1. **Screenshot the error**
2. **Check which phase you're in**
3. **Tell me**:
   - What step you're on
   - What error you see
   - What you expected to happen

I'll help you fix it immediately!

---

## ?? Ready to Start?

**Let's begin!**

Tell me:
1. ? "Ready to start Phase 1" - Prerequisites check
2. ?? "Skip to Phase [X]" - If you've done some steps already
3. ? "I have a question about [X]"

**Let's do this! ??**
