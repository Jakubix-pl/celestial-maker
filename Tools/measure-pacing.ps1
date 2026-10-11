param([string]$Tag='measured')
. "$PSScriptRoot/common.ps1"
$sdk=Join-Path (Split-Path (Get-Unity)) 'Data/DotNetSdk/dotnet.exe'
$env:DOTNET_CLI_HOME=Join-Path $script:Root 'Tools/.dotnet'
& $sdk run -c Release --project "$PSScriptRoot/Expansion/Expansion.csproj" -- $script:Root --pacing "--tag=$Tag" --verify-target
if($LASTEXITCODE -ne 0){throw 'A campaign did not finish within the 360-440 hour target band'}
