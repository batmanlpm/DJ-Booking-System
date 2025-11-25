# ?? SILENT EXIT TROUBLESHOOTING GUIDE

## Issue: App Silently Quits on Startup

### ? FIXES APPLIED

1. **Tutorial Video Logic Fixed**
   - ? **OLD:** Both tutorials played for all users
   - ? **NEW:** 
     - Regular users: ONLY `Tutorial_Users.mp4`
     - Managers/SysAdmins: `Tutorial_Users.mp4` THEN `Tutorial_Admin.mp4`

2. **Exception Handlers Added**
   - Added `AppDomain.CurrentDomain.UnhandledException` handler
   - Added `this.DispatcherUnhandledException` handler
   - Both will show error dialogs instead of silent crashes

3. **Debug Logging Enhanced**
   - App.xaml.cs now logs every step with `===` markers
   - MainWindow constructor logs each initialization step
   - Tutorial system logs which videos are being loaded

---

## ?? DEBUGGING STEPS

### Step 1: Run in Visual Studio with Debug Output

1. Open **Visual Studio**
2. Press **F5** (Start Debugging)
3. Open **Output Window** (View ? Output)
4. Select **Debug** from dropdown
5. Watch for debug messages:

```
=== APPLICATION_STARTUP CALLED ===
? Exception handlers installed
? InitializeApplicationAsync completed
=== MainWindow Constructor Started ===
? InitializeComponent completed
? Basic services initialized
...
```

### Step 2: Check for Exceptions

If app crashes, you should now see a **MessageBox** with the error:
- "FATAL ERROR" - Unhandled exception
- "UI THREAD ERROR" - Dispatcher exception
- "Startup Error" - App.xaml.cs exception

**If no MessageBox appears**, the crash is happening before exception handlers are set up.

---

## ?? COMMON CAUSES & FIXES

### 1. **Missing Tutorial Videos**

**Symptoms:**
- App starts, splash screen shows, then quits
- No error message

**Fix:**
```
Check folder: CandyBot_Training_Guides\
Expected files:
  - Tutorial_Users.mp4
  - Tutorial_Admin.mp4
```

If missing, the app will now show a warning and continue anyway.

---

### 2. **Azure Cosmos DB Connection Failure**

**Symptoms:**
- Splash screen shows "Connecting to Cosmos DB..."
- Then quits

**Fix:**
- Check internet connection
- Verify Azure Cosmos DB is accessible
- Check firewall rules

**You should see error message now** (added exception handlers).

---

### 3. **WebView2 Runtime Missing**

**Symptoms:**
- Silent crash during MainWindow initialization
- RadioBOSS views fail to load

**Fix:**
Install WebView2 Runtime:
https://developer.microsoft.com/microsoft-edge/webview2/

---

### 4. **Forced Tutorial Flag**

**Status:** ? **FIXED** - Changed to `false` in App.xaml.cs line 230

```csharp
bool forceShowTutorial = false; // Now disabled
```

---

### 5. **Corrupt User Data**

**Symptoms:**
- Works for some users, crashes for others
- Crashes after login

**Fix:**
```powershell
# Reset user's tutorial status
.\Tools\Reset-TutorialStatus-AllUsers.ps1
```

---

## ?? DEBUG OUTPUT GUIDE

### What You Should See:

```
=== APPLICATION_STARTUP CALLED ===
? Exception handlers installed
Initializing with AZURE COSMOS DB (cloud mode)
Initializing Azure Cosmos DB...
? Azure Cosmos DB initialized!
? Default admin created!
Waiting for splash screen to complete...
? Splash screen completed!
Checking for auto-login...
No auto-login found.
Closing splash screen and showing login window...
Creating LoginWindow...
Showing LoginWindow dialog...
Login dialog result: True
? Login successful for: username
Loading app settings...
? App settings loaded!
Creating MainWindow...
=== MainWindow Constructor Started ===
? InitializeComponent completed
? Basic services initialized
? CandyBot Sound initialized
? CandyBot Files initialized
? Image Generator initialized
? Document Generator initialized
? System Tray initialized
? Window Controls initialized
? Title Bar updated
? Settings applied
? Permissions applied
? User Preferences applied
? Connection monitoring started
=== MainWindow Constructor Completed Successfully ===
Showing MainWindow...
? MainWindow shown successfully!
?? MainWindow loaded - checking tutorial status
```

### What Indicates a Problem:

**Stops at any point without error = Silent crash**

```
=== APPLICATION_STARTUP CALLED ===
? Exception handlers installed
Initializing with AZURE COSMOS DB (cloud mode)
[STOPS HERE - No error message]
```

**Solution:** Run in Debug mode and check **Exception Settings** (Ctrl+Alt+E):
- Enable "Common Language Runtime Exceptions"
- This will break on ALL exceptions

---

## ?? QUICK FIX CHECKLIST

Run through these in order:

1. **Clean & Rebuild**
   ```powershell
   dotnet clean
   dotnet build
   ```

2. **Check Tutorial Files Exist**
   ```powershell
   Get-ChildItem "CandyBot_Training_Guides" -Filter "*.mp4"
   ```

3. **Reset Tutorial Status**
   ```powershell
   .\Tools\Reset-TutorialStatus-AllUsers.ps1
   ```

4. **Test with Different User**
   - Login as `SysAdmin` / `Admin123!`
   - If works, issue is user-specific

5. **Check Debug Output**
   - Run in Visual Studio (F5)
   - Watch Output window
   - Look for where it stops

6. **Install WebView2 Runtime**
   - Download from: https://developer.microsoft.com/microsoft-edge/webview2/

7. **Test Cosmos DB Connection**
   ```powershell
   Test-NetConnection -ComputerName fallen-collective.documents.azure.com -Port 443
   ```

---

## ?? EXPECTED BEHAVIOR (Fixed)

### For Regular Users (e.g., "1", "2", "3"):
1. Login
2. Splash screen
3. **Tutorial: Tutorial_Users.mp4** (ONLY)
4. MainWindow appears
5. CandyBot greeting plays

### For Admins (Manager, SysAdmin):
1. Login
2. Splash screen
3. **Tutorial 1: Tutorial_Users.mp4**
4. **Tutorial 2: Tutorial_Admin.mp4**
5. MainWindow appears
6. CandyBot greeting plays

---

## ?? DEBUG LOG EXAMPLE

Save this PowerShell script to capture debug output:

```powershell
# Capture-DebugLog.ps1
$outputFile = "debug-log.txt"

# Start app and redirect debug output
dotnet run > $outputFile 2>&1

Write-Host "Debug log saved to: $outputFile"
notepad $outputFile
```

---

## ?? STILL CRASHING?

If app still crashes silently after all fixes:

1. **Open Event Viewer**
   - Windows Logs ? Application
   - Look for .NET Runtime errors
   - Note the error code

2. **Enable First-Chance Exceptions**
   - In Visual Studio: Debug ? Windows ? Exception Settings
   - Check "Common Language Runtime Exceptions"
   - Run again - it will break on ALL exceptions

3. **Check Antivirus**
   - Temporarily disable antivirus
   - Try running app
   - If works, add exception for DJ Booking System

4. **Reinstall .NET 8 Runtime**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Install Desktop Runtime

---

## ? VERIFICATION

After applying fixes, verify:

1. App starts without crashing
2. Login works
3. Tutorial plays (correct # of videos based on role)
4. MainWindow appears
5. No error dialogs
6. CandyBot greeting plays

---

## ?? SUPPORT

If issue persists:
1. Run app in Debug mode (F5)
2. Capture Output window text
3. Save Event Viewer errors
4. Share debug log

**Common fix:** Tutorial videos in wrong location or corrupted.

---

**Last Updated:** 2025-01-23  
**Version:** 1.2.5  
**Status:** Fixes Applied ?
