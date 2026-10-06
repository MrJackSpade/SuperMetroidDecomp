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
movie produces 10,758 accepted input intervals: 10,655 outer main-loop dispatches
and 103 NMI continuations. Accepted input is not by itself a gameplay update.
Manifest v2 records native APU-upload and door-scroll-counter evidence: 61
continuations change the scroll counter, 41 occur during APU upload after scrolling
has stopped, and one completes the scrolling coroutine without a counter change.
The last group remains explicitly unclassified rather than inferred disposable.

The 41 APU intervals are source frames 294–334. Native `$80:8028` sets the WRAM
upload flag `$0617` around `SendAPUData`; the door IRQ keeps requesting NMI at
`$80:9823`, so accepted controller reads continue while the main CPU is uploading.
These intervals must not become artificial gameplay frames in the port. The
converter preserves them for now, with `hardwareUploadNormalizationComplete=false`;
the verifier refuses to execute them as gameplay. Input-latch and counter effects
still need normalization, including any simultaneous IRQ gameplay. A future
converter must not simply delete all samples with the upload flag set.

The converter already excludes 132 refreshes without accepted input, retains all
observed input edges, and verifies every private checkpoint through the original
movie's terminal state. Original SMV and ROM hashes remain unchanged. The earlier
claim that all 103 continuations were intentional gameplay waits was too broad.

The full-replay diagnostic imports state once and compares Samus position,
subpixels, movement speeds, animation, health, accepted NMI, RNG, dispatcher state,
room identity, and active enemies' identities, positions, health and visual cursors.
That comparison is still under development; conversion success is not port parity.
Door-entry and source-fade HDMA/RNG/actor omissions were reproduced and fixed,
along with missing RNG advancement through the outer loading dispatches. The
current checked gameplay properties match through update 178. Setup now applies
Samus's first displacement before destination rebasing; the atomic loader retains
the pre-setup source coordinates so it does not count that movement twice. A
focused failing-then-passing regression confirms native `$0013.5800` at setup,
the complete remaining IRQ trajectory, and the unchanged `$00D8.2000` final
position. All four trajectory checks, native camera alignment, and door autosave
continuation checks pass.

Destination placement now rebases both whole position words before the final
nudge, publishes the first moving IRQ while tiles load, and carries that progress
through the atomic room constructor instead of restarting it. The focused native
case matches `$010E.9000` and camera `$00F8` at update 178, all remaining scroll
steps, and the final alignment. Four-direction initial positions and trajectories,
native camera alignment, music timing, and autosave continuation checks pass.

The next raw checkpoint mismatch is update 179 (source frame 214): the port has
already loaded destination actors and entered the next outer loading dispatch
(RNG `$5A88`); the cartridge is still decompressing tiles with its source actor
pool and RNG `$784B`. Samus's position and the accepted NMI agree. Comparing
partially initialized loader ownership at equal video-input boundaries would
require reintroducing hardware delay. The replay still needs an explicit loading
comparison boundary: retain every IRQ movement check, and compare the initialized
owners/RNG when both loaders have completed, before destination gameplay runs.
No such mismatch is silently ignored in the current verifier.
Additional gameplay properties still need coverage before any full-match claim.
The old frames 375â€“744 Ridley-only comparison, which supplies recorded Samus state
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
