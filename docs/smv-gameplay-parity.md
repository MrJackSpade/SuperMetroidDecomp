# SMV gameplay parity without hardware lag

The project targets functionally exact cartridge gameplay while eliminating SNES
hardware lag. The user explicitly chose this policy: the port's smoothness is a
valued improvement. It applies to future recordings as well as the current
10,890-frame Ridley movie.

## Units of comparison

An SMV video-frame count is not a gameplay-update count. A native update can span
multiple video refreshes, and a coroutine can deliberately wait for another NMI.
Neither repeated inputs nor unchanged positions prove that a frame is disposable.

Compare the complete ordered sequence of gameplay updates and the inputs actually
consumed by those updates. Preserve authored timers, animation progression,
intentional waits, button presses/releases, and state transitions. Do not require
matching hardware-dependent rendering/CPU stalls or elapsed wall-clock time. This
policy does not permit injecting later reference state to repair a diverged replay.

## Converter contract

A reusable SMV converter is the intended diagnostic tool. Its output is a separate
port replay, not a replacement for the original SMV or a promise that a shortened
SMV will replay identically in a SNES emulator.

The converter must:

1. Verify and retain the original SMV and ROM identities and emulator/capture
   revision. Never rewrite the source movie.
2. Obtain an instrumented native trace of completed updates and actual input
   consumption, including input reads during nested transition/pause coroutines.
   Video-boundary WRAM captures alone are insufficient when the CPU is partway
   through a routine.
3. Map each replay update to its original SMV frame or frame interval. Retain
   meaningful NMI waits. Exclude only evidenced hardware stalls; do not delete
   samples simply because their buttons or screen contents repeat.
4. Preserve held inputs and press/release semantics. If a single controller mask
   per port update cannot represent the native consumption sequence, report that
   mismatch and extend the replay format rather than silently dropping events.
5. Emit a reproducible conversion manifest with input hashes, source coverage,
   update count, timing classification, and the source-to-update map. Refuse
   unsupported or ambiguous trace boundaries instead of guessing.
6. Initialize the port once from the movie's starting state, then run production
   gameplay using only the converted input events. Expected native state remains
   read-only comparison data.
7. Compare all relevant gameplay properties through the recording's end. Record
   the first divergence with both source video-frame and port-update identifiers.
   Matching a short interval or a few selected fields is not full-movie parity.

## Current evidence and remaining work

The original Ridley movie was played through all 10,890 video frames in Snes9x
1.60. The local native trace contains 10,891 WRAM records, including the initial
state and terminal record. Its source movie SHA-256 is
`7E12861DC56C5ABED12C2BFA2B00D24BFA418F49F2CE4C027D930CE9A3663F66`.

The reusable converter is `tools/convert-smv-updates.py`. Its native capture adapter
and build instructions are in `tools/smv-native-capture/`. The instrumented complete
movie produces 10,758 accepted input/update steps: 10,655 outer main-loop updates
and 103 NMI continuations. It excludes 132 hardware-lag refreshes, retains all
observed input edges, and verifies every private checkpoint through the original
movie's terminal state. Original SMV and ROM hashes remain unchanged.

The full-replay diagnostic imports state once and compares Samus position,
subpixels, movement speeds, animation, health, accepted NMI, RNG, dispatcher state,
room identity, and active enemies' identities, positions, health and visual cursors.
That comparison is still under development; conversion success is not port parity.
Door-entry and source-fade HDMA/RNG/actor omissions were reproduced. The next
unresolved boundary is door-header loading (update 173, source frame 198).
Additional gameplay properties still need coverage before any full-match claim.
The old frames 375–744 Ridley-only comparison, which supplies recorded Samus state
and RNG, remains an isolated regression.

Run the converted diagnostic from the hotfix worktree root:

```text
python tools/convert-smv-updates.py MOVIE TRACE_DIRECTORY --rom ROM --output TRACE_DIRECTORY/updates.json
dotnet csharp/src/SuperMetroid.Verification/bin/Release/net10.0/SuperMetroid.Verification.dll --ridley-full-movie TRACE_DIRECTORY
```

The current verifier deliberately requires the original supplied Ridley movie's
identity. The converter accepts supported one-controller SMVs with the matching
instrumented J/U trace; additional verifier starting-state importers are separate
work, not something the converter silently fabricates.
