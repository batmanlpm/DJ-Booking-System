# ?? HOSTINGER WEBSITE & AUTO-UPDATE CONFIGURED

## ? What Was Updated:

### 1. **Website Download Link** (`Website/index.html`)
- Download button now points to: 
  ```
  https://fallencollective.livepartymusic.fm/downloads/DJBookingSystem-Setup-v1.2.5.exe
  ```
- Added `download` attribute for force download
- Version: **1.2.5**

### 2. **Automatic Update System** (`Services/CandyBotUpdateService.cs`)
- Update check URL: `https://fallencollective.livepartymusic.fm/version.json`
- Download URL: `https://fallencollective.livepartymusic.fm/downloads`

### 3. **Version Manifest** (`Installer/Output/version.json`)
- Current version: **1.2.5**
- Download URL updated to Hostinger
- Update available: `false` (since this IS the latest)

---

## ?? Files to Upload to Hostinger:

Upload these to your **Hostinger public_html** folder:

### Required Files:
```
public_html/
??? index.html (already there)
??? version.json (UPLOAD THIS!)
??? downloads/
    ??? DJBookingSystem-Setup-v1.2.5.exe (UPLOAD YOUR INSTALLER!)
```

### File Locations in Your Project:
1. **version.json** ? `Installer/Output/version.json`
2. **Installer EXE** ? Build your installer, rename to `DJBookingSystem-Setup-v1.2.5.exe`

---

## ?? How It Works Now:

### **Website Downloads:**
1. User visits: `https://fallencollective.livepartymusic.fm`
2. Clicks **"?? Download Now"**
3. Downloads: `DJBookingSystem-Setup-v1.2.5.exe`

### **Automatic Updates:**
1. App checks: `https://fallencollective.livepartymusic.fm/version.json`
2. Compares current (1.2.5) vs latest (1.2.5)
3. If update available ? downloads from `/downloads/` folder

---

## ?? Next Steps:

1. **Build Installer:**
   - Use your installer builder (Inno Setup, etc.)
   - Output filename: `DJBookingSystem-Setup-v1.2.5.exe`

2. **Upload to Hostinger:**
   ```
   /public_html/version.json
   /public_html/downloads/DJBookingSystem-Setup-v1.2.5.exe
   ```

3. **Test:**
   - Visit website ? Click download ? Should download installer
   - Open app ? Settings ? Check for Updates ? Should say "Up to date"

---

## ?? For Future Updates:

When releasing v1.2.6:

1. Update `version.json`:
   ```json
   "currentVersion": "1.2.5",
   "latestVersion": "1.2.6",
   "updateAvailable": true,
   "downloadUrl": "...v1.2.6.exe"
   ```

2. Upload new installer to `/downloads/`

3. Upload updated `version.json`

4. App will auto-detect and prompt users to update!

---

**All systems now point to your Hostinger server!** ?
