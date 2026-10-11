$ErrorActionPreference='Stop'
$exe=Join-Path $PSScriptRoot 'CelestialMaker.exe'
$icon=Join-Path $PSScriptRoot 'CelestialMaker.ico'
if(!(Test-Path -LiteralPath $exe) -or !(Test-Path -LiteralPath $icon)){throw 'Extract the full game package before creating a shortcut.'}
$destination=Join-Path ([Environment]::GetFolderPath('Desktop')) 'Celestial Maker.lnk'
$shell=New-Object -ComObject WScript.Shell
try{
 if(Test-Path -LiteralPath $destination){$old=$shell.CreateShortcut($destination);if($old.TargetPath -ne $exe){throw 'A Celestial Maker shortcut for another folder exists. Rename it first to preserve it.'};[Runtime.InteropServices.Marshal]::ReleaseComObject($old)|Out-Null}
 $link=$shell.CreateShortcut($destination);$link.TargetPath=$exe;$link.WorkingDirectory=$PSScriptRoot;$link.IconLocation="$icon,0";$link.Description='Celestial Maker - Observatory';$link.Save();[Runtime.InteropServices.Marshal]::ReleaseComObject($link)|Out-Null
 Write-Output "Created $destination"
}finally{[Runtime.InteropServices.Marshal]::ReleaseComObject($shell)|Out-Null}
