# ?? GITHUB RELEASES SETUP GUIDE

## ? STEP 1: CREATE GITHUB REPOSITORY

1. Go to https://github.com/new
2. Repository name: `DJ-Booking-System` or `Fallen-Collective-Booking`
3. Set to **Private** (if you want) or Public
4. Click "Create repository"

---

## ? STEP 2: UPLOAD YOUR PROJECT

### **Option A: GitHub Desktop (Easy)**
1. Download GitHub Desktop: https://desktop.github.com/
2. Clone your new repository
3. Copy your project files into the folder
4. Commit and Push

### **Option B: Command Line**
```bash
cd "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking"
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YourUsername/DJ-Booking-System.git
git push -u origin main
```

---

## ? STEP 3: CREATE FIRST RELEASE

1. **Go to your repository on GitHub**
2. **Click "Releases" ? "Create a new release"**
3. **Fill in:**
   - Tag version: `v1.2.5`
   - Release title: `Version 1.2.5 - Initial Release`
   - Description: Your release notes
4. **Upload files:**
   - Drag and drop `DJBookingSystem.exe` from `bin\Release\net8.0-windows\`
   - Optionally: Add `publish.zip` with all files
5. **Click "Publish release"**

---

## ? STEP 4: GET YOUR STATIC DOWNLOAD URL

**Format:**
```
https://github.com/YourUsername/YourRepo/releases/latest/download/FileName.exe
```

**Your URL (example):**
```
https://github.com/FallenCollective/DJ-Booking-System/releases/latest/download/DJBookingSystem.exe
```

**This URL ALWAYS points to the latest version!**

---

## ? STEP 5: UPDATE YOUR CODE

### **Update `UpdateManager.cs`**

Find the download URL configuration and replace:

```csharp
// OLD (hardcoded version URL)
private const string UPDATE_DOWNLOAD_URL = "https://yourserver.com/downloads/v1.2.5/DJBookingSystem.exe";

// NEW (static latest URL)
private const string UPDATE_DOWNLOAD_URL = "https://github.com/YourUsername/DJ-Booking-System/releases/latest/download/DJBookingSystem.exe";
```

### **Update `update-info.json` URL**

```csharp
// In UpdateManager.cs
private const string UPDATE_CHECK_URL = "https://raw.githubusercontent.com/YourUsername/DJ-Booking-System/main/update-info.json";
```

---

## ? STEP 6: CREATE `update-info.json` IN REPOSITORY

**File:** `update-info.json` (in repository root)

```json
{
  "version": "1.2.5",
  "releaseDate": "2025-01-21T00:00:00Z",
  "downloadUrl": "https://github.com/YourUsername/DJ-Booking-System/releases/latest/download/DJBookingSystem.exe",
  "isCritical": false,
  "releaseNotes": "Version 1.2.5 - Tutorial System & Bug Fixes\n\n? Mandatory auto-playing tutorial with voice\n? Users panel maximizable\n? Tutorial checkbox functionality\n? Logout/login flow fixed\n? Voice file paths corrected\n? Element highlighting working",
  "minimumVersion": "1.0.0",
  "fileSize": 45678912,
  "sha256Hash": "ABC123..."
}
```

**Commit this file to your repository!**

---

## ? STEP 7: RELEASE NEW VERSIONS

**Every time you update:**

1. **Build Release version:**
   ```
   dotnet publish -c Release
   ```

2. **Go to GitHub Releases**

3. **Click "Draft a new release"**

4. **Fill in:**
   - Tag: `v1.2.6` (increment version)
   - Title: `Version 1.2.6 - New Features`
   - Upload new `DJBookingSystem.exe`

5. **Update `update-info.json`:**
   ```json
   {
     "version": "1.2.6",
     "releaseDate": "2025-01-22T00:00:00Z",
     ...
   }
   ```

6. **Commit and push** `update-info.json`

7. **Publish release**

**Your static URL still works - now points to v1.2.6!**

---

## ?? **HOW IT WORKS**

```
User opens app
  ?
App checks: https://raw.githubusercontent.com/.../update-info.json
  ?
Sees: "version": "1.2.6" (current is 1.2.5)
  ?
Shows update notification
  ?
User clicks "Update"
  ?
Downloads: https://github.com/.../releases/latest/download/DJBookingSystem.exe
  ?
Gets v1.2.6 automatically!
```

---

## ? **ADVANTAGES**

- ? **FREE** hosting
- ? **No server** required
- ? **Automatic** latest redirect
- ? **Version control** built-in
- ? **Release notes** included
- ? **Download statistics** available
- ? **CDN** powered (fast downloads)
- ? **HTTPS** secure

---

## ?? **ALTERNATIVE: PERMANENT DOWNLOAD PAGE**

Create `latest.html` in repository:

```html
<!DOCTYPE html>
<html>
<head>
    <meta http-equiv="refresh" content="0; url=https://github.com/YourUsername/DJ-Booking-System/releases/latest">
    <title>Download Latest Version</title>
</head>
<body>
    <h1>Redirecting to latest version...</h1>
    <p>If not redirected, <a href="https://github.com/YourUsername/DJ-Booking-System/releases/latest">click here</a></p>
</body>
</html>
```

**Share:** `https://yourusername.github.io/DJ-Booking-System/latest.html`

---

## ?? **FOR PRIVATE RELEASES**

If repository is private, use **GitHub Personal Access Tokens**:

1. Generate token: GitHub ? Settings ? Developer settings ? Personal access tokens
2. Scope: `repo` (full control)
3. Use in download URL:
   ```
   https://TOKEN@github.com/YourUsername/Repo/releases/latest/download/DJBookingSystem.exe
   ```

?? **Security:** Don't hardcode token in app! Store in encrypted settings.

---

## ?? **TESTING**

1. **Create test release** with v1.2.5
2. **Visit:** `https://github.com/YourUsername/YourRepo/releases/latest`
3. **Verify:** Redirects to v1.2.5
4. **Create v1.2.6**
5. **Visit same URL**
6. **Verify:** Now shows v1.2.6

? **Static URL always points to latest!**

---

## ?? **QUICK START CHECKLIST**

- [ ] Create GitHub account (if needed)
- [ ] Create new repository
- [ ] Upload project files
- [ ] Create first release (v1.2.5)
- [ ] Upload `DJBookingSystem.exe`
- [ ] Copy static download URL
- [ ] Create `update-info.json` in repo
- [ ] Update `UpdateManager.cs` with new URLs
- [ ] Test download link
- [ ] Test update check
- [ ] ? Done!

---

**YOUR STATIC DOWNLOAD URL:**
```
https://github.com/YourUsername/DJ-Booking-System/releases/latest/download/DJBookingSystem.exe
```

**This URL never changes, but always downloads the newest version!** ??
