# Morph Ball bounce parity (#449)

Status: the impact/rebound matrix passes. Controller-driven jump-to-morph timing
and grounded-roll contrast are still required before marking the full issue ready.

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
