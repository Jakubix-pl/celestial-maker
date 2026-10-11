param([switch]$Release)
. "$PSScriptRoot\common.ps1"
Invoke-Unity 'CelestialMaker.EditorTools.BuildTools.Web' 'build-web.log' 'webgl' -Release:$Release
$target=Join-Path $script:Root 'Website\public\game'
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -Path (Join-Path $script:Root 'Builds\Web\*') -Destination $target -Recurse -Force
Set-Content -LiteralPath (Join-Path $target 'expansion-version.txt') -Value '2.0.0'
& node (Join-Path $script:Root 'Website\build.mjs');if($LASTEXITCODE -ne 0){throw 'Website build failed'}
