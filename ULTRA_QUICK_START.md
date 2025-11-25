# ?? ULTRA QUICK START - Just Do Everything!

## One Command Setup

**Just double-click**: `RUN-COMPLETE-SETUP.bat`

That's it! The script will:

1. ? Check all prerequisites
2. ? Install Inno Setup if needed
3. ? Build your application
4. ? Create fancy installer
5. ? Generate version.json
6. ? Update all code with your domain
7. ? Create upload instructions

**Time**: 15-20 minutes (mostly automated)

---

## What You Need

1. **Your Hostinger domain** (script will ask)
2. **That's it!**

---

## What The Script Does

### Automated (No Input Needed):
- ? Installs Inno Setup
- ? Downloads WebView2 Runtime
- ? Builds .NET application
- ? Creates installer executable
- ? Generates version.json
- ? Updates client code

### Manual (You Do After):
1. Upload installer to Hostinger (via FTP)
2. Upload version.json to Hostinger
3. Enable SSL certificate
4. Get SSL fingerprint
5. Add fingerprint to code

**Instructions file created**: `HOSTINGER_UPLOAD_INSTRUCTIONS.txt`

---

## After Setup

### To Deploy Updates Later:

```powershell
# Build new version
.\Build-Installer.ps1

# Upload to Hostinger (FTP)
# Update version.json

# Done! All users get forced update within 1 hour
```

---

## File Locations

**After running setup:**

```
Your Project/
??? RUN-COMPLETE-SETUP.bat          ? Run this!
??? COMPLETE-SETUP.ps1              ? Main script
??? HOSTINGER_UPLOAD_INSTRUCTIONS.txt  ? Follow this after
??? Installer/
?   ??? Output/
?       ??? DJBookingSystem-Setup-v1.2.0.exe  ? Upload this
?       ??? version.json                       ? Upload this
```

---

## Expected Timeline

| Step | Time | Automated? |
|------|------|------------|
| Run script | 15-20 min | ? Yes |
| Enable SSL on Hostinger | 15 min | ?? Wait time |
| Upload files | 10-15 min | ? Manual |
| Get SSL fingerprint | 2 min | ? Manual |
| Add fingerprint to code | 1 min | ? Manual |
| **TOTAL** | **~45-60 min** | **Mostly automated** |

---

## Troubleshooting

### "Execution Policy Error"
**Fix**: Right-click `RUN-COMPLETE-SETUP.bat` ? Run as Administrator

### "Inno Setup failed to install"
**Fix**: Install manually from https://jrsoftware.org/isdl.php then re-run

### "Build failed"
**Fix**: Make sure Visual Studio is closed, then re-run

### "Can't connect to Hostinger"
**Fix**: Follow HOSTINGER_UPLOAD_INSTRUCTIONS.txt step by step

---

## What Happens After Everything is Set Up

```
[Every hour on the hour]
   ?
All running apps check your server
   ?
Update found?
   ?
YES ? Forced download starts automatically
      No cancel option
      Installation completes
      App restarts
      Done!
   ?
NO ? Check again next hour
```

---

## Quick Commands

```powershell
# Run complete setup
.\RUN-COMPLETE-SETUP.bat

# Build installer only (after first setup)
.\Build-Installer.ps1

# Build application only
dotnet build

# Test update check
dotnet run
# Wait 3 seconds, dialog should appear
```

---

## Success Indicators

You'll know it worked when:

1. ? Script completes without errors
2. ? Installer file exists: `Installer\Output\DJBookingSystem-Setup-v1.2.0.exe`
3. ? `HOSTINGER_UPLOAD_INSTRUCTIONS.txt` opens automatically
4. ? After Hostinger upload: `https://yourdomain.com/updates/version.json` works
5. ? After running app: Update dialog appears after 3 seconds

---

## Need Help?

**Error during setup?**
- Take screenshot
- Note what step failed
- Check error message

**Most common issues:**
- Internet connection (downloads fail)
- Antivirus blocking (temporarily disable)
- Disk space (need ~500MB free)

---

## Ready?

**Just run**: `RUN-COMPLETE-SETUP.bat`

**That's it!** ??

The script will guide you through everything else.

---

**Total Automation**: ~80%  
**Manual Steps**: ~20% (Hostinger upload)  
**Difficulty**: Easy (step-by-step)  
**Time**: ~1 hour total

**Let's go! ??**
