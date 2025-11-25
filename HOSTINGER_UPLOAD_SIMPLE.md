# ?? HOSTINGER UPLOAD - Complete Guide

## ? Correct Server Path

**Upload Location on Hostinger:**
```
public_html\Updates\
```

**NOT** `public_html/updates/installers/` - Keep it simple!

---

## ?? Folder Structure on Hostinger

```
public_html/
??? Updates/
    ??? version.json                          ? Upload here
    ??? DJBookingSystem-Setup-v1.2.0.exe     ? Upload here
    ??? DJBookingSystem-Setup-v1.1.0.exe     (older versions)
    ??? changelog.html                        (optional)
```

---

## ?? Quick Upload Steps

### **Step 1: Enable SSL (First Time Only)**

```
1. Login: https://hpanel.hostinger.com
2. Go to: Hosting ? [Your Domain]
3. Click: Security ? SSL
4. Click: Install SSL
5. Choose: Free (Let's Encrypt SSL)
6. Wait: 10-15 minutes for activation
```

**Test SSL**: Visit `https://yourdomain.com` (should see padlock ??)

---

### **Step 2: Create Updates Folder**

**Via Hostinger File Manager:**
```
1. Hostinger ? Files ? File Manager
2. Navigate to: public_html
3. Click: New Folder
4. Name: Updates
5. Done!
```

**Via FileZilla (FTP):**
```
1. Connect to FTP
2. Navigate to: /public_html/
3. Right-click ? Create directory: "Updates"
4. Done!
```

**Result:**
```
public_html/
??? Updates/  ? Created
```

---

### **Step 3: Upload Installer**

**Location**: `public_html\Updates\DJBookingSystem-Setup-v1.2.0.exe`

**Via FileZilla (Recommended for large files):**
```
1. Download FileZilla: https://filezilla-project.org/
2. Get FTP credentials from Hostinger:
   - Hostinger ? Files ? FTP Accounts
3. Connect:
   - Host: ftp.yourdomain.com
   - Username: [from Hostinger]
   - Password: [from Hostinger]
   - Port: 21
4. Local (left): Navigate to:
   K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Installer\Output\
5. Remote (right): Navigate to:
   /public_html/Updates/
6. Drag & drop:
   DJBookingSystem-Setup-v1.2.0.exe ? Right side
7. Wait: 5-15 minutes (depending on size)
```

**Via Hostinger File Manager:**
```
1. Go to: public_html/Updates/
2. Click: Upload
3. Select: DJBookingSystem-Setup-v1.2.0.exe
4. Wait for upload
```

? **Verify**: Visit `https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.2.0.exe`  
Should download the file!

---

### **Step 4: Create version.json**

**Via Hostinger File Manager:**

```
1. Go to: public_html/Updates/
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
  "downloadUrl": "https://YOUR-DOMAIN.com/Updates/DJBookingSystem-Setup-v1.2.0.exe",
  "changelogUrl": "https://YOUR-DOMAIN.com/Updates/changelog.html",
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
6. Click: Save
```

? **Verify**: Visit `https://yourdomain.com/Updates/version.json`  
Should show JSON content!

---

## ?? Update Client Code

### **Step 1: Get SSL Certificate Fingerprint**

**In Browser:**
```
1. Visit: https://YOUR-DOMAIN.com
2. Click: ?? Padlock icon
3. Click: Certificate
4. Go to: Details tab
5. Find: SHA-256 Fingerprint
6. Copy: AA:BB:CC:DD:EE:...
```

**Or PowerShell:**
```powershell
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

### **Step 2: Update SecureUpdateClient.cs**

**Open**: `Services\SecureUpdateClient.cs`

**Change these lines:**

```csharp
// Line ~25: Update server URL
private const string UPDATE_SERVER_URL = "https://YOUR-DOMAIN.com";

// Line ~27: Update endpoint
private const string UPDATE_CHECK_ENDPOINT = "/Updates/version.json";

// Line ~30-36: Add SSL fingerprint
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99"
    // ? Your actual SHA-256 fingerprint!
};
```

**Save and build:**
```powershell
dotnet build
```

---

## ?? Testing

### **Test 1: URLs Work**

**Open in browser:**
```
1. https://yourdomain.com/Updates/version.json
   ? Shows JSON

2. https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.2.0.exe
   ? Downloads installer
```

---

### **Test 2: App Update Check**

**Run your app:**
```
Wait 3 seconds ? Update dialog appears!
```

**Expected in Output window:**
```
Checking for updates on startup (secure connection)...
Connecting to: https://yourdomain.com
Certificate validated ?
Update available: 1.2.0 ?
Showing notification dialog ?
```

---

### **Test 3: Hourly Timer**

**Check Output window:**
```
Next update check scheduled for: [NEXT HOUR]:00:00
Time until next check: [X] minutes
Automatic update checks enabled ?
```

---

## ?? URL Reference

### **Your Update URLs:**

| Purpose | URL |
|---------|-----|
| Version Check | `https://yourdomain.com/Updates/version.json` |
| Installer Download | `https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.2.0.exe` |
| Changelog (optional) | `https://yourdomain.com/Updates/changelog.html` |

---

## ?? Deploying Future Updates

### **When You Release New Version:**

**1. Build new installer:**
```powershell
.\QUICK-BUILD.bat
```

**2. Upload to Hostinger:**
- Via FTP to: `/public_html/Updates/`
- File: `DJBookingSystem-Setup-v1.3.0.exe`

**3. Update version.json:**
```json
{
  "latestVersion": "1.3.0",
  "releaseDate": "2025-02-01T10:00:00Z",
  "downloadUrl": "https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.3.0.exe",
  "features": ["New feature 1", "New feature 2"],
  "bugFixes": ["Bug fix 1", "Bug fix 2"]
}
```

**4. All users get updated within 1 hour!**

---

## ? Final Checklist

- [ ] SSL enabled on Hostinger (https works)
- [ ] Folder created: `public_html/Updates/`
- [ ] Installer uploaded: `DJBookingSystem-Setup-v1.2.0.exe`
- [ ] `version.json` created and accessible
- [ ] SSL fingerprint obtained
- [ ] `SecureUpdateClient.cs` updated:
  - [ ] Domain URL
  - [ ] Endpoint: `/Updates/version.json`
  - [ ] SSL fingerprint
- [ ] App builds successfully
- [ ] URLs tested in browser
- [ ] Update dialog appears after 3 seconds

---

## ?? Quick Commands

```powershell
# Test URLs
Start-Process "https://yourdomain.com/Updates/version.json"
Start-Process "https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.2.0.exe"

# Build installer
.\QUICK-BUILD.bat

# Build app
dotnet build
```

---

## ?? Troubleshooting

### **"404 Not Found"**
```
? Check folder name is exactly: Updates (capital U)
? Check file uploaded successfully
? Test URL in browser
```

### **"Certificate validation failed"**
```
? Get correct SHA-256 fingerprint (not SHA-1)
? Format: AA:BB:CC:DD:...
? Update TRUSTED_FINGERPRINTS array
? Rebuild app
```

### **"Update check failed"**
```
? Verify SSL works: https://yourdomain.com
? Check version.json accessible
? Verify UPDATE_SERVER_URL matches domain
```

---

## ?? Success Indicators

**You'll know it works when:**

1. ? Browser opens version.json successfully
2. ? Browser downloads installer successfully
3. ? App shows "Checking for updates..." in debug
4. ? Update dialog appears after 3 seconds
5. ? Dialog shows version 1.2.0
6. ? Hourly timer is scheduled

---

**Server Path**: `public_html\Updates\`  
**Version Check**: `/Updates/version.json`  
**Installer**: `/Updates/DJBookingSystem-Setup-v1.2.0.exe`

**Simple. Clean. Works!** ??
