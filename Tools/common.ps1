Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$script:Root=Split-Path -Parent $PSScriptRoot
$script:Game=Join-Path $script:Root 'Game'
function Get-Unity {
 $version=(Get-Content -LiteralPath (Join-Path $script:Game 'ProjectSettings\ProjectVersion.txt') | Select-String '^m_EditorVersion: ').ToString().Split(':')[1].Trim()
 $editor=Join-Path $env:ProgramFiles "Unity\Hub\Editor\$version\Editor\Unity.exe"
 if(!(Test-Path -LiteralPath $editor)){throw "Pinned Unity $version missing"}; return $editor
}
function Invoke-Unity([string]$Method,[string]$Log,[string]$Target='', [switch]$Release,[switch]$NoQuit,[string[]]$Extra=@()){
 New-Item -ItemType Directory -Path (Split-Path -Parent (Join-Path $script:Root "Logs/$Log")) -Force | Out-Null
 $lock=Join-Path $script:Game 'Temp\UnityLockfile'
 if(Test-Path -LiteralPath $lock){try{$h=[IO.File]::Open($lock,'Open','ReadWrite','None');$h.Dispose()}catch{throw 'Project already open. Use an isolated project copy or defer this batch; do not close the interactive editor.'}}
 $args=@('-batchmode','-nographics','-projectPath',('"'+$script:Game+'"'),'-logFile',('"'+(Join-Path $script:Root "Logs\$Log")+'"'))
 if(!$NoQuit){$args+='-quit'};if($Method){$args+=@('-executeMethod',$Method)};if($Target){$args+=@('-buildTarget',$Target)};if($Release){$args+='-cmRelease'}
 if($Extra.Count){$args+=$Extra}
 $p=Start-Process -FilePath (Get-Unity) -ArgumentList $args -WindowStyle Hidden -PassThru
 # Wait for this editor only; its Roslyn build server can outlive a completed batch.
 $p.WaitForExit()
 if($p.ExitCode -ne 0){throw "Unity failed ($($p.ExitCode)); see Logs/$Log"}
}
