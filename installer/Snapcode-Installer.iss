; Snapcode Windows Installer Script (Inno Setup)
; Generates a professional Windows Setup Wizard (.exe)

#define MyAppName "Snapcode"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "hkpc labs"
#define MyAppURL "https://github.com/hkpc-labs"
#define MyAppExeName "Snapcode.exe"
#define MyAppAssocName MyAppName + " Project"
#define MyAppAssocExt ".snapcode"

[Setup]
AppId={{E8B623F1-3D24-4B79-A1B9-45F4D32A9C01}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=..\releases
OutputBaseFilename=Snapcode-Setup-v{#MyAppVersion}
SetupIconFile=..\src\Assets\app.ico
UninstallDisplayIcon={app}\Assets\app.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
CloseApplications=yes
RestartApplications=no
VersionInfoVersion=0.1.0.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Snapcode Visual Flutter App Builder

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[InstallDelete]
Type: files; Name: "{userdesktop}\Snapcode.lnk"
Type: files; Name: "{userprograms}\Snapcode.lnk"

[Files]
Source: "..\src\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\app.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\app.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
