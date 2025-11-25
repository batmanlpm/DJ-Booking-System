# ?? APP WON'T START - DIAGNOSTIC STEPS

Since the app won't start even with Ctrl+F5, let's diagnose the issue systematically:

## Step 1: Check Windows Event Viewer

1. Press `Win + X` and select **Event Viewer**
2. Go to **Windows Logs ? Application**
3. Look for recent errors (red X icons) around the time you tried to run the app
4. Check for ".NET Runtime" errors
5. **Copy the error message** and share it with me

## Step 2: Check if .NET 8 Runtime is installed

Open PowerShell and run:
```powershell
dotnet --list-runtimes
```

Look for:
- `Microsoft.WindowsDesktop.App 8.0.x`
- `Microsoft.NETCore.App 8.0.x`

If missing, install from: https://dotnet.microsoft.com/download/dotnet/8.0

## Step 3: Run from command line to see errors

```powershell
cd "K:\Customer Data\LPM\DJ_Booking\Fallen-Collective-Booking\bin\Debug\net8.0-windows"
.\DJBookingSystem.exe
```

This will show any error messages that Visual Studio might be hiding.

## Step 4: Check for XAML errors

Open **Error List** in Visual Studio:
- View ? Error List
- Look for any XAML parsing errors
- Share any errors you see

## Step 5: Simplify App.xaml.cs temporarily

The issue might be in the startup code. Let me create a minimal version to test.

---

## What to share with me:

1. **Event Viewer error** (if any)
2. **Output from `dotnet --list-runtimes`**
3. **Error when running .exe directly** from command line
4. **Any errors in Error List window**

This will help me pinpoint exactly what's preventing the app from starting!
