# Mockball parity (#469)

Status: reproduced and matched on the pinned NTSC cartridge. Ready for player
validation; no additional production change was needed after the shared movement
corrections recorded under #449. This is not a claim that Speedball block breaking
(#470), waterball (#450), or bomb-spread carry (#471) have been verified.

## Controller setup

The [technique reference](https://wiki.supermetroid.run/Mockball) describes a
running jump, down-aim, late second Down, and immediate forward input. Its
short-hop variant releases then re-presses Jump before aiming down. The captured
fixture follows those inputs without injecting velocity or a momentum flag.

Room: 144 x 80 blocks, solid floor row 16, side walls columns 0/143. Roof tiles
at row 14 cover columns 0..15 and 112..143, creating 16-pixel-high tunnels at
both ends. The longest short-hop success cases enter these tunnels. Samus starts
standing, pose $01/$02, X1024/Y235, zero subpixels and speeds, animation frame
zero/timer one. Previous pose and direction match; last-different history is
zero. Morph Ball alone is equipped, health 99, no enemies, liquid, or gameplay
cheats. No RNG-dependent actors participate.

CSV `carry` selects 8/16/24/40 running frames. During those frames hold Dash and
forward. `inputMode` one holds Jump from launch onward; mode zero releases it
only on launch+8 and re-presses it on launch+9. Forward stays held until the first
Down press: launch+40 for full jump, launch+10 for short hop. Release forward at
that point. First Down lasts one frame; Jump stays held afterward.

Second Down lasts one frame at launch+78+timing (full jump) or launch+20+timing
(short hop), where CSV `speed` is timing index 0..20. Forward resumes immediately
on the next frame and remains held. Both facings run this schedule for 200 frames.
All movement, pose lookup, animation, collision and landing logic is production
code. This yields 2 facings x 4 run-ups x 2 jump modes x 21 timings = 336 cases,
67,200 per-frame samples.

## Exact native results and assertions

- Full jump succeeds for timing indices 8..14: second Down at launch+86..92.
  Index 7 is the adjacent early hard-morph failure; 15 is too late.
- Short hop succeeds for indices 8..15: second Down at launch+28..35. Adjacent
  indices 7 and 16 fail to retain the run-speed component. Short-hop early
  failures need not bounce because their fall magnitude can be below threshold.
- Successful cases retain their controller-acquired extra speed throughout the
  jump and rolling trajectory: $0000.7000, $0000.F000, $0001.7000 or $0002.0000,
  according to run-up length. No successful case enters either rebound.
- Early full-jump cases enter bounce one on launch+92 and lose extra speed by
  the end of the sequence. Late cases also lose it. An eventual ordinary roll
  without the extra component is not accepted as a Mockball success.
- Successful cases end in grounded moving-ball pose $1E/$1F at Y249.FFFF,
  base speed $0003.4000 plus the retained extra speed. Long-run short-hop cases
  reach the ball-height tunnel, beyond X1792 right / below X256 left.
- The earliest full-jump soft morph with 40-frame run-up lands on frame 132.
  Its base-speed words over frames 132..141 are:
  `1.4000, 0.C000, 0.4000, 0, 0.C000, 1.8000, 2.4000, 3.0000, 3.C000, 3.4000`.
  Extra speed stays `2.0000`. Thus **base**, not total speed, reaches zero in
  this fixture; the overshoot before steady rolling is also preserved.

These are explicit assertions, in addition to exact X/Y/subpixel, pose, bounce,
vertical velocity/direction, base/extra speed, acceleration mode and animation
comparisons on every frame. We do not substitute an endpoint or no-crash check.

## Why this works on cartridge

At $90:A61C the posture-transition mover lands without entering the ordinary
airborne-ball bounce command, clearing the vertical/bounce state while leaving
the horizontal components intact. The animation then installs a grounded ball.
At $90:A521 a nonzero acceleration mode bypasses the stationary-ball teardown;
forward input changes to moving-ball before that teardown can clear the carried
extra component. The normal horizontal calculator produces the transient above.
Early morphing instead reaches the ordinary airborne-ball landing/bounce path.

These mechanisms already match in C#. This ticket adds regression evidence,
not a new Mockball-only flag, speed multiplier or adjustment.

## Capture and replay

Include `native-release-probe.h`, then `native-morph-bounce-probe.h`, after the
`StateRecorder` declaration in `sm_rtl.c`; temporarily dispatch
`DiagnosticMockball(romPath, newCsvPath)` before SDL. The shared loader restores
untouched cartridge bytes after harness initialization and the writer refuses
to overwrite a capture. Remove temporary hooks afterward. No player SRAM or
debugger state slots are used or changed.

The accepted tunnel fixture and an independent repeat have SHA256
`252B36D3D486ED075B9AB921D70EEBF3BDF9CE6F06F9EC57F37643A1C6B8F738`.
`mockball-native-capture.zip` preserves the accepted v2 CSV. The earlier flat-floor
capture has identical samples: the correctly morphed body fits through the new
tunnels without contact, which is separately asserted in the accepted fixture.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --mockball-comparison-audit "Super Metroid.smc" path/to/mockball-469-v2.csv
```

ROM: Japan/USA NTSC revision zero, SHA256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native source: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
No PAL timing claim is made. DebugRunner builds; all three existing bounce
matrices (84,480 frames) and crouch lock (19,200) also pass after extending the
shared audit, for 170,880 verified comparison frames in this test-only change.
