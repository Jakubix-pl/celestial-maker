param([string]$Destination='')
. "$PSScriptRoot/common.ps1"
$release=Join-Path $script:Root 'Builds/Windows/Observatory-Release'
$exe=Join-Path $release 'CelestialMaker.exe'
$icon=Join-Path $release 'CelestialMaker.ico'
if(!(Test-Path -LiteralPath $exe) -or !(Test-Path -LiteralPath $icon)){throw 'Build and icon are required first'}
if(!$Destination){$Destination=Join-Path $script:Root 'Celestial Maker.lnk'}
$linkPath=[IO.Path]::GetFullPath($Destination)
$shell=New-Object -ComObject WScript.Shell
try {
 if(Test-Path -LiteralPath $linkPath){$existing=$shell.CreateShortcut($linkPath);if($existing.TargetPath -ne $exe){throw 'Existing shortcut points elsewhere; preserve it'};[Runtime.InteropServices.Marshal]::ReleaseComObject($existing)|Out-Null}
 $shortcut=$shell.CreateShortcut($linkPath);$shortcut.TargetPath=$exe;$shortcut.WorkingDirectory=$release;$shortcut.IconLocation="$icon,0";$shortcut.Description='Celestial Maker - Observatory';$shortcut.Save();[Runtime.InteropServices.Marshal]::ReleaseComObject($shortcut)|Out-Null
 $verify=$shell.CreateShortcut($linkPath);if($verify.TargetPath -ne $exe -or $verify.IconLocation -ne "$icon,0"){throw 'Shortcut verification failed'};[pscustomobject]@{path=$linkPath;target=$verify.TargetPath;icon=$verify.IconLocation};[Runtime.InteropServices.Marshal]::ReleaseComObject($verify)|Out-Null
}finally{[Runtime.InteropServices.Marshal]::ReleaseComObject($shell)|Out-Null}
