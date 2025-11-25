# ?? Complete Dependency Inclusion Guide

## ? What Gets Included in the Installer

### **Automatic Inclusion (via `dotnet publish --self-contained`)**

#### **1. .NET Runtime (Complete)**
```
? .NET 8.0 Runtime
? Windows Desktop Runtime
? WPF Framework
? All .NET Core libraries
? System.* assemblies
? Microsoft.* assemblies
```

**Location after publish**: `bin\Release\net8.0-windows\publish\`  
**Size**: ~100-150 MB

---

#### **2. Application Files**
```
? DJBookingSystem.exe (main executable)
? DJBookingSystem.dll (if separate)
? DJBookingSystem.pdb (debug symbols)
? DJBookingSystem.deps.json (dependency manifest)
? DJBookingSystem.runtimeconfig.json (runtime configuration)
```

---

#### **3. NuGet Package Dependencies**

All packages from your `.csproj` are automatically included:

**Core Packages:**
```
? Microsoft.Web.WebView2.Core.dll
? Microsoft.Web.WebView2.Wpf.dll
? Microsoft.Web.WebView2.WinForms.dll
? Newtonsoft.Json.dll
? Azure.Core.dll
? Azure.Cosmos.dll (if used)
? System.Data.SqlClient.dll (if used)
```

**Native Libraries:**
```
? WebView2Loader.dll
? runtimes\win-x64\native\*.dll
? Any P/Invoke dependencies
```

---

#### **4. Resource Files**
```
? appsettings.json
? config files
? localization resources (if any)
? embedded resources
```

---

### **Manual Inclusion (via installer.iss [Files] section)**

#### **5. Prerequisites**
```
? MicrosoftEdgeWebview2Setup.exe (WebView2 installer)
? windowsdesktop-runtime-8.0-win-x64.exe (optional backup)
```

#### **6. Assets & Graphics**
```
? Assets\* (all images, icons)
? Themes\* (XAML theme files)
? Resources\* (additional resources)
```

#### **7. Documentation**
```
? README.md
? LICENSE.txt
? CHANGELOG.md
```

---

## ?? Verification Checklist

### **After Running `dotnet publish`:**

Check `bin\Release\net8.0-windows\publish\` contains:

**Must Have:**
- [ ] DJBookingSystem.exe (main app)
- [ ] DJBookingSystem.dll (if exists)
- [ ] Microsoft.Web.WebView2.*.dll (3+ files)
- [ ] Newtonsoft.Json.dll
- [ ] System.*.dll (many files)
- [ ] runtimes\ folder (native libraries)
- [ ] api-ms-win-*.dll (Windows API wrappers)

**Application-Specific:**
- [ ] Azure.*.dll (if using Cosmos DB)
- [ ] Any custom DLLs you created
- [ ] Third-party libraries

**Total Expected Files**: 200-400+ files (varies by dependencies)

---

## ?? Dependency Breakdown by Category

### **Category 1: Core .NET (Self-Contained)**
```
Size: ~100 MB
Files: 150+ DLLs
Includes:
  - .NET Runtime
  - WPF Framework
  - Windows Desktop Runtime
  - Base Class Libraries
```

### **Category 2: Application Code**
```
Size: ~5-20 MB
Files: Your compiled code + resources
Includes:
  - DJBookingSystem.exe
  - Your ViewModels, Services, etc.
  - Embedded resources
```

### **Category 3: NuGet Packages**
```
Size: ~20-50 MB
Files: 30-100 DLLs
Includes:
  - WebView2 (15 MB)
  - Azure SDKs (10 MB)
  - Json.NET (500 KB)
  - Other packages
```

### **Category 4: Native Dependencies**
```
Size: ~5-10 MB
Files: Native DLLs in runtimes\win-x64\native\
Includes:
  - WebView2Loader.dll
  - Native Windows libraries
  - C++ runtime components
```

### **Category 5: Assets & Resources**
```
Size: ~1-5 MB
Files: Images, themes, configs
Includes:
  - Application icons
  - Theme XAML files
  - Configuration files
```

**Total Installer Size**: ~130-185 MB (typical)

---

## ??? Build Command Breakdown

### **Current `dotnet publish` Command:**

```powershell
dotnet publish -c Release `
    -r win-x64 `                              # Target Windows x64
    --self-contained true `                   # Include .NET runtime
    -p:PublishSingleFile=false `              # Keep files separate
    -p:PublishReadyToRun=false `              # Standard compilation
    -p:IncludeNativeLibrariesForSelfExtract=true ` # Include native DLLs
    -p:PublishTrimmed=false `                 # Don't trim dependencies
    /p:DebugType=embedded `                   # Include debug info
    /p:DebugSymbols=true                      # Keep .pdb files
```

**What Each Flag Does:**

| Flag | Purpose |
|------|---------|
| `-c Release` | Build in Release configuration |
| `-r win-x64` | Target 64-bit Windows |
| `--self-contained true` | **Include .NET runtime** (users don't need .NET installed) |
| `-p:PublishSingleFile=false` | Keep all DLLs separate (not single .exe) |
| `-p:PublishReadyToRun=false` | Standard JIT compilation |
| `-p:IncludeNativeLibrariesForSelfExtract=true` | **Include native DLLs** |
| `-p:PublishTrimmed=false` | **Keep ALL dependencies** (don't remove unused) |
| `/p:DebugType=embedded` | Embed debug info in assemblies |
| `/p:DebugSymbols=true` | Include .pdb files for crash reports |

---

## ?? What Gets Copied to User's PC

### **Installation Directory: `C:\Program Files\DJ Booking System\`**

```
C:\Program Files\DJ Booking System\
??? DJBookingSystem.exe                    ? Main executable
??? DJBookingSystem.dll
??? DJBookingSystem.pdb
??? DJBookingSystem.deps.json              ? Dependency manifest
??? DJBookingSystem.runtimeconfig.json     ? Runtime config
?
??? *.dll (200+ files)                     ? All runtime DLLs
?   ??? System.*.dll                       ? .NET libraries
?   ??? Microsoft.*.dll                    ? Framework libraries
?   ??? Azure.*.dll                        ? Azure SDKs
?   ??? Newtonsoft.Json.dll                ? JSON library
?   ??? Microsoft.Web.WebView2.*.dll       ? WebView2
?
??? runtimes\                              ? Native libraries
?   ??? win-x64\
?       ??? native\
?           ??? WebView2Loader.dll
?           ??? *.dll (native)
?
??? Assets\                                ? Graphics & UI
?   ??? Icons\
?   ??? Images\
?   ??? Themes\
?
??? Themes\                                ? XAML themes
?   ??? SpaceTheme.xaml
?   ??? GreenTheme.xaml
?
??? appsettings.json                       ? Configuration
??? README.md
??? LICENSE.txt
??? CHANGELOG.md
```

**Total**: 200-400 files, ~130-180 MB

---

## ?? Common Issues & Solutions

### **Issue: "Missing DLL" Error on User's PC**

**Cause**: Dependency not included in publish

**Solution**:
```powershell
# Ensure self-contained is true
dotnet publish --self-contained true

# Verify DLL exists in publish folder
ls bin\Release\net8.0-windows\publish\*.dll
```

---

### **Issue: "WebView2 Not Found"**

**Cause**: WebView2 Runtime not installed

**Solution**: Installer automatically installs it via:
```iss
[Run]
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; 
Parameters: "/silent /install"; 
Flags: waituntilterminated;
```

---

### **Issue: "App Won't Start - Missing Runtime"**

**Cause**: Not published as self-contained

**Solution**: Always use `--self-contained true`

---

### **Issue: "Installer Too Large (>500 MB)"**

**Cause**: Including unnecessary files

**Solution**: 
- Don't use `PublishTrimmed=true` (can break reflection)
- Exclude development files:
  ```iss
  ; Don't include these:
  ; Source: "*.vshost.exe"
  ; Source: "*.vshost.exe.config"
  ```

---

## ? Final Verification Steps

### **Before Building Installer:**

```powershell
# 1. Clean build
dotnet clean

# 2. Publish with dependencies
dotnet publish -c Release -r win-x64 --self-contained true

# 3. Verify publish folder
cd bin\Release\net8.0-windows\publish

# 4. Check file count
Get-ChildItem -Recurse -File | Measure-Object | Select-Object Count
# Should be 200-400+ files

# 5. Check key DLLs
ls DJBookingSystem.exe          # Main app
ls Microsoft.Web.WebView2.*.dll # WebView2
ls Newtonsoft.Json.dll          # JSON
ls Azure.*.dll                  # Azure (if used)

# 6. Check size
Get-ChildItem -Recurse | Measure-Object -Property Length -Sum
# Should be 100-200 MB
```

### **After Building Installer:**

```powershell
# 1. Check installer size
Get-Item Installer\Output\*.exe
# Should be 130-180 MB

# 2. Test on clean VM
# - Windows 10/11 fresh install
# - No .NET installed
# - No WebView2 installed
# ? App should run perfectly!
```

---

## ?? What Users Need

### **On User's PC: NOTHING!**

Everything is included:
- ? .NET Runtime (self-contained)
- ? WebView2 (installed automatically)
- ? All dependencies (in installer)
- ? All DLLs (in installer)

**Users just:**
1. Download installer
2. Run installer
3. Click through setup
4. Launch app
5. **It just works!**

---

## ?? Summary

### **What's Included Automatically:**
? .NET 8 Runtime (~100 MB)  
? All NuGet packages  
? All referenced DLLs  
? Native libraries  
? Application code  

### **What You Add Manually:**
? WebView2 installer  
? Assets & themes  
? Documentation  
? Config files  

### **Total Package:**
?? **~130-180 MB installer**  
?? **200-400 files installed**  
?? **~150-200 MB on disk**  

**Result**: Completely self-contained application that runs on ANY Windows 10/11 PC without requiring any pre-installed software!

---

## ?? Quick Test

```powershell
# Build installer
.\Build-Installer.ps1

# Test on clean machine:
# 1. Fresh Windows install
# 2. Run installer
# 3. Launch app
# ? Should work perfectly!
```

**Status**: ? **Complete Dependency Inclusion Configured!**
