# Celestial Maker 2.8.4 — Android touch repair

English-only Android ARM64 hotfix. Windows downloads remain [2.8.3](https://github.com/Jakubix-pl/celestial-maker/releases/tag/v2.8.3).

- Keep pressed controls alive through automatic discovery, sorting and layout refresh.
- Give each scene its own UI input actions and restore them on focus/resume.
- Scale tap-versus-drag tolerance for dense phone screens; deliberate swipes still scroll.
- Enable touch on resource headers.
- Keep existing saves, the 2.8.3 economy, onboarding and completed-last ordering.

17/17 hidden Unity tests pass, including real synthetic touch dispatch to Menu, purchases, tabs and dialogs. Three targeted regressions fail with their fixes disabled. APK archive, signature, API/ABI and 16 KB alignment checks pass. Signing identity matches 2.8.3, so install this APK as an update over the current matching installation.

Physical S26 Ultra touch behavior still needs confirmation. No device was connected through ADB, and the complete device-reported input failure was not independently reproduced. No desktop control or visible game/editor was used. No fresh balance or FPS claims are made.

[Detailed evidence](../Android/TOUCH-2.8.4.md) · [APK verification](../Android/Touch-2.8.4/apk-verification.json).
