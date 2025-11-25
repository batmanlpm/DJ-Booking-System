; DJ Booking System - Professional Installer Script
; The Fallen Collective
; Fancy Space-Themed Installer with Neon Green Accents

#define MyAppName "DJ Booking System"
#define MyAppVersion "1.2.4"
#define MyAppPublisher "The Fallen Collective & Mega Byte I.T Services"
#define MyAppURL "https://djbookupdates.com"
#define MyAppExeName "DJBookingSystem.exe"
#define MyAppId "{{8A3D5E2F-9B4C-4D1E-8F3A-7C6B5A4D3E2F}}"

[Setup]
; Basic Information
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/support
AppUpdatesURL={#MyAppURL}/updates
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=Installer\Output
OutputBaseFilename=DJBookingSystem-Setup-v{#MyAppVersion}
SetupIconFile=Assets\AppIcon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

; Window Settings
WindowVisible=yes
DisableWelcomePage=no
DisableProgramGroupPage=yes
DisableDirPage=no
DisableReadyPage=no
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Version Information
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
VersionInfoCopyright=Copyright (C) 2025 {#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode
Name: "autostart"; Description: "Launch {#MyAppName} at Windows startup"; GroupDescription: "Additional tasks:"

[Files]
; ============================================
; MAIN APPLICATION FILES - COMPLETE COPY
; ============================================
; Main executable
Source: "bin\Release\net8.0-windows\win-x64\publish\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

; ALL application files, DLLs, and dependencies (recursive)
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; This includes:
; - All .dll files (runtime dependencies)
; - All .json config files (appsettings.json, etc.)
; - All .deps.json and .runtimeconfig.json files
; - All native libraries
; - All language resources
; - All content files

; ============================================
; .NET RUNTIME FILES (if self-contained)
; ============================================
; These are automatically included in publish folder above, but explicitly noting:
; - .NET 8 Runtime libraries
; - Windows Desktop Runtime
; - ASP.NET Core Runtime (if needed)
; - All native dependencies

; ============================================
; PREREQUISITES & INSTALLERS
; ============================================
; WebView2 Runtime (for Discord integration)
Source: "Prerequisites\MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

; .NET Desktop Runtime (backup - in case publish isn't self-contained)
; Uncomment if needed:
; Source: "Prerequisites\windowsdesktop-runtime-8.0-win-x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

; ============================================
; GRAPHICS & UI ASSETS
; ============================================
; Application icons, images, themes - Optional
; Source: "Assets\*"; DestDir: "{app}\Assets"; Flags: ignoreversion recursesubdirs createallsubdirs

; Themes folder (if exists) - Optional
; Source: "Themes\*"; DestDir: "{app}\Themes"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: DirExists('Themes')

; ============================================
; DOCUMENTATION & LICENSE
; ============================================
; These files are optional - uncomment if they exist
; Source: "README.md"; DestDir: "{app}"; Flags: ignoreversion isreadme
; Source: "LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
; Source: "CHANGELOG.md"; DestDir: "{app}"; Flags: ignoreversion

; ============================================
; CONFIGURATION FILES
; ============================================
; User settings, config files (if in root) - Optional
; Source: "appsettings.json"; DestDir: "{app}"; Flags: ignoreversion; Check: FileExists('appsettings.json')

; ============================================
; DATABASE / DATA FILES (if needed)
; ============================================
; Local database files (if you use SQLite or similar) - Optional
; Source: "Data\*"; DestDir: "{app}\Data"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: DirExists('Data')

; ============================================
; ADDITIONAL DEPENDENCIES
; ============================================
; Any other folders your app needs - Optional, uncomment if they exist:
; - Resources folder
; - Templates folder
; - Sounds folder
; - etc.

; Resources (if exists) - Optional
; Source: "Resources\*"; DestDir: "{app}\Resources"; Flags: ignoreversion recursesubdirs createallsubdirs; Check: DirExists('Resources')

; ============================================
; NUGET PACKAGES & NATIVE LIBRARIES
; ============================================
; The publish folder should include all NuGet dependencies automatically
; This includes packages like:
; - Microsoft.Web.WebView2
; - Newtonsoft.Json
; - Azure.Cosmos
; - Any other NuGet packages
; All are in bin\Release\net8.0-windows\publish\ already

; ============================================
; RUNTIME DEPENDENCIES
; ============================================
; Windows Desktop Runtime components
; Native Windows libraries
; C++ redistributables (if needed)
; All included in self-contained publish

[Icons]
; Start Menu
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Comment: "Launch {#MyAppName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"; Comment: "Uninstall {#MyAppName}"
Name: "{group}\README"; Filename: "{app}\README.md"

; Desktop
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\{#MyAppExeName}"; Comment: "Launch {#MyAppName}"

; Quick Launch
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

; Startup
Name: "{userstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: autostart; Comment: "Launch {#MyAppName} at Windows startup"

[Registry]
; Register application for updates
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey

; Add to Windows Apps & Features
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1"; ValueType: string; ValueName: "DisplayIcon"; ValueData: "{app}\{#MyAppExeName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1"; ValueType: string; ValueName: "DisplayName"; ValueData: "{#MyAppName}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1"; ValueType: string; ValueName: "DisplayVersion"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1"; ValueType: string; ValueName: "Publisher"; ValueData: "{#MyAppPublisher}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1"; ValueType: string; ValueName: "URLInfoAbout"; ValueData: "{#MyAppURL}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1"; ValueType: dword; ValueName: "NoModify"; ValueData: "1"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\Microsoft\Windows\CurrentVersion\Uninstall\{#MyAppId}_is1"; ValueType: dword; ValueName: "NoRepair"; ValueData: "1"; Flags: uninsdeletekey

[Run]
; Install WebView2 Runtime if not present
Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "Installing Microsoft Edge WebView2 Runtime..."; Flags: waituntilterminated; Check: not IsWebView2Installed

; Launch application after installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

; Open README
Filename: "{app}\README.md"; Description: "View README file"; Flags: postinstall shellexec skipifsilent unchecked

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
// Check if WebView2 Runtime is already installed
function IsWebView2Installed: Boolean;
var
  RegKey: string;
begin
  RegKey := 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}';
  Result := RegKeyExists(HKLM, RegKey) or RegKeyExists(HKCU, RegKey);
end;
