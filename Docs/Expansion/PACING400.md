# Celestial Maker 2.1.0 — approximately 400 hours

Delivered locally on 2026-10-11. This replaces the previous 60–90-day pacing
target. It changes the existing game and preserves schema-2 saves.

## IMPLEMENTED

- Ascension now needs 8 earned Prestige Points, previously 16. Genesis needs
  12 earned Ascension Points, previously 26. Spent points still count; reset
  retention and refunds keep their existing contracts.
- Prestige evaluates the larger of lifetime Energy and 0.35 × lifetime Particles
  within the current run. It never adds both. This lets efficient Matter chains
  contribute without counting imported stock or counting the same flow twice.
  The first-Prestige threshold and cube-root reward scale are unchanged.
- Construction work is `6 hours × 2.06^currentLevel`, previously
  `6 hours × 3^currentLevel`. Resource costs and construction speed upgrades remain.
- Fixed the planner holding its queue on an already-building project. It now
  reserves inputs for another idle project, allowing all three to build together.
  Before Endless Horizon it automatically targets stage 1, sufficient for Genesis;
  afterwards it targets stage 6. Manual construction still allows stages 2–3 early.
- A main-campaign finish requires all 9 Ascension nodes, all 3 Genesis foundations,
  home plus 2 parallel universes, and stage 6 of every megastructure. The existing
  final-system unlock alone is insufficient. Completion and its actual simulation
  timestamp are recorded once and survive later resets and reloads. Challenges,
  records, old optional talents and repeatable endgame are outside this finish.
- Overview and Roadmap show these goals, progress and completion; automation help
  explains when to pause lower resets and finish construction. No 400-hour hard
  gate, forced victory timer, resource injection or simulation speed multiplier.
- Desktop players run while unfocused, with rendering capped at 5 FPS in the
  background. The economy retains the 60 Hz logical clock and analytical catch-up;
  foreground rendering uses the player's FPS setting. Real application suspension
  uses offline catch-up. Saving/quitting while suspended preserves the suspension
  timestamp; resuming resets the live clock to avoid counting that interval twice.

## OBSERVED — real game Core

Fresh saves, normal buying/reset APIs, no injected balance-test resources.
The simulator links the game's actual `Core/*.cs` files. Completion time below
comes from the in-game permanent record, not the next polling interval.

| Strategy | Frequent decisions: every 15 min | Background: decisions every hour |
|---|---:|---:|
| Energy | 382.64 h | 408.44 h |
| Matter | 367.25 h | 436.44 h |
| Time | 364.23 h | 403.68 h |
| Mean | 371.37 h | 416.19 h |

Overall mean **393.78 hours**, range **364.23–436.44 hours**. All six are within the
chosen ±10% acceptance band around 400 hours. Every run finishes with all 18
megastructure stages, all 12 tree nodes and three universes; each acquired all 90
ordinary/research upgrades over its history, although resets can remove them.

First Prestige: 2 hours in every policy. First Ascension: 9.25 hours for frequent
decisions or 17 hours for hourly decisions. First Genesis: 49–55.75 hours or
63–91 hours respectively. See `Logs/Pacing400/final.json` for all milestones.

Both schedules make decisions every 15 minutes during the first two hours. A
decision buys affordable research and Prestige upgrades, invests first in the
named Ascension branch and then in Genesis foundations, applies that branch's
laws, enables guided autobuy and construction planning, and creates available
universes. Automatic Prestige uses a reward threshold of 2 and stops at 8 earned
points. At 12 earned Ascension Points the policy pauses lower resets to complete
the Genesis prerequisites. Once Genesis foundations are complete, it buys the
remaining Ascension branches; after all trees are complete it stops resetting
and lets construction finish. Between visits only game automation acts.

This models a player making regular progression decisions while the economy
runs. It does not mean 400 hours of clicking or that an unattended fresh save
finishes without decisions. A different strategy, neglected resets or buying
optional talents first can change the result. Offline time still counts at the
same economic rate, with no duration cap; no additional offline penalty was added.
These measurements establish reproducible pacing, not playtest enjoyment or an
exhaustive optimum across all strategies.

An additional full Matter/background run used `BeginOffline`/`ProcessOffline`
between decisions and serialized/reloaded the save every 24 hours: **18 reloads**,
the same **436.4370815103946-hour** completion time. Unit fixtures separately
compare fixed 60 Hz, 5 background ticks/s and offline construction boundaries.

The old 80–85-day result used two check-ins daily and measured only final-system
unlocks. It is not the same finish or schedule. Exploratory baselines and failed
tuning runs remain separately labeled under `Logs/Pacing400`; they are not the
delivered settings. The current formulas are in `ProgressionBalance.cs`.

## COMPILED and BUILT

- **117/117 Core assertions**, including legacy fixtures, all reset contracts,
  production accounting, parallel economies, respecs, new reset boundaries,
  planner concurrency, strict completion requirements and permanent completion.
- **20/20 Unity headless checks**: 17 reflected NUnit test methods plus three
  JSON/asset/dependency checks. The two new Unity tests cover completion round-trip
  and a schema-2 save with no completion timestamp field.
- Unity **6000.6.3f1**, sequential `-batchmode -nographics`, Windows Release
  **2.1.0**, **0 errors**. New script metadata was generated by Unity import.
- Updated `Builds/Windows/Expansion-Release/CelestialMaker.exe` and
  `Builds/CelestialMaker-Expansion-Windows.zip` (40,925,715 bytes, 192 entries).
  The ZIP runtime assembly matches the built assembly. SHA-256:
  `9C6DD4E6D40F21AAC4E4A56EC8061350D8EE90D16411D5A3F7DFA04B1384401E`.
- Rebuilt the local website and verified its HTTP download route. Its downloadable
  ZIP is byte-identical to the release. No publication or new Web/Android player.

**No visible player, Play Mode, desktop control, screenshots or browser automation
were used.** Rendered layout, actual FPS, focus/minimize/resume behavior in the
Windows player and physical interaction remain **UNVERIFIED**. The background
clock change is implemented and compiled; numerical equivalence is observed in
Core tests, not in a launched graphical player.

## Saves and evidence

No forced restart or schema-number bump. `campaignCompletedAt` is an optional
schema-2 field; older saves default to 0 and gain a record only when requirements
are met. Existing invested points, resources and work remain. Work already beyond
the shorter construction threshold completes on the next simulation step.
All three previous expansion balance saves load without changing their state.
Five matching files in the user's native save directory are byte-identical to
their pre-expansion backups. No user save was written by this task.

Pre-change source, evidence and ZIP: `Backups/Pacing400-20261011-093557`.
Current evidence: `Logs/Pacing400/final-verification.json`, `final.json`, the six
final saves/reset histories, `offline-resume.json`, tests, source hashes and
artifact hash. Unity build logs remain in `Logs/Expansion`. All work is local and
uncommitted; unrelated projects, remote Git and production signing were untouched.

Reproduce from the repository root:

```powershell
./Tools/validate.ps1
./Tools/measure-pacing.ps1 -Tag measured
./Tools/build-expansion.ps1
node Website/build.mjs
node Tools/Expansion/web-test.mjs
./Tools/verify-pacing.ps1
```

`measure-pacing.ps1` writes a separate tagged report and fails if a policy misses
360–440 hours. `verify-pacing.ps1` validates the delivered `final` evidence and
current package. The runner also supports `--style=background`, `--strategy=Matter`
and `--offline-resume`. Unity scripts check the project lock; never close a user's
editor to run them. Restore neither source nor saves over newer work automatically.
