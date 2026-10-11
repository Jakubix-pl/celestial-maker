param([string]$Compiler='')
. "$PSScriptRoot/common.ps1"
New-Item -ItemType Directory -Path (Join-Path $script:Root 'Logs/Discovery') -Force | Out-Null
& "$PSScriptRoot/package-observatory.ps1" -PrepareOnly
if(!$Compiler){$Compiler=Join-Path $PSScriptRoot '.tools/InnoSetup-6.7.3/ISCC.exe'}
if(!(Test-Path -LiteralPath $Compiler)){throw 'Inno Setup compiler missing. Pass -Compiler with the path to ISCC.exe. See Docs/BUILDING.md.'}
& $compiler ('/DProjectRoot='+$script:Root) (Join-Path $PSScriptRoot 'Installer/CelestialMaker.iss') *> (Join-Path $script:Root 'Logs/Discovery/installer-compile.log')
if($LASTEXITCODE -ne 0){throw 'Installer compilation failed; see Logs/Discovery/installer-compile.log'}
$installer=Join-Path $script:Root 'Builds/CelestialMaker-Setup-2.7.0.exe'
Get-Item -LiteralPath $installer | Select-Object FullName,Length
