# Current balance: approximately 400 hours, version 2.1.0

[Current measured campaign results, formula changes and exact decision policy](PACING400.md). Six fresh-save policies complete the main campaign in 364.23–436.44 hours (mean 393.78). The current reproduction entrypoint is Tools/measure-pacing.ps1.

Everything below is the historical 2.0.0 balance, retained for comparison. Its two-check-ins-daily schedule and final-system-unlock endpoint differ from the new campaign finish. Historical formulas below are no longer active.

---

# Measured PC expansion balance — 2026-10-11

All policies execute the actual `Core/*.cs` linked into `Tools/Expansion`, not a
separate model. Fresh saves, normal buying and reset APIs; no balance run grants
resources, edits stage progress, uses paid effects, accelerates wall-clock state,
or skips input consumption. The three runs each cover 90 simulated days.

| Strategy | First Prestige, hours | First Ascension, days | First Genesis, days | Final systems unlocked, days | Distinct ordinary/research upgrades acquired |
|---|---:|---:|---:|---:|---:|
| Energy | 2.00 | 2.58 | 28.08 | 80.62 | 70/90 |
| Matter | 2.00 | 2.58 | 28.58 | 82.12 | 70/90 |
| Time | 2.00 | 2.58 | 31.08 | 84.62 | 90/90 |
| Design target | 1–2 | 2–3 | 21–35 | 60–90 | Not a timing target |

“Final systems unlocked” means Endless Horizon acquired and three-universe
capacity available. This exposes the final megastructure levels; it is **not**
100% collection, all level-six constructions completed or proof of player fun.
All three runs create three economies and complete at least level three of all
three megastructures. Repeatable progression continues through Genesis,
expedition levels, alternate strategies and the remaining project levels.

Policy: an active first two-hour session with a decision every 15 minutes,
then two check-ins daily. Total 188 check-ins. Between check-ins only the game's
own automation acts. At a visit the policy buys affordable research and Prestige
upgrades, invests in its named Ascension branch, sets its two-law combination,
enables construction planning, opens available universes, and chooses the highest
available reset. Basic guided autobuy is enabled. Automatic Prestige stops after
16 earned points; it does not loop forever while blocking Ascension. Time enables
the research automation it actually unlocks. This is a competent reproducible
policy, not a prediction for every player or every possible visit schedule.

Historical 2.0.0 formulas: Prestige cube-root scale above 1.5849e6 lifetime Energy;
Ascension floor(sqrt(earned Prestige / 16)) at Stars; Genesis requires Universes,
one completed stage of every mega and 26 earned Ascension Points. Its reward is
floor(cuberoot(earned Ascension / 26)) × (1 + completed Genesis resets). Earned
totals include spent currency and clear at the next higher reset. Foundation costs
are 1, 3 and 2; their effects and prerequisites differ.

Energy boosts primary supply, Matter reduces input consumption and strengthens
refined chains, Time completes research and expeditions with less intervention.
The observed policies reach similar late horizons while Time obtains more
research. Free respec and three stored profiles permit mixed strategies. These
results do not establish an exhaustive optimum or statistical balance across
all combinations.

Evidence: `Logs/Expansion/balance.json`, `balance-final.log`, the three final
`balance-*-save.json` files, and `Tools/Expansion/Program.cs`. Earlier failed
calibrations are retained in separate logs. A construction budget starvation
problem and a floating reserve-boundary loop were fixed before this final run.

Reproduce with Unity's bundled .NET SDK:

```powershell
. ./Tools/common.ps1
$sdk=Join-Path (Split-Path (Get-Unity)) 'Data/DotNetSdk/dotnet.exe'
$env:DOTNET_CLI_HOME=Join-Path $PWD 'Tools/.dotnet'
& $sdk run -c Release --project Tools/Expansion/Expansion.csproj -- $PWD --balance
```

Optional diagnostic flags: `--strategy=Energy`, `--days=6`, `--probe`. Short
diagnostic runs overwrite the main balance JSON; archive a final report first.
