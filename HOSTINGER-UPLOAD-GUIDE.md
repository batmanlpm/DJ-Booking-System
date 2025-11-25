# HOSTINGER UPLOAD INSTRUCTIONS - DJ Booking System Static Site

## ?? Files to Upload

### **FTP Connection Details:**
- **Host:** `ftp://153.92.10.234`
- **Port:** `21`
- **Username:** `u833570579.Upload`
- **Password:** [Use the password from Hostinger panel]
- **Upload Directory:** `/home/u833570579/domains/djbookupdates.com/public_html`

---

## ?? Step-by-Step Upload Process

### **Step 1: Prepare Files**

Copy these files to a temporary folder for upload:

1. **`DJBookingSystem-Setup.exe`** ? **STATIC FILENAME (Always Latest)**
   - Source: `Installer\Output\DJBookingSystem-Setup.exe`
   - **This file is ALWAYS overwritten with each update**
   - Auto-updater always downloads this same filename

2. **`index.html`** (Main landing page)
   - Source: `Website\index.html`
   - Contains download link + registration form

3. **`version.json`** (Auto-updater configuration)
   - Source: `Installer\Output\version.json`
   - Tells app what version is available

4. **`changelog.html`** (Version history)
   - Source: `CHANGELOG.md`
   - Convert to HTML or upload as .md

5. **Backup Files (Optional - Keep Locally):**
   - `DJBookingSystem-Setup-v1.2.5.exe` - Version-specific backup
   - `DJBookingSystem-Setup-v1.2.4.exe` - Previous version

---

### **Step 2: Connect via FTP**

#### **Using FileZilla (Recommended):**

1. Open FileZilla
2. **Quick Connect:**
   - Host: `153.92.10.234`
   - Username: `u833570579.Upload`
   - Password: [Your password]
   - Port: `21`
3. Click **Quickconnect**

#### **Using SmartFTP:**
1. Create new connection
2. Enter host, username, password, port
3. Connect

---

### **Step 3: Upload Files**

1. **Navigate to Remote Directory:**
   ```
   /home/u833570579/domains/djbookupdates.com/public_html
   ```

2. **Upload Main Page:**
   - Drag `index.html` to `public_html/`

3. **Upload Version File:**
   - Drag `version.json` to `public_html/`

4. **Upload Installer (STATIC FILENAME):**
   - Create `downloads` folder if it doesn't exist
   - Upload `DJBookingSystem-Setup.exe` to `public_html/downloads/`
   - ?? **This OVERWRITES the previous version automatically!**

---

### **Step 4: Verify Upload**

Visit these URLs to confirm:

1. **Main Page:**
   ```
   https://djbookupdates.com/
   ```

2. **Version File:**
   ```
   https://djbookupdates.com/version.json
   ```

3. **Installer Download (STATIC URL):**
   ```
   https://djbookupdates.com/downloads/DJBookingSystem-Setup.exe
   ```
   ? **This URL NEVER changes - always points to latest version!**

---

## ?? File Structure on Server

```
/public_html/
??? index.html                          (landing + registration page)
??? version.json                        (auto-updater config)
??? changelog.html                      (version history)
??? downloads/
    ??? DJBookingSystem-Setup.exe       ? STATIC - Always Latest Version
```

---

## ? Quick Checklist

- [ ] Connect to FTP successfully
- [ ] Navigate to `/public_html` directory
- [ ] Upload `index.html`
- [ ] Upload `version.json`
- [ ] Create `downloads` folder (if needed)
- [ ] Upload `DJBookingSystem-Setup.exe` to `downloads/`
- [ ] Test URL: https://djbookupdates.com/
- [ ] Test version.json URL
- [ ] Test installer download link (should start download)

---

## ?? Update Process (Future Updates)

### **For Version 1.2.6, 1.2.7, etc.:**

1. **Build new installer** using `Build-Installer-v1.2.x.ps1`
2. **Update version.json:**
   - Change `latestVersion` to new version
   - Update `releaseDate`
   - Update `releaseNotes`
3. **Upload to Hostinger:**
   - Upload new `DJBookingSystem-Setup.exe` (OVERWRITES old one)
   - Upload new `version.json`
   - Upload updated `index.html` (if changed)
4. **Done!** All users get auto-update notification

### **Why This Works Better:**
? **Same download URL every time** - No need to update links
? **Auto-updater works seamlessly** - Always downloads from same URL
? **Easier to maintain** - Just overwrite the same file
? **Version-specific backups** - Kept locally for rollback if needed

---

## ??? Troubleshooting

### **Can't Connect to FTP:**
- Check firewall settings
- Verify credentials from Hostinger panel
- Try port 21 or check if passive mode is needed

### **404 Error on Website:**
- Ensure `index.html` is in root of `public_html`
- Check file permissions (should be 644)
- Verify domain is pointed to correct directory

### **Version.json Not Found:**
- Ensure it's in root directory (`/public_html/version.json`)
- Check file permissions (should be 644)
- Clear browser cache and retry

### **Download Link Not Working:**
- Verify file is in `/public_html/downloads/DJBookingSystem-Setup.exe`
- Check file permissions (should be 644)
- Ensure `downloads` folder exists

---

## ?? Notes

- **Current Version:** 1.2.5
- **Static Download URL:** `https://djbookupdates.com/downloads/DJBookingSystem-Setup.exe`
- **Always overwrite this file** - It's designed to be replaced
- **Keep version-specific backups locally** for rollback capability
- **Last Updated:** 2025-01-23
- **Maintained By:** The Fallen Collective & Mega Byte I.T Services

---

## ?? Key Advantage: Static URLs

**Old Way (Version-Specific):**
```
downloads/DJBookingSystem-Setup-v1.2.5.exe  ? Changes every update
```

**New Way (Static):**
```
downloads/DJBookingSystem-Setup.exe  ? Never changes
```

**Benefits:**
- Auto-updater URL never needs changing
- Website download link stays the same
- Much easier to manage
- Professional deployment strategy

---

**Ready to upload! ??**
