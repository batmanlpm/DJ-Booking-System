# QUICK START - Fancy Installer + Hostinger Setup

## ?? Complete Setup in 30 Minutes

### **Phase 1: Create Graphics (10 mins)**

#### **Option A: Use Canva (Easiest)**
1. Go to https://canva.com
2. Create 164x314 canvas ? Add space bg + text ? Download as PNG
3. Convert PNG to BMP: https://convertio.co/png-bmp/
4. Create 55x55 icon ? Download ? Convert to BMP
5. Save both as:
   - `Assets/WizardImage.bmp`
   - `Assets/WizardSmallImage.bmp`

#### **Option B: Skip Graphics (Use Placeholders)**
- Installer will work without graphics
- Can add them later

---

### **Phase 2: Build Installer (5 mins)**

```powershell
# 1. Install Inno Setup
# Download: https://jrsoftware.org/isdl.php
# Run installer, click Next ? Install

# 2. Run build script
.\Build-Installer.ps1

# Output: Installer\Output\DJBookingSystem-Setup-v1.2.0.exe
```

---

### **Phase 3: Hostinger Setup (10 mins)**

#### **Step 1: Login to Hostinger**
https://hpanel.hostinger.com

#### **Step 2: Enable SSL (Important!)**
```
1. Go to: Security ? SSL
2. Click: Install SSL
3. Choose: Free Let's Encrypt SSL
4. Wait 10-15 minutes for activation
```

#### **Step 3: Create Folder Structure**
```
1. Go to: Files ? File Manager
2. Navigate to: public_html
3. Create folder: updates
4. Inside updates, create: installers
```

Result:
```
public_html/
??? updates/
    ??? version.json (create next)
    ??? installers/ (upload installer here)
```

#### **Step 4: Upload Installer**
```
1. Go to: public_html/updates/installers/
2. Click: Upload
3. Select: DJBookingSystem-Setup-v1.2.0.exe
4. Wait for upload (may take 5-10 mins depending on size)
```

#### **Step 5: Create version.json**
```
1. Go to: public_html/updates/
2. Click: New File
3. Name: version.json
4. Paste this content:
```

```json
{
  "updateAvailable": true,
  "latestVersion": "1.2.0",
  "releaseDate": "2025-01-15T10:00:00Z",
  "downloadUrl": "https://YOURDOMAIN.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe",
  "releaseNotes": "Major update with new features",
  "features": [
    "Discord integration with auto-login",
    "Hourly automatic updates",
    "Improved performance"
  ],
  "bugFixes": [
    "Fixed initialization errors",
    "Fixed status tracking"
  ],
  "isCritical": true,
  "minimumVersion": "1.0.0"
}
```

**Replace YOURDOMAIN.com with your actual domain!**

---

### **Phase 4: Update Your App (5 mins)**

#### **1. Get SSL Certificate Fingerprint**

**In Browser:**
```
1. Visit: https://yourdomain.com
2. Click padlock icon ? Certificate
3. Go to Details tab
4. Find: SHA-256 Fingerprint
5. Copy the value (e.g., AA:BB:CC:DD:...)
```

#### **2. Update SecureUpdateClient.cs**

Find line 25:
```csharp
private const string UPDATE_SERVER_URL = "https://djbookupdates.com";
```

Change to:
```csharp
private const string UPDATE_SERVER_URL = "https://YOURDOMAIN.com";
```

Find line 27:
```csharp
private const string UPDATE_CHECK_ENDPOINT = "/api/updates/check";
```

Change to:
```csharp
private const string UPDATE_CHECK_ENDPOINT = "/updates/version.json";
```

Find lines 30-36:
```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "YOUR_CERTIFICATE_SHA256_FINGERPRINT_HERE",
};
```

Replace with your actual fingerprint:
```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99",
};
```

#### **3. Build and Test**
```powershell
dotnet build
```

---

## ? Verification Checklist

### **Hostinger:**
- [ ] SSL enabled (https:// works)
- [ ] `/updates/` folder created
- [ ] `/updates/installers/` folder created
- [ ] Installer uploaded
- [ ] `version.json` created
- [ ] File permissions set to 644

### **Client App:**
- [ ] UPDATE_SERVER_URL updated
- [ ] UPDATE_CHECK_ENDPOINT updated
- [ ] SSL fingerprint added
- [ ] Build successful

### **Testing:**
- [ ] Visit: `https://yourdomain.com/updates/version.json` (should show JSON)
- [ ] Visit: `https://yourdomain.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe` (should download)
- [ ] Run app ? Wait for update check (3 seconds)
- [ ] Check debug output for success

---

## ?? Test Update System

### **1. Manual Test**
```csharp
// In your app, press a test button that calls:
await UpdateManager.CheckForUpdatesOnStartupAsync(
    showNotifications: true, 
    forceDownload: false
);
```

### **2. Expected Behavior**
```
[3 seconds after startup]
Checking for updates on startup (secure connection)...
Connecting to: https://yourdomain.com/updates/version.json
Update available: 1.2.0
Security Status: ? Secure connection | ? Certificate pinned
[Dialog appears]
User sees update notification
```

### **3. Debug Output**
Check Visual Studio Output window for:
```
[UpdateManager] Checking for updates...
[SecureUpdateClient] Connecting to: https://yourdomain.com
[SecureUpdateClient] Certificate validated
[UpdateManager] Update available: 1.2.0
[UpdateManager] Showing notification dialog
```

---

## ?? Troubleshooting

### **Problem: "Update check failed"**
```
Solution:
1. Check if version.json is accessible:
   https://yourdomain.com/updates/version.json
2. Verify SSL is enabled
3. Check UPDATE_SERVER_URL matches domain exactly
```

### **Problem: "Certificate validation failed"**
```
Solution:
1. Get correct SSL fingerprint from browser
2. Update TRUSTED_FINGERPRINTS array
3. Rebuild application
```

### **Problem: "Download URL returns 404"**
```
Solution:
1. Verify installer uploaded to correct path:
   /public_html/updates/installers/
2. Check downloadUrl in version.json matches exactly
3. Test URL in browser first
```

### **Problem: "Hourly checks not working"**
```
Solution:
1. Check debug output for: "Next update check scheduled for..."
2. Verify timer is created: "Automatic update checks enabled"
3. Wait until next hour strikes
4. Check output for: "Hourly update check triggered"
```

---

## ?? File Locations Summary

### **Your Computer:**
```
K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\
??? Assets/
?   ??? WizardImage.bmp
?   ??? WizardSmallImage.bmp
?   ??? SetupIcon.ico
??? installer.iss
??? Build-Installer.ps1
??? Installer\Output\
    ??? DJBookingSystem-Setup-v1.2.0.exe
```

### **Hostinger Server:**
```
public_html/
??? updates/
    ??? version.json
    ??? installers/
        ??? DJBookingSystem-Setup-v1.2.0.exe
```

### **User's Computer (After Install):**
```
C:\Program Files\DJ Booking System\
??? DJBookingSystem.exe
??? (all app files)
??? Assets\
```

---

## ?? Quick Commands Reference

### **Build Installer:**
```powershell
.\Build-Installer.ps1
```

### **Upload to Hostinger (FTP):**
```bash
# Using WinSCP
open ftp://username@ftp.yourdomain.com
cd /public_html/updates/installers
put "Installer\Output\DJBookingSystem-Setup-v1.2.0.exe"
exit
```

### **Test URLs:**
```
Version Check: https://yourdomain.com/updates/version.json
Installer: https://yourdomain.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe
```

### **Get SSL Fingerprint:**
```powershell
# PowerShell (Windows)
$cert = [Net.ServicePointManager]::FindServicePoint("https://yourdomain.com", $null).Certificate
$cert.GetCertHashString("SHA256")
```

---

## ?? Support

### **Hostinger Support:**
- Live Chat: https://hpanel.hostinger.com
- Knowledge Base: https://support.hostinger.com
- 24/7 Support available

### **Inno Setup Help:**
- Documentation: https://jrsoftware.org/ishelp/
- Forum: https://groups.google.com/g/innosetup

---

## ?? Success Indicators

You'll know it's working when:

1. ? `version.json` loads in browser
2. ? Installer downloads in browser
3. ? App shows: "Checking for updates..." in debug
4. ? Update dialog appears after 3 seconds
5. ? "Update available: 1.2.0" in debug output
6. ? Download progress shows
7. ? Installation completes
8. ? App restarts with new version

---

## ?? Timeline

| Task | Time | Status |
|------|------|--------|
| Create graphics | 10 min | ? |
| Build installer | 5 min | ? |
| Hostinger setup | 10 min | ? |
| Update client code | 5 min | ? |
| **TOTAL** | **30 min** | ? |

---

## ?? Ready to Start!

**Next Step**: Choose your path:

**Path A: Full Setup (30 mins)**
1. Create graphics with Canva
2. Build fancy installer
3. Upload to Hostinger
4. Users get beautiful installer!

**Path B: Quick Start (15 mins)**
1. Skip graphics (use defaults)
2. Build basic installer
3. Upload to Hostinger
4. Add graphics later

**Both paths work! Choose based on time available.**

---

**?? Your URL After Setup:**
```
Update Check: https://YOURDOMAIN.com/updates/version.json
Installer: https://YOURDOMAIN.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe
```

**?? Update Flow:**
```
Deploy ? Hostinger ? Users check (every hour) ? Forced download ? Install ? Done!
```

**?? Result:**
- ? Professional fancy installer
- ? Hourly automatic updates
- ? Forced downloads (no cancel)
- ? SSL-secured updates
- ? All hosted on Hostinger

**Let's build! ??**
