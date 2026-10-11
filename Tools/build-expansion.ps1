. "$PSScriptRoot\common.ps1"
Invoke-Unity -Method CelestialMaker.EditorTools.ExpansionBatch.Validate -Log Expansion/unity-tests.log -Target Win64
Invoke-Unity -Method CelestialMaker.EditorTools.PcUiAssets.Build -Log Expansion/unity-release.log -Target Win64 -Release -Extra @('-cmExpansion')
& "$PSScriptRoot\package-expansion.ps1"
