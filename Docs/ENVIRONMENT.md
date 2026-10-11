# Environment — expansion, 2026-10-11

Unity 6000.6.3f1 and its bundled .NET SDK 8.0.318 are used. The Unity project is `Game` only. Windows compilation/build and reflected headless editor tests use `-batchmode -nographics`. No Play Mode or visible player is started.

URP, Input System, uGUI/TMP and Test Framework remain pinned in Game/Packages. Unity Purchasing was removed from manifest, lock and runtime assembly references. Backend now depends on openid-client only; no Stripe or Google SDK is required. Website retains its pinned TypeScript compiler. Node 24 provides the built-in SQLite API.

Android module/JDK history and the deferred SDK/NDK boundary remain in ANDROID_ACCEPTANCE.md. No new Android or Web player is claimed. Previous environment/performance evidence is archived under Backups/Expansion-20261011-083628/Docs-before-free.

BreakInfinity.cs remains the existing MIT implementation with mantissa/exponent serialization, approximately 15 significant digits and validated exponent bounds. Current measurements are in Logs/Expansion; CPU-only simulation timings are not rendered FPS measurements.
