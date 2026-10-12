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

---

# Celestial Maker 2.8.3 — First Steps

English-only Windows x64 and Android ARM64.

- Adaptive optional tutorial: real purchases count, the production chain is explained, and the first Prestige is reviewed before confirmation.
- First-run consumers preserve 20% of incoming supply for purchases. Production continues, no resources are granted, and the buffer ends at first Prestige.
- Purchased and truly maxed items move behind pending items, including automatic purchases. Normal ownership milestones are not treated as caps.
- Existing saves, paid Prestige autobuyers, costs and full tick-by-tick offline replay remain supported.

Guided two-hour simulations reach first Prestige in 35 minutes at five-minute visits and 60 minutes at fifteen-minute visits. These are bounded manual policies, not human playtest results.

Literal six-hour offline campaign completion:

| Strategy | Completion |
| --- | --- |
| Energy | 416.936 h |
| Matter | 407.152 h |
| Time | 395.249 h |

Nine event-stepped policies: 370.554–416.936 hours. Long-term policies enable Development Plans, manually confirm first resets and choose engineering modules/universe roles.

Core 332/332, systems 63/63, Unity Core 20/20, desktop UI 1500/1500, phone UI 464/464; packaged/installed players 107/107 checks, installer 32, APK 34. Both builds have zero errors. Private renders and a host workload benchmark were inspected. Physical S26 Ultra installation, touch/lifecycle and display FPS remain unverified. No desktop control or visible game launch.

Use the Windows installer or extract the entire ZIP. Android: `CelestialMaker-2.8.3-Android-arm64.apk`, target API 37, minimum API 26, development signing for sideloading. Export your save before updating. SHA-256 files identify these exact packages.

[Changes and audit](FIRST-STEPS-2.8.3.md) · [Validation](VALIDATION.md) · [Android](../Android/STATUS.md)
