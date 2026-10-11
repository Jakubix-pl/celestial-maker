param([switch]$Release)
$failed=@()
foreach($platform in @('windows','web','android')){try{& "$PSScriptRoot\build-$platform.ps1" -Release:$Release}catch{$failed+=$platform;Write-Warning "$platform : $_"}}
if($failed.Count -gt 0){throw "Build-all incomplete: $($failed -join ', ')"}
