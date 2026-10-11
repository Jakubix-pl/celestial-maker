# Current delivery: Celestial Maker 2.1.0

The current Windows package uses the new approximately 400-hour campaign balance.
See [2.1.0 delivery, six measured policies and validation](PACING400.md): 364.23–436.44 hours, mean 393.78; 117/117 Core checks, 20/20 Unity checks, Windows Release with zero errors. Visual/player acceptance remains unverified.

The report below describes the historical 2.0.0 expansion. Its timings, test counts and package checksum are superseded. Original source, ZIP and logs are preserved under Backups/Pacing400-20261011-093557.

---

# Celestial Maker PC expansion — delivery, 2026-10-11

## IMPLEMENTED

The existing Unity project was expanded in place, with source/save backup at
`Backups/Expansion-20261011-083628`. No unrelated project was used. Changes remain
local and uncommitted; no remote Git, publication or production signing occurred.

- Prestige → Ascension → Genesis with explicit reset previews, spent-currency
  accounting, nine Ascension nodes across Energy/Matter/Time, three Genesis
  foundations, free respec, three JSON-importable strategy profiles and rate
  forecasts calculated by the real engine.
- Dyson Swarm, Neutron Forge and Reality Loom: three resource-chain construction
  costs each, staged work, distinct output/input/expedition/exchange/universe
  effects, three initial levels and six after Endless Horizon.
- Nine reality laws in three exclusive families, capacity limits and a ten-minute
  simulation-time switch cooldown; two named meta discoveries with one-time rewards.
- Home plus two parallel universes with isolated stocks/processes/laws,
  differentiated economy rules, comparison/production controls and bounded hourly
  exchanges. Transfers lose 20%, respect reserves and never award lifetime output.
- Free basic autobuy; guided supply routing, repeat Prestige, expedition return,
  construction planning and unlockable research automation. Analytics exposes
  gross/input/net rates, reserves, multiplier contributions, bottlenecks, estimated
  goals and bounded recent history. History samples are runtime data; permanent
  records and currency history survive saves.
- A 60 Hz logical accumulator independent of render cadence, monotonic foreground
  time, analytical catch-up with saved remainder and unlimited-duration offline
  processing. Production input depletion and event boundaries use the same Core.
- Existing processes, research, specializations, challenges, expeditions, stories,
  settings, tutorials, save export/import and platform source retained. The PC
  navigation includes the new systems and hides their locked tabs progressively.
- Billing/store/checkout/paid boost effects removed from runtime, package
  references, backend routes/dependencies and active website. Cosmetics are earned
  at 8/24 discoveries. Historical entitlement/transaction records remain inert
  and recoverable; optional OIDC infrastructure remains independent of gameplay.

See [retention/migration](RETENTION.md), [measured balance](BALANCE.md) and
[research applied](RESEARCH.md) for exact rules, limitations and source links.

## COMPILED

Pinned Unity **6000.6.3f1**, sequential `-batchmode -nographics` processes. Editor
lock checked before each run. No Play Mode. New C# imported and Windows player
compiled with **0 build errors**. Website TypeScript build also passed.

- **89/89 Core regression assertions**, including eleven real legacy save
  fixtures, migration idempotence, spent-point reconstruction, reset retention,
  respec conservation, nine laws, all three projects, online/offline agreement,
  slow-frame catch-up, parallel exchange/save-load equivalence, retained challenge
  and expedition behavior, preview immutability and invalid metadata rejection.
- **18/18 Unity headless checks**: fifteen existing NUnit test methods invoked
  reflectively plus expansion JSON round-trip, PC route/assets and billing-package
  checks. This is not a claim of Play Mode or interactive UI acceptance.
- **1/1 backend integration test**: zero-key startup, removed billing endpoints
  under GET and POST, authenticated cosmetic projection of a temporary historical
  database, refunded-row exclusion and unchanged historical rows.
- **1 website HTTP verification script passed**: active HTML, no billing code,
  current PC ZIP and explicit unavailability of a refreshed Web player. No browser.
- **Three 90-day balance runs** on real Core. Prestige 2 h, Ascension 2.58 d,
  Genesis 28.08–31.08 d, final system unlocks 80.62–84.62 d. The schedule and
  definition of final unlocks are documented; these are not universal promises.

Offline 1 h, 24 h, 7 d, 30 d and 365 d fixtures balance before + produced + granted
− spent − reset loss = after. These long-duration fixtures disable autobuy;
separate automation, transfer and 90-day policy tests cover active systems.
No arbitrary duration cap exists, but many automation events still require CPU
time and resumable batches. No claim that every 365-day configuration is instant.

Core CPU benchmark, 600 logical steps (10 simulated seconds), prepared fixtures:
early 4.62 ms / 72 KB allocated; middle 20.83 ms / 72 KB; late 16.92 ms / 96 KB.
These are single local samples, not frame percentiles, rendering benchmarks or
proof of 60 FPS. See machine-readable logs for exact measurements.

## BUILT

- Windows release: `Builds/Windows/Expansion-Release/CelestialMaker.exe`.
- Archive: `Builds/CelestialMaker-Expansion-Windows.zip`, 40,923,862 bytes,
  192 entries. Unity's “do not ship” backup folder excluded; required exe,
  UnityPlayer and runtime assembly checked. SHA-256 in `Logs/Expansion/artifact.json`.
- Launcher updated: `Play-CelestialMaker.cmd` (not executed by this task).
- Free website: `Website/dist-free`, rebuilt and HTTP-checked locally. Previous
  `Website/dist` and old Windows/Web packages remain historical. New Web/Android
  players were not built; the active site does not silently load the old store.

## OBSERVED / acceptance still open

Observed only headless numerical outcomes, compiler/build results, serialization
and local HTTP behavior. **No visible game, desktop control, screenshot, browser
automation or Play Mode was used. Rendered UI, focus/keyboard behavior, tooltip
placement, modal fit, actual FPS/GC in the player and long-session interaction are
UNVERIFIED.** Prior UI-overhaul captures describe the earlier edition only.

Deferred visual checklist: all new tabs at 1280×720/1920×1080, text scaling,
hover/disabled states, scrolling, reset and profile confirmations, parallel
economy controls, offline summary, minimize/resume and measured render-frame
percentiles. Run only after a later instruction allows a graphical test.

Evidence is under `Logs/Expansion`: tests, Unity headless report, build log/report,
backend/site tests, balance, fixture benchmarks, source hashes and artifact hash.
Reproduce regressions with `Tools/validate.ps1`; safe sequential Unity validation
and build with `Tools/build-expansion.ps1`.

