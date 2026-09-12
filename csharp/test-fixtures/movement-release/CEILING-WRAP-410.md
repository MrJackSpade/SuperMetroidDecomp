# Ceiling wrap-around investigation (#410)

Status: native address generation and synthetic PLM exhaustion reproduced and
fixed. Frog Speedway's actual traversal remains unverified; #410 is not complete.

## Original CPU experiment

`native-ceiling-wrap-probe.h` runs the original `$94:A352` routine with all-air
synthetic rooms, width 112/128/144 blocks and height 32. Projectile slot 0 has
X = 63/64/65 tiles, Y = 4 pixels, radii 1/8, zero velocities and subpixels.
Fresh zeroed WRAM per case. No gameplay cheats or instruction substitutions.
It logs CPU X at `$94:A3D0`, before each horizontal block reaction. The loader
restores the original ROM bytes after the comparison harness initializes.

The pinned NTSC ROM and upstream revisions are the same as WRAP-SHOTS-409.md.
Two independent runs produced identical 18-record CSVs. These are synthetic
register/address observations, not exported cartridge assets or player saves.

| Width | Leading tile X | First byte offset | Second byte offset |
| --- | --- | --- | --- |
| 128 | 63 | FF7E | 007E |
| 128 | 64 | FF80 | 0081 |
| 128 | 65 | FF82 | 0083 |

The 112- and 144-block width controls retain even offsets. At width 128, the
first width addition overflows for X >= 64; the second ADC consumes that carry
and adds an extra byte. Pinned bank-94 `$A3D4..A3DB` confirms this ordering.
`$A1B5` bounds the byte offset, then `$A1BB..A1C1` separately floors the block
index and reads the level word at the original, potentially odd byte address.
Therefore merely wrapping a normal block index loses observable behavior.

The old C# horizontal Wave path called `ScanHorizontalShotReactions`, which
rejected the underflowed top row before dispatching any block reaction. The new
Wave-specific scanner preserves the native gates, multiplier truncation and
chained carry. Odd reads construct an unaligned dispatch word while retaining
the aligned index/BTS for PLM setup. Ordinary non-Wave scanning is unchanged.
Both producer-owned initial collision and continuing Wave movement use it.

## Reproduction wiring

Temporarily include `native-release-probe.h` and this probe after `state_recorder`
in upstream-sm/src/sm_rtl.c. Dispatch `DiagnosticCeilingWrap(rom, output)` before
SDL initialization and suppress SDL warning/error dialogs for that headless
entry only. Rebuild Release x64, then run:

```
sm.exe --ceiling-wrap-probe "Super Metroid.smc" NEW_OUTPUT.csv
```

The output uses exclusive creation. Remove only these temporary hooks afterward;
do not reset unrelated upstream changes. CPU execution has a 100,000-instruction
budget per case and returns a nonzero exit status on exhaustion.

## Native PLM allocation comparison

`DiagnosticCeilingPlm` uses the same 128x32 geometry with all aligned words set
to `$0040` and BTS zero. These are synthetic graphics values: odd reads see
`$4000` (shootable air), but setup reads/writes the aligned owner. Projectile
type is Wave, direction is right, velocity is zero. No projectile AI, rendering,
PLM frame stepping or player movement runs. Each of X tiles 63/64/65 receives
45 original `$94:A352` calls. The adjacent tile-63 case allocates nothing.
Tiles 64/65 allocate one actor per call through 40, then stay at 40. Their aligned
owners become `$0052`, while the following word remains `$0040`, allowing the
odd read to continue selecting shootable air after the aligned mutation.

The managed comparison invokes the real initial collision path with the same
registers, recording active count, first physical owner byte offset, aligned
owner word and next word. Before the fix, 90 of 135 records differed. Afterward
all 135 match. Two independent native captures and `ceiling-plm-410.csv` share
SHA-256 `4092C17B48E80BD86DB53E61C685787635EA0FE439845F3903B3E5870D5FCDFC`.

```
sm.exe --ceiling-plm-probe "Super Metroid.smc" NEW_OUTPUT.csv
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --ceiling-wrap csharp/test-fixtures/movement-release/ceiling-plm-410.csv
```

Use the same temporary headless wiring, selecting `DiagnosticCeilingPlm` instead.
The managed comparison runs in the default suite. This establishes allocation
pressure without deduplicating owners; it does not prove sustained pressure with
PLM instructions advancing, authored Frog Speedway tile interpretation, or passage
through its speed blocks. Those remain required before player validation. Odd
reads crossing the end of the modeled level allocation fail explicitly rather
than reading host memory; the verified fixtures do not reach that boundary.

## Authored room and exhausted speed-block collision

`VerifyFrogSpeedwayPoolCollision` loads the actual `$8F:B106` room, retaining
terrain, BTS and resident PLMs. Samus is placed standing at (1237,139), radii
5/21, just right of the first speed blocks, with Wave+Spazer and no Speed Booster.
Continuous Shoot in the up-left aiming pose runs real projectiles and PLM
instructions for 344 frames. The pool first reaches 40 on frame 343. Before
overload a one-pixel left collision probe is blocked; afterward it enters the
unchanged speed block at X=1236 without allocating another actor.

The old speed-block method rejected non-boosting Samus before checking whether
setup could allocate a slot. Original CPU `DiagnosticSpeedPool` enters `$94:90CB`
and stops at `$94:90E1`, after the real allocator: 39 occupied slots yields carry
set, 40 yields carry clear, and both retain `$B106`. The full pool preserves the
carry from BTS indexing because setup never runs. The production method now
returns passing contact for a resolved speed block when the pool is exhausted,
without changing its terrain. The real-room test failed before this fix and
passes after it. `speed-pool-410.csv` preserves the two native boundary records.

```
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --ceiling-wrap-room
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --ceiling-wrap-room-audit "Super Metroid.smc"
```

The debug search runs live PLM timers, not manually filled slots. Its extended
probe applies 2.75-pixel left collision movement each frame while continuing to
shoot; it reaches X=977 and blocks at frame 438 with 38 slots. That experiment
does not model full Samus movement/aim transitions and is not a completed route.
It establishes sustained authored-room overload and its later loss, not complete
Speedless Speedway parity. Full traversal and native timing remain open work.

## Full-runtime input reproduction

The fixed 2.75-pixel movement probe was replaced in the debug search by the real
running-left movement routine, retaining gravity/ground probing and normal dash
acceleration. Standing overload followed by running reaches X=837, then collides
at frame 438 when occupancy falls to 38. Wave+Plasma gives the same endpoint in
this setup. Starting the component probe in a running pose instead does not fill
the pool within 420 frames (peak 33 at the original launch point). This difference
means pose transitions cannot be omitted from a cartridge timing comparison.

`--ceiling-wrap-runtime` now runs the complete production `StepFrame` instead.
It loads the unchanged room, sets Samus to (1237,139), standing up-left aim, Wave+
Spazer, no equipped items, refreshes collision radii/animation, and sets camera
(1109,0). It continuously holds Left, Run, Aim Up and Shoot for 900 frames,
recording position/subpixels, pose, active PLM count and camera. No direct position
or speed writes occur after initialization. No gameplay cheat is enabled.

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --ceiling-wrap-runtime "Super Metroid.smc"
```

Observed: while blocked the runtime alternates wall-stop pose `$D0` and standing
aim `$06`; the pool fills on frame 343, movement begins on frame 344 in pose `$10`,
and Samus ultimately stops at X=842. By frame 899 no PLMs remain active. This is
a managed reproduction, not a native golden trace or proof of an additional
defect. The next comparison must include the cartridge's pose/input dispatch,
projectile production, camera and PLM handler timing. No speculative movement or
PLM lifetime change was made from this incomplete evidence.
