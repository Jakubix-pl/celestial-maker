# Observatory refinement — 2.8.1

Implemented and measured on 2026-10-11, Unity 6000.6.3f1. Scope: balance around 400 hours, UI performance and autobuyer onboarding. Public release artifacts are described in [validation](../Release/VALIDATION.md).

## Balance

The previous six-hour campaigns took 430.878 / 450.753 / 438.012 hours. Higher resets cleared paid buyers and left the opted-in plan rebuilding too slowly. The first and recovery Prestige now earn one point at log10 production 5.4, once per reset tier, while subsequent rewards retain log10 6.2 and their cube-root curve. Points are earned through lifetime production and an actual reset, then spent through the normal unlock path.

Reset seeds are 3 dynamos after Prestige, 12 after Ascension and 24 with Persistent seed. The first stage of each megastructure requires 14,400 base seconds; later stages require 24,400 × 2.06^level, modified by actual construction effects. Conserved matter retains its input discount without a primary-output penalty. The opted-in Matter plan uses Open dimensions while preparing Genesis and returns to Folded dimensions afterward. The harness preserves the plan's law decisions instead of overwriting them at visits.

| Six-hour literal offline policy | Completion |
| --- | --- |
| Energy | 417.011 h |
| Matter | 407.344 h |
| Time | 395.945 h |

All three replay every missing tick, including paid buying, goals, reset decisions and transfers. Save/reload occurs at each visit. Nine separately labeled event-stepped online policies complete in 376.544–417.011 hours. The 360–440-hour band is an acceptance target, not an exact player guarantee. Policies enable the optional Development Plan, select modules/roles and manually confirm first resets. No stock is injected into pacing campaigns. [Raw results](../Release/pacing.json).

## UI and introduction

Static orbital geometry is reused while dynamic bodies animate. Scrolling visibility is cached until the snapshot, content position or target geometry changes; existing offscreen-row tests exposed and resolved a missing geometry invalidation. Per-frame resource identity classification was retained because a trial cache broke immediate satellite navigation. Snapshot updates avoid duplicate presentation; a pinned goal uses a fixed-height status area. Counters still read authoritative state every frame.

The optional five-step lesson runs in its own Simulation and demonstrates actual point spending and paid automatic purchases. Supplied practice stock/reset prerequisites are explicitly labeled and isolated. Completion stores only a dismissal preference. It can be reopened; old saves need no new campaign. Core, Unity UI and final EXE tests verify campaign resources/points/ownership/resets remain unchanged.

## Performance

Same historical late-game fixture, two satellite economies, six purchased buyers, animated Overview, actual 60 Hz Core and uGUI/URP private RenderTextures. Each run has 22 cases and 5,280 frames after warm-up; RTX 4070 Ti / Ryzen 7 9800X3D. Final repeat ran after simulator and installer jobs finished. An intermediate run with a parallel simulator is retained and labeled exploratory.

| UI CPU p95, representative case | Before | Final after |
| --- | --- | --- |
| Production | 0.8917 ms | 0.6943 ms |
| Research-goal | 0.6566 ms | 1.0068 ms |
| Overview | 1.1498 ms | 0.7955 ms |

Whole-workload samples above 16.67 ms: before 6, after 3. Maximum samples: 26.765 / 23.080 ms. Production and Overview improve in this run; Research with a pinned goal is higher, so no universal timing improvement is claimed. Submission and synchronous GPU readback are measured separately. The readback itself adds host/GPU synchronization overhead. Unity Mono's allocation counter is unsupported: final values are -1, and old zero values must not be interpreted as zero allocations. Static-geometry reuse and idle/scroll cache behavior also have direct regression checks.

This workload cannot establish physical display FPS. Human playtesting, Windows DPI, other hardware and clean-machine installation remain open. [Before](../Release/performance-before.json), [final](../Release/performance.json), [exploratory](../Release/performance-exploratory.json).

## Verification boundaries

Core 297/297 and systems 63/63 also pass from a clean source copy excluding personal saves, backups, binaries and Library. Unity Core 20/20, desktop UI 1484/1484, phone geometry 432/432. Built and installed Windows EXE 108/108, installer lifecycle 32/32. Windows Release has zero errors. All 193 package entries match. All campaign saves/exports/backups are unchanged; 18 of 19 baseline files match, while Unity's Editor Analytics/values cache updated during private rendering. Screenshots are actual private UI renders, including all five lesson steps and 20 distinct motion phases. No desktop was controlled or captured and no visible game/editor was launched. Android and WebGL binaries are not refreshed.
