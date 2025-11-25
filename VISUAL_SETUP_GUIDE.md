# ?? VISUAL SETUP GUIDE - Complete Automation

## ?? What You Have Now

```
Your Project Folder/
?
??? ?? RUN-COMPLETE-SETUP.bat          ? START HERE!
??? ?? COMPLETE-SETUP.ps1              (automated script)
??? ?? ULTRA_QUICK_START.md            (quick reference)
??? ?? installer.iss                   (installer config)
```

---

## ?? STEP 1: Run Setup (15-20 min - AUTOMATED)

### Action:
```
Double-click: RUN-COMPLETE-SETUP.bat
```

### What Happens:
```
????????????????????????????????????????????????
?  DJ BOOKING SYSTEM - COMPLETE AUTOMATED SETUP ?
????????????????????????????????????????????????

Enter your Hostinger domain: fallencollective.com
                            ? Type your domain here

Press any key to start...
```

### Script Will:
```
[1/10] ? Checking .NET SDK...
[2/10] ? Installing/Checking Inno Setup...
[3/10] ? Creating directories...
[4/10] ? Downloading WebView2 Runtime...
[5/10] ? Building application...
[6/10] ? Updating installer script...
[7/10] ? Building installer...
[8/10] ? Updating client code...
[9/10] ? Creating version.json...
[10/10] ? Creating upload instructions...

DONE! ?
```

### Output Files Created:
```
Installer/
??? Output/
    ??? DJBookingSystem-Setup-v1.2.0.exe  (~125 MB) ? Upload this
    ??? version.json                                ? Upload this

HOSTINGER_UPLOAD_INSTRUCTIONS.txt  ? Read this next
```

---

## ?? STEP 2: Upload to Hostinger (15-20 min - MANUAL)

### 2.1: Enable SSL Certificate

```
Browser ? https://hpanel.hostinger.com
   ?
Security ? SSL
   ?
Install SSL ? Choose "Free Let's Encrypt"
   ?
? Wait 10-15 minutes
   ?
? Test: https://yourdomain.com (green padlock appears)
```

### 2.2: Create Folder Structure

```
Hostinger ? Files ? File Manager
   ?
public_html/
   ?
Create folder: "updates"
   ?
Enter "updates" folder
   ?
Create folder: "installers"
```

**Result:**
```
public_html/
??? updates/          ? You are here
    ??? installers/   ? Created
```

### 2.3: Upload version.json

```
Go to: public_html/updates/
   ?
Click: Upload
   ?
Select: Installer\Output\version.json
   ?
? Uploaded!
```

**Test**: `https://yourdomain.com/updates/version.json`  
Should show JSON content ?

### 2.4: Upload Installer (LARGE FILE)

**Option A: FileZilla (Recommended)**
```
1. Download FileZilla: https://filezilla-project.org/
2. Hostinger ? Files ? FTP Accounts ? Get credentials
3. Connect:
   Host: ftp.yourdomain.com
   Username: [from Hostinger]
   Password: [from Hostinger]
   Port: 21
4. Local (left): Navigate to Installer\Output\
5. Remote (right): Navigate to /public_html/updates/installers/
6. Drag & drop: DJBookingSystem-Setup-v1.2.0.exe
7. ? Wait 5-15 minutes
8. ? Done!
```

**Option B: Hostinger File Manager**
```
Go to: public_html/updates/installers/
   ?
Click: Upload
   ?
Select: DJBookingSystem-Setup-v1.2.0.exe
   ?
? Wait (may be slow for large files)
   ?
? Uploaded!
```

**Test**: `https://yourdomain.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe`  
Should download the file ?

---

## ?? STEP 3: Get SSL Fingerprint (2 min - MANUAL)

### Browser Method:
```
Browser ? https://yourdomain.com
   ?
Click: ?? Padlock icon
   ?
Click: "Connection is secure"
   ?
Click: "Certificate is valid"
   ?
Tab: Details
   ?
Find: "SHA-256 Fingerprint"
   ?
Copy: AA:BB:CC:DD:EE:FF:00:11:22:33:...
```

### PowerShell Method:
```powershell
$url = "yourdomain.com"
$tcpClient = New-Object System.Net.Sockets.TcpClient($url, 443)
$sslStream = New-Object System.Net.Security.SslStream($tcpClient.GetStream())
$sslStream.AuthenticateAsClient($url)
$cert = $sslStream.RemoteCertificate
$hash = $cert.GetCertHashString("SHA256")
$formatted = ($hash -replace '(.{2})', '$1:').TrimEnd(':')
Write-Host "SHA-256: $formatted"
```

---

## ?? STEP 4: Add Fingerprint to Code (1 min - MANUAL)

### Action:
```
Open: Services\SecureUpdateClient.cs
   ?
Find: Line ~30-36
   ?
Replace:
```

**Before:**
```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "YOUR_CERTIFICATE_SHA256_FINGERPRINT_HERE",
};
```

**After:**
```csharp
private static readonly string[] TRUSTED_FINGERPRINTS = new[]
{
    "AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99:AA:BB:CC:DD:EE:FF:00:11:22:33:44:55:66:77:88:99",
    // ? Your actual fingerprint from step 3
};
```

### Save & Build:
```powershell
dotnet build
# Should succeed ?
```

---

## ?? STEP 5: Test Everything (2 min - MANUAL)

### Test 1: URLs Work
```
Browser ? https://yourdomain.com/updates/version.json
? Shows JSON

Browser ? https://yourdomain.com/updates/installers/DJBookingSystem-Setup-v1.2.0.exe
? Downloads file
```

### Test 2: App Update Check
```
Run: .\bin\Debug\net8.0-windows\DJBookingSystem.exe
   ?
? Wait 3 seconds
   ?
? Update dialog appears!
```

**Debug Output Should Show:**
```
Checking for updates on startup (secure connection)...
Connecting to: https://yourdomain.com
Certificate validated ?
Update available: 1.2.0 ?
Showing notification dialog ?
```

### Test 3: Hourly Timer
```
Check output:
"Next update check scheduled for: 11:00:00"
"Time until next check: 23.5 minutes"
"Automatic update checks enabled (every hour on the hour)"
? Timer working!
```

---

## ? SUCCESS! What Happens Now

### For You (Developer):
```
Deploy new version:
1. .\Build-Installer.ps1
2. Update version.json (change version number)
3. Upload both to Hostinger
4. Done! All users get forced update within 1 hour
```

### For Your Users:
```
Every hour on the hour:
   ?
App checks your server
   ?
Update found?
   ?
YES ? Dialog appears (cannot close)
      Download starts automatically
      Installation completes
      App restarts
      ? Updated to latest version!
```

---

## ?? Complete Timeline

| Phase | Time | Who Does It |
|-------|------|-------------|
| Run RUN-COMPLETE-SETUP.bat | 15-20 min | ?? Automated |
| SSL activation wait time | 15 min | ? Hostinger |
| Upload files | 10-15 min | ?? You (FTP) |
| Get SSL fingerprint | 2 min | ?? You |
| Add fingerprint | 1 min | ?? You |
| Test | 2 min | ?? You |
| **TOTAL** | **~45-60 min** | **80% automated** |

---

## ?? Current Status Checklist

Track your progress:

- [ ] Step 1: Ran RUN-COMPLETE-SETUP.bat successfully
- [ ] Step 2.1: SSL enabled on Hostinger (green padlock works)
- [ ] Step 2.2: Folders created (updates/ and installers/)
- [ ] Step 2.3: version.json uploaded and accessible
- [ ] Step 2.4: Installer uploaded and downloadable
- [ ] Step 3: SSL fingerprint obtained
- [ ] Step 4: Fingerprint added to code & built successfully
- [ ] Step 5: All tests pass (URLs work, dialog appears)

**All checked?** ?? **YOU'RE DONE!**

---

## ?? Quick Troubleshooting

### Script fails at "Building application"
```
Solution:
1. Close Visual Studio
2. Delete bin/ and obj/ folders
3. Re-run script
```

### SSL certificate not activating
```
Solution:
1. Wait longer (can take 20-30 min)
2. Check domain DNS is pointing to Hostinger
3. Contact Hostinger support (24/7 live chat)
```

### Upload times out
```
Solution:
1. Use FileZilla instead of File Manager
2. Check internet connection
3. Try uploading at off-peak hours
```

### Update dialog doesn't appear
```
Solution:
1. Check debug output for error message
2. Verify URLs are accessible in browser
3. Check SSL fingerprint matches exactly
4. Verify UPDATE_SERVER_URL is correct
```

---

## ?? Support

**Stuck?** Check:
1. HOSTINGER_UPLOAD_INSTRUCTIONS.txt (detailed steps)
2. Error message in console
3. Hostinger live chat support (24/7)

---

## ?? Final Result

```
?????????????????????????????????????????????????????????
?                                                       ?
?  Professional Fancy Installer with Graphics ?        ?
?  Hosted on Hostinger with SSL ?                      ?
?  Hourly Automatic Forced Updates ?                   ?
?  Complete Deployment Workflow ?                      ?
?                                                       ?
?  ?? CONGRATULATIONS! SETUP COMPLETE! ??              ?
?                                                       ?
?????????????????????????????????????????????????????????

Your users will now:
• Get beautiful branded installer
• Auto-update every hour
• Never miss critical updates
• Experience zero downtime

You can:
• Deploy updates in minutes
• Force updates when needed
• Track all deployments
• Sleep well knowing users are current

ENJOY! ??
```
