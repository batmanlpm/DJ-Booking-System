# ?? RADIOBOSS.FM STATIC DOWNLOAD LINK SETUP

**Your Static URL:** `https://c40.radioboss.fm/u/98`

---

## ? **STEP 1: BUILD RELEASE VERSION**

### **Option A: Visual Studio**
1. In Visual Studio, change build configuration:
   - Top toolbar: `Debug` ? Change to **`Release`**
2. Right-click project ? **Publish**
3. Or: Build ? Publish DJBookingSystem

### **Option B: Command Line**
```powershell
# Navigate to project directory
cd "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"

# Build Release version
dotnet publish -c Release --self-contained false -r win-x64

# Output will be in:
# bin\Release\net8.0-windows\win-x64\publish\
```

**Files to upload:**
- `DJBookingSystem.exe` (main executable)
- All DLL files
- `InteractiveGuide/` folder (with voice files)
- `Voices/` folder (if separate)

---

## ? **STEP 2: CREATE ZIP PACKAGE**

**Create two versions:**

### **1. Full Installer Package** (`DJBookingSystem-Full-v1.2.5.zip`)
Contains everything needed for new installations:

```
DJBookingSystem-Full-v1.2.5.zip
??? DJBookingSystem.exe
??? *.dll (all dependencies)
??? InteractiveGuide/
?   ??? Voices/
?       ??? Detailed_Welcome.mp3
?       ??? Intro_Bookings.mp3
?       ??? ... (all voice files)
??? Voices/
?   ??? ... (additional voices)
??? README.txt
??? CHANGELOG.txt
```

### **2. Update-Only Package** (`DJBookingSystem-Update-v1.2.5.zip`)
Only the executable for existing users:

```
DJBookingSystem-Update-v1.2.5.zip
??? DJBookingSystem.exe
```

**PowerShell Script to Create Packages:**

```powershell
# Navigate to publish folder
cd "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\bin\Release\net8.0-windows\win-x64\publish"

# Create Full Package
$fullZip = "DJBookingSystem-Full-v1.2.5.zip"
Compress-Archive -Path * -DestinationPath $fullZip -Force

# Create Update-Only Package
$updateZip = "DJBookingSystem-Update-v1.2.5.zip"
Compress-Archive -Path DJBookingSystem.exe -DestinationPath $updateZip -Force

Write-Host "? Packages created:"
Write-Host "   Full: $fullZip"
Write-Host "   Update: $updateZip"
```

---

## ? **STEP 3: UPLOAD TO RADIOBOSS.FM**

### **Upload via Web Interface:**

1. **Go to:** https://c40.radioboss.fm/u/98
2. **Login** with your RadioBoss credentials
3. **Upload files:**
   - `DJBookingSystem-Full-v1.2.5.zip`
   - `DJBookingSystem.exe` (standalone)
4. **Note the direct links** (they'll be static)

### **Your Static Download URLs:**

**Full Package:**
```
https://c40.radioboss.fm/u/98/DJBookingSystem-Full-v1.2.5.zip
```

**Update Only (EXE):**
```
https://c40.radioboss.fm/u/98/DJBookingSystem.exe
```

?? **Important:** The `.exe` filename should **always be the same** so the link never changes!

---

## ? **STEP 4: CREATE VERSION INFO JSON**

**File:** `update-info.json`

Upload this alongside your EXE:

```json
{
  "version": "1.2.5",
  "releaseDate": "2025-01-21T00:00:00Z",
  "downloadUrl": "https://c40.radioboss.fm/u/98/DJBookingSystem.exe",
  "fullPackageUrl": "https://c40.radioboss.fm/u/98/DJBookingSystem-Full-v1.2.5.zip",
  "isCritical": false,
  "releaseNotes": "Version 1.2.5 - Tutorial System Complete\n\n? Mandatory auto-playing tutorial with CandyBot voice\n? Users panel pop-out window (maximizable)\n? Tutorial checkbox in Users panel\n? Tutorial highlighting fixed\n? Voice narration working\n? Panel positioning inside MainWindow\n? 100% black overlay outside window\n? Logout/login flow fixed",
  "minimumVersion": "1.0.0",
  "fileSize": 45678912,
  "sha256Hash": "",
  "features": [
    "Mandatory tutorial system",
    "Auto-playing voice narration",
    "UI element highlighting",
    "Users panel maximizable",
    "Tutorial re-training capability"
  ],
  "bugFixes": [
    "Fixed logout DialogResult error",
    "Fixed tutorial audio playback",
    "Fixed element highlighting",
    "Fixed panel positioning"
  ]
}
```

**Upload to:** `https://c40.radioboss.fm/u/98/update-info.json`

---

## ? **STEP 5: UPDATE YOUR CODE**

### **Update `Services/UpdateManager.cs` or create config:**

**Option A: Hardcode URLs** (quick)

Find where update URLs are defined and update:

```csharp
// Update check URL
private const string UPDATE_CHECK_URL = "https://c40.radioboss.fm/u/98/update-info.json";

// Download URL (from update-info.json)
// App will read this from JSON response
```

**Option B: Configuration File** (recommended)

Create `appsettings.json`:

```json
{
  "UpdateSettings": {
    "CheckUrl": "https://c40.radioboss.fm/u/98/update-info.json",
    "StaticDownloadUrl": "https://c40.radioboss.fm/u/98/DJBookingSystem.exe"
  }
}
```

---

## ? **STEP 6: CREATE DOWNLOAD PAGE (OPTIONAL)**

**File:** `download.html`

Upload to RadioBoss for a user-friendly download page:

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Download DJ Booking System</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
            color: #00ff00;
            margin: 0;
            padding: 40px;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
        }
        .container {
            background: rgba(26, 26, 46, 0.9);
            border: 2px solid #00ff00;
            border-radius: 15px;
            padding: 40px;
            max-width: 600px;
            box-shadow: 0 0 30px rgba(0, 255, 0, 0.3);
        }
        h1 {
            color: #ff1493;
            text-align: center;
            margin-bottom: 10px;
            text-shadow: 0 0 10px #ff1493;
        }
        .version {
            text-align: center;
            color: #00ff00;
            margin-bottom: 30px;
            font-size: 18px;
        }
        .download-btn {
            display: block;
            width: 100%;
            padding: 15px;
            margin: 15px 0;
            background: linear-gradient(135deg, #00ff00 0%, #00cc00 100%);
            color: #000;
            text-decoration: none;
            text-align: center;
            border-radius: 8px;
            font-weight: bold;
            font-size: 18px;
            transition: all 0.3s;
            border: none;
            cursor: pointer;
        }
        .download-btn:hover {
            background: linear-gradient(135deg, #00cc00 0%, #009900 100%);
            box-shadow: 0 0 20px rgba(0, 255, 0, 0.5);
            transform: translateY(-2px);
        }
        .secondary-btn {
            background: linear-gradient(135deg, #ff1493 0%, #cc1177 100%);
        }
        .secondary-btn:hover {
            background: linear-gradient(135deg, #cc1177 0%, #990055 100%);
            box-shadow: 0 0 20px rgba(255, 20, 147, 0.5);
        }
        .info {
            margin-top: 30px;
            padding: 20px;
            background: rgba(0, 0, 0, 0.3);
            border-radius: 8px;
            border-left: 4px solid #00ff00;
        }
        .info h3 {
            color: #00ff00;
            margin-top: 0;
        }
        .info ul {
            margin: 10px 0;
            padding-left: 20px;
        }
        .info li {
            margin: 8px 0;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>?? DJ Booking System</h1>
        <div class="version">Version 1.2.5 - Latest Release</div>
        
        <a href="DJBookingSystem.exe" class="download-btn">
            ?? Download Update (EXE Only)
        </a>
        
        <a href="DJBookingSystem-Full-v1.2.5.zip" class="download-btn secondary-btn">
            ?? Download Full Package (ZIP)
        </a>
        
        <div class="info">
            <h3>? What's New in v1.2.5</h3>
            <ul>
                <li>?? Mandatory tutorial system with CandyBot voice narration</li>
                <li>?? Interactive UI highlighting</li>
                <li>??? Users panel pop-out window</li>
                <li>?? Tutorial re-training for admins</li>
                <li>?? Multiple bug fixes and improvements</li>
            </ul>
        </div>
        
        <div class="info">
            <h3>?? System Requirements</h3>
            <ul>
                <li>.NET 8.0 Runtime</li>
                <li>Windows 10 or later</li>
                <li>Internet connection for updates</li>
                <li>Audio device for tutorial narration</li>
            </ul>
        </div>
        
        <div class="info">
            <h3>?? Links</h3>
            <ul>
                <li><a href="update-info.json" style="color: #00ff00;">Version Info (JSON)</a></li>
                <li><a href="https://livepartymusic.fm" style="color: #00ff00;" target="_blank">Live Party Music</a></li>
            </ul>
        </div>
    </div>
</body>
</html>
```

**Access at:** `https://c40.radioboss.fm/u/98/download.html`

---

## ? **STEP 7: UPDATE WORKFLOW**

### **When Releasing New Version:**

1. **Build Release** (v1.2.6)
2. **Create packages:**
   - `DJBookingSystem.exe` (keep same name!)
   - `DJBookingSystem-Full-v1.2.6.zip` (new name)
3. **Upload to RadioBoss:**
   - **Replace** `DJBookingSystem.exe` (overwrites old one)
   - **Add new** `DJBookingSystem-Full-v1.2.6.zip`
4. **Update** `update-info.json`:
   ```json
   {
     "version": "1.2.6",
     "downloadUrl": "https://c40.radioboss.fm/u/98/DJBookingSystem.exe",
     "fullPackageUrl": "https://c40.radioboss.fm/u/98/DJBookingSystem-Full-v1.2.6.zip",
     ...
   }
   ```
5. **Update** `download.html` with new version number

**Your static URLs never change!** ?

---

## ?? **STATIC URL ARCHITECTURE**

```
Your Static Links (Never Change):
??? https://c40.radioboss.fm/u/98/DJBookingSystem.exe
?   ?? Always latest EXE (gets overwritten)
?
??? https://c40.radioboss.fm/u/98/update-info.json
?   ?? Version info (gets updated)
?
??? https://c40.radioboss.fm/u/98/download.html
?   ?? Download page (gets updated)
?
??? Version-specific packages (keep all):
    ??? DJBookingSystem-Full-v1.2.5.zip
    ??? DJBookingSystem-Full-v1.2.6.zip
    ??? DJBookingSystem-Full-v1.2.7.zip
```

---

## ?? **HOW AUTO-UPDATE WORKS**

```
App starts
  ?
Checks: https://c40.radioboss.fm/u/98/update-info.json
  ?
Reads: "version": "1.2.6"
  ?
Compares with current: "1.2.5"
  ?
Shows: "Update available!"
  ?
Downloads: https://c40.radioboss.fm/u/98/DJBookingSystem.exe
  ?
Gets latest version automatically!
```

---

## ? **ADVANTAGES OF YOUR SETUP**

- ? **Your own server** - Full control
- ? **No GitHub required** - Self-hosted
- ? **Static URLs** - Never change
- ? **Fast downloads** - RadioBoss CDN
- ? **Version history** - Keep all ZIPs
- ? **Custom page** - Branded experience
- ? **Easy updates** - Just upload new EXE

---

## ?? **QUICK START CHECKLIST**

- [ ] Build Release version
- [ ] Create `DJBookingSystem-Full-v1.2.5.zip`
- [ ] Create `DJBookingSystem.exe` (standalone)
- [ ] Create `update-info.json`
- [ ] Create `download.html` (optional)
- [ ] Upload all files to `https://c40.radioboss.fm/u/98/`
- [ ] Test download links
- [ ] Update app code with new URLs
- [ ] Test update check in app
- [ ] ? Share static link with users!

---

## ?? **YOUR FINAL STATIC URLS**

**For Users:**
```
Download Page: https://c40.radioboss.fm/u/98/download.html
Direct EXE:    https://c40.radioboss.fm/u/98/DJBookingSystem.exe
Full Package:  https://c40.radioboss.fm/u/98/DJBookingSystem-Full-v1.2.5.zip
```

**For App Updates:**
```
Version Check: https://c40.radioboss.fm/u/98/update-info.json
Download URL:  https://c40.radioboss.fm/u/98/DJBookingSystem.exe
```

---

## ?? **NEXT STEPS**

1. **Build release version** (see Step 1)
2. **Create ZIP packages** (see Step 2)
3. **Upload to RadioBoss** at https://c40.radioboss.fm/u/98/
4. **Test download links**
5. **Share with users!**

---

**PERFECT SETUP! Your static link at c40.radioboss.fm/u/98 will always point to the latest version!** ???
