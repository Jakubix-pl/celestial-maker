# Build and test Celestial Maker

The Unity project is **Game**, pinned to **6000.6.3f1** in `Game/ProjectSettings/ProjectVersion.txt`. Run commands below from the repository root. Preserve `.meta` files. Generated assets and scenes are maintained through the project's Editor APIs.

## Portable core tests

Install the .NET 8 SDK, then run:

```powershell
dotnet build Tools/Expansion/Expansion.csproj -c Release
dotnet Tools/Expansion/bin/Release/net8.0/Expansion.dll "$PWD"
dotnet Tools/Expansion/bin/Release/net8.0/Expansion.dll "$PWD" --system-audit
```

The default suite has 318 checks. The separate system audit has 63. These compile the actual game Core sources, not a duplicate economy model. Output is written under `Logs`, which is ignored by Git. Synthetic fixtures in `Tools/Expansion/Fixtures` make these checks portable. The historical `--local-legacy` option additionally reads private task-local backup fixtures; those files are intentionally not published and that option is not required for the portable suite.

`Tools/CI/github-core.yml` contains a Linux GitHub Actions template. To enable hosted CI, place it at `.github/workflows/core.yml` using an account credential with workflow permission. The publication credential lacked that scope, so hosted execution is not claimed. Unity's bundled .NET SDK is also usable locally: `Editor/Data/DotNetSdk/dotnet.exe`.

## Windows release

Install and license the pinned Unity editor through Unity Hub, with Windows build support. Scripts locate it at `%ProgramFiles%/Unity/Hub/Editor/6000.6.3f1/Editor/Unity.exe`.

```powershell
./Tools/build-observatory.ps1
```

This runs the Core checks, Unity Core/UI validation, the PC asset/build pipeline, actual headless player checks and ZIP packaging. Unity processes run sequentially in hidden batch mode. Scripts refuse a locked project; use a separate checkout if an interactive editor is open. They do not close editors. `-SkipValidation` is available for already-validated source, but does not establish a new full validation result.

Outputs:

- `Builds/Windows/Observatory-Release/CelestialMaker.exe`
- `Builds/CelestialMaker-Observatory-Windows.zip`
- `Logs/Observatory/` for build/player/package evidence

The build captures a read-only save baseline before validation. Test fixtures and saves are isolated from production profiles. Do not copy synthetic fixtures over player saves.

## Installer

The release used **Inno Setup 6.7.3**. Its compiler is not vendored. Obtain Inno Setup from its official publisher, then pass its `ISCC.exe` path:

```powershell
./Tools/build-installer.ps1 -Compiler 'C:/Program Files (x86)/Inno Setup 6/ISCC.exe'
```

The default fallback path is `Tools/.tools/InnoSetup-6.7.3/ISCC.exe`. Build the Windows player first. The output is `Builds/CelestialMaker-Setup-2.8.2.exe`, with SHA-256 checksums in `Builds`.

The per-user installer is English, offers an optional desktop shortcut, preserves saves on removal, and does not automatically launch the game. Code signing is not configured. `Tools/test-installer.ps1` performs a silent install, upgrade, installed-player checks and uninstall; it refuses an existing normal installation and should be run in an isolated test environment.

## Offscreen UI evidence

```powershell
./Tools/render-observatory.ps1 -After -Publication -LateFixture Tools/Expansion/Fixtures/Presentation-2.8.2.json
./Tools/render-observatory.ps1 -After -DiscoveryOnly
./Tools/benchmark-observatory.ps1 -After
```

These require a working graphics device. They run hidden Unity batch sessions and render the actual uGUI/TMP hierarchy through URP into private RenderTextures. No desktop screenshot or visible player is used. The benchmark includes synchronous GPU readback overhead and must not be labeled as desktop/player FPS.

## Android ARM64 sideload

After explicitly accepting Google's SDK terms, install the project-local tools and build the single APK:

```powershell
./Tools/setup-android-sdk.ps1 -AcceptGoogleSdkLicense
./Tools/build-android.ps1 -Sideload
python Tools/verify-android-apk.py --apk Builds/Android/CelestialMaker-2.8.3-Android-arm64.apk --sdk Tools/AndroidSetup/SDK --jdk Tools/AndroidSetup/OpenJDK --output Logs/AndroidVerification
```

Use JDK 17, NDK r27c, SDK platform `platforms;android-37.0`, build tools 36.0.0 and the pinned editor. The setup script expects the pinned JDK under `Tools/AndroidSetup/OpenJDK` or `CM_ANDROID_JDK`. SDK/NDK paths can be overridden through `CM_ANDROID_SDK` / `CM_ANDROID_NDK`. The APK uses Unity's development signing identity, targets API 37 and contains ARM64 libraries only. The validation checks package/signature/16 KB compatibility without installing or launching it. Device testing remains separate. [Android results](Android/STATUS.md).

## Optional web source

The Windows game works without this service. With a recent Node version supporting TypeScript stripping:

```powershell
node --test Backend/test/*.test.ts
node Website/build.mjs
```

The website build can succeed without a WebGL export; that does not create a playable WebGL game. Account integrations require separately configured services. WebGL is not refreshed in 2.8.2. The Android addition has separate package evidence and open device tests described above.
