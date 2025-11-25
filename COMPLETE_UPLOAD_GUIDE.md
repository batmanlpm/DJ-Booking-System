# ?? COMPLETE UPLOAD GUIDE - Ready to Go!

## ? Your FTP Details

```
Host: ftp://153.92.10.234
Username: u833570579.djbookupdates.com
Password: Fraser1960@
Port: 21
Upload Folder: public_html\Updates
```

---

## ?? Files to Upload

**From**: `K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Installer\Output\`

**To**: `public_html\Updates\`

**Files:**
1. `DJBookingSystem-Setup-v1.2.0.exe` (~145 MB)
2. `version.json` (you already have this on server)

---

## ?? FileZilla Setup (Step-by-Step)

### **Step 1: Download & Install FileZilla**
https://filezilla-project.org/download.php?type=client

### **Step 2: Connect to Your Server**

**Open FileZilla and enter:**

| Field | Value |
|-------|-------|
| Host | `153.92.10.234` |
| Username | `u833570579.djbookupdates.com` |
| Password | `Fraser1960@` |
| Port | `21` |

**Click**: Quickconnect

### **Step 3: Navigate to Upload Folder**

**In FileZilla (right side - Remote site):**
```
1. Navigate to: /public_html/Updates/
2. You should see: version.json (already there)
```

### **Step 4: Upload Installer**

**In FileZilla (left side - Local site):**
```
1. Navigate to:
   K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Installer\Output\

2. Find: DJBookingSystem-Setup-v1.2.0.exe

3. Drag & drop to right side (Remote site)

4. Wait: 5-15 minutes (depending on your upload speed)
```

**Progress bar will show upload status**

---

## ? Update version.json on Server

**Your version.json should contain:**

```json
{
  "updateAvailable": true,
  "currentVersion": "1.0.0",
  "latestVersion": "1.2.0",
  "releaseDate": "2025-01-15T10:00:00Z",
  "downloadUrl": "https://djbookupdates.com/Updates/DJBookingSystem-Setup-v1.2.0.exe",
  "changelogUrl": "https://djbookupdates.com/Updates/changelog.html",
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

**Edit via Hostinger File Manager:**
```
1. Login: https://hpanel.hostinger.com
2. Go to: Files ? File Manager
3. Navigate to: public_html/Updates/
4. Right-click version.json ? Edit
5. Update the content
6. Save
```

---

## ?? Get SSL Certificate Fingerprint

### **Method 1: Browser (Easy)**

```
1. Visit: https://djbookupdates.com
2. Click: ?? Padlock icon in address bar
3. Click: "Connection is secure"
4. Click: "Certificate is valid"
5. Go to: Details tab
6. Find: SHA-256 Fingerprint
7. Copy the entire fingerprint (format: AA:BB:CC:DD:EE:...)
```

### **Method 2: PowerShell**

```powershell
$url = "djbookupdates.com"
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

**Copy the output!**

---

## ?? Update Your Code

### **Open**: `Services\SecureUpdateClient.cs`

**Find lines 30-36** and replace with your actual fingerprint:

```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    // Primary certificate fingerprint - GET THIS FROM BROWSER!
    "PASTE_YOUR_ACTUAL_SHA256_FINGERPRINT_HERE",
    // Backup certificate (leave as placeholder for now)
    "BACKUP_CERTIFICATE_SHA256_FINGERPRINT_HERE"
};
```

**Example with real fingerprint:**
```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "4A:6E:BA:5D:C9:29:4F:55:84:F2:CD:69:B8:0D:FC:59:92:26:13:0D:97:13:23:24:E8:A4:4D:CC:6F:93:2E:92",
    "BACKUP_CERTIFICATE_SHA256_FINGERPRINT_HERE"
};
```

**Save the file!**

---

## ?? Build & Test

### **Step 1: Build Application**

```powershell
dotnet build
```

**Should succeed!**

---

### **Step 2: Test URLs in Browser**

```
1. Test version.json:
   https://djbookupdates.com/Updates/version.json
   ? Should show JSON

2. Test installer download:
   https://djbookupdates.com/Updates/DJBookingSystem-Setup-v1.2.0.exe
   ? Should download the file
```

---

### **Step 3: Test Update Check in App**

```
1. Run your application
2. Wait 3 seconds
3. Update dialog should appear!
```

**Expected in Output window:**
```
Checking for updates on startup (secure connection)...
Connecting to: https://djbookupdates.com
Certificate validated ?
Update available: 1.2.0 ?
Showing notification dialog ?
```

---

### **Step 4: Test Hourly Timer**

**Check Output window:**
```
Next update check scheduled for: [NEXT HOUR]:00:00
Time until next check: [X] minutes
Automatic update checks enabled ?
```

---

## ? Complete Checklist

- [ ] FileZilla installed
- [ ] Connected to FTP server (153.92.10.234)
- [ ] Navigated to: /public_html/Updates/
- [ ] Uploaded: DJBookingSystem-Setup-v1.2.0.exe
- [ ] Verified upload (file visible on server)
- [ ] Updated version.json with correct downloadUrl
- [ ] Got SSL fingerprint from djbookupdates.com
- [ ] Added fingerprint to TRUSTED_FINGERPRINTS array
- [ ] Rebuilt application (`dotnet build`)
- [ ] Tested URLs in browser (both work)
- [ ] Ran app - update dialog appeared
- [ ] Hourly timer scheduled

---

## ?? Quick Commands

```powershell
# Build app
dotnet build

# Test URLs
Start-Process "https://djbookupdates.com/Updates/version.json"
Start-Process "https://djbookupdates.com/Updates/DJBookingSystem-Setup-v1.2.0.exe"

# Run app
.\bin\Debug\net8.0-windows\DJBookingSystem.exe
```

---

## ?? Final Server Structure

```
public_html/
??? Updates/
    ??? version.json                          ? Already there
    ??? DJBookingSystem-Setup-v1.2.0.exe     ? Upload this
```

---

## ?? Troubleshooting

### **"Connection failed" in FileZilla**
```
? Check all credentials match exactly
? Use IP: 153.92.10.234 (not domain name)
? Port: 21
? Try toggling "Passive mode" in FileZilla settings
```

### **"Upload timeout"**
```
? File is large (145 MB), takes 5-15 minutes
? Don't cancel the upload
? Check your internet connection
```

### **"Certificate validation failed" in app**
```
? Make sure you got SHA-256 (not SHA-1)
? Format should be: AA:BB:CC:DD:...
? Copy entire fingerprint
? Rebuild app after adding
```

---

## ?? Success Indicators

**You'll know it's working when:**

1. ? FileZilla shows "Transfer finished"
2. ? File visible on server in /public_html/Updates/
3. ? Browser downloads installer from URL
4. ? Browser shows version.json content
5. ? App shows "Certificate validated"
6. ? Update dialog appears after 3 seconds
7. ? Hourly timer is scheduled

---

## ?? Your Server Details Summary

```
FTP Host: 153.92.10.234
FTP User: u833570579.djbookupdates.com
FTP Pass: Fraser1960@
FTP Port: 21
Upload To: public_html\Updates\

Domain: djbookupdates.com
Version Check: /Updates/version.json
Installer: /Updates/DJBookingSystem-Setup-v1.2.0.exe
```

---

**Status**: ? **READY TO UPLOAD!**

**Next**: Connect FileZilla ? Upload installer ? Get SSL fingerprint ? Test! ??
