# Kago airborne platform passage (#455)

Uses the ROM/native/disassembly pins in `KAGO-KZAN.md`. This completes the
previous contact, solidity and Kamer rider fixtures with actual passage through
platform bodies, not walking off an edge. All captures execute original CPU
routines; the comparison runs production C# runtime frames.

## Reproduction

900 independent 80-frame cases (72,000 frames): three actors, two starting
heights, both facings, turn/morph/unmorph inputs and delays 0 through 24.
Synthetic 144x80-block room, floor row 48, no other terrain or PLMs. Morph Ball
only, no beams/liquids/debug cheats, health 999, zero subpositions/momentum,
Y speed 3 downward, animation frame 0/timer 1, matching previous pose.

Samus starts X1024, Y448 or 464, plus 12 pixels for an initially morphed pose.
All platforms start (1024,512), properties $A800:

- Vertical Kamer $D5FF: init $0010, extra $0404, parameters $8000/$0008.
  Upward one-pixel motion after eight-tick activation delay.
- Horizontal Kamer $D83F: init/extra zero, parameters $0000/$2800.
  Zero lateral speed isolates the ordinary suspensor sinking behavior.
- Kzan $DFFF: init/extra zero, parameters $0040/$8018. Bottom $E03F at
  (1024,524), properties $0900. Twenty-four-tick wait, four-pixel fall.

Turn holds the opposite direction from the selected delay through frame 39.
Morph taps Down at delay and delay+4; unmorph taps Up at delay. Native order:
alpha, enemy active list/main and interaction list, movement, animation,
interruption/block/pose dispatch, gamma, feet adjustment, enemy timers.
Camera fixed at (896,384). No RNG-dependent actors; managed RNG returns zero.
The native header records the exact initialization and call sequence.

All 17 Samus fields documented in `KAGO-KZAN.md`, platform fixed X/Y,
external carry X/Y and spritemap are compared each frame.

## Actual successful versus adjacent failed timing

Morph success delays for lower/upper starting-height indices 0/1 respectively:
vertical Kamer 6/3; horizontal Kamer 7/3; Kzan 6/2. In every case, one frame
later fails to pass through. Named assertions at frame 30 check both facings:
unchanged X1024, Samus below the entire platform after success, above after
failure, and no knockback. Successful Kzan passage reduces health to 799;
the adjacent failed attempt remains at 999. This is not invincibility.

## Reproduced defects and fixes

Baseline: 7,454 mismatching frames.

1. Unmorph omitted prospective command seven's shared bounce cancellation.
   Original `Samus_HandleTransitionsA_7` performs that cancellation even when
   its alignment-table entry is zero. Morph and unmorph now call the same
   conditional cancellation helper after successful alignment. Mismatches: 20.
2. Shared vertical collision skipped solid-enemy probing for zero displacement.
   Original `$90:90E2` still dispatches downward when the signed amount is zero;
   `$90:9440` probes solid enemies before the terrain zero-motion shortcut.
   Exact tangency therefore finishes a settling ball bounce immediately.
   Removing the premature solid-enemy skip yields zero mismatches.

Two independent native captures are byte-identical, SHA-256:
`FE785002F166321AE3A0674B1CB98ADC3BCCFF5FDC9BA7B523795953AF3D526B`.
CSV is archived in `kago-passage-native-capture.zip`; the audit verifies its hash.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --kago-passage-audit 'Super Metroid.smc' PATH/kago-passage-455-v1.csv
```

For recapture, apply `native-kago-passage-entrypoint.patch` in upstream-sm,
rebuild the native host, then invoke `--diagnostic-kago-passage ROM NEW.csv`.
The explicit headless path suppresses SDL error/warning dialogs. Temporary
source hooks were removed after capture; the saved patch passes apply --check.
No player saves were modified by this fixture.

Full core verification passes. Passage, Kamer, Kzan, normal contact, Quick Drop
timeline/bomb and arm-pump audits total 166,360 zero-mismatch frames. This is
pinned-revision, synthetic room-local evidence, not a whole-game controller run.
Issue #455 is ready for player validation, not closed.
