param([switch]$SkipValidation)
. "$PSScriptRoot/common.ps1"
New-Item -ItemType Directory -Path (Join-Path $script:Root 'Logs/Observatory') -Force | Out-Null
& "$PSScriptRoot/capture-save-baseline.ps1"
if (!$SkipValidation) {
 $sdk=Join-Path (Split-Path (Get-Unity)) 'Data/DotNetSdk/dotnet.exe'
 $env:DOTNET_CLI_HOME=Join-Path $script:Root 'Tools/.dotnet'
 & $sdk run -c Release --project "$PSScriptRoot/Expansion/Expansion.csproj" -- $script:Root | Out-File (Join-Path $script:Root 'Logs/Observatory/core-tests.log')
 if($LASTEXITCODE -ne 0){throw 'Core regression failed'}
 Copy-Item -LiteralPath (Join-Path $script:Root 'Logs/Expansion/tests.json') -Destination (Join-Path $script:Root 'Logs/Observatory/core-tests.json')
 Invoke-Unity -Method CelestialMaker.EditorTools.ExpansionBatch.Validate -Log Observatory/unity-core.log -Target Win64
 Copy-Item -LiteralPath (Join-Path $script:Root 'Logs/Expansion/unity-headless-tests.json') -Destination (Join-Path $script:Root 'Logs/Observatory/unity-core-tests.json')
 Invoke-Unity -Method CelestialMaker.EditorTools.ObservatoryBatch.Validate -Log Observatory/ui-full.log -Target Win64
}
Invoke-Unity -Method CelestialMaker.EditorTools.PcUiAssets.Build -Log Observatory/unity-release.log -Target Win64 -Release -Extra @('-cmObservatory')
& "$PSScriptRoot/test-observatory-player.ps1"
& "$PSScriptRoot/package-observatory.ps1"
