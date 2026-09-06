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

## Native-rate BRR versus extracted PCM reproduction

The same integration patch also enables an isolated DSP source test:

```powershell
& './upstream-sm/build/bin-x64-Release/sm.exe' --dsp-sample-probe standalone-assets/audio/streams/00-SPCEngine.spcu standalone-assets/audio/streams/09-Music_LowerCrateria.spcu 19 | Out-File -Encoding ascii csharp/test-temp/dsp-sample-19.log
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --dsp-sample-comparison-audit standalone-assets/audio D288CA 19 csharp/test-temp/dsp-sample-19.log
```

Both render one voice at native rate and unit pitch, with matching gain/volume,
echo disabled, and no sound-driver timing involved. Compare 128 consecutive
534-sample stereo blocks by deterministic hash. The managed resampler is an
identity here, so host interpolation cannot explain a difference. The command
fails on any mismatching block or incomplete trace.

Reproduced before an extractor fix, Lower Crateria bank:

| Source | PCM length / loop | Mismatching blocks | First mismatch |
| --- | --- | --- | --- |
| 18 | 4224 / none | 0 | none |
| 19 | 160 / 96 | 128 | block 0 |
| 1B | 7952 / 80 | 20 | block 14 |
| 1C | 9456 / 3680 | 11 | block 17 |
| 02 | 1408 / 64 | 55 | block 2 |
| 05 | 1760 / 64 | 0 | none |
| 07 | 1392 / 48 | 0 | none |

The first mismatch tracks the first loop boundary. The extractor currently
decodes one pass with zero initial BRR history and repeats its PCM loop. Native
BRR decoding carries the last two decoded values into the loop, so filtered loop
heads can differ on later passes. This is a reproduced waveform defect, not yet
a verified fix or proof that it explains every reported pause-audio symptom.

## Verified extractor correction

The extractor now unfolds BRR loop passes until the loop entry repeats the exact
two-sample predictor history. It retains changing passes in the WAV prefix and
points the PCM loop at the recurring state. Filter-zero loop heads discard history
and keep their original compact loop. Non-looping sources are unchanged. A bounded
search fails loudly rather than accepting an approximate loop.

The failing sources above now match every native PCM block. The complete catalog
also passed: all 112 canonical sources, 128 blocks each, with zero mismatches.
38 WAVs changed; all 112 stable IDs and all 935 aliases remain intact. The runtime
still plays replaceable PCM and has no added BRR decoder.

`VerifyBrrLoopExtractionRetainsPredictorHistory` adds a one-block constructed
filtered loop, checks 1024 samples against its independent integer recurrence,
and checks filter-zero, one-shot, and invalid-loop-address cases.

The native DLL remains an optional diagnostic dependency only. This command
compares complete managed/native PCM and acknowledgement frames, using native-rate
output from the DLL and the tested host resampler:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --native-audio-corpus-audit standalone-assets/audio csharp/native/SuperMetroid.AudioNative/bin/x64/Release/SuperMetroid.AudioNative.dll
```

It matched all 4560 frames of the historical shared-cue corpus, independently
establishing its updated PCM SHA-256
`06FE91966B29B05F2012C6A542864B5692B178A7B028C2A4A56F6D34E8F8D06C`.
The acknowledgement hash is unchanged. It additionally matched all 24 bank-specific
track-five scenarios for 600 frames each, including active Charge Beam cancellation:
14,400 additional complete PCM/acknowledgement frames. This latter oracle is the
native translated driver/DSP; the separate SPC700 interpreter comparison above
checks original-driver sequencer behavior for four selected banks.

The full Verification suite, Game Release build, managed-audio corpus, and live
frontend pause-audio route pass. #54 should remain open for player listening
confirmation: mathematical waveform parity does not itself confirm what the
player hears through their host/RDP audio path. Restart and load ordinary SRAM
for validation so the new sample assets, not an existing audio session, are used.
