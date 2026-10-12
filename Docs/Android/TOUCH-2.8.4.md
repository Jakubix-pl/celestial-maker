# Android 2.8.4 — touch repair

The player reported that the main menu responded but in-game buttons, the Menu
control and scrolling did not. This release repairs reproducible input defects
and makes scene transitions own their input actions. The complete failure on the
physical S26 Ultra has not been independently reproduced or verified as resolved.

## Implemented

- Each Observatory scene creates and disposes its own default UI action asset and
  references. It re-enables UI actions on focus/resume, preserving Unity's touch,
  mouse, pen and gamepad bindings. It does not poll and replace modules every frame.
- Automatic discovery, completed-list sorting and safe-area rebuilds wait until
  the pointer gesture, including its release frame, finishes. Live resource
  presentation continues. A pressed button can no longer be removed by those
  automatic updates before its click is dispatched.
- Phone drag tolerance scales with physical resolution and reported density.
  Typical values are 36 pixels at 1080 pixels wide and 48 at 1440; the old scene
  threshold was 10 pixels. Deliberate swipes still scroll and do not buy anything.
- The phone resource-meter canvas now has its own GraphicRaycaster, so tapping
  the header opens resource details.

Economy, costs, offline tick logic, purchased-last sorting and save schema retain
2.8.3 behavior. Windows downloads remain the previously published 2.8.3 packages.

## Compiled and observed on the host

Unity 6000.6.3f1: **17/17 PlayMode tests pass**. Eleven tests send synthetic finger
states through Touchscreen → InputSystemUIInputModule → EventSystem → raycast →
the real game button. Five test density/orientation threshold calculations. One
is the existing startup/save smoke test, which uses callback invocation.

The touch cases cover New Game, Continue after scene reload, Menu, Production,
Research, repeated purchases, unaffordable purchases, deliberate scroll gestures,
finger jitter, resource dialogs, long-press hints, mouse/pen coexistence, offline
completion, input resume and an unlock while a finger is held on Menu.

With the corresponding fixes temporarily removed, all three targeted regression
tests fail: the resource header is not hit, a small movement cancels a tap, and
an unlock destroys the pressed Menu button. Restoring the fixes passes the full
suite. The temporary changes were restored before building.

The fixture uses private saves and controlled input/focus settings in the hidden
Unity host. It does not focus a window, send OS input, capture a screen or launch
a visible game. Earlier layout/callback checks did not exercise this input path.
No pacing simulations or performance measurements were repeated for this repair.

[Passing tests](Touch-2.8.4/touch-tests.json) ·
[Controlled failures before fixes](Touch-2.8.4/before-fixes.json)

## APK and device acceptance

Build/signature/compatibility checks are recorded in [Android status](STATUS.md).
There was no ADB-connected device during this work. Physical Android input
delivery, S26 Ultra installation, rotation, pause/resume and display FPS remain
**UNVERIFIED**. Synthetic lifecycle tests are not a physical-device certificate.

The APK is an update for the existing matching sideload signing identity. Keep
the current installation and install the new APK over it to retain its app data.
