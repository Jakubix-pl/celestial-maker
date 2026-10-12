param([string]$Label='touch',[string]$Filter='TouchInputTests')
. "$PSScriptRoot/common.ps1"
if($Label -notmatch '^[a-zA-Z0-9_-]+$'){throw 'Invalid evidence label'}
$output=Join-Path $script:Root 'Logs/AndroidTouch'
New-Item -ItemType Directory -Path $output -Force | Out-Null
$lock=Join-Path $script:Game 'Temp/UnityLockfile'
if(Test-Path -LiteralPath $lock){try{$handle=[IO.File]::Open($lock,'Open','ReadWrite','None');$handle.Dispose()}catch{throw 'Unity project is already open; do not close the active editor.'}}
$arguments=@('-batchmode','-force-d3d11','-projectPath',('"'+$script:Game+'"'),'-buildTarget','Win64','-runTests','-testPlatform','PlayMode','-testFilter',$Filter,'-testResults',('"'+(Join-Path $output "$Label.xml")+'"'),'-logFile',('"'+(Join-Path $output "$Label.log")+'"'))
$editor=Start-Process -FilePath (Get-Unity) -ArgumentList $arguments -WindowStyle Hidden -PassThru
$editor.WaitForExit()
if($editor.ExitCode -ne 0){throw "Touch tests failed ($($editor.ExitCode)); see Logs/AndroidTouch/$Label.xml"}
[xml]$result=Get-Content -LiteralPath (Join-Path $output "$Label.xml") -Raw
if($result.'test-run'.result -ne 'Passed' -or [int]$result.'test-run'.total -eq 0){throw 'No passing test evidence'}
Write-Output "TOUCH TESTS PASS: $($result.'test-run'.passed)"
