; Optional installer script — only needed if you want a proper
; "FireBird Config Tool Setup.exe" instead of handing people the raw .exe.
;
; Requires Inno Setup (https://jrsoftware.org/isinfo.php).
; Build the app first:
;   dotnet publish "..\FireBird Config Tool.csproj" -c Release -o ..\release
; Then compile this script:
;   "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "FireBird Config Tool.iss"

#define MyAppName "FireBird Config Tool"
#define MyAppVersion "1.0.0"
#define MyAppExeName "FireBird Config Tool.exe"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=FireBirdConfigToolSetup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "..\release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
