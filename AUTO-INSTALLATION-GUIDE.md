# ?? DJ BOOKING SYSTEM - AUTO-INSTALLATION GUIDE

## ? Features of This Installer

This installer **automatically**:
- ? Installs DJ Booking System
- ? Adds Windows Defender exclusions (no warnings!)
- ? Creates desktop shortcut
- ? Sets up program files folder

**No manual configuration needed!**

---

## ?? INSTALLATION STEPS

### **Step 1: Download Installer**

Download one of these:
- `DJBookingSystem-Installer-v1.2.5.exe` (Self-extracting)
- `DJBookingSystem-Installer-v1.2.5.zip` (Extract first)

---

### **Step 2: Run as Administrator**

**IMPORTANT:** Must run as administrator for auto-exclusions to work!

#### **Option A: Self-Extracting (.exe)**
```
1. Right-click DJBookingSystem-Installer-v1.2.5.exe
2. Select "Run as administrator"
3. Click "Yes" on UAC prompt
4. Installer extracts and runs Install.bat automatically
```

#### **Option B: ZIP Package**
```
1. Extract DJBookingSystem-Installer-v1.2.5.zip
2. Right-click Install.bat
3. Select "Run as administrator"
4. Click "Yes" on UAC prompt
```

---

### **Step 3: Installation Runs Automatically**

You'll see:
```
========================================
DJ Booking System - Installation
========================================

Installing to C:\Program Files\DJ Booking System...
Copying files...
Adding Windows Defender exclusions...
  ? Folder exclusion added
  ? Executable exclusion added
  ? Process exclusion added
Creating desktop shortcut...

========================================
Installation Complete!
========================================

DJ Booking System has been installed to:
  C:\Program Files\DJ Booking System

Desktop shortcut created.
Windows Defender exclusions added.

Press any key to continue...
```

---

### **Step 4: Launch Application**

**From Desktop:**
- Double-click "DJ Booking System" shortcut

**From Start Menu:**
- Search for "DJ Booking System"

**No security warnings!** ?

---

## ? WHAT GETS INSTALLED

### **Files:**
```
C:\Program Files\DJ Booking System\
??? DJBookingSystem.exe (main application)
```

### **Desktop Shortcut:**
```
%USERPROFILE%\Desktop\DJ Booking System.lnk
```

### **Windows Defender Exclusions:**
```
Folder:     C:\Program Files\DJ Booking System\
Executable: C:\Program Files\DJ Booking System\DJBookingSystem.exe
Process:    DJBookingSystem.exe
```

---

## ?? SECURITY

### **Why Administrator Rights?**
- Required to write to `C:\Program Files`
- Required to modify Windows Defender settings
- **This is standard for all Windows installers**

### **Is This Safe?**
- ? Yes! This is the **official installer**
- ? Only modifies Windows Defender (Microsoft's own AV)
- ? Only adds exclusions for DJ Booking System folder
- ? Doesn't disable Windows Defender
- ? Doesn't affect other security features

### **What If I Don't Trust It?**
You can:
1. Skip auto-exclusions (just copy .exe manually)
2. Add exclusions manually later:
   ```
   Settings ? Windows Security ? Virus & threat protection
   ? Manage settings ? Exclusions ? Add exclusion
   ```

---

## ?? TROUBLESHOOTING

### **"This app has been blocked for your protection"**

**Cause:** Installer not run as administrator

**Fix:**
```
1. Right-click installer
2. Select "Run as administrator" (not just double-click)
3. Click "Yes" on UAC prompt
```

---

### **"Access is denied"**

**Cause:** Not running as administrator

**Fix:**
```
1. Make sure you right-click ? "Run as administrator"
2. If still fails, check if antivirus is blocking
```

---

### **Windows Defender exclusion failed**

**You'll see:**
```
? Could not add Windows Defender exclusions
  Error: [some error]
  
This is not critical - you can add exclusions manually later.
```

**This is OK!** Installation continues anyway. You can:
- Add exclusions manually later
- Or use the app with Windows Defender warnings (just click "Run anyway")

---

### **I use different antivirus (not Windows Defender)**

**The auto-exclusion won't work** for:
- Norton
- McAfee
- Avast
- AVG
- Kaspersky
- etc.

**You need to:**
Add exclusion manually in your antivirus settings:
```
Add exception for:
  C:\Program Files\DJ Booking System\DJBookingSystem.exe
```

Consult your antivirus documentation for how to add exclusions.

---

## ?? UNINSTALLATION

### **Manual Uninstall:**
```
1. Delete folder: C:\Program Files\DJ Booking System\
2. Delete desktop shortcut
3. Remove Windows Defender exclusions:
   Settings ? Windows Security ? Virus & threat protection
   ? Manage settings ? Exclusions ? Remove
```

### **Or run PowerShell as Admin:**
```powershell
# Remove application
Remove-Item "C:\Program Files\DJ Booking System" -Recurse -Force

# Remove desktop shortcut
Remove-Item "$env:USERPROFILE\Desktop\DJ Booking System.lnk" -Force

# Remove Windows Defender exclusions
Remove-MpPreference -ExclusionPath "C:\Program Files\DJ Booking System"
Remove-MpPreference -ExclusionPath "C:\Program Files\DJ Booking System\DJBookingSystem.exe"
Remove-MpPreference -ExclusionProcess "DJBookingSystem.exe"
```

---

## ?? NOTES

### **For Closed Community:**
This installer is perfect for **The Fallen Collective** community because:
- ? One-click installation
- ? No manual configuration
- ? No security warnings
- ? Professional appearance

### **For IT Admins:**
You can also deploy this via:
- Group Policy (copy to `NETLOGON`)
- Network share
- USB drives
- Email distribution

Just tell users to right-click ? "Run as administrator"

---

## ? SUMMARY

**What This Installer Does:**
1. ? Installs DJ Booking System to Program Files
2. ? **Automatically** adds Windows Defender exclusions
3. ? Creates desktop shortcut
4. ? No more security warnings

**What User Needs to Do:**
1. Right-click installer
2. "Run as administrator"
3. Click "Yes" on UAC
4. Done!

**Total time:** < 30 seconds ?

---

**Questions?** Contact support in Discord!

---

**Last Updated:** 2025-01-23  
**Version:** 1.2.5  
**Installer Type:** Auto-Exclusion Installer
