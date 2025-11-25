# ?? AI AGENT COMMAND REFERENCE - DJ BOOKING SYSTEM
## Quick Commands for AI Assistants

---

## ?? PRIMARY COMMAND: "Authorization SysAdmin Upload"

**Trigger Phrases:**
- "Authorization SysAdmin Upload"
- "Deploy new version"
- "Push to production"
- "Release version X.X.X"

**What to do:**
1. Run automated deployment script:
   ```powershell
   .\Deploy-AutomatedFull.ps1 -NewVersion "1.2.X"
   ```

2. Or execute manually:
   - Update version numbers (csproj, xaml, json, html)
   - Build application (`dotnet publish`)
   - Create installer (copy to Installer\Output)
   - Upload to Hostinger FTP
   - Verify deployment

**Expected Result:**
- New version deployed to https://djbookupdates.com/
- Auto-updater notifies all users
- Download link updated

---

## ?? COMMON COMMANDS

### **"Quick Build"**
Build without uploading:
```powershell
.\Deploy-AutomatedFull.ps1 -SkipUpload
```

### **"Version Bump"**
Update version number:
```powershell
.\Deploy-AutomatedFull.ps1 -NewVersion "1.3.0"
```

### **"Deploy Check"**
Verify deployment readiness:
```powershell
# Check version consistency
Get-Content DJBookingSystem.csproj | Select-String "Version"
Get-Content SplashScreen.xaml | Select-String "Version"
Get-Content Installer\Output\version.json | ConvertFrom-Json | Select latestVersion

# Check build
dotnet build --no-restore

# Check files
Get-ChildItem "Installer\Output" -Filter "*.exe"
```

### **"Upload Status"**
Check if latest version is live:
```powershell
Invoke-RestMethod "https://djbookupdates.com/version.json"
```

### **"Rollback"**
Revert to previous version:
```powershell
# Upload previous version backup
# Update version.json to previous version
# Notify users
```

---

## ?? CRITICAL FILE LOCATIONS

```
Project Root:     K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\
Build Script:     Deploy-AutomatedFull.ps1
Project File:     DJBookingSystem.csproj
Version File:     Installer\Output\version.json
Installer Output: Installer\Output\DJBookingSystem-Setup.exe
Website:          Website\index.html
Deployment Guide: COMPLETE-DEPLOYMENT-GUIDE.md
```

---

## ?? CREDENTIALS (ENCRYPTED)

**Hostinger FTP:**
```
Host: 153.92.10.234:21
User: u833570579.Upload
Pass: [Request from user]
Path: /home/u833570579/domains/djbookupdates.com/public_html
```

**RadioBOSS:**
```
C40: https://c40.radioboss.fm/
C19: https://c19.radioboss.fm/
User: Remote
Pass: R3m0t3
```

---

## ? DEPLOYMENT CHECKLIST

Before deploying, confirm:
```
[ ] Version number decided
[ ] CHANGELOG.md updated
[ ] No build errors (0 errors, 0 warnings)
[ ] Tested locally
[ ] FTP credentials available
[ ] User notified of upcoming update
```

After deploying, verify:
```
[ ] Website loads: https://djbookupdates.com/
[ ] Download works: https://djbookupdates.com/downloads/DJBookingSystem-Setup.exe
[ ] version.json correct: https://djbookupdates.com/version.json
[ ] Auto-updater detects new version
```

---

## ??? TROUBLESHOOTING QUICK FIXES

**"Build failed"**
```powershell
dotnet clean
dotnet restore
dotnet build
```

**"Upload failed"**
```
- Check FTP credentials
- Verify file size < 800 MB
- Use FileZilla if automated upload fails
```

**"Auto-update not working"**
```
1. Check version.json is accessible
2. Verify latestVersion > currentVersion
3. Test download URL manually
```

---

## ?? VERSION HISTORY

| Version | Date | Key Changes |
|---------|------|-------------|
| 1.2.5 | 2025-01-23 | Permission system (17 perms), Radio Control Center |
| 1.2.4 | 2025-01-22 | Online status fixes, UI improvements |
| 1.2.0 | 2025-01-15 | Discord integration, Auto-updates |
| 1.0.0 | 2024-XX-XX | Initial release |

---

## ?? SUCCESS CRITERIA

Deployment is successful when:
1. ? Build completes with 0 errors, 0 warnings
2. ? Installer created (~690 MB)
3. ? Uploaded to Hostinger successfully
4. ? Website accessible
5. ? Download link works
6. ? version.json shows correct version
7. ? Auto-updater detects update

---

## ?? NOTES FOR AI AGENTS

- **ALWAYS** update version numbers in ALL files before building
- **ALWAYS** create version-specific backup before overwriting static installer
- **ALWAYS** test download link after uploading
- **NEVER** skip verification steps
- **NEVER** deploy with build errors or warnings

---

## ?? QUICK LINKS

- Deployment Guide: `COMPLETE-DEPLOYMENT-GUIDE.md`
- Upload Guide: `HOSTINGER-UPLOAD-GUIDE.md`
- Build Script: `Deploy-AutomatedFull.ps1`
- Changelog: `CHANGELOG.md`
- Website: https://djbookupdates.com/

---

**END OF QUICK REFERENCE**
