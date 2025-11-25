# ? COMPLETE SETUP - All Information Ready!

## ?? Your Server Configuration

### **FTP Access:**
```
Host: 153.92.10.234
Username: u833570579.djbookupdates.com
Password: Fraser1960@
Port: 21
```

### **Server Paths:**
```
Upload Folder: public_html\Updates\
Domain: djbookupdates.com
```

### **URLs:**
```
Version Check: https://djbookupdates.com/Updates/version.json
Installer: https://djbookupdates.com/Updates/DJBookingSystem-Setup-v1.2.0.exe
```

---

## ?? Quick Start - 3 Simple Steps

### **Step 1: Upload Installer (10 minutes)**

**Via FileZilla:**
1. Download FileZilla: https://filezilla-project.org/
2. Open FileZilla
3. **Quick Connect:**
   - Host: `153.92.10.234`
   - Username: `u833570579.djbookupdates.com`
   - Password: `Fraser1960@`
   - Port: `21`
4. Navigate to: `/public_html/Updates/`
5. Drag & drop: `Installer\Output\DJBookingSystem-Setup-v1.2.0.exe`
6. Wait for upload (5-15 minutes)

**OR use the connection file:**
- Double-click: `filezilla-djbookupdates.xml`
- FileZilla will open with settings pre-configured!
- Click Connect!

---

### **Step 2: Get SSL Fingerprint (2 minutes)**

**In Browser:**
```
1. Visit: https://djbookupdates.com
2. Click: ?? Padlock
3. Click: Certificate
4. Details ? SHA-256 Fingerprint
5. Copy: AA:BB:CC:DD:EE:...
```

**OR use PowerShell:**
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

### **Step 3: Add Fingerprint & Build (2 minutes)**

**Edit:** `Services\SecureUpdateClient.cs`

**Line 30-36** - Replace with your fingerprint:
```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "YOUR_ACTUAL_SHA256_FINGERPRINT_HERE"  // ? Paste here!
};
```

**Build:**
```powershell
dotnet build
```

**Done!** ?

---

## ?? Testing (2 minutes)

### **Test 1: URLs**
```
https://djbookupdates.com/Updates/version.json
https://djbookupdates.com/Updates/DJBookingSystem-Setup-v1.2.0.exe
```
Both should work!

### **Test 2: Run App**
```
Run app ? Wait 3 seconds ? Update dialog appears!
```

### **Test 3: Check Output**
```
Certificate validated ?
Update available: 1.2.0 ?
Next update check scheduled ?
```

---

## ?? Files Created for You

| File | Purpose |
|------|---------|
| `COMPLETE_UPLOAD_GUIDE.md` | Detailed step-by-step guide |
| `filezilla-djbookupdates.xml` | FileZilla connection file (double-click to import) |
| `COMPLETE_SETUP_READY.md` | This summary |

---

## ?? Total Time Estimate

| Task | Time |
|------|------|
| Upload installer via FileZilla | 10-15 min |
| Get SSL fingerprint | 2 min |
| Add fingerprint to code & build | 2 min |
| Test everything | 2 min |
| **TOTAL** | **~20 minutes** |

---

## ? Final Checklist

**Preparation:**
- [x] Installer built (`DJBookingSystem-Setup-v1.2.0.exe`)
- [x] FTP credentials ready
- [x] Endpoint updated in code (`/Updates/version.json`)
- [x] Build successful

**Upload:**
- [ ] FileZilla installed
- [ ] Connected to FTP
- [ ] Uploaded installer to `/public_html/Updates/`
- [ ] Verified file on server

**Configuration:**
- [ ] Got SSL fingerprint
- [ ] Added to `TRUSTED_FINGERPRINTS`
- [ ] Rebuilt application
- [ ] Tested URLs in browser

**Testing:**
- [ ] version.json accessible
- [ ] Installer downloads
- [ ] App shows update dialog
- [ ] Certificate validated
- [ ] Hourly timer scheduled

---

## ?? One-Line Commands

```powershell
# Build
dotnet build

# Test URLs
Start-Process "https://djbookupdates.com/Updates/version.json"

# Run app
.\bin\Debug\net8.0-windows\DJBookingSystem.exe
```

---

## ?? Server Structure (Final)

```
djbookupdates.com
??? public_html/
    ??? Updates/
        ??? version.json                          ? Already exists
        ??? DJBookingSystem-Setup-v1.2.0.exe     ? Upload now!
```

---

## ?? What Happens After Setup

**Every hour on the hour:**
1. All running apps check for updates
2. Find version 1.2.0
3. Dialog appears (cannot close)
4. Download starts automatically
5. Installation completes
6. App restarts with new version

**No user interaction needed!**

---

## ?? Quick Reference

```
Server: 153.92.10.234
User: u833570579.djbookupdates.com
Pass: Fraser1960@
Port: 21
Path: public_html\Updates\

Domain: djbookupdates.com
Endpoint: /Updates/version.json
```

---

## ?? READY TO GO!

**Next Action:**
1. Open FileZilla (or double-click `filezilla-djbookupdates.xml`)
2. Upload installer
3. Get SSL fingerprint
4. Add to code
5. Test!

**Estimated total time: 20 minutes**

---

**Status**: ? **EVERYTHING CONFIGURED AND READY!**

**All information provided. All files created. All settings updated.**

**Just upload, add fingerprint, and test!** ??
