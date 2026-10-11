. "$PSScriptRoot/common.ps1"
$directory=Join-Path $env:USERPROFILE 'AppData/LocalLow/CelestialMaker/Celestial Maker'
New-Item -ItemType Directory -Path (Join-Path $script:Root 'Logs/Observatory') -Force | Out-Null
$files=@(if(Test-Path -LiteralPath $directory){Get-ChildItem -LiteralPath $directory -File -Recurse | ForEach-Object {[pscustomobject]@{path=$_.FullName;sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}}})
ConvertTo-Json -InputObject $files | Set-Content -LiteralPath (Join-Path $script:Root 'Logs/Observatory/saves-before.json')
Write-Output ("Recorded hashes for "+$files.Count+" local files; no save was changed.")
