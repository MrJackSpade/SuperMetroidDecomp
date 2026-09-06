# Recorded generated-audio comparison (#54)

Run from the repository root:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release --no-restore -- --recorded-native-audio-audit 'Super Metroid.smc' input-recordings/SuperMetroid-input-20260905-172633-005.smrec csharp/native/SuperMetroid.AudioNative/bin/x64/Release/SuperMetroid.AudioNative.dll
```

The probe restores the recording's initial SRAM and options, verifies the ROM
digest, and replays production frontend inputs without a window or sound device.
Both players receive identical upload and port commands. Native acknowledgements
feed back into the frontend. Every generated stereo PCM sample and all four port
acknowledgements are compared. Native-rate output uses the same host resampler.

Current expected diagnostic failure: frame 2975, interleaved sample 412,
managed -4860 versus native -4475. The preceding commands include map scrolling,
the palette-loop beep, and menu confirmation. End-of-frame DSP registers match.
This is a reproducible numerical discrepancy, not yet a diagnosed cause of the
reported pause distortion. It occurs before the reported pause interval.

The reference is the native translated SPC player/BRR decoder, not an original
SPC700 CPU execution. This probe does not exercise Windows/RDP audio delivery or
establish that the audible player report is resolved. A mismatch exits with a
console stack trace; it must not be silenced or replaced with a passing golden.

## Reduced menu sound sequence

The normal `--native-audio-corpus-audit AUDIO_DIRECTORY NATIVE_DLL` command now
includes `map-scroll-confirm-overlap`: upload the engine and start map-scroll
plus palette-loop sounds at frame 0, clear both input ports at frame 3, then
request menu confirmation at frame 4. It reproduces a PCM mismatch at frame 4,
sample 1448 (managed -3459, native -3457), without any gameplay or music upload.
The frame's DSP writes restore voice 7's source to zero before key-off; its
envelope is still nonzero. This exposes live source/loop behavior omitted from
the earlier isolated-cue corpus. Full DSP writes and voice sources/envelopes are
printed on failure to support further diagnosis.

An experimental live-source lookup at the loop boundary moved the first mismatch
to sample 1476 but did not establish parity; that production experiment was
removed. Predictor history and the precise source-directory loop target still
need investigation. This is not a completed fix for #54.

## Whole-recording survey

Use `--recorded-native-audio-survey` in place of `--recorded-native-audio-audit`
to collect contiguous mismatch intervals rather than stopping at the first PCM
difference. Port mismatches and gameplay exceptions still stop immediately;
any PCM mismatch yields a nonzero final result. This is a diagnostic survey,
not a weakened passing regression.

On the current replay, all 2,127 stable pause frames match native PCM exactly;
pause exits at frame 11,786. Thus the early sample-switch discrepancy does not
explain distortion during this pause interval. This does not validate Windows
playback, RDP delivery, or the translated native reference against original SPC
hardware. Runtime revisions have also changed the pause boundaries relative to
older replay reports; these are the boundaries observed by the current frontend.

Before the #320 fix, replay stopped at frame 17,691 on a separate bomb
special-block BTS range exception. With the complete normal reaction table,
all 20,874 recorded frames execute: 320 PCM frames differ, but all 2,127 stable
pause frames and every port acknowledgement match. The survey still exits 1
for those real non-pause PCM differences; this is not an audio-fix pass.
