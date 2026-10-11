. "$PSScriptRoot/common.ps1"
$installer=Join-Path $script:Root 'Builds/CelestialMaker-Setup-2.8.0.exe'
$source=Join-Path $script:Root 'Builds/Windows/Observatory-Release'
$rootOutput=Join-Path $script:Root ('Logs/Discovery/Installer-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
$destination=[IO.Path]::GetFullPath((Join-Path $rootOutput 'Installed Game'))
if(!$destination.StartsWith([IO.Path]::GetFullPath($script:Root)+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Installer test destination is outside workspace'}
$registration='HKCU:/Software/Microsoft/Windows/CurrentVersion/Uninstall/{58AB1501-432A-41EE-9954-114732B64301}_is1'
if(Test-Path -LiteralPath $registration){throw 'An existing Celestial Maker installer registration must not be changed by this test'}
New-Item -ItemType Directory -Path $rootOutput -Force | Out-Null
$checks=[Collections.Generic.List[string]]::new()
function Assert-Installer($condition,[string]$name){if(!$condition){throw $name};$checks.Add($name)}
function Install-Test([string]$phase){
 $arguments=@('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/NOCLOSEAPPLICATIONS','/NORESTARTAPPLICATIONS','/SP-','/NOICONS','/TASKS=""','/LANG=english',('/DIR="'+$destination+'"'),('/LOG="'+(Join-Path $rootOutput ($phase+'.log'))+'"'))
 $process=Start-Process -FilePath $installer -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
 Assert-Installer ($process.ExitCode -eq 0) ($phase+' exits successfully')
}
function Verify-Payload([string]$phase){
 $count=0
 foreach($file in Get-ChildItem -LiteralPath $source -Recurse -File | Where-Object {$_.FullName -notlike '*BackUpThisFolder*'}){
  $relative=$file.FullName.Substring($source.Length+1);$installed=Join-Path $destination $relative
  if(!(Test-Path -LiteralPath $installed) -or (Get-FileHash -LiteralPath $installed).Hash -ne (Get-FileHash -LiteralPath $file.FullName).Hash){throw "Installed file differs: $relative"};$count++
 }
 $checks.Add($phase+' payload hash matches all '+$count+' files')
}
try {
 Install-Test 'install';Verify-Payload 'install'
 $entry=Get-ItemProperty -LiteralPath $registration
 Assert-Installer ($entry.DisplayVersion -eq '2.8.0' -and $entry.DisplayName -eq 'Celestial Maker') 'Windows uninstall registration has the correct version'
 Assert-Installer ($entry.InstallLocation.TrimEnd('\') -eq $destination) 'Windows uninstall registration points only to the isolated test folder'
 Assert-Installer (Test-Path -LiteralPath (Join-Path $destination 'CelestialMaker.ico')) 'Game icon is installed'
 $sentinel=Join-Path $destination 'user-notes.txt';Set-Content -LiteralPath $sentinel -Value 'Untracked user files must survive upgrades and uninstall.'
 $sentinelHash=(Get-FileHash -LiteralPath $sentinel).Hash
 Install-Test 'upgrade';Verify-Payload 'upgrade'
 Assert-Installer ((Get-FileHash -LiteralPath $sentinel).Hash -eq $sentinelHash) 'Upgrade preserves an untracked user file'
 $playerArtifacts=Join-Path $rootOutput 'Player';New-Item -ItemType Directory -Path $playerArtifacts | Out-Null
 $arguments=@('-batchmode','-nographics','-cmObservatoryCheck','-cmArtifacts',('"'+$playerArtifacts+'"'),'-logFile',('"'+(Join-Path $playerArtifacts 'player.log')+'"'))
 $player=Start-Process -FilePath (Join-Path $destination 'CelestialMaker.exe') -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
 Assert-Installer ($player.ExitCode -eq 0) 'Installed game exits its headless regression successfully'
 $result=Get-Content -LiteralPath (Join-Path $playerArtifacts 'result.json') -Raw | ConvertFrom-Json
 Assert-Installer ($result.pass -and $result.version -eq '2.8.0' -and @($result.errors).Count -eq 0) ('Installed game passes '+$result.checks.Count+' runtime checks')
} finally {
 $uninstaller=Join-Path $destination 'unins000.exe'
 if(Test-Path -LiteralPath $uninstaller){
  $entry=Get-ItemProperty -LiteralPath $registration
  if($entry.InstallLocation.TrimEnd('\') -ne $destination){throw 'Uninstaller registration differs from verified test folder'}
  $process=Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',('/LOG="'+(Join-Path $rootOutput 'uninstall.log')+'"')) -WindowStyle Hidden -PassThru -Wait
  Assert-Installer ($process.ExitCode -eq 0) 'Uninstaller exits successfully'
 }
}
Assert-Installer (!(Test-Path -LiteralPath $registration)) 'Test registration removed'
Assert-Installer (!(Test-Path -LiteralPath (Join-Path $destination 'CelestialMaker.exe'))) 'Installed game removed'
Assert-Installer ((Get-FileHash -LiteralPath $sentinel).Hash -eq $sentinelHash) 'Uninstaller preserves an untracked user file'
$before=Get-Content -LiteralPath (Join-Path $script:Root 'Logs/Observatory/saves-before.json') -Raw | ConvertFrom-Json
foreach($save in $before | Where-Object {$_.path -notlike '*TestResults.xml'}){Assert-Installer ((Get-FileHash -LiteralPath $save.path).Hash -eq $save.sha256) ('Protected file unchanged: '+[IO.Path]::GetFileName($save.path))}
[pscustomobject]@{pass=$true;checks=$checks.ToArray();output=$rootOutput;installer=$installer;sha256=(Get-FileHash -LiteralPath $installer).Hash;bytes=(Get-Item -LiteralPath $installer).Length;englishOnly=$true;signed=$false;visibleWindows=$false;utc=[DateTime]::UtcNow.ToString('o')} | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $script:Root 'Logs/Discovery/installer-tests.json')
Get-Content -LiteralPath (Join-Path $script:Root 'Logs/Discovery/installer-tests.json')
