# ?? COMPREHENSIVE DIAGNOSTIC - APP SILENT SHUTDOWN

## Run this to get detailed startup diagnostics

### What to do:

1. **Press F5** in Visual Studio to run in Debug mode
2. **Open Output Window**: View ? Output ? Select "Debug" from dropdown
3. **Watch for debug messages** as the app starts
4. **Copy ALL output** and save to a file

### Expected Output (if working):

```
=== APPLICATION_STARTUP CALLED ===
? Exception handlers installed
=== InitializeApplicationAsync Started ===
? CosmosDbService created
? OnlineUserStatusService initialized
Creating splash screen...
? Splash screen shown
Initializing Azure Cosmos DB...
? Azure Cosmos DB initialized!
? Default admin created!
Waiting for splash screen to complete...
? Splash screen completed!
?? Checking for intro videos...
?? No intro videos found - skipping intro
Checking for auto-login...
No auto-login found.
Showing login window...
Creating LoginWindow...
Showing LoginWindow dialog...
```

### Where it might fail:

#### **If stops at "Creating splash screen":**
- XAML parsing error in SplashScreen.xaml
- Missing resources/images

#### **If stops at "Initializing Azure Cosmos DB":**
- Network/firewall issue
- Cosmos DB credentials wrong
- Internet connection problem

#### **If stops at "Creating MainWindow":**
- XAML parsing error in MainWindow.xaml
- Missing control definitions
- Resource dictionary errors

#### **If stops at "MainWindow Constructor":**
- Exception in constructor
- Missing partial class methods
- Service initialization failure

### What to check in Output window:

Look for these patterns:

**Pattern 1: XAML Error**
```
XamlParseException: ...
```
**Cause:** XAML markup error

**Pattern 2: Type Not Found**
```
TypeLoadException: ...
FileNotFoundException: Could not load...
```
**Cause:** Missing assembly or type

**Pattern 3: Null Reference**
```
NullReferenceException: Object reference not set...
```
**Cause:** Trying to use null object

**Pattern 4: Constructor Failure**
```
? FATAL ERROR in MainWindow Constructor: ...
```
**Cause:** Exception during MainWindow initialization

### If no output appears:

1. Check Output window is set to "Debug" (not "Build")
2. Check Debug ? Windows ? Output is enabled
3. Try running: `System.Diagnostics.Debugger.Launch();` at top of App_Startup

### Share this information:

When reporting the issue, include:

1. **ALL Output window content** (from start to crash)
2. **Last message before silence**
3. **Any exception messages**
4. **Event Viewer errors**: Windows Logs ? Application ? .NET Runtime

---

## Quick Tests:

### Test 1: Can XAML Parse?
Open MainWindow.xaml in designer - does it show preview?
- YES ? XAML is valid
- NO ? XAML has errors (check Error List)

### Test 2: Can Constructor Run?
Add MessageBox at start of MainWindow constructor:
```csharp
public MainWindow(...)
{
    MessageBox.Show("Constructor started!");
    //... rest of code
}
```
- Shows? ? Constructor is reached
- Doesn't show? ? Crashes before constructor

### Test 3: Check Event Viewer
1. Open Event Viewer
2. Windows Logs ? Application
3. Look for .NET Runtime errors
4. Check timestamp matching crash time

---

## Most Likely Causes:

1. **XAML parsing error** (90% of silent crashes)
2. **Missing partial class file** (if methods not found)
3. **Null reference in constructor** (if services fail)
4. **Resource dictionary error** (if themes broken)
5. **Update check SSL failure** (if certificate validation fails)

---

**Next step:** Run in Debug mode and share the Output window content!
