# Damaged-pose bomb-jump reference (#413)

Status: constructed-seam and normal-placement/full-fuse matrices match the pinned
cartridge. **Ready for player validation.**

`native-hurt-bomb-probe.h` runs original CPU instructions after restoring the
unmodified cartridge bytes. It admits an enemy-projectile hit through the native
collision handler, then injects a centered bomb at its timer-eight collision seam.
This constructed bomb exercises real overlap and arbitration; it intentionally
does not establish normal placement, fuse, or bomb animation correctness.

The matrix contains both facings, ten bomb-contact frames (0–9), and sixteen
first-held Jump + opposite-direction frames (0–15), each traced for 40 frames:
320 cases / 12,800 rows. Equipment is Bombs + Morph Ball, cheats disabled, air,
99 initial health, integer position (128,160), zero initial subpixels/speeds.
The synthetic room is 16 by 32 blocks, with ceiling row zero, floor row sixteen,
and walls at columns zero/fifteen. Ceiling collision is intentionally possible;
do not mistake its termination of upward movement for reaching the natural apex.

## Observed reference case

For right-facing Samus, bomb contact at frame zero and boost input from frame 15:

| Frame | Pose | Hurt timer/direction | Bomb direction | Movement handler |
| --- | --- | --- | --- | --- |
| 0 | $53 | $0004 / $0001 | $0002 | $DF38 knockback |
| 4 | $53 | $0000 / $0001 | $0002 | $DF38 knockback |
| 5 | $53 | $0000 / $0000 | $0002 | $A337 normal |
| 6 | $53 | $0000 / $0000 | $0802 | $E025 bomb start |
| 7 | $53 | $0000 / $0000 | $0802 | $E032 bomb movement |
| 25 | $53 | $0000 / $0000 | $0802 | $E032 bomb movement |
| 26 | $50 | $0000 / $0000 | $0000 | $A337 normal |

Frame seven's upward speed is $0002.C000. The damaged pose survives launch and
the held input is admitted after bomb movement terminates. With input instead
starting at frame zero, pose $50 is selected immediately; the bomb never enters
its start/main handler in that case. These are fixture-local frame indices.

Pinned disassembly confirms the mechanism: `$90:DE78` is the bomb branch of the
post-movement hit-interruption routine, behind knockback-timer/direction handling;
`$90:E012` retains the current pose; `$91:EE80` installs the bomb-start handler and
locks the input handler. The comparison initially failed in 7,324 frames. The
managed fix moves setup to the post-movement interruption seam with native hurt
priority, clears bomb direction when the input table selects a different pose,
and restores input during fractional-speed ascent before the apex. Bomb setup
also no longer runs the ordinary jump initializer for forward-jump poses. All
12,800 rows then match position/subpixels, pose, hurt/bomb words, and velocities.
The trace's native handler pointer is diagnostic data, not a compared C# pointer.

Run after extracting the archive to a new directory:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --hurt-bomb-comparison-audit "Super Metroid.smc" path/to/hurt-bomb-413.csv
```

## Reproduction and preservation

Include `native-release-probe.h` then `native-hurt-bomb-probe.h` from `sm_rtl.c`
after `struct StateRecorder;`. Temporarily dispatch
`DiagnosticHurtBomb(romPath, outputCsvPath)` from `main` before SDL initialization.
Build the x64 Release native target, run the probe, and remove those hooks.
The output file must not already exist. Filter repetitive `RunAsmCode!` stdout
without terminating the process early. No GUI is launched.

`hurt-bomb-native-capture.zip` preserves the CSV, not ROM or player save data.
Two independent executions produced identical CSV SHA-256:
`F5077D8E9342DB7153C8A9C5EB15713DBA6E1F2274E435A116DACF0EA6498F00`.

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native C checkout: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly checkout: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

The constructed-seam C# matrix uses the audited knockback initializer
at the admitted-contact boundary and the real runtime bomb-overlap pass, with a
constructed non-animated bomb stimulus matching the native collision-only seam.

## Normal placement and full-fuse integration

`native-live-hurt-bomb-probe.h` and `LiveHurtBombComparisonAudit` add 320 cases /
32,000 frames. Place a normal bomb through the real Shoot-edge producer with
Morph Ball + Bombs at (128,249), then start standing over it at (128,235).
Unmorphing is intentionally outside this fixture, not part of the parity claim.
After that setup boundary neither actor is repositioned, and the fuse is never
shortened. The placement frame includes the first bomb update in both engines.

Both facings are covered. An inert twenty-damage enemy projectile overlaps on
one selected frame from 44–53. Jump + opposite direction is held from one frame
in 52–67. All cases run 100 frames, including bomb explosion and deletion. Room
block dimensions and screen dimensions are initialized for native bounds checks.
The first exploratory capture omitted screen dimensions and therefore skipped
terrain reaction; it is invalid and is NOT the archived reference.

The accepted capture reproduces 84 damaged-pose launches and 84 later boosts;
the remaining 236 adjacent timing cases have different outcomes and are also
compared, not filtered out. Every row checks Samus position/subpixels, pose,
hurt and bomb words, vertical/horizontal speed and health; bomb count, type,
fuse, position, list/timer, spritemap and collision radii are checked too.

This uncovered stale bomb-movement flags when hurt replaced the native movement
pointer. Fix: clear the old movement owner immediately while retaining the
independent pose-input lock until native hurt expiry restores input. The full
matrix then matches with zero deviations. Focused regression assertions cover
this separate movement/input ownership, and the full core suite passes.

`live-hurt-bomb-native-capture.zip` contains the accepted CSV (no ROM/save data).
Two independent CPU runs produced SHA-256:
`C3CF5EE45936F557A980633846C447CB824288B60D3A77B6840930810C598360`.
Use the same pinned ROM and source revisions listed above. Temporary native
hooks use `DiagnosticLiveHurtBomb(romPath, outputCsvPath)` before SDL and are
removed after capture.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --live-hurt-bomb-comparison-audit "Super Metroid.smc" path/to/live-hurt-bomb-413-v2.csv
```

These are bounded synthetic-room parity tests, not claims about every bomb
technique, modified ROMs, or PAL timing. Ordinary bomb-jump variants remain #412.
