. "$PSScriptRoot\common.ps1"
$sdk=Join-Path (Split-Path (Get-Unity)) 'Data\DotNetSdk\dotnet.exe'
$env:DOTNET_CLI_HOME=Join-Path $script:Root 'Tools\.dotnet'
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
& $sdk run -c Release --project "$PSScriptRoot\Expansion\Expansion.csproj" -- $script:Root | Tee-Object -FilePath (Join-Path $script:Root 'Logs\Expansion\tests.log')
if($LASTEXITCODE -ne 0){throw 'Simulator validation failed'}
