# ?? DEPLOYMENT SYSTEM - README

## What is this?

This folder contains everything needed to deploy new versions of the DJ Booking System to production.

---

## ?? DOCUMENTS IN THIS PACKAGE

### **1. COMPLETE-DEPLOYMENT-GUIDE.md** (MAIN GUIDE)
The master reference document. Contains:
- ? Complete system architecture
- ? All credentials and access information
- ? Step-by-step deployment procedures
- ? File locations and paths
- ? Troubleshooting guide
- ? AI Agent command reference

**Use when:** You need complete information about the system.

---

### **2. Deploy-AutomatedFull.ps1** (AUTOMATION SCRIPT)
PowerShell script that automates the entire deployment process.

**Usage:**
```powershell
# Full deployment
.\Deploy-AutomatedFull.ps1 -NewVersion "1.2.6"

# Build only (no upload)
.\Deploy-AutomatedFull.ps1 -SkipUpload

# Force (skip confirmations)
.\Deploy-AutomatedFull.ps1 -Force
```

**What it does:**
1. ? Updates all version numbers
2. ? Builds application
3. ? Creates installer
4. ? Verifies build
5. ? Guides upload process
6. ? Tests deployment
7. ? Generates report

---

### **3. AI-AGENT-QUICK-REFERENCE.md** (FOR AI ASSISTANTS)
Quick command reference for AI agents (like GitHub Copilot, ChatGPT, etc.)

**Key Commands:**
- **"Authorization SysAdmin Upload"** - Full deployment
- **"Quick Build"** - Build without upload
- **"Version Bump"** - Update version numbers
- **"Deploy Check"** - Verify readiness

**Use when:** Training a new AI assistant or need quick command syntax.

---

### **4. HOSTINGER-UPLOAD-GUIDE.md** (UPLOAD REFERENCE)
Detailed FTP upload instructions.

**Contains:**
- FTP credentials
- Upload step-by-step
- File structure on server
- Verification steps

**Use when:** Manually uploading files to Hostinger.

---

### **5. CHANGELOG.md** (VERSION HISTORY)
Complete changelog of all versions.

**Update this:** Every time you release a new version.

---

## ?? QUICK START FOR NEW USERS

### First Time Setup:
1. Read `COMPLETE-DEPLOYMENT-GUIDE.md` (sections 1-4)
2. Verify you have FTP credentials
3. Install FileZilla or FTP client
4. Test FTP connection

### To Deploy New Version:
1. Update `CHANGELOG.md` with new version notes
2. Run `.\Deploy-AutomatedFull.ps1 -NewVersion "X.X.X"`
3. Follow prompts
4. Upload files via FTP when prompted
5. Verify deployment

---

## ?? FOR AI ASSISTANTS

If you are an AI assistant being handed this project:

### Your Mission:
Help deploy new versions of the DJ Booking System to production.

### Primary Command:
When user says **"Authorization SysAdmin Upload"**, execute:
```powershell
.\Deploy-AutomatedFull.ps1 -NewVersion "X.X.X"
```

### Resources Available:
1. **COMPLETE-DEPLOYMENT-GUIDE.md** - Full system documentation
2. **AI-AGENT-QUICK-REFERENCE.md** - Your quick command guide
3. **Deploy-AutomatedFull.ps1** - Automation script

### Key Facts:
- Project: `.NET 8 WPF Application`
- Location: `K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\`
- Hosting: `Hostinger (djbookupdates.com)`
- Database: `Azure Cosmos DB`
- Auto-Update: `version.json on website`

---

## ?? DEPLOYMENT WORKFLOW

```
User says: "Authorization SysAdmin Upload"
    ?
Check prerequisites (location, .NET SDK, no errors)
    ?
Update version numbers (csproj, xaml, json, html)
    ?
Build application (dotnet publish)
    ?
Create installer (copy to Installer\Output)
    ?
Verify (0 errors, 0 warnings, correct size)
    ?
Upload to Hostinger (FTP or FileZilla)
    ?
Verify deployment (test URLs)
    ?
Generate report
    ?
COMPLETE ?
```

---

## ?? SECURITY NOTES

### These files contain SENSITIVE information:
- ? FTP credentials
- ? Database connection strings
- ? RadioBOSS passwords
- ? Server paths

### KEEP SECURE:
- ? Do NOT commit to public GitHub
- ? Do NOT share publicly
- ? Do NOT email unencrypted
- ? Keep in private repository
- ? Share only with authorized personnel

---

## ?? SUPPORT

**Project Owner:** The Fallen Collective  
**Technical Support:** Mega Byte I.T Services  

For issues:
1. Check troubleshooting section in `COMPLETE-DEPLOYMENT-GUIDE.md`
2. Review recent deployments
3. Test locally before deploying
4. Contact project owner if needed

---

## ?? VERSION

**Deployment System Version:** 1.0  
**Last Updated:** 2025-01-23  
**Compatible with:** DJ Booking System 1.2.5+  

---

## ? CHECKLIST FOR HANDOVER

When handing this over to someone new, ensure they have:

- [ ] Read `COMPLETE-DEPLOYMENT-GUIDE.md`
- [ ] FTP credentials (username + password)
- [ ] FileZilla or FTP client installed
- [ ] Tested FTP connection successfully
- [ ] .NET 8 SDK installed
- [ ] PowerShell execution policy set (for scripts)
- [ ] Access to project folder
- [ ] Understand the "Authorization SysAdmin Upload" command

---

## ?? SUCCESS METRICS

Deployment is successful when:
- ? Website loads: https://djbookupdates.com/
- ? Download works: https://djbookupdates.com/downloads/DJBookingSystem-Setup.exe
- ? version.json correct: https://djbookupdates.com/version.json
- ? Auto-updater detects new version
- ? Users can download and install

---

**END OF README**

*For detailed information, see COMPLETE-DEPLOYMENT-GUIDE.md*
