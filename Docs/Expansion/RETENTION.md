# Expansion save contract — schema 2

Version 2.1.0 keeps schema 2. The optional `campaignCompletedAt` field defaults
to zero in old saves. The permanent `campaign-complete` record and its timestamp
survive all resets; completion adds no production multiplier. Existing construction
work and paid resource costs remain; work exceeding the shorter 2.1.0 target
finishes on the next simulation step. No mandatory new game or save rewrite was
performed. See [current pacing and compatibility evidence](PACING400.md).

The existing `celestial-v1.json` filename and profile slots remain authoritative.
Schema version is now 2; changing the filename would hide existing progress.
Loading a v0/v1 native save first creates `.pre-expansion.bak` once. Normal
atomic `.tmp`/`.bak` writes remain. Export/import uses the same schema on all
platforms. No production save was written during this headless implementation.

| State | Prestige | Ascension | Genesis |
|---|---|---|---|
| Resources, lifetime within run, processes, pauses, ordinary upgrades/research, reserves | Reset | Reset | Reset |
| Active expeditions and their levels | Reset | Reset | Reset |
| Prestige currency and its four upgrades | Retain | Reset, including spent upgrades | Reset |
| Ascension currency, its tree and old talents | Retain | Retain | Reset |
| Genesis currency and foundations | Retain | Retain | Retain |
| Resonance Links | Retain | Reset | Reset |
| Advanced reset/expedition/research automation | Retain | Disabled/reset | Disabled/reset |
| Free basic autobuy and guided routing choice | Retain | Retain | Retain |
| Construction planner preference | Retain | Retain | Retain |
| Megastructures and committed work | Retain | Retain | Reset; Cosmic Memory preserves one completed level |
| Laws | Retain | Retain | Reset |
| Parallel stocks/processes/discoveries | Retain | Restart | Restart |
| Parallel identities/types | Retain | Retain | Retain |
| Transfer schedules | Retain | Disabled; explicitly re-enable | Disabled; explicitly re-enable |
| Historical earned currency/reset counts, discoveries, challenge records, stories, cosmetics, settings and saved profiles | Retain | Retain | Retain |

Reset is refused during an active challenge. Profiles are preserved plans; they
cannot bypass unlocks, points, link capacity or the ten-minute law cooldown.
The reset dialog uses `ResetRules` and `ResetRequirement` from the same Core as
the simulator. Its recovery multiplier is explicitly an estimate.

Legacy currency values, process IDs, purchases, talents, achievements and settings
are retained. Spent Prestige upgrades are reconstructed from the fixed old costs
1/4/16/64. Legacy Ascension talent costs use the frozen t0..t23 schedule 1..8 in
each branch. This reconstructed earned total is a lower bound on historical
earnings because old saves did not record previously discarded currencies.
Migration runs once and does not award an extra reset or refund talents twice.
Unknown future schemas or invalid numbers are rejected.

Previously cached themes/supporter unlock both cosmetic themes without an income
effect. Account hash, supporter status and counts of unused/activated boost
records are retained as inert history. The original PlayerPrefs entitlement cache
is untouched. Uncached server records stay in the original SQLite database;
the optional authenticated `/profile` projects cosmetics from paid historical
rows without mutating those rows. It excludes refunded rows. New games earn the
same themes at 8 and 24 discoveries. There is no purchase or boost activation.

Offline time has no duration cap. A saved cursor and aggregate report permit
bounded event batches and resumed loading. Imported stock is not lifetime
production. Exchanges send 5% of unreserved stock hourly with 20% loss, one
outgoing route per source; source/destination clocks settle before mutation.

Backup: `Backups/Expansion-20261011-083628`, including eleven actual legacy save
fixtures. Never restore a backup over a newer player save without user intent.
