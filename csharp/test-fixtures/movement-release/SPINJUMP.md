# Spinjump input timing — #474

`native-spinjump-probe.h` uses the existing unpatched-ROM loader from
`native-release-probe.h`. To reproduce, include both headers from `sm_rtl.c`
after `struct StateRecorder;` and dispatch `DiagnosticSpinjump(argv[2], argv[3])`
from `main.c` for `argc == 4 && strcmp(argv[1], "--spinjump-probe") == 0`,
before SDL initialization. These temporary integration edits are not retained.

Build `upstream-sm/src/sm.vcxproj` Release/x64 with installed toolset v145
and `SolutionDir` set to the absolute `upstream-sm` directory (trailing slash).
Run `sm.exe --spinjump-probe "Super Metroid.smc" csharp/test-temp/spinjump-native.csv`.
The upstream CPU runner prints one debug line per call; stdout may be discarded.
The probe itself writes the CSV. It does not open a window or modify saves.

Compare using:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --spinjump-comparison-audit "Super Metroid.smc" csharp/test-temp/spinjump-native.csv
```

108 cases: both facings, dry/submerged, three input scenarios, delays 0..8,
24 warmup frames and 40 sampled frames. Scenarios are direction-before-jump,
jump-before-opposite-direction, and Jump held for the last input-locked warmup
frame before unlocking and pressing the opposite direction at the chosen delay.
No equipment or gameplay cheats. Native calls include collision-radius refresh,
the installed input handler ($90:E90F), gravity refresh, movement, animation,
pose collision/transition stages, and draw-time input history ($90:EAB3).
The comparer asserts every sampled X/Y fixed-point position, pose and movement type.

Initial result: 68 mismatches out of 2,880 samples. Both dry direction-first
cases at delay 6 diverge at frame 6: native accepts spinjump ($19/$1A), managed
finishes the turn into standing ($01/$02) and loses the fresh Jump. The remaining
34 frames of each case diverge. This is an opt-in failing reproduction, not a fix.
Do not weaken the assertion to accept the managed trace.

The production F8 interpreter now receives alpha's prospective input pose and
preserves the four jump targets explicitly tested by native `$90:8370`. It no
longer unconditionally publishes the higher-priority standing transition.
After this change all 2,880 frame positions and poses match. The core suite also
checks four jump targets, absent/non-jump targets, and the locked-input exception.
This fixes the reproduced turnaround-completion input loss, not the remaining
elevator-specific portion of the ticket.

Expanded result: all 4,320 samples match with the installed auto-jump handler and
draw-time Jump history included. In the dry zero-delay input-unlock case, frame
zero enters turn pose $26, frame one enters spin pose $19, and movement starts on
frame two, exactly matching the cartridge. Both facings and water are checked.
This is an input-unlock fixture, not a complete elevator actor/room reproduction.

This does not cover full elevator departure, landing buffers, or all animation states.
Those remain part of #474. In particular, Blue Brinstar and Lower Norfair elevator
exceptions cannot be inferred from this synthetic flat floor.
