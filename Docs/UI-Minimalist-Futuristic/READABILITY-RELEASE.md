# Observatory 2.4.0 — readability and planning

User choices: subtle category accents plus lightly tinted surfaces; adaptive,
compact rows with expandable details; decision support first, navigation second,
planning third. The interface remains English. No desktop input, screen capture,
visible game window, or user editor control is used during this work.

## Implemented

- Six category colors and distinct geometric symbols, readable names, a tinted
  current section, stronger selected tabs and filters. Night and Dawn supported.
- Compact Research and Upgrades rows. Details expand on demand. Missing resources
  and prerequisites are explicit; positive current net rates give an approximate
  ETA. No ETA is promised for zero/negative rates or missing prerequisites.
- A production comparison evaluates the real engine with the purchase cost in a
  separate state. It compares actual net rates, not just the nominal multiplier.
  It becomes available when the purchase requirements are met. Nothing is bought
  by opening the preview. A purchase is always rechecked against the real state.
- Below 1100 pixels per text-scale unit, the navigation rail becomes a drawer and
  section selection/filters become dialogs. The contextual inspector already
  hides below its own width threshold. Font size never shrinks automatically.
- Search for screens, processes, research and upgrades; up to 30 results, with a
  refinement hint. Per-view research filters, searches, favorites and production
  filters are saved. Research and Upgrades keep independent filters.
- Ctrl+K opens search, Alt+1–6 switches categories, F1 opens the guide. Existing
  Tab/Shift+Tab and Escape remain available. Global navigation shortcuts do not
  run while typing or inside an open dialog.
- One pinned research/upgrade goal, associated with its economy, with an in-game
  notification when ready or complete. Notifications can be disabled in Settings.
  There are no OS notifications or external messages.
- A research plan for each economy. Missing prerequisites are added in order;
  removing a prerequisite removes its dependants. Moves preserve dependencies.
  Manual acquisition removes the item from the plan. Plans and UI choices survive
  saving/loading. Old saves get empty plans and neutral view preferences.
- Automatic queue execution is opt-in and requires the existing Temporal workshop
  unlock. It runs at simulation decision boundaries, online and offline, before
  ordinary autobuy. It respects reserves/construction funding and waits on the
  first blocked item. Existing general automatic research waits while this queue
  has work. Autobuy can still spend remaining stock; the queue is not a reservation.
- Resets pause automatic execution and retain pending intent, restoring missing
  prerequisites lost in that reset. Acquired items are not automatically requeued.

## Evidence

COMPILED: Core **144/144**, Unity Core
**20/20**, UI **1384/1384**.
The actual Windows EXE passed **45** null-graphics runtime checks,
including debounced search, favorite/goal/queue callbacks and save/reload.

BUILT: Windows Release **2.4.0**, **0 build errors**. ZIP:
`Builds/CelestialMaker-Observatory-Windows.zip`, 49,653,355 bytes,
193 archive entries hash-compared with the build folder.
SHA256: `2F5082D101C4F5FB2913E7A264FF4F42CAA389981B064E8D84A009FFE0418DAD`.
Existing desktop shortcut still targets this EXE and its custom ICO.

OBSERVED: **297 actual Unity URP renders**, including both themes,
early/late progression, 960/1280/1920/2560/3440-pixel-wide samples, 100/125/200%
text, dialogs, scrolling, expanded research, pinned goal, search, queue and compact
filters. PNG decoding, dimensions and hashes passed; those checks alone are not a
visual readability verdict. Direct inspection covered representative screens,
both themes, compact mode, expanded content and dialogs. The automated geometry
matrix also covers 150% text and all original routes.

Core queue tests compare a large offline span with one-second simulation steps
for both home and parallel economies, and test decision timing with every other
automation disabled. An initial audit found a missing queue decision boundary;
the final engine includes it. Queue defaults preserve the previous behavior.

**13 existing user files remain hash-identical.** Test saves are isolated.
Progression constants match the previous release. Runtime sources predate the
final successful build. Final evidence: `Logs/ObservatoryReadability/final-verification.json`.

### Offscreen performance

NVIDIA GeForce RTX 4070 Ti; AMD Ryzen 7 9800X3D 8-Core Processor. Late-game fixture, 45 warmup frames and
240 measured frames per case, actual Core 60 Hz + uGUI + URP into a private texture
with synchronous one-pixel readback. **16 cases / 3840 measured
frames; 0 frames exceeded 16.67 ms.** Frame times include GPU readback and are
not a measurement of desktop/player presentation. Allocation measurement remains
unavailable. UI counters still read authoritative values each rendered frame.

| Case | UI mean, ms | Complete frame p95, ms | p99, ms | Maximum, ms | >16.67 ms |
|---|---:|---:|---:|---:|---:|
| Production | 0.329 | 2.283 | 2.741 | 5.370 | 0 |
| Analytics | 0.122 | 1.759 | 2.555 | 2.682 | 0 |
| Automation | 0.244 | 2.132 | 5.962 | 7.101 | 0 |
| Parallel universes | 0.099 | 1.732 | 2.271 | 2.741 | 0 |
| Trees | 0.233 | 2.055 | 2.222 | 2.421 | 0 |
| Research | 0.297 | 2.061 | 2.350 | 2.499 | 0 |
| Production-QHD | 0.342 | 2.385 | 2.595 | 2.921 | 0 |
| Production-4K | 0.359 | 2.539 | 3.032 | 3.983 | 0 |
| Overview | 0.088 | 1.807 | 2.209 | 2.796 | 0 |
| Prestige | 0.107 | 1.708 | 2.008 | 4.183 | 0 |
| Megastructures | 0.101 | 1.766 | 1.997 | 2.989 | 0 |
| Satellite | 0.338 | 2.111 | 2.430 | 3.408 | 0 |
| Automation burst | 0.158 | 1.964 | 2.241 | 3.793 | 0 |
| Research-expanded | 0.314 | 2.310 | 2.671 | 3.233 | 0 |
| Research-goal | 0.321 | 2.158 | 2.557 | 2.668 | 0 |
| Search | 0.174 | 1.906 | 2.159 | 2.201 | 0 |

Research now shows more rows, states and actions. At the same late-game fixture,
UI CPU mean changed from 0.186
to 0.297 ms. This is a
measured cost of the additional visible information, within the sampled budget;
it is not described as a speedup. Short isolated samples cannot certify sustained
60 FPS. Earlier releases also recorded occasional readback spikes.

## Boundaries

The 400-hour progression constants are unchanged. The new opt-in queue introduces
a player strategy choice; campaign pacing has not been remeasured with that choice.
Offscreen rendering is evidence of Unity layout and pixels, not physical input,
Windows DPI behavior, or sustained frame pacing in a visible player.

Backup: `Backups/ObservatoryReadability-20261011-115344`.
Primary evidence: `Logs/ObservatoryReadability`, `Logs/Observatory`,
`Logs/ObservatoryVisual/after` and `Logs/ObservatoryPerformance/after.json`.
