# Android acceptance boundary

Observed 2026-10-10: the exact existing Unity 6000.6.3f1 editor now has
Android Build Support. The official signed component installer completed
with exit 0; the editor was not reinstalled. OpenJDK 17.0.18+8 was prepared
in Tools/AndroidSetup/OpenJDK and its java executable was checked.

SDK and NDK remain absent. The official Unity release manifest explicitly
attaches a separate Google SDK/NDK agreement to that component:
"Please review and accept the license terms before downloading and installing
Android's SDK and NDK."
No SDK/NDK license was accepted, no license hashes were fabricated, and these
components were not downloaded behind that requirement. Consequently neither
APK nor AAB is claimed. This is an approval boundary, not a missing phone,
Google Play account or production keystore excuse for skipping a debug APK.

Review the [Google Android SDK terms](https://developer.android.com/studio/terms)
and the exact manifest in ANDROID_COMPONENTS.json. After explicit acceptance,
install the manifest's SDK tools 36.0.0, NDK r27c, command-line tools 16,
platform tools 36.0.0 and a supported platform (the manifest provides 34/36/37).
OpenJDK is 17.0.18+8. These are the existing editor's dependencies, not a new
Unity version. The [Unity dependency matrix](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/android/getting-started/sdksetup/supported-dependency-versions)
is the compatibility source.

Set CM_ANDROID_SDK / CM_ANDROID_NDK / CM_ANDROID_JDK to the actual tool roots
if not using editor-bundled locations. build-android.ps1 validates them before
starting Unity and configures AndroidExternalToolsSettings in the Android
editor process. Existing package ID com.celestialmaker.game is preserved.

- build-android.ps1: development ARM64 IL2CPP APK, debug signing.
- build-android.ps1 -TestBundle: development ARM64 IL2CPP AAB, debug signing,
  CelestialMaker-test.aab; no production keystore is required.
- build-android.ps1 -Release: production AAB requires all existing signing
  environment values. No production key is created or overwritten.

Once built, inspect ABI, manifest and debug signature using SDK tools before
calling an APK verified. A connected authorized device has not been observed;
physical touch/rotation/background/import/offline/performance remains UNVERIFIED.

The expansion removes Unity IAP and Google Play billing dependencies. No billing acceptance is required by the free game. Android compilation remains deferred.

Historical note (not a current process inspection): two earlier installer probes remain as protected administrator processes
(PIDs 30428 and 34860). Windows denied terminating them from this agent's
session. Their hidden idle wizards can be closed by the user with appropriate
rights; no unrelated application or installer was stopped.

User decision, 2026-10-10: mobile is deferred. SDK/NDK license acceptance was declined for now. Do not install SDK/NDK or build Android until newly authorized.

New request, 2026-10-11: prepare an installable APK for Galaxy S26 Ultra on
Android 17 / One UI 9. This reopens Android work. A specific SDK/NDK license
acceptance question is pending; the earlier deferral has not been silently
converted into license acceptance. Mobile UI and build-profile preparation
can proceed independently. Current results and remaining steps are in
Docs/Android/STATUS.md. No APK or device compatibility pass is claimed yet.
