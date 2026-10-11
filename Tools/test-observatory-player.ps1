. "$PSScriptRoot/common.ps1"
$exe=Join-Path $script:Root 'Builds/Windows/Observatory-Release/CelestialMaker.exe'
if(!(Test-Path -LiteralPath $exe)){throw 'Build Observatory Release first'}
$output=Join-Path $script:Root ('Logs/Observatory/Player-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $output -Force | Out-Null
$log=Join-Path $output 'player.log'
$arguments=@('-batchmode','-nographics','-screen-width','1920','-screen-height','1080','-cmObservatoryCheck','-cmArtifacts',('"'+$output+'"'),'-logFile',('"'+$log+'"'))
$process=Start-Process -FilePath $exe -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if($process.ExitCode -ne 0){throw "Headless release check failed: $($process.ExitCode). See $log"}
$result=Get-Content -LiteralPath (Join-Path $output 'result.json') -Raw | ConvertFrom-Json
if(!$result.pass -or @($result.errors).Count){throw 'Headless result did not pass'}
Copy-Item -LiteralPath (Join-Path $output 'result.json') -Destination (Join-Path $script:Root 'Logs/Observatory/player-headless.json')
[pscustomobject]@{output=$output;exitCode=$process.ExitCode;passed=$result.checks.Count;version=$result.version;graphicsEnabled=$false} | ConvertTo-Json
