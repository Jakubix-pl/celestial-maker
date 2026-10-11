. "$PSScriptRoot\common.ps1"
$website=Join-Path $script:Root 'Website'
$node=Get-Command node -ErrorAction SilentlyContinue
if(!$node){throw 'Node.js 24 is required. Install/use the documented runtime, then retry.'}
if(!(Test-Path -LiteralPath (Join-Path $website 'dist-free\index.html'))){& $node.Source (Join-Path $website 'build.mjs');if($LASTEXITCODE){throw 'Website build failed'}}
& $node.Source (Join-Path $website 'server.mjs')
if($LASTEXITCODE){throw 'Website server could not start; inspect the address/port error above.'}
