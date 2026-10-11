# Release validation — 2.8.3

Evidence collected 2026-10-11. [First two hours and implementation](FIRST-STEPS-2.8.3.md). Earlier evidence remains in Git and versioned reports.

**IMPLEMENTED:** optional adaptive guide, genuine first-run supply buffer, purchased/maxed-last ordering and automatic completion refresh. English only; existing saves and owned IDs retained.

**COMPILED / TESTED:**

| Check | Passed | Scope |
| --- | --- | --- |
| Shared Core | 332/332 | Real economy, migration and starter rules; also clean source |
| Extended systems | 63/63 | Challenges/scenarios; also clean source |
| Unity Core | 20/20 | Integration/serialization |
| Desktop UI | 1500/1500 | Early/late geometry, themes, text scales, guide/actions, sorting |
| Phone UI | 464/464 | Portrait/landscape geometry, guide/actions, sorting |
| Packaged EXE | 107/107 | Hidden null-graphics runtime |
| Installed EXE | 107/107 | Isolated installed payload |
| Installer | 32/32 | Silent install/upgrade/uninstall and hashes |
| APK | 34/34 | Manifest, ARM64/16 KB, signature, icons, archive |

The optional Continue calculation action may disappear before a runtime sample when its catch-up finishes quickly. The literal campaigns independently replay all offline ticks and persist/reload at each visit.

**BUILT:** Windows Release, installer/ZIP and ARM64 IL2CPP APK, zero errors. Android version/code 2.8.3/28301, target API 37, minimum 26, non-debuggable development-signed sideload package. Certificate continuity with 2.8.2 is checked before delivery. No billing integration.

**OBSERVED:** private actual uGUI/TMP/URP renders from fresh first steps, a real two-hour guided checkpoint and a simulated late campaign. 36 phone logical renders. The guide advances through actual actions and supports hide/reopen. Pending research appears before purchased research. Twenty Overview motion phases are rendered. These are not phone captures or human usability observations.

Two-hour guided first Prestige: 35.0/60.0 minutes with five-/fifteen-minute visits; corresponding maximum pre-Prestige manual purchase intervals 5.0/15.0 minutes. The obsolete static policy remains a failing control, with missing Atomic Forge and no first Prestige.

| Literal six-hour policy | Campaign completion |
| --- | --- |
| Energy | 416.936 h |
| Matter | 407.152 h |
| Time | 395.249 h |

Nine event-stepped active/hourly/six-hour policies: 370.554–416.936 hours. Plans/modules/roles and first reset confirmations are specified policy choices. Six-hour profiles first Prestige at 12 h because purchases and confirmations await visits; this differs from the guided introductory profiles.

Earlier completed late-performance run (no further measurements after the user requested skipping them): 22 cases / 5,280 measured frames after warm-up. Worst workload p95 4.279 ms; worst UI p95 0.754 ms; 1 samples above 16.67 ms. Actual Core/UI plus private GPU submission/readback; this host result does not establish phone/display FPS. Allocation counters are unsupported where marked -1. [Raw measurements](performance.json).

Gameplay saves/exports/backups remain byte-identical. 18/19 protected files unchanged; 1 non-save Editor Analytics cache updates. Clean-source publication audit excludes credentials, personal saves, SDK/JDK, Library and binary output.

**UNVERIFIED:** physical S26 Ultra install/touch/lifecycle/FPS, physical Windows FPS/DPI, human usability and clean-machine installer behavior. No OS input, desktop capture, visible editor or visible game was used. No further performance measurements were started after the user requested skipping them.
