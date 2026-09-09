# Morph Ball bounce parity (#449)

Status: impact/rebound, seeded-fall controller timing, and complete controller
run/jump/morph matrices pass. Ready for player validation on the pinned NTSC ROM.
The separate #450 liquid-entry and #469 soft-morph techniques are not claimed here.

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

## Complete controller run/jump/morph sequence

This third variant starts standing ($01/$02) at X1024/Y235, zero subpixels and
zero horizontal/vertical speeds. It uses the same wide flat room, Morph Ball
alone, health 99, no enemies or gameplay cheats. Unlike the earlier seeds, no
momentum flag or speed component is injected. RNG is not consumed by this
enemy-free movement fixture. All 180 frames go through the ordinary dispatcher.

CSV `carry` now selects run-up length 8/16/24/40 frames; `speed` selects morph
timing 0..15. Hold Dash+forward during run-up. On the launch frame press Jump+Up;
then hold forward+Jump until the morph input window. The cartridge enters spin
pose $19/$1A, with Down later admitting the compact normal-jump pose $17/$18.
First Down is launch+8+2*timing; release Down for one frame, then hold it for six.
Forward is released during that eight-frame window and held afterward. Input
mode one holds Jump throughout; zero releases it on launch+8. Both facings run
the same schedule. Starting near the room center avoids side-wall interactions.

The four run-ups acquire extra speeds $0000.7000, $0000.F000, $0001.7000 and
$0002.0000. All held-Jump cases preserve those exact words through first bounce
on launch+92, second on launch+113, and final landing on launch+116. Final landing
clears base speed; the following grounded movement frame clears extra speed.
Released-Jump controls have a shortened arc and no bounce. The audit explicitly
asserts all these properties and compares all eleven per-frame output fields.
Together with the timing matrix's adjacent late-morph miss and the impact
matrix's exact speed boundary, this covers the ticket's success/failure contrasts.

The initial run/jump matrix reproduced 14,802 divergent frames. Normal jumping
was missing the same fallback command one/two already translated for airborne
Morph Ball. Alpha must retain the pose when base momentum is present, and beta
command one folds extra into base speed after movement before cancelling the
running-momentum flag. In the shortest released-Jump case this is observable at
frame 17. The shared helper is now named `ApplyDeceleratingInputFallback` and is
used by both movement types, including retained compact poses without restarting
their animation. The adjacent command-two path preserves its cartridge behavior.

All 256 cases / 46,080 frames match. The accepted centered-runway v2 capture and
an independent repeat have SHA256
`4A7CBD6DAC40D9A55A89B9EABF005EF04035052F9A14C8B77CDBF22BF759079C`.
The accepted CSV is preserved in `run-jump-morph-native-capture.zip`. Generate via
`DiagnosticRunJumpMorph(romPath, newCsvPath)` using the headless hooks above;
temporary hooks are removed after capture. Replay with:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --run-jump-morph-comparison-audit "Super Metroid.smc" path/to/run-jump-morph-449-v2.csv
```

Sources for this correction: pinned bank $91 `$82D9/$8304` fallback selection,
`$EC50` command one and `$ECD0` command two. The same ROM/disassembly hashes
listed above apply. These are NTSC revision-zero results, not a PAL claim.

Final verification: full core suite passed; all three bounce matrices (84,480
frames), damage boost (616,608), ceiling-steering bombs (138,240) and live
hurt/bomb (32,000) match, totaling 871,328 cartridge-comparison frames. The
magic-number audit also passes with the fallback sentinel in the pose-data
catalog. One matrix run was interrupted by rebuilding its apphost; the complete
97-capture damage-boost matrix was rerun successfully after the build finished.
