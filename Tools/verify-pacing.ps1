. "$PSScriptRoot/common.ps1"
function Read-Json([string]$Path){Get-Content -LiteralPath (Join-Path $script:Root $Path) -Raw | ConvertFrom-Json}
$rows=@(Read-Json 'Logs/Pacing400/final.json')
if($rows.Count -ne 6){throw 'Expected six balance policies'}
foreach($style in @('active','background')){foreach($branch in @('Energy','Matter','Time')){
 $row=@($rows | Where-Object {$_.style -eq $style -and $_.strategy -eq $branch})
 if($row.Count -ne 1 -or $row[0].completionHours -lt 360 -or $row[0].completionHours -gt 440){throw "Pacing target failed: $style $branch"}
 $save=Read-Json "Logs/Pacing400/final-$style-$branch-save.json"
 if($save.version -ne 2 -or $save.expansion.records -notcontains 'campaign-complete' -or $save.expansion.ascensionUpgrades.Count -ne 9 -or $save.expansion.genesisUpgrades.Count -ne 3 -or $save.expansion.universes.Count -ne 2 -or @($save.expansion.megas | Where-Object {$_.level -ne 6}).Count){throw "Incomplete campaign save: $style $branch"}
 if([Math]::Abs($save.expansion.campaignCompletedAt/3600-$row[0].completionHours) -gt 1e-7){throw 'Completion timestamp mismatch'}
}}
$offline=@(Read-Json 'Logs/Pacing400/offline-resume.json')[0]
$reference=$rows | Where-Object {$_.style -eq 'background' -and $_.strategy -eq 'Matter'}
if(!$offline.offline -or $offline.resumes -ne 18 -or [Math]::Abs($offline.completionHours-$reference.completionHours) -gt 1e-7){throw 'Offline/save-resume pacing changed'}
$core=Read-Json 'Logs/Expansion/tests.json';$unity=Read-Json 'Logs/Expansion/unity-headless-tests.json';$build=Read-Json 'Logs/Expansion/build-release.json'
if($core.fail -ne 0 -or $core.pass -lt 117 -or $unity.fail -ne 0 -or $unity.pass -lt 20 -or $build.result -ne 'Succeeded' -or $build.errors -ne 0){throw 'Compiler/test/build evidence failed'}
$settings=Get-Content -LiteralPath (Join-Path $script:Root 'Game/ProjectSettings/ProjectSettings.asset') -Raw
if($settings -notmatch 'bundleVersion: 2.1.0' -or $settings -notmatch 'runInBackground: 1'){throw 'Player settings not updated by Unity'}
$zipPath=Join-Path $script:Root 'Builds/CelestialMaker-Expansion-Windows.zip'
$archiveHash=(Get-FileHash -LiteralPath $zipPath).Hash
if($archiveHash -ne (Get-FileHash -LiteralPath (Join-Path $script:Root 'Website/dist-free/downloads/CelestialMaker-Expansion-Windows.zip')).Hash){throw 'Website download differs'}
$zip=[IO.Compression.ZipFile]::OpenRead($zipPath)
try {
 $dll=$zip.Entries | Where-Object {$_.FullName -eq 'CelestialMaker_Data/Managed/CelestialMaker.Runtime.dll'}
 if(!$dll){throw 'Runtime assembly missing from ZIP'}
 $stream=$dll.Open();$sha=[Security.Cryptography.SHA256]::Create()
 try{$packagedHash=([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-','')}finally{$sha.Dispose();$stream.Dispose()}
 if($packagedHash -ne (Get-FileHash -LiteralPath (Join-Path $script:Root 'Builds/Windows/Expansion-Release/CelestialMaker_Data/Managed/CelestialMaker.Runtime.dll')).Hash){throw 'Packaged assembly differs from built player'}
}finally{$zip.Dispose()}
$metadata=Read-Json 'Backups/Expansion-20261011-083628/backup.json'
$saveChecks=@(Get-ChildItem -LiteralPath $metadata.SaveSource -Filter '*.json' | ForEach-Object {
 $copy=Join-Path $script:Root ('Backups/Expansion-20261011-083628/SaveInputs/'+$_.Name)
 if(Test-Path -LiteralPath $copy){[pscustomobject]@{file=$_.Name;matchesPreChangeBackup=(Get-FileHash -LiteralPath $_.FullName).Hash -eq (Get-FileHash -LiteralPath $copy).Hash}}
})
if($saveChecks.Count -lt 5 -or @($saveChecks | Where-Object {!$_.matchesPreChangeBackup}).Count){throw 'User saves differ; inspect before delivery'}
$saveChecks | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:Root 'Logs/Pacing400/save-integrity.json')
$stats=$rows | Measure-Object completionHours -Minimum -Maximum -Average
$result=[pscustomobject]@{version='2.1.0';corePassed=$core.pass;coreFailed=$core.fail;unityPassed=$unity.pass;unityFailed=$unity.fail;build=$build.result;errors=$build.errors;campaigns=$rows.Count;minimumHours=$stats.Minimum;maximumHours=$stats.Maximum;meanHours=$stats.Average;offlineResumeCount=$offline.resumes;offlineTimingMatches=$true;packagedAssemblyMatches=$true;downloadMatchesRelease=$true;archiveSHA256=$archiveHash;legacyFilesUnchanged=$saveChecks.Count;visual='UNVERIFIED';playerLaunched=$false;utc=[DateTime]::UtcNow.ToString('o')}
$result | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:Root 'Logs/Pacing400/final-verification.json')
$result | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:Root 'Logs/Expansion/final-verification.json')
$files=@(& rg --files Game/Assets/_CelestialMaker/Scripts Game/Assets/_CelestialMaker/Editor Game/Assets/_CelestialMaker/Tests Tools/Expansion -g '*.cs' -g '*.csproj')+@('Game/Assets/_CelestialMaker/Resources/content.json','Game/ProjectSettings/ProjectSettings.asset')
$hashes=@($files | ForEach-Object {[pscustomobject]@{path=$_;sha256=(Get-FileHash -LiteralPath (Join-Path $script:Root $_)).Hash}})
$hashes | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:Root 'Logs/Pacing400/source-hashes.json')
$result
