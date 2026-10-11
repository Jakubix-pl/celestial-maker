param([switch]$After,[string]$Output='',[string]$Pages='')
. "$PSScriptRoot/common.ps1"
$lock=Join-Path $script:Game 'Temp/UnityLockfile'
if(Test-Path -LiteralPath $lock){try{$h=[IO.File]::Open($lock,'Open','ReadWrite','None');$h.Dispose()}catch{throw 'Project in use; do not close the user editor.'}}
$phase=if($After){'after'}else{'before'}
$arguments=@('-batchmode','-force-d3d11','-projectPath',('"'+$script:Game+'"'),'-logFile',('"'+(Join-Path $script:Root "Logs/ObservatoryPerformance/$phase-unity.log")+'"'),'-quit','-executeMethod','CelestialMaker.EditorTools.ObservatoryBatch.Benchmark','-buildTarget','Win64')
if($After){$arguments+='-cmAfter'}
if($Output){$outputPath=[IO.Path]::GetFullPath((Join-Path $script:Root $Output));if(!$outputPath.StartsWith($script:Root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Performance output must stay inside this repository'};$arguments+=@('-cmPerfOutput',('"'+$outputPath+'"'))}
if($Pages){$arguments+=@('-cmPerfPages',$Pages)}
$p=Start-Process -FilePath (Get-Unity) -ArgumentList $arguments -WindowStyle Hidden -PassThru
$p.Id | Set-Content Logs/ObservatoryPerformance/process.txt
$p.WaitForExit()
if($p.ExitCode -ne 0){throw "Benchmark failed ($($p.ExitCode))"}
