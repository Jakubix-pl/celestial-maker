# Android 2.8.4 — touch repair

[Download APK](https://github.com/Jakubix-pl/celestial-maker/releases/download/v2.8.4/CelestialMaker-2.8.4-Android-arm64.apk) · [SHA-256](https://github.com/Jakubix-pl/celestial-maker/releases/download/v2.8.4/CelestialMaker-2.8.4-Android-SHA256.txt).

**IMPLEMENTED:** scene-owned UI input, focus/resume activation, stable pressed controls during automatic refresh, density-scaled finger tolerance, and resource-header touch handling. Includes the complete English 2.8.3 game, adaptive First steps and purchased/maxed-last lists. Economy and save schema are unchanged.

**COMPILED / TESTED:** 17/17 hidden Unity PlayMode tests (11 input-path tests, five threshold cases, one startup/save smoke test); all three targeted cases fail with their fixes removed. APK 34/34 checks pass. Unity 6000.6.3f1, JDK 17, NDK r27c, target/compile API 37, minimum API 26, ARM64, version code 28401. ELF and ZIP 16 KB alignment verified. No billing permission.

**BUILT:** 40,873,935-byte non-debuggable APK, zero build errors. Unity development signing for sideloading; certificate matches 2.8.3. All 178 compiled PNG resources match the preceding package. The repair classes are present in the APK's IL2CPP metadata.

**OBSERVED:** synthetic finger events pass through Unity's actual input module, raycasting, scrolling and game button callbacks. The reported total loss of in-game input on the physical S26 Ultra is not independently reproduced. Device installation, touch, rotation, pause/resume and FPS remain **UNVERIFIED**. No desktop control, capture or visible game/editor launch.

Install this APK over the matching existing installation; do not uninstall the game to update it. Older APKs are retained. Windows downloads remain 2.8.3. No new pacing or performance measurements were made.

[Repair and limitations](TOUCH-2.8.4.md) · [Passing tests](Touch-2.8.4/touch-tests.json) · [Controlled pre-fix failures](Touch-2.8.4/before-fixes.json) · [APK verification](Touch-2.8.4/apk-verification.json) · [Update continuity](Touch-2.8.4/update-continuity.json).

The earlier 2.8.3 geometry/action, core and simulation evidence remains in [the previous release audit](../Release/FIRST-STEPS-2.8.3.md). Those callback checks did not validate physical touch.
