. "$PSScriptRoot\common.ps1"
$release=Join-Path $script:Root 'Builds/Windows/Expansion-Release'
if(!(Test-Path -LiteralPath (Join-Path $release 'CelestialMaker.exe'))){throw 'Build the expansion first'}
$parts=Get-ChildItem -LiteralPath $release | Where-Object { $_.Name -notlike '*BackUpThisFolder*' } | ForEach-Object { $_.FullName }
$zipPath=Join-Path $script:Root 'Builds/CelestialMaker-Expansion-Windows.zip'
Compress-Archive -LiteralPath $parts -DestinationPath $zipPath -CompressionLevel Optimal -Force
$zip=[IO.Compression.ZipFile]::OpenRead($zipPath)
try {
 $names=@($zip.Entries | ForEach-Object FullName)
 if(!($names -contains 'CelestialMaker.exe') -or !($names -contains 'UnityPlayer.dll') -or !($names | Where-Object { $_ -like '*Managed/CelestialMaker.Runtime.dll' })){throw 'ZIP required payload missing'}
 $count=$zip.Entries.Count
} finally {$zip.Dispose()}
[pscustomobject]@{path=$zipPath;bytes=(Get-Item -LiteralPath $zipPath).Length;sha256=(Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash;entries=$count;windowsBuild='Succeeded';playerLaunched=$false;visual='UNVERIFIED';utc=[DateTime]::UtcNow.ToString('o')} | ConvertTo-Json | Set-Content (Join-Path $script:Root 'Logs/Expansion/artifact.json')
