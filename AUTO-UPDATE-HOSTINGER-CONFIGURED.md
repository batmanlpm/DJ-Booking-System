# ? AUTO-UPDATE URLs UPDATED TO HOSTINGER

**Date:** 2025-01-21  
**Status:** ? COMPLETE

---

## ?? **ALL UPDATE URLs NOW POINT TO:**

```
https://c40.radioboss.fm/u/98/
```

---

## ?? **FILES UPDATED:**

### **1. Services/SecureUpdateClient.cs**
```csharp
// OLD:
private const string UPDATE_SERVER_URL = "https://djbookupdates.com";
private const string UPDATE_CHECK_ENDPOINT = "/version.json";

// NEW:
private const string UPDATE_SERVER_URL = "https://c40.radioboss.fm/u/98";
private const string UPDATE_CHECK_ENDPOINT = "/update-info.json";
```

**Also:**
- ? Disabled SSL certificate pinning (Hostinger uses shared hosting)
- ? Update check will use: `https://c40.radioboss.fm/u/98/update-info.json`

---

### **2. Services/UpdateDeploymentService.cs**
```csharp
// OLD:
private const string DEPLOYMENT_URL = "https://djbookupdates.com/api/deploy";
download_url = $"https://djbookupdates.com/updates/app-v{version}.exe"
checkUrl = "https://djbookupdates.com/updates/version.json"

// NEW:
private const string DEPLOYMENT_URL = "https://c40.radioboss.fm/u/98";
download_url = "https://c40.radioboss.fm/u/98/DJBookingSystem.exe"
checkUrl = "https://c40.radioboss.fm/u/98/update-info.json"
```

**Also:**
- ? Updated SFTP remote path to `/public_html/`
- ? Download URL is now static (always `DJBookingSystem.exe`)

---

### **3. MainWindow.MenuHandlers.cs**
```csharp
// OLD:
await Services.CertificateManager.TestSslConnectionAsync("https://djbookupdates.com");

// NEW:
await Services.CertificateManager.TestSslConnectionAsync("https://c40.radioboss.fm/u/98");
```

---

### **4. UpdaterAdminWindow.xaml.cs**
```csharp
// OLD:
await CertificateManager.TestSslConnectionAsync("https://djbookupdates.com");

// NEW:
await CertificateManager.TestSslConnectionAsync("https://c40.radioboss.fm/u/98");
```

---

## ?? **HOW AUTO-UPDATE WORKS NOW:**

```
App Starts
  ?
Checks: https://c40.radioboss.fm/u/98/update-info.json
  ?
Reads current version
  ?
If newer version available:
  ?
Downloads: https://c40.radioboss.fm/u/98/DJBookingSystem.exe
  ?
Installs update
  ?
Restarts application
```

---

## ?? **REQUIRED FILES ON HOSTINGER:**

Upload these to `public_html/`:

1. ? **update-info.json**
   ```json
   {
     "version": "1.2.5",
     "releaseDate": "2025-01-21T00:00:00Z",
     "downloadUrl": "https://c40.radioboss.fm/u/98/DJBookingSystem.exe",
     "fullPackageUrl": "https://c40.radioboss.fm/u/98/DJBookingSystem-Full-v1.2.5.zip",
     ...
   }
   ```

2. ? **DJBookingSystem.exe**
   - Always same filename
   - Gets overwritten with each version

3. ? **DJBookingSystem-Full-v1.2.5.zip**
   - Version-specific filename
   - Keep all versions

4. ? **index.html**
   - Download page

---

## ? **TESTING AUTO-UPDATE:**

### **Manual Test:**

1. Upload v1.2.5 to Hostinger
2. Run application (v1.2.5)
3. Update `update-info.json` on Hostinger to v1.2.6
4. In app: Help ? Check for Updates
5. Should detect new version
6. Download and install

### **Expected Behavior:**

- ? App checks for updates on startup
- ? Shows notification if update available
- ? Downloads from Hostinger
- ? Installs automatically
- ? Restarts application

---

## ?? **SECURITY NOTES:**

### **Certificate Pinning:**
- ?? **Disabled** for Hostinger (shared hosting)
- Hostinger uses shared SSL certificates
- Certificate pinning not practical for shared hosting
- Still uses HTTPS for encryption

### **Verification:**
- ? SHA256 hash verification enabled
- ? File integrity checked before install
- ? Secure HTTPS connection

---

## ?? **CONFIGURATION SUMMARY:**

| Setting | Value |
|---------|-------|
| Update Server | `https://c40.radioboss.fm/u/98` |
| Version Check | `/update-info.json` |
| Download URL | `/DJBookingSystem.exe` (static) |
| Full Package | `/DJBookingSystem-Full-v{version}.zip` |
| Certificate Pinning | Disabled (shared hosting) |
| SFTP Path | `/public_html/` |

---

## ? **BUILD STATUS:**

- ? Build successful
- ? No errors
- ? Ready for testing

---

## ?? **NEXT STEPS:**

1. ? Upload v1.2.5 to Hostinger using `Upload-Version.ps1`
2. ? Test auto-update system
3. ? Verify download links work
4. ? Test with actual users

---

**AUTO-UPDATE SYSTEM NOW FULLY CONFIGURED FOR HOSTINGER!** ???
