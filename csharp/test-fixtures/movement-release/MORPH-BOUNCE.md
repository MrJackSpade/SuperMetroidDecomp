# Morph Ball bounce parity (#449)

Status: impact/rebound and seeded-fall controller-morph timing matrices pass.
A ground-run/jump prefix that acquires and retains momentum through morphing is
still required before marking the full issue ready. A seeded ordinary fall is
not interchangeable with a descending normal-jump movement type.

## Impact fixture

`native-morph-bounce-probe.h` executes the unmodified cartridge CPU instructions
in a constructed room: 16 by 32 blocks, floor row 16, side walls columns 0/15,
air, Morph Ball equipped, health 99, no enemies or gameplay cheats. Samus begins
at (128,249), zero subpixels, falling Morph Ball pose $31/$32, direction down,
bounce state zero, animation frame zero/timer one. Both facings are covered.

The eight initial vertical magnitudes are $0000.0000, $0001.FFFF, $0002.C7FF,
$0002.E3FF, $0002.E400, $0002.FFFF, $0003.0000 and $0005.0000. Four horizontal
seeds vary base speed alone (0, 1.25, 3, 5); four others use base 1.25 plus
separate carried dash speed (0.25, 0.75, 1.25, 2). Input is either neutral or
held Jump plus facing direction. The capture contains 256 cases of 96 frames,
24,576 total. It compares position/subpixels, pose, bounce state, vertical
velocity/direction, base/extra horizontal speed, acceleration mode and animation.

These are explicit impact-state seeds, not a claim that the fixture has yet
reproduced every way of acquiring the seed through a real jump/morph sequence.

## Assertions and findings

* Gravity updates the stored fall magnitude before the landing threshold test.
  The $0002.E3FF seed remains below three while $0002.E400 reaches three and
  launches the first rebound. Both sides are explicitly asserted.
* Qualifying impacts enter bounce two on frame 21 and finish grounded on frame
  24. Final landing clears base momentum; held-input cases retain their separate
  carried dash word through both rebounds and that final collision frame.
* The zero-speed/held-Jump case legitimately unmorphs instead of bouncing. On
  reaching the wall, the C# shared grounded-pose helper restarted the wall-stop
  animation every frame although the native pose-unchanged guard preserves it.
  This reproduced 320 divergent frames in the initial 128-case matrix. The
  helper now initializes animation only when the resolved ordinary pose changes.
  Frame 54 explicitly requires timer 15 rather than a restarted 16, and direct
  verification checks timer preservation for all six wall-stop poses.

All 24,576 expanded-matrix frames match after this correction. No bounce impulse
or momentum table was changed. The full core verification suite passes, as do
all 97 accepted damage-boost captures (616,608 frames), ceiling-steering bomb
coverage (138,240 frames), and live hurt/bomb coverage (32,000 frames).

## Capture and replay

The independently repeated native CSV has SHA-256:
`5058D231B738D7CCDBC7C14580A47324DAD1CF7E3B4427CD08A36EC8F536B075`.
`morph-bounce-native-capture.zip` preserves this accepted v3 CSV. Earlier local
captures without side walls or without separate dash-word seeds are not substitutes.

Include `native-release-probe.h` and `native-morph-bounce-probe.h` in `sm_rtl.c`
after the `StateRecorder` forward declaration. Temporarily dispatch
`DiagnosticMorphBounce(romPath, newCsvPath)` before SDL initialization, and remove
the temporary hooks after capture. Output creation refuses to overwrite a file.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --morph-bounce-comparison-audit "Super Metroid.smc" path/to/morph-bounce-449-v3.csv
```

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native C: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
Cross-checks: bank $91 first/second bounce and final cleanup, and the
pose-unchanged animation guard at $91:FB64-$FB67.

## Controller-driven fall-to-morph timing

The timing variant uses a 144-by-80-block room (the Landing Site header's 9-by-5
screen geometry), empty rows above floor row 16, side walls at columns 0/143.
Start X512, Y180, zero subpixels, ordinary falling pose $29/$2A, downward speed
$0001.8000, base speed $0001.4000. Extra-speed seeds are 0, 0.75, 1.25 and 2;
the running-momentum flag is set exactly when that seed is nonzero. Other state,
equipment and the CPU schedule match the impact fixture.

The CSV keeps the impact schema: `speed` here is timing index 0..7, selecting
first Down on frame 2*index, release for one frame, then Down for six frames.
Facing direction begins at 2*index+8, after the morph input window. Jump is held
throughout or never held. Timing index 8 instead starts grounded ball pose
$1D/$41 at Y249, with facing direction throughout and no Down. With Jump held
that baseline correctly unmorphs; without Jump it rolls on the floor.

Both facings, nine timing/baseline modes, four carries, two Jump modes and 96
frames yield 144 cases / 13,824 samples. Native early modes 0..6 bounce first on
frame 22, second on 43 and settle on 46. Adjacent late mode 7 is still morphing
at impact and does not bounce. Grounded baselines never manufacture a bounce.
These boundaries are explicit assertions in addition to all eleven output fields.

This fixture reproduced 9,216 divergent frames before the correction:

* Falling aim initialization omitted $91:F60D's acceleration-mode selection from
  the extra-speed pair, including the compact aiming-down path. Timing zero with
  nonzero carried speed must install mode two on frame zero.
* Falling lookup failure omitted command eight ($91:EC8E): after movement and pose
  selection, cancel running momentum and clear extra speed while retaining base
  speed and acceleration mode. It runs even when the definition retains the same
  pose. All falling seeds have lost the extra component by frame one.

Both paths now match the CPU; no bounce impulse or speed table was adjusted.
Post-fix verification also passed the full core suite, original 24,576 impact
frames, all 616,608 damage-boost frames, 138,240 ceiling-steering bomb frames and
32,000 live hurt/bomb frames (825,248 comparison frames including this fixture).
The latter behavior is why this is **not** evidence for acquiring successful
jump-carried speed through a complete run/jump/morph sequence.

`morph-timing-native-capture.zip` contains the independently repeated CSV with SHA256
`5F22BB9321A35A58E084BD0BAE8FFF9575B3BCFCE3E61DE99F6292810585CE80`.
Generate via `DiagnosticMorphTiming(romPath, newCsvPath)` using the temporary
headless hooks described above. Replay with:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --morph-timing-comparison-audit "Super Metroid.smc" path/to/morph-timing-449-v2.csv
```
