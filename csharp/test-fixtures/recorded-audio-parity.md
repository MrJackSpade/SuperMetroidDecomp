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
