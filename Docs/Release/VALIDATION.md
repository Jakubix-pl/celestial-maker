# Release validation — 2.7.0

Evidence collected on 2026-10-11. Status categories are deliberately separate.

## IMPLEMENTED

All eight Discovery feature groups are implemented in the existing game: engineering modules, ordered goals, isolated six-hour plan experiments, challenge constraints/readiness, permanent expedition routes, universe roles, safe lessons, and the chronicle with separate optional scenarios. The game and installer use English. Runtime sources and content match the verified Windows release; publication changes affect documentation and test/build tooling.

Portable fixture preparation removes the public test suite's dependency on local backups. Two UI callback tests now explicitly start from an unselected focus/module, so they do not assume a particular campaign's prior choices. No player save is included in these fixtures.

## COMPILED / TESTED

| Check | Result | Scope |
| --- | --- | --- |
| Portable shared-core suite | 250/250 | Passed from a clean source copy without Logs, Backups, Library or binaries. |
| Historical local legacy fixtures | 11/11 | Additional private migration inputs; intentionally excluded from public source. |
| Extended system audit | 63/63 | Repeated from the clean source copy: module tradeoffs, all challenges and nine fresh scenario/target combinations. |
| Unity Core | 20/20 | Shared economy integration. |
| Observatory UI | 1479/1479 | Repeated with committed fixtures: early/late, Night/Dawn, 100–200% text, HD through ultrawide, geometry and callbacks. |
| Unity framework | 17 EditMode + 1 PlayMode | Isolated save paths; scene transition, production, save/reload and catch-up. |
| Six-hour tick audit | 4/4 | 1,296,000 ticks; online/offline/reload agreement, 14 transfers and eight signals. |
| Windows player | 94/94 | Actual final EXE, null graphics and isolated profiles. |
| Installed player | 94/94 | Same tests executed from the installed directory. |
| Installer | 26/26 | Silent install, file hashes, upgrade, uninstall and preservation checks. |
| Optional backend | 1/1 | Local service without account keys; removed billing routes. |
| Optional website | Build passed | Static site only; no WebGL player or public service deployment. |

The 250 public Core checks plus the 11 private fixtures explain the historical 261 total. Missing private inputs are not counted as passes by the public suite.

## BUILT

Windows x64 Release 2.7.0, Unity 6000.6.3f1, zero build errors. The installer and portable ZIP are the exact tested artifacts, with [sizes and SHA-256 hashes](artifacts.json). The ZIP contains 193 entries verified against the packaged files. The installer is per-user and unsigned; it offers an optional desktop icon, does not launch automatically and preserves saves on uninstall.

## OBSERVED

Actual 60 Hz campaigns with strict six-hour visits and serialized reloads completed in:

| Strategy | Completion |
| --- | --- |
| Energy | 415.625284 hours |
| Matter | 371.800284 hours |
| Time | 407.082663 hours |

These scripted policies enable the Development Plan and make required first-reset decisions at visits. They are not measurements of human play. All nine optional scenario/target combinations completed; the Hungry cosmos scenario's upper tiers require upstream investment rather than the naive purchase policy. Legacy unlock-gap measurements still show long stretches and do not count the new module/route decisions; no claim is made that all pacing gaps are eliminated.

The offscreen benchmark used the actual Core, uGUI/TMP and URP camera into a private RenderTexture, including synchronous GPU readback. Hardware: NVIDIA GeForce RTX 4070 Ti and AMD Ryzen 7 9800X3D. Each of 22 late-game cases had 45 warm-up and 240 measured frames: **5,280 measured frames, zero over 16.67 ms**, worst case p95 **3.6681 ms**, maximum sample **15.5070 ms**. Cases include 1080p, QHD, 4K, satellite economy, forced automation decisions, large text and Discovery dialogs. [Raw measurements](performance.json).

Nine published screenshots were inspected as image files. They are actual UI renders from synthetic states, with no desktop capture. [Screenshot manifest](screenshots.json).

## UNVERIFIED / LIMITS

- Physical desktop FPS, Windows DPI behavior, touch hardware, human gameplay/usability and clean-machine installation remain unverified.
- Performance samples cover one PC and short repeatable workloads, not a long thermal or hardware compatibility test.
- Forecasts exclude future manual choices; reported blocked durations are samples.
- No Android or refreshed WebGL artifact is covered by this Windows report.
- No signing certificate, store review or external account/payment service is claimed.

No desktop input, desktop capture or visible game/editor was used. All 13 protected local files matched their pre-work hashes. Full local evidence remains under Logs; personal backups and logs containing local paths are not included in the public repository.
