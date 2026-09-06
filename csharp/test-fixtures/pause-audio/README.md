# Original SPC driver cancellation probe (#54)

This bounded, console-only experiment runs the uploaded original sound driver on
the existing SPC700 interpreter. It neither opens SDL nor plays sound. The input
files are separately extracted cartridge streams; no ROM or player data is in
this fixture. It uses the local emulator's DSP, not independent hardware DSP
emulation, so it is a stronger sequencer reference but not a complete audio oracle.

Apply `integration.patch` inside `upstream-sm`, build its Release x64 target,
then run from the repository root:

```powershell
& './upstream-sm/build/bin-x64-Release/sm.exe' --spc-cpu-probe standalone-assets/audio/streams/00-SPCEngine.spcu standalone-assets/audio/streams/09-Music_LowerCrateria.spcu
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --spc-cancellation-audit standalone-assets/audio D288CA
```

Reverse only the integration patch after use. Do not reset unrelated native work.
The patch dispatches before SDL initialization. Runtime errors stay on the console.

Both probes run 600 frames, request track five at frame 60, start the sustained
Charge Beam SFX at frame 240, and send the three pause-cancellation commands at
frame 300. They print acknowledgement ports, per-voice source numbers,
noise/echo/flags registers, peak amplitude, mean-square amplitude, and first
nonzero PCM frame. The CPU starts at the uploaded initialization entry; the
managed player initializes itself before applying the streams. Neither reproduces
the complete cartridge upload handshake. CPU output is native rate, managed
output is host-resampled. Do not assert raw waveform equality from these traces.

Initial Lower Crateria result: both settle on acknowledgements `05,02,71,01`
after cancellation, with `FLG=00`, `NON=00`, `EON=01` in the sampled frames.
CPU first nonzero PCM is frame 67; managed is frame 64. The different startup
phase must be addressed before using note/sample trajectories as parity evidence.
With the active charge sound added, both references assign sources `02,05` to
voices six/seven, advance the latter to `07`, then clear them on cancellation.
The following temporary source sequence `11,09,15` on voices five through seven
also occurs in the original driver before returning to zero. These observations
do not show a missing instrument restoration in this specific case. They do not
prove waveform parity or rule out another pause-audio defect.

The older `ManagedAudioRegressionSmokeTest` hashes are not a room-music oracle:
its music scenarios upload each bank but request shared track one. The extracted
engine, Lower Crateria and Upper Norfair streams all retain track one's pointer
`530E`; their track-five pointers differ (`582E`, `582C`, `582A`). Keep the old
hashes as coverage of shared cues/upload handling, not evidence that room songs
match. No fix or player validation is claimed by this experiment.

## Tick-aligned sequencer comparison

With the same integration patch applied and native build completed:

```powershell
& './upstream-sm/build/bin-x64-Release/sm.exe' --spc-cpu-tick-probe standalone-assets/audio/streams/00-SPCEngine.spcu standalone-assets/audio/streams/09-Music_LowerCrateria.spcu | Out-File -Encoding ascii csharp/test-temp/spc-ticks.log
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --spc-tick-comparison-audit standalone-assets/audio D288CA csharp/test-temp/spc-ticks.log
```

The original interpreter is sampled at its loop synchronization PCs `15C4/15C5`.
Each record includes elapsed timer ticks, all DSP registers, and latched input
bytes. The managed probe calls the actual private driver methods through a
diagnostic-only reflection adapter, using those same timer ticks and newly
latched input changes. It reconstructs reset before stepping: calling reset twice
without clearing cached echo-delay state is not equivalent to a fresh CPU reset.

This compares register state after each driver iteration, excluding DSP-generated
ENVX, OUTX and ENDX. It does not compare write order, sample-cycle timing, PCM,
input-port latency or host output. Music begins after tick 500, Charge Beam after
1800, and cancellation after 2300; the run stops at 3000 ticks. Missing ticks,
malformed register records or any differing compared register produce failure.

Verified: Lower Crateria (`D288CA`), Green Brinstar (`D3933C`), Upper Norfair
(`D4B86C`), and Maridia (`D5C844`) each matched all 3000 ticks, including active
Charge Beam cancellation. This narrows the investigation toward sample rendering,
DSP timing or host playback for these scenarios; it does not resolve #54.
