# PC usability follow-up — 11 October 2026

After the initial PC redesign passed its focused runtime review, a bounded research
pass selected improvements that fit the existing incremental game. These are UI
changes; production allocation, prices, stage thresholds and persistence rules
remain unchanged.

## Evidence and application

| Primary source | Useful principle | Implemented application |
|---|---|---|
| [Microsoft XAG 101: Text display](https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/101) | Adjustable readable text and preserved visual hierarchy | Display/body font pairing, existing 125% mode, content scrolling, text-overflow review at all three target resolutions. Rebuilding at a new scale starts content at its readable top. |
| [Microsoft XAG 112: UI navigation](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/112) | Predictable navigation and context | Persistent navigation, selected states, modal backdrop interception, Escape closes dialogs. Production diagnosis offers a direct route to the input's chapter. |
| [Nielsen Norman Group: Visibility of system status](https://www.nngroup.com/articles/visibility-system-status/) | Show relevant current state and feedback | Production cards distinguish running, paused, unowned and input-limited processes; reserve buttons show On/Off. Research shows a live count of ready discoveries. |
| [Nielsen Norman Group: Recognition rather than recall](https://www.nngroup.com/articles/recognition-and-recall/) | Keep needed choices and context visible | All / Not owned / Purchased research filters; next-horizon card shows the exact lifetime requirement and progress, including when enlarged text hides the optional inspector. |
| [Factorio Friday Facts 416: Fluids 2.0](https://www.factorio.com/blog/post/fff-416) | Production systems benefit from understandable flow and constraints | Design inference: explain actual versus installed output and protected input reserves in a production diagnosis. No Factorio simulation changes were copied. |

The diagnostics use the game's current cached allocation and multiplier. They
describe the state when the dialog is opened; production continues behind it.
Filters never remove game content: All research restores every discovered item.
The progress bar is a linear ratio of lifetime production to the existing stage
threshold; it does not estimate time to completion.

## Follow-up acceptance

The opt-in Development Player review exercises research filters, ScrollRect wheel
events, a deliberately reserve-constrained process, its healthy state after the
constraint is removed, diagnosis navigation, modal raycast/callback blocking,
export/import round trip, scale rebuilding and next-goal presence at 125%.
The final per-resolution results and unaltered Unity screenshots are under
`Logs/UIOverhaul/{1920x1080,2560x1440,3440x1440}`.

## Remaining optional polish

- Dedicated illustrations and icons for each later chapter, beyond the shared
  cosmic panorama and six resource accent identities.
- A complete keyboard focus/auto-scroll accessibility pass; this iteration
  verifies mouse-oriented PC flows and Escape, not full controller navigation.
- User playtesting for visual preference and long-session comfort. Automated
  overflow checks do not establish accessibility conformance.

These are future polish opportunities, not missing core screens. Payment-provider
and account end-to-end flows need their separate configured environments.
