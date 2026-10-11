# Approachable purchases — 2.8.2

The patch makes the next purchase easier to understand while retaining the existing production chains, reset layers, engineering choices and optional automation. All game text remains English. Existing owned quantities, purchased IDs, currencies and save slots are retained; new definitions apply to future prices and the effects of retained upgrade IDs. No refund or reset is performed on loading.

## Costs

| Within one chapter | Previous base prices | New base prices |
| --- | --- | --- |
| Four buildings | 10, 50, 75, 100 | 10, 40, 100, 180 |
| Ten process upgrades | 50, 60, 70, 80, 90, 500, 600, 700, 800, 900 | 50, 75, 110, 160, 230, 330, 475, 685, 985, 1,420 |
| Five research ranks | 200, 200, 2,000, 2,000, 20,000 | 200, 600, 1,800, 5,400, 16,200 |

Building and upgrade bases multiply by 100 per chapter; research bases multiply by 10. Resource denominations differ between chapters. Existing geometric building ownership scaling is retained. Research's total chain cost changes from 24,400 to 24,200 in the first chapter, removing duplicate plateaus without broadly reducing the budget. Each process upgrade now targets a process in its own chapter rather than an unrelated future chapter. Purchased IDs retain their ownership, with their target following the corrected definition.

## Purchase feedback

Three-pixel bars sit inside the bottom of purchase buttons. Yellow measures current funding; green requires actual eligibility as well as funding. A full yellow research bar can still be waiting for its prerequisite. Purchased one-time items hide the bar. The existing labels and explicit missing-resource reasons remain, so color is supplementary.

Building bars show the next unit and use unreserved stock. Bulk purchases retain partial buying. Research uses the same raw stock that manual research actually spends. Prestige/Ascension/Genesis nodes and buyer packages use their respective currency. Construction shows the least-funded of its three resource costs and respects goal reserves. Expedition funding is hidden in flight and reward claiming remains available after return. Private universe purchases use that universe's balance.

Bars read the authoritative economy each rendered frame for visible rows. Fill changes do not rebuild layout, and the bar does not intercept input. Modal purchases continue updating while open; closing the dialog removes its temporary live bindings. Offline replay blocks purchasing until catch-up completes.

## A clearer everyday loop

Research offers one suggested prerequisite-ready discovery with a direct navigation action. Suggestions do not spend resources or override filters or choices. Research details, queue planning, comparisons and advanced tools remain available. Automation introduces the strategies in plain language: Energy for a first cosmos, Matter for supply chains, and Time for construction. Development Plans remain explicit, opt-in choices with reset/spending confirmation.

## Design references

The developer's [ISEPS listing](https://play.google.com/store/apps/details?hl=en_US&id=com.AppSociety.ISEPS) describes a relaxing particle simulation with progressively expanding upgrades and prestige layers. Our interpretation is to make repeated actions legible and let deeper systems unfold over time.

The official [Antimatter Dimensions automation view](https://github.com/IvarK/AntimatterDimensionsSourceCode/blob/master/src/components/tabs/autobuyers/AutobuyersTab.vue) presents unlock-dependent explanations and can consolidate individual autobuyers into a simpler view. Our application is to explain the three existing plans before the detailed controls and preserve progressive discovery. These are design references, not evidence of player preference or a guarantee of wider appeal. No code or artwork was copied.

Measurements, builds and acceptance gaps are recorded in [VALIDATION.md](VALIDATION.md).
