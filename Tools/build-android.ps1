param([switch]$Release,[switch]$TestBundle,[switch]$Sideload)
if(@(@($Release,$TestBundle,$Sideload) | Where-Object {$_}).Count -gt 1){throw 'Choose one Android build profile.'}
. "$PSScriptRoot\common.ps1"
$android=Join-Path (Split-Path (Get-Unity)) 'Data\PlaybackEngines\AndroidPlayer'
$localSdk=Join-Path $script:Root 'Tools/AndroidSetup/SDK'
$sdk=if($env:CM_ANDROID_SDK){$env:CM_ANDROID_SDK}elseif(Test-Path (Join-Path $localSdk 'platform-tools/adb.exe')){$localSdk}else{Join-Path $android 'SDK'}
$localNdk=Join-Path $sdk 'ndk/27.2.12479018'
$ndk=if($env:CM_ANDROID_NDK){$env:CM_ANDROID_NDK}elseif(Test-Path (Join-Path $localNdk 'source.properties')){$localNdk}else{Join-Path $android 'NDK'}
$jdk=if($env:CM_ANDROID_JDK){$env:CM_ANDROID_JDK}elseif(Test-Path (Join-Path $script:Root 'Tools\AndroidSetup\OpenJDK\bin\java.exe')){Join-Path $script:Root 'Tools\AndroidSetup\OpenJDK'}else{Join-Path $android 'OpenJDK'}
if(!(Test-Path (Join-Path $sdk 'platform-tools\adb.exe')) -or !(Test-Path (Join-Path $ndk 'source.properties')) -or !(Test-Path (Join-Path $jdk 'bin\java.exe'))){throw 'BLOCKED: Android SDK/NDK/JDK incomplete. Review Google SDK/NDK license terms before installing missing dependencies. No APK/AAB was created.'}
$env:CM_ANDROID_SDK=$sdk;$env:CM_ANDROID_NDK=$ndk;$env:CM_ANDROID_JDK=$jdk
$extra=@();if($TestBundle){$extra=@('-cmTestAAB')};if($Sideload){$extra=@('-cmSideload')}
Invoke-Unity 'CelestialMaker.EditorTools.BuildTools.Android' 'build-android.log' 'android' -Release:$Release -Extra $extra
