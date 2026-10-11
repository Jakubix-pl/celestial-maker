. "$PSScriptRoot\common.ps1"
Start-Process -FilePath (Get-Unity) -ArgumentList @('-projectPath',('"'+$script:Game+'"'))
