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

72 cases: both facings, dry/submerged, direction-before-jump or jump-before-
opposite-direction, delays 0..8, 24 neutral warmup frames and 40 sampled frames.
No equipment or gameplay cheats. Native calls include collision-radius refresh,
input, gravity refresh, movement, animation, and pose collision/transition stages.
The comparer asserts every sampled X/Y fixed-point position and pose.

Initial result: 68 mismatches out of 2,880 samples. Both dry direction-first
cases at delay 6 diverge at frame 6: native accepts spinjump ($19/$1A), managed
finishes the turn into standing ($01/$02) and loses the fresh Jump. The remaining
34 frames of each case diverge. This is an opt-in failing reproduction, not a fix.
Do not weaken the assertion to accept the managed trace.

This does not cover elevator departure, landing buffers, or all animation states.
Those remain part of #474. The jump-first cases here begin on an ordinary floor,
not with elevator-owned input suppression.
