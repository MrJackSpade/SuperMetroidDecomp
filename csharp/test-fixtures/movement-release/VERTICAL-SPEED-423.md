# Vertical speed, jump height, and medium transitions (#423)

## Result and scope

The pinned Japan/USA revision-zero cartridge and the C# fixed-point vertical
movement agree across the complete matrix below. The new original-CPU capture
contains 74 deterministic cases and 20,010 compared frames with zero mismatches.
No production movement correction was required.

The ROM identity is SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
The source cross-checks are the pinned disassembly's `$90:90C4`, `$90:90E2`,
`$90:98BC`, `$90:9949`, `$90:9A2C`, and `$90:9C5B` routines. The wiki description
was treated as a test lead; all numeric expectations came from running those
original cartridge instructions.

This project targets the Japan/USA revision. PAL acceleration tables are
deliberately not claimed or compiled into the runtime; a future PAL-ROM target
would need its own capture and region-selected definition set rather than
silently reusing these NTSC values.

## Original-CPU matrix

The native probe constructs an empty 16-by-255-block room with Samus at
`(128, 2048)`, zero subpixels, standing-right radius metadata, no enemies, no
PLMs, no cheats, and no solid terrain. It executes the unpatched cartridge CPU.

- Normal jump, wall jump, and bomb jump launch routines.
- Air, fully submerged water, and fully submerged lava/acid physics.
- Hi-Jump absent/present.
- Speed Booster absent, a representative `3.9000` extra-run pair, and a
  fractional-overflow `5.F000` pair. Bomb-jump cases retain those inputs to prove
  the native bomb table ignores both Hi-Jump and Speed Booster.
- 320 movement frames after every launch, comparing full 16.16 Y position,
  full 16.16 Y speed, direction, and full 16.16 acceleration every frame.
- Eight upward surface-crossing cases. These launch underwater/in lava, cross a
  surface at Y 2020, and then select air gravity from the actual bottom-pixel
  boundary on the following movement samples.
- Twelve terminal-speed cases: each medium starts at `4.FFFF`, `5.0000`,
  `5.0001`, or `6.0000` and runs eight frames. The adjacent values prove that
  `$90:9112` is a whole-word equality test. Both `5.0000` and `5.0001` retain
  their speed; values above five are not clamped and continue accelerating.

Space Jump does not own a separate launch curve: its accepted input calls the
same `$90:98BC` normal-jump initializer, then the spin-jump handler reaches the
same `$90:90E2` mover. Spring Ball's in-air state likewise uses the ordinary
jump mover when it is not in bounce state. The existing controller-level
Spinjump and Morph Ball bounce audits cover those admission/state wrappers;
this fixture proves the shared launch/gravity arithmetic they consume.

## Related full-path regression checks

The current build was also replayed against the existing independent cartridge
captures after adding this oracle:

| Fixture | Coverage | Result |
| --- | --- | --- |
| Crouch jump | 480 air/water/Gravity/Hi-Jump/input cases, 115,200 frames | exact |
| Morph Ball / Spring Ball bounce | 256 impact and rebound cases, 24,576 frames | exact |
| Waterball | 108 water/lava/acid entry cases, 32,400 frames | exact |
| Bomb chains | 72 controller-driven cases, 12,960 frames | exact |
| Charged wall jump | 26 wall-jump cases, 7,020 frames | exact |

The crouch-jump comparison needed a diagnostic-only phase correction after the
later #442 alpha-radius work: the external audit now reads the current pose's
compiled radius before `StepFrame`, matching the native alpha sample. Position,
velocity, pose, and production movement were already exact; no gameplay behavior
was changed to satisfy the diagnostic.

## Reproduce

The accepted numeric-only trace is in `vertical-speed-native-capture.zip`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --vertical-speed-audit 'Super Metroid.smc' `
  csharp/test-fixtures/movement-release/vertical-speed-native-capture.zip
```

For an independent capture, temporarily apply
`native-vertical-speed-entrypoint.patch` to the pinned `upstream-sm`, build its
Release/x64 host, and run:

```powershell
upstream-sm/build/bin-x64-Release/sm.exe --diagnostic-vertical-speed `
  'Super Metroid.smc' NEW.csv
```

The probe source is `native-vertical-speed-probe.h`. It refuses to overwrite an
existing output, and the temporary native integration must be reversed after
capture. The committed archive contains only numeric state, never ROM or save
data. The accepted decompressed CSV SHA-256 is
`5B5A5B4663D553A421D8BCBF59323C480BFC6680B94C8CF58788272E8E4135F8`.
