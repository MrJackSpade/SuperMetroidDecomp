# Ridley death distortion — #478

Capture the existing retail-room combat/death fixture's ordered enemy audio calls:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --norfair-ridley-death-audio-trace "Super Metroid.smc" NEW_OUTPUT.csv
```

Output uses CreateNew: it never overwrites an earlier trace or a player fixture.
The fixture seeds deterministic combat projectiles and a zero-health grab, then
executes production enemy AI, instruction streams, projectile updates and draw
owners through all death phases. It does not run the frontend audio queues, music
history, PCM mixer or host playback. This is a sequencing diagnostic, NOT an
audible reproduction or a fix for the reported distortion.

Current trace spans 896 death frames. Library 2 command $59 occurs at frame 417
(queue limit 6). Command $24 occurs 33 times, every five frames from 444 through
604 (limit 3). Music track 3 is requested at frame 895 with eight-frame delay.
The small-explosion cadence and terminal music request agree with pinned C
`Ridley_Func_69` ($A6:C623) and `Ridley_Func_67` ($A6:C5DA), respectively.
No endlessly retained death sound was observed in this fixture.

Next: feed the complete command sequence with the actual Ridley music into both
audio players, capture the mixed segment, and compare against the original SPC
path where needed. A clean individual explosion or roar sample cannot establish
that this issue is fixed; host timing remains a separate possible cause.

## Mixed native-player comparison

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --ridley-death-native-audio-audit standalone-assets/audio csharp/native/SuperMetroid.AudioNative/bin/x64/Release/SuperMetroid.AudioNative.dll "Super Metroid.smc" NEW_OUTPUT.csv
```

Use the current CSV header ending in `queueLimitOrDelayFrames`; music delay is a
numeric frame count. The replay loads the actual room music bank ($24), starts
the reveal's fight track five, then submits the captured requests through
`CartridgeAudioState` with native acknowledgements. Music warmups of 120, 600,
and 1,800 frames test three overlap phases; each includes 240 tail frames.
All 5,928 stereo-PCM and four-port acknowledgement frames currently match.

This reference is the native translated SPC player and decoder, not an original
SPC700 CPU. Song phase and prior battle effects are constructed rather than the
player's recorded history. Thus this rules out managed/native divergence in these
three sampled mixes, not the reported audible distortion, original-hardware
parity, or host delivery starvation. No production fix or issue closure follows
from this passing comparison.

## Original SPC700 sequencer overlap

Apply `pause-audio/ridley-integration.patch` inside the pinned upstream checkout,
build Release/x64 using v145 and its absolute SolutionDir, then run:

```
upstream-sm/build/bin-x64-Release/sm.exe --ridley-spc-probe standalone-assets/audio/streams/00-SPCEngine.spcu standalone-assets/audio/streams/24-Music_BossFight1.spcu NEW_TRACE.log
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-launch-profile -- --ridley-spc-tick-comparison-audit standalone-assets/audio D7BF73 NEW_TRACE.log
```

The output file is exclusive-create. The original uploaded SPC700 driver executes
for 6,000 bounded loop snapshots, with music five, roar $59, 33 repeated $24
explosions, and music three. These inputs are scheduled in driver ticks, not the
captured video-frame timeline. All 6,000 managed register snapshots agree when
driven by the same latched inputs and timer values. DSP-produced ENVX/OUTX/ENDX
are excluded, as in the existing loop comparator. This strengthens sequencer
evidence but does not establish waveform/host timing or reproduce the audible
report. The temporary upstream integration was removed after the run.

## Player recording search

Issue #478 was created at 2026-09-08 21:42:46 UTC. The nearby local recording
`SuperMetroid-input-20260908-173740-944.smrec` starts at 21:37:40 UTC, but its
complete current-build replay never enters Ridley's room $B32E. It starts in
$B741 and spends much of the run around $B585 before reaching $B5D5.
SHA-256: `C372C1407AEEF6C4720D65F1DC4C053BAEF6B3C1EC1E3692DEAB5131A62EE49E`.

The preceding long `SuperMetroid-input-20260908-165945-507.smrec` also completes
without entering $B32E; it runs through Lower Norfair but revisits $B3A5/$B457.
SHA-256: `3E9D0FDEE4180120CA2117573D28095FDB3E32B64CC26765F9EFAB179DC4A468`.
These recordings are under the ignored local `input-recordings` directory and
were not modified or uploaded. A date match does not prove either is the reported
battle. Gameplay changes and/or a different session may explain the mismatch;
no cause is established. Neither replay is evidence of an audio fix. A new
pre-death state would allow exact player-segment investigation; #478 stays open.
