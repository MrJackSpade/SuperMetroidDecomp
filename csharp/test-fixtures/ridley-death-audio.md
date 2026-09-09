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
