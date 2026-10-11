# Research applied to this expansion

Consulted 2026-10-11. These sources inform implementation choices; they do not
prove this game's balance or rendered performance.

- [Unity 6000.6 fixed updates](https://docs.unity.com/en-us/engine/6000.6/manual/scripting/managing-time-and-frame-rate/fixed-updates): simulation cadence and render cadence differ. The game uses a custom fixed 1/60 logical accumulator with bounded catch-up, keeping the shared analytical Core as the authority.
- [Unity time variation](https://docs.unity.com/en-us/engine/6000.0/manual/scripting/managing-time-and-frame-rate/time-handling-variations): Unity clamps delta time in long frames. The runtime measures monotonic elapsed time, retains its remainder and uses UTC absence on suspension instead of silently losing elapsed production.
- [Kongregate, idle mathematics I](https://www.kongregate.com/en/pages/the-math-of-idle-games-part-i): geometric costs and bulk affordability. Existing BreakInfinity geometric MaxBuy remains; comparisons at resource reserves now tolerate numerical rounding consistently.
- [Kongregate, prestige mathematics III](https://blog.kongregate.com/the-math-of-idle-games-part-iii/): prestige curves change replay incentives. Ascension counts spent as well as unspent Prestige earnings; free respec conserves that total. Three policies are measured against real production code rather than fitted spreadsheet formulas.

Measured failures produced concrete improvements: construction budgets prevent
autobuy from consuming all project inputs; automatic Prestige stops at the
Ascension earning requirement; forecasts show actual input-constrained rates;
named meta discoveries reward combining three law families and completing
inter-universe exchanges, without adding another paid currency or click loop.

Future visual review is explicitly manual and unperformed. No desktop control,
screenshots, visible player, Play Mode, browser automation or public deployment
was used in the expansion pass.
