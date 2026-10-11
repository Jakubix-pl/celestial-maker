param([switch]$Probe,[switch]$After,[switch]$ReadabilityOnly,[switch]$TabsOnly,[switch]$ResourcesOnly,[switch]$UnlocksOnly,[switch]$EvolutionOnly,[switch]$CadenceOnly,[switch]$DiscoveryOnly,[switch]$Publication,[switch]$Phone,[string]$LateFixture='')
. "$PSScriptRoot/common.ps1"
$lockPath=Join-Path $script:Game 'Temp/UnityLockfile'
if(Test-Path -LiteralPath $lockPath){try{$handle=[IO.File]::Open($lockPath,'Open','ReadWrite','None');$handle.Dispose()}catch{throw 'Project open: use an isolated copy; do not close an editor.'}}
$reviewDir=Join-Path $script:Root 'Logs/ObservatoryVisual'
New-Item -ItemType Directory -Force -Path $reviewDir | Out-Null
$phase=if($After){'after'}else{'before'}
$arguments=@('-batchmode','-force-d3d11','-projectPath',('"'+$script:Game+'"'),'-logFile',('"'+(Join-Path $reviewDir "$phase-unity.log")+'"'),'-quit','-executeMethod','CelestialMaker.EditorTools.ObservatoryBatch.RenderReview','-buildTarget','Win64')
if($ReadabilityOnly){$arguments+="-cmReadabilityOnly"}
if($TabsOnly){$arguments+="-cmTabsOnly"}
if($ResourcesOnly){$arguments+="-cmResourcesOnly"}
if($EvolutionOnly){$arguments+="-cmEvolutionOnly"}
if($DiscoveryOnly){$arguments+="-cmDiscoveryOnly"}
if($Publication){$arguments+='-cmPublication'}
if($Phone){$arguments+='-cmPhoneUi'}
if($CadenceOnly){$arguments+="-cmCadenceOnly"}
if($UnlocksOnly){$arguments+="-cmUnlocksOnly"}
if($Probe){$arguments+='-cmRenderProbe'}
if($After){$arguments+='-cmAfter'}
if($LateFixture){$fixturePath=[IO.Path]::GetFullPath((Join-Path $script:Root $LateFixture));if(!$fixturePath.StartsWith($script:Root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Render fixture must stay in this repository'};if(!(Test-Path -LiteralPath $fixturePath)){throw 'Render fixture missing'};$arguments+=@('-cmLateFixture',('"'+$fixturePath+'"'))}
$process=Start-Process -FilePath (Get-Unity) -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.Id | Set-Content -LiteralPath (Join-Path $reviewDir 'render-process.txt')
$process.WaitForExit()
if($process.ExitCode -ne 0){throw "Offscreen Unity render failed ($($process.ExitCode)); inspect $phase-unity.log"}
