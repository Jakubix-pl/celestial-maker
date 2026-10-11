#define GameVersion "2.8.1"
#ifndef ProjectRoot
  #error ProjectRoot is required
#endif
[Setup]
AppId={{58AB1501-432A-41EE-9954-114732B64301}
AppName=Celestial Maker
AppVersion={#GameVersion}
AppVerName=Celestial Maker {#GameVersion}
AppPublisher=Celestial Maker
DefaultDirName={localappdata}\Programs\Celestial Maker
DefaultGroupName=Celestial Maker
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputDir={#ProjectRoot}\Builds
OutputBaseFilename=CelestialMaker-Setup-{#GameVersion}
SetupIconFile={#ProjectRoot}\Design\CelestialMaker.ico
UninstallDisplayIcon={app}\CelestialMaker.exe
UninstallDisplayName=Celestial Maker
VersionInfoVersion={#GameVersion}.0
VersionInfoDescription=Celestial Maker Installer
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
DisableWelcomePage=no
DisableProgramGroupPage=no
AllowNoIcons=yes
ShowLanguageDialog=no
UsePreviousLanguage=no
CloseApplications=no
RestartApplications=no
Uninstallable=yes
InfoBeforeFile={#ProjectRoot}\Tools\Installer\Install-Notes.txt
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; Flags: unchecked
[Files]
Source: "{#ProjectRoot}\Builds\Windows\Observatory-Release\*"; DestDir: "{app}"; Excludes: "*BackUpThisFolder*"; Flags: ignoreversion recursesubdirs createallsubdirs
[Icons]
Name: "{group}\Celestial Maker"; Filename: "{app}\CelestialMaker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\CelestialMaker.ico"
Name: "{group}\Uninstall Celestial Maker"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Celestial Maker"; Filename: "{app}\CelestialMaker.exe"; WorkingDir: "{app}"; IconFilename: "{app}\CelestialMaker.ico"; Tasks: desktopicon
[Messages]
FinishedLabel=Celestial Maker is installed.%n%nStart the game from the Start menu or your installation folder. Your saved progress is kept separately and is preserved when uninstalling.
