. "$PSScriptRoot\common.ps1"
Invoke-Unity '' 'compile.log'
Invoke-Unity 'CelestialMaker.EditorTools.ProjectBootstrap.ImportTMP' 'import-tmp.log' -NoQuit
Invoke-Unity 'CelestialMaker.EditorTools.ProjectBootstrap.Run' 'bootstrap.log'
