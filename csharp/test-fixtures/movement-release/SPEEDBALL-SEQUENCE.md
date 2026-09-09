# Controller-acquired Speedball (#470)

This test-only extension verifies acquisition and retention, not the entire
ticket. Temporary Blue Suit conversion remains open; block-family contrasts are
now recorded in [SPEEDBALL-FAMILIES.md](SPEEDBALL-FAMILIES.md).
The preceding contact fixes are recorded in [SPEEDBALL-BLOCKS.md](SPEEDBALL-BLOCKS.md).

## Fixture and inputs

The [technique reference](https://wiki.supermetroid.run/Mockball#Speedball)
defines Speedball as a blue-speed Mockball. This fixture starts stationary,
never writes a boost counter or velocity, and uses the full production movement
dispatcher with the same controller sequence as untouched cartridge instructions.

Room: 144 by 80 blocks, solid floor row 32, walls columns 0/143. Start X128
right / X2176 left, Y491, all subpixels/velocities zero. Standing pose 01/02,
animation frame zero/timer one, matching previous pose/direction, last-different
history zero. Morph Ball and Speed Booster equipped (2004), health 99, no liquids,
enemies or cheats. The taller floor placement prevents high boosted jumps from
crossing world Y zero; the finite walls remain real collision boundaries.

Run forward with Dash for 64/80/96/112 frames (CSV carry). At launch, hold Jump
and forward; release Dash. Full jumps keep Jump held. Short hops release Jump
only at launch+8 and repress at +9. First Down is at launch+40 full / +10 short,
releasing forward at that point. Second Down is one frame at:

- Full: launch + 96/117/117/140, respectively, + timing index.
- Short: launch + 20 + timing index.

Timing indices 0..24 are swept. Forward resumes immediately after second Down;
Jump stays held. Both directions, four run-ups, both jump modes and 25 timings
run for 300 frames: 400 cases, 120,000 samples.

The CPU harness has an initially empty audio queue and no APU dequeue. C# uses
the actual GameplayAudioFramePublication and synchronous echo queue callback to
match this contract. This matters because the native stage-four animation uses
the sound-call return accumulator to select its reload table. An initial test
without the callback differed at frame 89; that was a fixture mismatch, not a
new production defect. No gameplay code changed for this sequence audit.

## Exact assertions

At launch the extra speeds are 3.F000, 4.F000, 5.F000, 6.F000 and boost counters
0201, 0301, 0401, 0401. Thus the shorter run-ups provide non-blue controls.
The native soft-morph timing windows are:

| Run-up | Short hop | Full jump |
|---|---|---|
| 64 | 10..17 | 8..14 |
| 80 | 12..19 | 8..14 |
| 96 | 12..19 | 8..14 |
| 112 | 14..21 | 7..13 |

Every successful case explicitly retains acquired extra speed and boost through
the first grounded moving-ball frame, with no rebound. That first roll is seven
frames after second Down, pose 1E/1F, Y505.FFFF. All other timings must fail to
produce any grounded moving ball with retained extra speed. This gives 120
successful cases, including 60 blue-speed cases, with adjacent failures covered.
Later wall contact is not mistaken for failed acquisition: all 300 frames still
compare, including any eventual collision/cancellation.

All samples compare X/Y including subpixels, pose, bounce state, vertical speed
and direction, base/extra speed, acceleration, animation frame/timer and complete
boost word. This is stronger than a final position/no-crash check. No combined
claim about the seeded block-contact matrix or Blue Suit conversion is inferred.

## Capture and verification

Include native-release-probe.h then native-morph-bounce-probe.h after the native
StateRecorder declaration. Dispatch DiagnosticSpeedball before SDL, then remove
the temporary hooks. No GUI, player SRAM or debugger slots are involved.

speedball-sequence-native-capture.zip preserves the accepted v3 CSV. A second
independent run is byte-identical, SHA256:
`C9BB380511B552C6E3D376C70A4FDAEC37470C8780D894A7850D5634B1623D55`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --speedball-comparison-audit "Super Metroid.smc" path/to/speedball-sequence-470-v3.csv
```

Pinned ROM: Japan/USA NTSC rev0, SHA256
12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72.
Native source: 578f90b3cc49557bb70060ad033bb90b8cf8ac50.
Disassembly: 362be646929cf8e483f692b73a6561cfc2dc1d0d.
Native movement/animation uses the shared Mockball probe entry points, including
90:A521 grounded ball, 90:A61C posture movement and 90:852C boost animation.

DebugRunner builds cleanly. The new matrix passes 120,000 frames; unchanged
Mockball and all three bounce matrices pass another 151,680 frames after extending
the shared fixture. Total: 271,680 exact comparison frames, zero mismatches.
The core suite passed for the preceding production contact commit; this change
only adds/extends diagnostic code and fixtures. No PAL result is claimed.
