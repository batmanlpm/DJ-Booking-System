# ?? DEPLOY TO HOSTINGER - Version 1.3.0

**Date:** January 23, 2025  
**Version:** 1.3.0 - Friends List & DM System  
**Website:** https://fallencollective.com

---

## ?? FILES TO UPLOAD

### **1. Website Files** (Upload to public_html)

**Location:** `K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Website\`

Upload these files to your Hostinger File Manager at `public_html/`:

- `index.html` ? (Updated with v1.3.0 info)
- `changelog.html` (if you have one)
- Any CSS/JS files in the Website folder

---

### **2. Downloads Folder** (Upload to public_html/downloads/)

**Location:** `K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Installer\Output\`

Upload to `public_html/downloads/`:

- `version.json` ? (Updated with v1.3.0 info)
- `DJBookingSystem-Setup-v1.3.0.exe` (Your compiled installer - you'll need to build this)

---

## ?? HOSTINGER UPLOAD STEPS

### **Method 1: File Manager (Recommended for Small Files)**

1. **Login to Hostinger**
   - Go to: https://www.hostinger.com
   - Login with your credentials
   - Select your hosting plan

2. **Open File Manager**
   - Click "File Manager" in the hosting panel
   - Navigate to `public_html/`

3. **Upload index.html**
   - Click "Upload" button
   - Select `Website/index.html` from your computer
   - Click "Upload" and wait for completion
   - **Overwrite** if file already exists

4. **Upload to downloads folder**
   - Navigate to `public_html/downloads/`
   - If folder doesn't exist, create it:
     - Click "New Folder"
     - Name it "downloads"
   - Upload these files:
     - `version.json`
     - `DJBookingSystem-Setup-v1.3.0.exe` (when ready)

---

### **Method 2: FTP (For Large Files like .exe)**

1. **Get FTP Credentials**
   - In Hostinger panel, go to "Files" ? "FTP Accounts"
   - Note down:
     - Hostname: usually `ftp.fallencollective.com`
     - Username: your FTP username
     - Password: your FTP password
     - Port: 21

2. **Use FileZilla (or any FTP client)**
   - Download FileZilla: https://filezilla-project.org/
   - Connect using credentials above
   - Navigate to `/public_html/` on remote side
   - Drag and drop files from local to remote

3. **Upload Files**
   - Upload `index.html` to `/public_html/`
   - Upload `version.json` to `/public_html/downloads/`
   - Upload `.exe` to `/public_html/downloads/`

---

## ?? UPDATED CONTENT

### **index.html Changes:**
? Version badge changed to "1.3.0"
? Added "NEW in Version 1.3.0" section with Friends List features
? Updated download link to: `https://fallencollective.com/downloads/DJBookingSystem-Setup-v1.3.0.exe`
? Updated changelog link to: `https://fallencollective.com/changelog.html`

### **version.json Changes:**
? Updated to version 1.3.0
? Added Friends List & DM features list
? Updated download URL to Hostinger domain
? Updated release date to 2025-01-23

---

## ? VERIFICATION CHECKLIST

After uploading, verify:

1. **Website Loads**
   - Visit: https://fallencollective.com
   - Check version badge shows "1.3.0"
   - Check features list shows Friends List

2. **Download Links Work**
   - Click download button
   - Verify it points to correct URL
   - Test download (if .exe is uploaded)

3. **Version.json Accessible**
   - Visit: https://fallencollective.com/downloads/version.json
   - Should display JSON with version 1.3.0

4. **Auto-Updater Will Work**
   - Once users run the app, it will check version.json
   - If they have v1.2.6 or lower, they'll get update notification
   - They can download v1.3.0

---

## ?? BUILD THE INSTALLER FIRST

**Before uploading the .exe**, you need to build it:

1. **Build Release Version**
   ```
   - In Visual Studio: Build ? Configuration Manager
   - Set to "Release"
   - Build ? Build Solution
   ```

2. **Create Installer**
   - Use your installer tool (Inno Setup, WiX, etc.)
   - Output file should be named: `DJBookingSystem-Setup-v1.3.0.exe`
   - Save to: `Installer/Output/`

3. **Then Upload to Hostinger**

---

## ?? USER COMMUNICATION

### **Announcement Template:**

```
?? VERSION 1.3.0 IS LIVE!

New Features:
• Discord-style friends list
• Send & receive friend requests
• Private direct messaging (DM)
• Real-time online/offline status (green/gray dots)

Download Now:
https://fallencollective.com

Or wait for the auto-updater notification in the app!

Enjoy! ??
```

---

## ?? QUICK UPLOAD COMMANDS

**If you prefer command line FTP:**

```bash
# Upload index.html
curl -T "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Website\index.html" ftp://fallencollective.com/public_html/ --user username:password

# Upload version.json
curl -T "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Installer\Output\version.json" ftp://fallencollective.com/public_html/downloads/ --user username:password
```

Replace `username` and `password` with your FTP credentials.

---

## ?? POST-DEPLOYMENT

After upload:
1. Clear browser cache and reload website
2. Test download link
3. Check version.json URL directly
4. Monitor for any 404 errors
5. Announce to users

---

**Files Ready to Upload:**
? `Website/index.html` - Updated to v1.3.0
? `Installer/Output/version.json` - Updated to v1.3.0
? `DJBookingSystem-Setup-v1.3.0.exe` - Build first, then upload

**Upload Destination:**
- Website: `public_html/index.html`
- Version info: `public_html/downloads/version.json`
- Installer: `public_html/downloads/DJBookingSystem-Setup-v1.3.0.exe`

---

**Ready to deploy!** ??
