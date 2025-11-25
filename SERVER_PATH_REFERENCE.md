# ?? CORRECT SERVER PATHS - Quick Reference

## ? Hostinger Upload Location

### **Correct Path:**
```
public_html\Updates\
```

**Simple structure:**
```
public_html/
??? Updates/
    ??? version.json                     ? Upload here
    ??? DJBookingSystem-Setup-v1.2.0.exe ? Upload here
```

---

## ?? URLs

**Your update URLs will be:**

| File | URL |
|------|-----|
| Version Check | `https://yourdomain.com/Updates/version.json` |
| Installer | `https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.2.0.exe` |

---

## ?? Client Code Configuration

**In `Services\SecureUpdateClient.cs`:**

```csharp
private const string UPDATE_SERVER_URL = "https://yourdomain.com";
private const string UPDATE_CHECK_ENDPOINT = "/Updates/version.json";
```

**Note**: Capital 'U' in Updates!

---

## ?? version.json Template

```json
{
  "updateAvailable": true,
  "latestVersion": "1.2.0",
  "releaseDate": "2025-01-15T10:00:00Z",
  "downloadUrl": "https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.2.0.exe",
  "releaseNotes": "Update with new features",
  "features": ["Feature 1", "Feature 2"],
  "bugFixes": ["Fix 1", "Fix 2"],
  "isCritical": true,
  "minimumVersion": "1.0.0"
}
```

---

## ?? Quick Upload

**Via FileZilla:**
1. Connect to: `ftp.yourdomain.com`
2. Navigate to: `/public_html/Updates/`
3. Drag and drop files

**Via Hostinger File Manager:**
1. Go to: `public_html/Updates/`
2. Click Upload
3. Select files

---

## ? Verification

**Test these URLs in browser:**
```
? https://yourdomain.com/Updates/version.json
? https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.2.0.exe
```

Both should work!

---

**Path**: `public_html\Updates\`  
**Endpoint**: `/Updates/version.json`  
**Simple. Direct. Clean.** ?
