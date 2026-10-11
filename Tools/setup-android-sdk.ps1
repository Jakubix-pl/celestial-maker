param([switch]$AcceptGoogleSdkLicense)
# This switch must represent the user's explicit acceptance, not an inferred default.
if(!$AcceptGoogleSdkLicense){throw 'Review https://developer.android.com/studio/terms and explicitly accept before installing Android SDK/NDK.'}
. "$PSScriptRoot/common.ps1"
$setup=Join-Path $script:Root 'Tools/AndroidSetup'
$sdk=Join-Path $setup 'SDK'
$jdk=if($env:CM_ANDROID_JDK){$env:CM_ANDROID_JDK}else{Join-Path $setup 'OpenJDK'}
if(!(Test-Path -LiteralPath (Join-Path $jdk 'bin/java.exe'))){throw 'Prepare the pinned OpenJDK 17 first, or set CM_ANDROID_JDK.'}
$manifest=Get-Content -LiteralPath (Join-Path $script:Root 'Docs/ANDROID_COMPONENTS.json') -Raw | ConvertFrom-Json
$sdkModule=$manifest.subModules | Where-Object id -eq 'android-sdk-ndk-tools'
$cli=$sdkModule.subModules | Where-Object {$_.id -like '*cmdline*' -or $_.id -like '*command-line*'}
if(@($cli).Count -ne 1){throw 'Expected one official pinned Android command-line tools package.'}
if(([Uri]$cli.url).Host -ne 'dl.google.com'){throw 'Unexpected Android tools download host.'}
New-Item -ItemType Directory -Force -Path $setup,$sdk | Out-Null
$archive=Join-Path $setup 'command-line-tools.zip'
$destination=Join-Path $sdk 'cmdline-tools/16.0'
if(!(Test-Path -LiteralPath (Join-Path $destination 'bin/sdkmanager.bat'))){
 if(!(Test-Path -LiteralPath $archive)){Invoke-WebRequest -Uri $cli.url -OutFile $archive}
 if((Get-Item -LiteralPath $archive).Length -ne $cli.downloadSize.value){throw 'Command-line tools size differs from the official Unity manifest.'}
 if($cli.integrity -notlike 'sha384-*'){throw 'A SHA-384 package integrity value is required.'}
 $actualHex=(Get-FileHash -LiteralPath $archive -Algorithm SHA384).Hash
 $actualBytes=New-Object byte[] 48
 for($i=0;$i -lt 48;$i++){$actualBytes[$i]=[Convert]::ToByte($actualHex.Substring($i*2,2),16)}
 if([Convert]::ToBase64String($actualBytes) -ne $cli.integrity.Substring(7)){throw 'Android tools integrity mismatch.'}
 $unpack=Join-Path $setup 'command-line-tools-unpacked'
 if(Test-Path -LiteralPath $unpack){throw 'Existing extraction found; inspect it before retrying.'}
 Expand-Archive -LiteralPath $archive -DestinationPath $unpack
 New-Item -ItemType Directory -Path $destination -Force | Out-Null
 Get-ChildItem -LiteralPath (Join-Path $unpack 'cmdline-tools') | Copy-Item -Destination $destination -Recurse
}
$env:JAVA_HOME=$jdk
$manager=Join-Path $destination 'bin/sdkmanager.bat'
# Accept only license prompts required by the selected SDK/NDK packages.
# Use sdkmanager's real mechanism; never fabricate license hashes.
((1..20 | ForEach-Object {'y'}) -join "`n") | & $manager "--sdk_root=$sdk" 'platform-tools' 'platforms;android-37' 'build-tools;36.0.0' 'ndk;27.2.12479018' 'cmake;3.22.1'
if($LASTEXITCODE -ne 0){throw 'Android SDK package installation failed.'}
Write-Output "Android tools prepared at $sdk"
