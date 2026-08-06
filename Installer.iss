[Setup]
AppName=Pierre Client
AppVersion=1.0.0
AppPublisher=Pierre
AppPublisherURL=https://github.com/aliisinne/PierreClient
DefaultDirName={autopf}\Pierre Client
DefaultGroupName=Pierre Client
AllowNoIcons=yes
OutputDir=Output
OutputBaseFilename=PierreClient_Setup_v1.0
SetupIconFile=PierreLauncher\Assets\icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "Publish\PierreLauncher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Start.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "Oyunu Başlat.bat"; DestDir: "{app}"; Flags: ignoreversion
Source: "Publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\Pierre Client"; Filename: "{app}\PierreLauncher.exe"; IconFilename: "{app}\PierreLauncher.exe"
Name: "{autodesktop}\Pierre Client"; Filename: "{app}\PierreLauncher.exe"; Tasks: desktopicon; IconFilename: "{app}\PierreLauncher.exe"

[Run]
Filename: "{app}\PierreLauncher.exe"; Description: "{cm:LaunchProgram,Pierre Client}"; Flags: nowait postinstall skipifsilent
