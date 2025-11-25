# ??? CORRECT FOLDER STRUCTURE

## ? Simple and Clean Structure

```
?? yourdomain.com (Hostinger)
?
??? ?? public_html/
    ?
    ??? ?? Updates/                                      ? CREATE THIS FOLDER
    ?   ?
    ?   ??? ?? version.json                              ? UPLOAD HERE
    ?   ??? ?? DJBookingSystem-Setup-v1.2.0.exe         ? UPLOAD HERE
    ?   ??? ?? DJBookingSystem-Setup-v1.1.0.exe         (older versions)
    ?   ??? ?? changelog.html                            (optional)
    ?
    ??? ?? wp-content/ (if WordPress)
    ??? ?? index.html
    ??? (other website files)
```

---

## ?? How URLs Map to Files

### **File System ? URL Mapping:**

| File on Server | URL in Browser |
|----------------|----------------|
| `public_html/Updates/version.json` | `https://yourdomain.com/Updates/version.json` |
| `public_html/Updates/DJBookingSystem-Setup-v1.2.0.exe` | `https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.2.0.exe` |

---

## ?? Example with Real Domain

**If your domain is**: `fallencollective.com`

### **Server Structure:**
```
public_html/
??? Updates/
    ??? version.json
    ??? DJBookingSystem-Setup-v1.2.0.exe
```

### **URLs:**
```
https://fallencollective.com/Updates/version.json
https://fallencollective.com/Updates/DJBookingSystem-Setup-v1.2.0.exe
```

### **Client Code:**
```csharp
private const string UPDATE_SERVER_URL = "https://fallencollective.com";
private const string UPDATE_CHECK_ENDPOINT = "/Updates/version.json";
```

---

## ?? Why This Structure?

### **Simple:**
- ? One folder: `Updates`
- ? All update files in one place
- ? Easy to manage

### **Clean URLs:**
```
? yourdomain.com/Updates/version.json
? NOT: yourdomain.com/updates/installers/version.json
```

### **Easy to Remember:**
- Updates folder = Update files
- Capital 'U' = Professional
- No nested folders = Simple

---

## ?? Creating the Folder

### **Method 1: Hostinger File Manager**
```
1. Login ? Files ? File Manager
2. Click: public_html
3. Click: New Folder
4. Type: Updates
5. Click: Create
```

### **Method 2: FileZilla (FTP)**
```
1. Connect to FTP
2. Navigate: /public_html/
3. Right-click ? Create Directory
4. Type: Updates
5. Press Enter
```

---

## ?? Uploading Files

### **Method 1: FileZilla**
```
Local (Left Side):
K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Installer\Output\
    ??? DJBookingSystem-Setup-v1.2.0.exe

Remote (Right Side):
/public_html/Updates/
    ??? (drag and drop here)
```

### **Method 2: Hostinger File Manager**
```
1. Navigate: public_html/Updates/
2. Click: Upload
3. Select: DJBookingSystem-Setup-v1.2.0.exe
4. Wait for upload
```

---

## ? Final Structure Check

**After uploading, your server should look like:**

```
?? public_html/
    ??? ?? Updates/
    ?   ??? ?? version.json              ? Present
    ?   ??? ?? DJBookingSystem-Setup-v1.2.0.exe  ? Present
    ??? (other files)
```

**Test in browser:**
```
? https://yourdomain.com/Updates/version.json
   ? Shows JSON content

? https://yourdomain.com/Updates/DJBookingSystem-Setup-v1.2.0.exe
   ? Downloads installer
```

---

## ?? That's It!

**Path on server**: `public_html\Updates\`  
**URL in browser**: `https://yourdomain.com/Updates/`  
**Endpoint in code**: `/Updates/version.json`

**Simple. Clean. Professional.** ?
