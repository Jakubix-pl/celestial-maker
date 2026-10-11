param([switch]$Release)
. "$PSScriptRoot\common.ps1"
Invoke-Unity 'CelestialMaker.EditorTools.BuildTools.Windows' 'build-windows.log' 'win64' -Release:$Release
$variant=if($Release){'Release'}else{'Development'}
$files=Get-ChildItem -LiteralPath (Join-Path $script:Root "Builds\Windows\$variant") | Where-Object { $_.Name -notlike '*BackUpThisFolder*' } | Select-Object -ExpandProperty FullName
$archive=Join-Path $script:Root "Builds\CelestialMaker-Windows-$variant.zip"
Compress-Archive -LiteralPath $files -DestinationPath $archive -Force
if($Release){Copy-Item -LiteralPath $archive -Destination (Join-Path $script:Root 'Builds\CelestialMaker-Windows.zip') -Force}
