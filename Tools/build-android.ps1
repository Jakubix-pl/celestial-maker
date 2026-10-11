param([switch]$Release,[switch]$TestBundle)
if($Release -and $TestBundle){throw 'Choose production Release or debug-signed TestBundle, not both.'}
. "$PSScriptRoot\common.ps1"
$android=Join-Path (Split-Path (Get-Unity)) 'Data\PlaybackEngines\AndroidPlayer'
$sdk=if($env:CM_ANDROID_SDK){$env:CM_ANDROID_SDK}else{Join-Path $android 'SDK'}
$ndk=if($env:CM_ANDROID_NDK){$env:CM_ANDROID_NDK}else{Join-Path $android 'NDK'}
$jdk=if($env:CM_ANDROID_JDK){$env:CM_ANDROID_JDK}elseif(Test-Path (Join-Path $script:Root 'Tools\AndroidSetup\OpenJDK\bin\java.exe')){Join-Path $script:Root 'Tools\AndroidSetup\OpenJDK'}else{Join-Path $android 'OpenJDK'}
if(!(Test-Path (Join-Path $sdk 'platform-tools\adb.exe')) -or !(Test-Path (Join-Path $ndk 'source.properties')) -or !(Test-Path (Join-Path $jdk 'bin\java.exe'))){throw 'BLOCKED: Android SDK/NDK/JDK incomplete. Review Google SDK/NDK license terms before installing missing dependencies. No APK/AAB was created.'}
$env:CM_ANDROID_SDK=$sdk;$env:CM_ANDROID_NDK=$ndk;$env:CM_ANDROID_JDK=$jdk
$extra=@();if($TestBundle){$extra=@('-cmTestAAB')}
Invoke-Unity 'CelestialMaker.EditorTools.BuildTools.Android' 'build-android.log' 'android' -Release:$Release -Extra $extra
