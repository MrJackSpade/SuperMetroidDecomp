# Underwater aerial turnaround regression — #315

The reported sequence starts stationary facing a ledge, jumps, then turns away.
The native reference executes the original 65816 instructions, not the translated
C functions. No ROM, save data, historical checkout, or GUI launch is required in
this fixture directory. Supply the user's private ROM separately.

## Reproduction and cause

Before the fix, the flat-floor comparison failed on 516 of 2,640 samples. For a
right-facing underwater start, Jump at frame zero and held Left from frame four,
the two versions agreed until the turn ended at frame twenty. The cartridge
initialized normal-jump acceleration; C# retained reverse-deceleration mode one.
At frame 59 the C# position was 14.4375 pixels too far right. Y matched exactly.

Extending to raised ledges exposed a second error: releasing all input while the
turn remained active omitted native momentum command two. A retained pose is not
a retained acceleration mode. Conversely, an empty transition table returns
immediately for nonzero input; it must not select that fallback while Jump is held.

The fix shares normal aerial momentum initialization with the existing spin-exit
and walked-off-floor paths. It does not tune water speed, zero legitimate velocity,
or special-case the Maridia room.

## Repeating the comparison

Apply `jump-turn-integration.patch` to `upstream-sm` (without the other integration
patch), build its Release x64 target using the installed C++ toolset, then run:

```powershell
./upstream-sm/build/bin-x64-Release/sm.exe --jump-turn-probe 'Super Metroid.smc' > native-jump-turn.log
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0/SuperMetroid.DebugRunner.dll --jump-turn-comparison-audit 'Super Metroid.smc' native-jump-turn.log
```

The probe dispatches before SDL and disables fault dialogs. Reverse only this
integration patch after use. Remove generated traces and temporary builds when
finished; do not leave another runnable historical checkout.

There are 176 cases, each with 24 neutral warmup frames and 180 compared frames:
flat floor / raised ledge, air / water, both facings, tapped / held opposite
direction, and eleven turn timings. Jump is released at frame sixty. The raised
ledge starts 64 pixels above the floor with Samus touching its side. Its turns
start at frames 40–50, after clearance, and include landings on the upper platform.
Both simulations use Hi-Jump and the reported non-Gravity equipment set.

All **31,680 frames** match X/Y including subpixels, pose, base horizontal speed,
and acceleration mode. Aerial-turn animation frames/timers also match. This checks
the entire landing trajectory, not just eventual facing or a no-crash endpoint.
It is constructed geometry, not an exact replay of the screenshots' room.

Non-turn animation timers are printed but are outside these assertions: the
expanded diagnostic also observed differences in some neutral-jump timers that
do not alter the matched trajectory. Do not describe this as all-animation parity.

The ROM-free Verification suite additionally covers empty/nonempty input table
semantics and twelve jumping/falling/facing/extra-momentum endpoint combinations,
including retained velocity, release behavior, and unchanged unfinished animation.
The original endpoint regression failed with expected mode zero / actual mode one
before production changes. Leave #315 awaiting player confirmation.
