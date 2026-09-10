# Spin-jump door exit investigation (#517)

Affected player version: **0.1.1**. A reproduced velocity-lifetime defect is
fixed; the issue remains open for player confirmation. The original preliminary
investigation below is retained to distinguish it from the later native comparison.

The reported entrance is Flyway `$8F:9879` through `$83:8BC2` into Bomb Torizo
`$8F:9804`. The pinned disassembly's `Door_Flyway_1` confirms a rightward
horizontal entrance, destination screen (0,0), default spawn distance `$8000`.
Flyway is three screens wide. Using an arbitrary left-edge source coordinate
for this rightward door produces an invalid placement and is not a reproduction.

Run the bounded, headless constructed-boundary diagnostic:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --spin-door-exit-momentum-audit 'Super Metroid.smc' 9879 9804 02F8 0080
```

This extends the existing running-door diagnostic without changing gameplay.
It explicitly seeds a descending spin at the source boundary, uses the ROM's
spin horizontal-speed cap, and compares absent/present two-pixel dash momentum
(including its required ownership flag). It executes the real frontend door
coroutine and then forty neutral-input gameplay frames. Output records each
phase, fixed positions, frame displacement, base/extra speed, vertical speed
and direction, pose, environment, and input lock.

Observed on the project ROM: horizontal base speed remains `$0001.6000`
through the transition; extra speed remains zero or `$0002.0000`. The first
unlocked frame moves 1.375 or 3.375 pixels respectively, with no extra horizontal
speed introduced at the boundary in these two cases. These numbers do **not**
show that the player report is resolved: the input-driven approach, held-input
exit, other momentum states, and original-CPU comparison remain outstanding.

The trace also exposes that the managed room loader clears vertical speed during
placement. Source inspection of the native placement/final-handoff routines does
not establish an equivalent clear, but the complete native transition must be
checked before attributing the reported trajectory to that difference. Do not
remove it based solely on this observation.

## Original-CPU comparison and fix

`native-spin-door-probe.h` executes original cartridge bytes for the real door
header, complete room/level/scroll loading, scrolling setup, placement and tile
loading, all remaining room actors/PLMs, and final narrow-door nudge. It retains
the descending velocity `$0002.3456` after **every** stage. This includes both
whole and fractional words, rather than only observing a direction flag.

The probe acknowledges VRAM completion and NMI/vblank waits at their polling
boundaries. It runs the directional IRQ's remaining 63 steps and publishes its
completion flag. It does not emulate the display timing, palette-fade duration,
source-room enemy frames, or music queue. These exclusions do not constitute a
complete recorded playthrough. No velocity corrections are injected into the
cartridge state.

After loading, forty ordinary cartridge movement frames cover descending,
touching down, the landing pose, and returning to standing. The saved CSV includes
six loading-stage observations plus all forty movement frames. The managed
comparison asserts every frame's X/Y fixed position, base and extra horizontal
speed, vertical speed/direction, and pose against this trace.

Before the fix, the new assertion failed after the actual managed frontend door
coroutine: expected velocity `00023456`, actual `00000000`. The loader was
resetting ordinary jump speed while setting the destination coordinates, despite
the door IRQ owning a separate movement speed. That restarts a descending jump
at zero falling speed and prolongs its horizontal travel. The fix removes those
two writes; it does not slow horizontal movement or introduce a room-specific cap.

After the fix all forty native/managed movement rows match exactly, including
landing at X=`0077E000`, Y=`00BBFFFF`, and the transition back to standing.
The no-dash constructed case also preserves vertical velocity and completes.
The initial gap with the player's precise input/equipment history remains a
limitation: player confirmation is still required for the perceived rocket exit.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --spin-door-native-comparison 'Super Metroid.smc' `
  csharp/test-fixtures/movement-release/spin-door-native-517-v1.csv
```

LF-normalized CSV SHA-256:
`2DF88FD63FC4D173EC87CED263E9A79FD2C048E80924385A3C04ADC7D73B110D`.
Source ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native host pinned at `578f90b3cc49557bb70060ad033bb90b8cf8ac50`;
disassembly at `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

Verification: Release build, exact native comparison, full Core verification,
and elevator frontend handoff audit pass. The latter retains 75 frozen
destination frames and then resumes arrival. Temporary native entrypoint changes
were removed after the capture. No graphical application was launched.
