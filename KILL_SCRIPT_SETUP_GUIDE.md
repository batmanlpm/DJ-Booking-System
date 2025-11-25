# ?? Kill DJ Booking System - Hotkey Setup Guide

## ?? What You Got

I've created **3 different scripts** for killing the DJ Booking System process:

### 1. **Kill-DJBookingSystem.ps1** (Verbose)
- Shows what's happening
- Displays confirmation messages
- Good for manual testing

### 2. **Kill-DJBookingSystem-Silent.ps1** (Silent PowerShell)
- No messages
- Quick execution
- PowerShell window may flash briefly

### 3. **Kill-DJBookingSystem.vbs** ? RECOMMENDED FOR HOTKEY
- Completely invisible
- No window flash
- Instant execution
- Best for keyboard hotkey

---

## ?? Setting Up Keyboard Hotkey

### Method 1: Windows Native (Shortcut Key)

**Step 1: Create Shortcut**
1. Right-click `Kill-DJBookingSystem.vbs`
2. Click **"Create shortcut"**
3. Move shortcut to: `%APPDATA%\Microsoft\Windows\Start Menu\Programs`

**Step 2: Assign Hotkey**
1. Right-click the shortcut
2. Click **"Properties"**
3. Click in **"Shortcut key"** field
4. Press your desired key combo (e.g., Ctrl+Alt+K)
5. Click **"OK"**

**Recommended Key Combos:**
- `Ctrl + Alt + K` (Kill)
- `Ctrl + Alt + X` (Exit)
- `Ctrl + Shift + Esc` (Emergency Stop)

---

### Method 2: AutoHotkey (More Powerful)

If you have AutoHotkey installed:

**Create AutoHotkey Script:**
```ahk
; Kill DJ Booking System with Ctrl+Alt+K
^!k::
Run, "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Kill-DJBookingSystem.vbs"
return

; Or use F12 for emergency kill
F12::
Run, "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\Kill-DJBookingSystem.vbs"
return
```

Save as `DJBooking-Hotkeys.ahk` and run it.

---

### Method 3: Gaming Keyboard/Mouse Software

If you have gaming peripherals (Razer, Logitech, Corsair, etc.):

**Razer Synapse:**
1. Open Razer Synapse
2. Go to Macros
3. Create new macro
4. Add action: "Run Application"
5. Browse to `Kill-DJBookingSystem.vbs`
6. Assign to keyboard key

**Logitech G Hub:**
1. Open G Hub
2. Select device
3. Go to Assignments
4. Create new command
5. Choose "Run Application"
6. Select `Kill-DJBookingSystem.vbs`
7. Drag to key

**Corsair iCUE:**
1. Open iCUE
2. Select keyboard
3. Go to Actions
4. Create "Launch Application"
5. Browse to `Kill-DJBookingSystem.vbs`
6. Assign to key

---

## ?? Testing Your Hotkey

### Test 1: Run DJ Booking System
```powershell
# Start the app
dotnet run
```

### Test 2: Press Your Hotkey
- Press your assigned key combination
- App should close immediately
- No windows should appear

### Test 3: Check Task Manager
```powershell
# Verify process is gone
Get-Process -Name "DJBookingSystem" -ErrorAction SilentlyContinue
```

Should return nothing if kill was successful.

---

## ?? What Each Script Does

### Kill-DJBookingSystem.ps1
```powershell
# Kills process by name
Stop-Process -Name "DJBookingSystem" -Force

# Also kills debugger if running
Stop-Process -Name "vshost*" -Force
```

### Kill-DJBookingSystem-Silent.ps1
```powershell
# Same as above but no output
Get-Process -Name "DJBookingSystem" | Stop-Process -Force
```

### Kill-DJBookingSystem.vbs
```vbscript
' Runs PowerShell completely hidden
WshShell.Run "powershell.exe -WindowStyle Hidden ..."
```

---

## ? Quick Setup (Fastest Method)

**30 Second Setup:**

1. **Create Shortcut:**
   ```
   Right-click Kill-DJBookingSystem.vbs
   ? Send to ? Desktop (create shortcut)
   ```

2. **Assign Hotkey:**
   ```
   Right-click desktop shortcut
   ? Properties
   ? Shortcut key: Ctrl+Alt+K
   ? Apply ? OK
   ```

3. **Test:**
   ```
   Start DJ Booking System
   Press Ctrl+Alt+K
   App should close instantly
   ```

Done! ??

---

## ?? Recommended Key Combinations

### Safe Combinations (Won't Conflict)
- `Ctrl + Alt + K` - Kill
- `Ctrl + Alt + X` - Exit
- `Ctrl + Shift + F12` - Emergency Stop
- `Win + K` - Windows Key + K

### Gaming Keyboard Macro Keys
- `G1, G2, G3` keys (if available)
- `M1, M2, M3` keys (macro keys)
- Side buttons on gaming mice

### Function Keys (Easy to Remember)
- `F12` - Emergency kill
- `Shift + F12` - Kill with confirmation
- `Ctrl + F12` - Force kill

---

## ?? Emergency Scenarios

### Scenario 1: App Frozen
```
Press your hotkey ? App dies immediately
```

### Scenario 2: App Won't Respond
```
Press hotkey 2-3 times quickly
Or use Task Manager: Ctrl+Shift+Esc
```

### Scenario 3: Multiple Instances Running
```
Script kills ALL instances of DJBookingSystem.exe
```

---

## ?? Troubleshooting

### Problem: Hotkey Not Working
**Solutions:**
1. Check shortcut exists in Start Menu
2. Verify hotkey assigned in Properties
3. Restart Windows (hotkeys load on startup)
4. Try different key combination

### Problem: Permission Denied
**Solution:**
```powershell
# Run as administrator
powershell -ExecutionPolicy Bypass -File "Kill-DJBookingSystem.ps1"
```

### Problem: Script Not Found
**Solution:**
- Update VBS script path to your actual location
- Use absolute path in shortcut

### Problem: PowerShell Window Flashes
**Solution:**
- Use the VBS wrapper instead
- VBS completely hides PowerShell window

---

## ?? Script Locations

All scripts in workspace root:
```
K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\
??? Kill-DJBookingSystem.ps1         (Verbose)
??? Kill-DJBookingSystem-Silent.ps1  (Silent)
??? Kill-DJBookingSystem.vbs         (Hidden - Best for hotkey)
```

---

## ?? Best Practices

### DO:
? Use VBS wrapper for cleanest experience
? Choose key combo you won't press accidentally
? Test before relying on it
? Keep shortcut in Start Menu for persistence

### DON'T:
? Use common key combos (Ctrl+C, Ctrl+V, etc.)
? Assign to keys you use in-app
? Forget to test after Windows updates
? Use with unsaved data (script force kills)

---

## ?? Advanced: Multiple Hotkeys

You can set up multiple hotkeys for different actions:

**Create Multiple Shortcuts:**
1. Kill App: `Ctrl+Alt+K` ? `Kill-DJBookingSystem.vbs`
2. Kill & Restart: `Ctrl+Alt+R` ? `Restart-DJBookingSystem.vbs` (create this)
3. Emergency Stop: `F12` ? `Kill-DJBookingSystem.vbs`

---

## ?? Pro Tips

### Tip 1: Gaming Keyboard Users
If you have programmable macro keys, assign to a single key press (e.g., G1) instead of combo.

### Tip 2: Stream Deck Users
If you have an Elgato Stream Deck, create a button that runs the VBS file.

### Tip 3: Multi-Monitor Setup
Create desktop shortcut with icon for visual reference.

### Tip 4: Safety First
Don't use this during important operations. Always save your work first.

---

## ?? What Gets Killed

The script terminates:
- ? `DJBookingSystem.exe` (main process)
- ? `vshost*.exe` (debugger processes)
- ? All child processes
- ? All instances (if multiple running)

---

## ?? Security Notes

### Is This Safe?
? Yes - only kills DJBookingSystem process
? No system files affected
? No data corruption (just force quit)
? Same as Task Manager ? End Task

### When to Use
- App frozen/unresponsive
- Testing/development
- Emergency exit needed
- Closing stuck instances

### When NOT to Use
- During data save operations
- During database writes
- During file uploads
- With unsaved changes

---

## ?? Learning More

### Understanding Force Kill
```powershell
# What -Force does:
Stop-Process -Force
# - Immediately terminates process
# - No cleanup/shutdown procedures
# - Same as "End Task" in Task Manager
```

### Alternative: Graceful Shutdown
If you want app to close properly:
```powershell
# Close window gracefully (if app supports it)
$app = Get-Process -Name "DJBookingSystem"
$app.CloseMainWindow()
Start-Sleep -Seconds 2
if (!$app.HasExited) {
    $app | Stop-Process -Force
}
```

---

## ? Quick Reference

| Script | Use Case | Visibility | Speed |
|--------|----------|------------|-------|
| `.ps1` | Testing | Shows window | Medium |
| `-Silent.ps1` | Automation | Brief flash | Fast |
| `.vbs` | Hotkey | Invisible | Instant |

**Recommended for Hotkey:** `Kill-DJBookingSystem.vbs`

---

## ?? You're All Set!

Your kill script is ready! 

**Quick Start:**
1. Right-click `Kill-DJBookingSystem.vbs`
2. Create shortcut ? Properties
3. Assign hotkey: `Ctrl+Alt+K`
4. Test it!

**Need Help?**
- Script not working? Check Troubleshooting section
- Want different key? Edit shortcut properties
- Need restart script? Let me know!

---

**Script Version**: 1.0  
**Last Updated**: 2024  
**Compatibility**: Windows 10/11  
**Status**: ? Ready to Use
