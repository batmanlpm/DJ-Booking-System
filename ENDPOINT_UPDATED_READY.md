# ? ENDPOINT UPDATED - Ready to Test!

## ?? What Was Changed

### **SecureUpdateClient.cs Updated:**

**Before:**
```csharp
private const string UPDATE_CHECK_ENDPOINT = "/api/updates/check";
private const string UPDATE_DOWNLOAD_ENDPOINT = "/updates/";
```

**After:**
```csharp
private const string UPDATE_CHECK_ENDPOINT = "/Updates/version.json";
private const string UPDATE_DOWNLOAD_ENDPOINT = "/Updates/";
```

? **Build Status**: Successful!

---

## ?? Your Hostinger Setup

**Confirmed Structure:**
```
public_html/
??? Updates/
    ??? version.json  ? Already created
```

**URLs:**
```
Version Check: https://djbookupdates.com/Updates/version.json
Installer: https://djbookupdates.com/Updates/DJBookingSystem-Setup-v1.2.0.exe
```

---

## ?? Next Steps

### **Step 1: Upload Installer to Hostinger**

**Upload to**: `public_html/Updates/`

**File to upload**: `DJBookingSystem-Setup-v1.2.0.exe` (~145 MB)

**Via FileZilla:**
```
1. Connect to: ftp.djbookupdates.com
2. Navigate to: /public_html/Updates/
3. Drag & drop: Installer\Output\DJBookingSystem-Setup-v1.2.0.exe
4. Wait: 5-15 minutes
```

---

### **Step 2: Update version.json**

**Edit the file** on Hostinger: `public_html/Updates/version.json`

**Update these fields:**
```json
{
  "updateAvailable": true,
  "latestVersion": "1.2.0",
  "releaseDate": "2025-01-15T10:00:00Z",
  "downloadUrl": "https://djbookupdates.com/Updates/DJBookingSystem-Setup-v1.2.0.exe",
  "releaseNotes": "Major update with Discord integration and hourly auto-updates",
  "features": [
    "Discord WebView2 integration with auto-login",
    "Hourly automatic forced updates",
    "Improved UI performance"
  ],
  "bugFixes": [
    "Fixed WebView2 initialization error",
    "Fixed online status tracking"
  ],
  "isCritical": true,
  "minimumVersion": "1.0.0"
}
```

**Make sure `downloadUrl` points to the correct location!**

---

### **Step 3: Get SSL Certificate Fingerprint**

**In your browser:**
```
1. Visit: https://djbookupdates.com
2. Click: ?? Padlock icon
3. Click: Certificate
4. Details tab
5. Find: SHA-256 Fingerprint
6. Copy: AA:BB:CC:DD:EE:...
```

**Or use PowerShell:**
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

---

### **Step 4: Add SSL Fingerprint to Code**

**Open**: `Services\SecureUpdateClient.cs`

**Find line ~30-36** and replace with your actual fingerprint:

```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99"
    // ? Your actual SHA-256 fingerprint from djbookupdates.com
};
```

**Save and rebuild:**
```powershell
dotnet build
```

---

### **Step 5: Test Everything**

**Test 1: URLs in Browser**
```
? https://djbookupdates.com/Updates/version.json
   ? Should show JSON

? https://djbookupdates.com/Updates/DJBookingSystem-Setup-v1.2.0.exe
   ? Should download installer
```

**Test 2: Run Your App**
```
Run app ? Wait 3 seconds ? Update dialog should appear!
```

**Expected in Output window:**
```
Checking for updates on startup (secure connection)...
Connecting to: https://djbookupdates.com
Certificate validated ?
Update available: 1.2.0 ?
Showing notification dialog ?
```

**Test 3: Check Hourly Timer**
```
Output window shows:
"Next update check scheduled for: [NEXT HOUR]:00:00"
"Automatic update checks enabled ?"
```

---

## ? Current Status

| Item | Status |
|------|--------|
| Endpoint updated | ? `/Updates/version.json` |
| Code compiled | ? Build successful |
| Hostinger folder | ? `Updates/` exists |
| version.json | ? Created |
| Installer built | ? Ready to upload |

---

## ?? Remaining Tasks

- [ ] Upload installer to Hostinger (`public_html/Updates/`)
- [ ] Update version.json with correct download URL
- [ ] Get SSL fingerprint from djbookupdates.com
- [ ] Add fingerprint to `TRUSTED_FINGERPRINTS` array
- [ ] Rebuild app with fingerprint
- [ ] Test URLs in browser
- [ ] Test app update check

---

## ?? Quick Commands

```powershell
# Build installer (if needed)
.\QUICK-BUILD.bat

# Build app
dotnet build

# Test URLs
Start-Process "https://djbookupdates.com/Updates/version.json"
Start-Process "https://djbookupdates.com/Updates/DJBookingSystem-Setup-v1.2.0.exe"
```

---

## ?? Your URLs

**Server**: `djbookupdates.com`  
**Folder**: `public_html/Updates/`  
**Version Check**: `/Updates/version.json`  
**Download**: `/Updates/DJBookingSystem-Setup-v1.2.0.exe`

---

**Status**: ? **Code Updated & Built Successfully!**

**Next**: Upload installer ? Add SSL fingerprint ? Test! ??
