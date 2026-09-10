# Elevator grab contact seam (#454, partial evidence)

This does **not** complete #454. It verifies collision publication and enemy
activation, not an input-driven quick-stop/landing trajectory through the runtime's
previous-frame contact handoff. Keep the issue open without awaiting validation.

## Accepted capture

`elevator-grab-contact-native-capture.zip` contains `elevator-grab-454-v2.csv`:
SHA-256 `34AD146CA59AD8C1E9A0FD4B37EE96A5CF92F151A1EE83020E3A2D018F4AA3DC`.
Independent recapture is identical. All 3,072 C# cases match native CPU results.
The discarded v1 fixture guessed spin radius incorrectly; v2 reads the actual
pose-definition radius from ROM. No production change was justified by v1.

```
SuperMetroid.DebugRunner --elevator-grab-comparison-audit "Super Metroid.smc" elevator-grab-454-v2.csv
```

The [technique reference](https://wiki.supermetroid.run/Elevator_Grab) describes
partial-overlap activation depending on the collision oscillator. The native
$94:9763 scan, $94:93CE pseudo-door reaction and $A3:952A/$9548 elevator AI are
executed, not replaced with a hand-coded activation formula. C# uses production
MoveVertical, RoomLevelData contact publication and RoomEnemySystem.StepFrame.

## Fixture and exact windows

Flat 16x32-block geometry, solid floor row16, one type-nine pad at column8
(pixels128..143). Green Brinstar's real $8F:9B00 door list, entry9, resolves the
cartridge elevator pseudo-door. No patched ROM bytes, SRAM or cheats are used.
The lone $D73F actor is at (136,256), initialized with each direction parameter.
Samus starts touching the floor and receives a one-pixel downward collision probe.

Cases cross both elevator directions, both NMI parities, both Samus facings,
standing/running/crouching/spinning, center X120..151, and no new direction,
correct new direction, or wrong new direction. Native pose radii are retained.
Capture columns include contact, collision carry, collision Y, activation status,
final pose and final X/Y after the actor pins Samus when activation succeeds.

For the ten-pixel-wide standing body:

- Even (left-to-right): contact for center X133..148.
- Odd (right-to-left): contact for center X124..139.
- Full overlap X133..139: both scans succeed.
- One-pixel overlap X124 or X148: only the scan starting on the pad succeeds.
- X123/X149: neither scan succeeds. Non-standing pose controls never arm the actor.
- A correct newly pressed direction activates a published contact; no new press
  or the opposite direction does not. Held-input timing is not inferred here.

The test does not enlarge the trigger rectangle or skip the first solid neighbor.
The hash and dimensions are enforced and the native-derived overlap windows have
explicit managed assertions. Release build and the complete comparison pass.

## Recreate and next step

Apply `native-elevator-grab-entrypoint.patch` in pinned upstream-sm with
`git apply --unidiff-zero`, build Release x64 and run:

```
sm.exe --diagnostic-elevator-grab "Super Metroid.smc" elevator-grab-454-v2.csv
```

Reverse the patch afterward. It dispatches before SDL, explicitly suppresses GUI
dialogs, and bounds CPU execution. The archive contains numeric diagnostics only.

Next: input-driven approach/quick-stop and landing cases that publish contact in
beta and consume it in the following enemy phase, including failed-edge retries.
Do not mark #454 ready based on this isolated seam alone. No PAL, renderer or
door-travel/camera claim is made here; #11 remains independent.

Pins: Japan/USA rev0 ROM SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`;
upstream-sm `578f90b3cc49557bb70060ad033bb90b8cf8ac50`;
disassembly `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
