# Observatory 2.3.0 — final visual direction

Review date: 2026-10-11. Extends PLAN.md and the existing Observatory, preserving
the six primary sections and all gameplay routes. This is an implementation
review, not a user study or a claim that another game's UI caused its success.

## Comparisons and decisions

Official publisher screenshots retained in `Logs/UIPlanning/References` were
inspected again alongside current official product descriptions. The screenshots
can depict an older release. No competing game was launched or copied.

| Reference | Observed pattern / scope | Applied to Celestial Maker |
| --- | --- | --- |
| [Antimatter Dimensions](https://store.steampowered.com/app/1399720/Antimatter_Dimensions/) | Stable resource header, dense aligned production rows, purchases adjacent to quantities; layered progression and automation in the official description. | Keep the resource header and buy controls stable. Refresh live balances every frame while expensive affordability decisions have their own cadence. Preserve scientific notation and Core number precision. |
| [Melvor Idle](https://store.steampowered.com/app/1267910/Melvor_Idle/) | Persistent navigation, consistent activity cards, explicit locked/active states in its official screenshot. | Keep the six groups, show selected tabs clearly, retain readable disabled controls. Artwork sits in menu and overview rather than behind production numbers. |
| [Revolution Idle](https://store.steampowered.com/app/2763740/Revolution_Idle/) | Prominent progress numbers and orbital shapes in official promotional imagery. | Use an original orbital observatory and stellar emblem as the identity. Add short feedback transitions; avoid large continuously rotating graphics over the data. |
| [Factorio GUI development](https://www.factorio.com/blog/post/fff-246) | The developers describe consistent dialog flow, high-resolution UI source graphics, and choosing when an expensive preview updates. This is a historical design article. | Native-resolution geometry, larger source font atlases, downsample-only artwork, consistent existing reset confirmations, separate fast presentation and slower derived information. |

Performance choices also follow [Unity's UI guidance](https://create.unity3d.com/Unity-UI-optimization-tips):
split independent canvases and avoid making static layout do work for rapidly
changing data. [TMP font asset creation](https://learn.unity.com/tutorial/textmesh-pro-font-asset-creation-1)
documents how sampling size and the atlas affect source detail.
[Factorio's rendering investigation](https://www.factorio.com/blog/post/fff-281)
is a useful example of measuring invisible overdraw; our concrete change is
skipping the 3D scene hidden behind the opaque desktop Observatory.

## Applied visual system

- Night and Dawn retain established contrast pairs. Turquoise identifies primary
  actions and selected controls. Warnings keep their separate semantic color.
- Rajdhani HD gives headings and resource totals a stronger hierarchy; IBM Plex
  Sans HD remains the body/control face. Both use 96-point SDF sampling and
  2048-pixel atlases. Existing fonts and metadata are preserved as older assets.
- The canvas stays at native pixel scale (1). Layout responds to available width;
  text accessibility remains 80–200%. We do not shrink the entire interface to
  make a small display fit a 4K design. The 96pt source covers the maximum 96pt
  menu heading at 200%; smaller rendered type samples down from that source.
- Raster artwork keeps its aspect ratio and a hard native-pixel size ceiling.
  Large panels provide surrounding space instead of stretching the source.
- Page/dialog reveals last 140 ms; button state color transitions last 100 ms.
  Notifications use the same reveal. Reduced motion removes these transitions.
  There is no movement of the controls while the user is targeting them.
- Production balances and visible analytics update per rendered frame, with a
  default desktop cap of 60 and optional 120 FPS. Formatting can legitimately
  leave a displayed value unchanged when its next significant digit has not
  changed. No artificial production is invented to make a counter move.

## Original assets

Generated with the built-in `image_gen` tool through the imagegen skill.
Exact prompts: `IMAGE-PROMPTS.json`. Source PNGs are committed to the workspace
under `Game/Assets/_CelestialMaker/Resources/UI/PC/`:

- `ObservatoryNebula.png`: landscape overview art, 1672 × 941.
- `ObservatoryPortrait.png`: tall menu art, 1094 × 1437.
- `CelestialIcon.png`: transparent emblem, 1254 × 1254.

The requested high-resolution dimensions in the prompts were suggestions to
the generation tool; the list above records actual files. None was upsampled.
`Design/CelestialMaker.ico` packages the emblem at 16, 24, 32, 48, 64, 128 and
256 pixels. Unity Editor APIs assign the PNG to the default and Windows build
icons. The package includes the ICO and a shortcut-creation script.

Final verification and performance limitations are recorded separately in
`PERFORMANCE-RELEASE.md`; this design document does not certify player FPS.
