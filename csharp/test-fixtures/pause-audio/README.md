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

Both probes run 600 frames, request track five at frame 60, and send the three
pause-cancellation commands at frame 300. They print acknowledgement ports,
noise/echo/flags registers, peak amplitude, mean-square amplitude, and first
nonzero PCM frame. The CPU starts at the uploaded initialization entry; the
managed player initializes itself before applying the streams. Neither reproduces
the complete cartridge upload handshake. CPU output is native rate, managed
output is host-resampled. Do not assert raw waveform equality from these traces.

Initial Lower Crateria result: both settle on acknowledgements `05,02,71,01`
after cancellation, with `FLG=00`, `NON=00`, `EON=01` in the sampled frames.
CPU first nonzero PCM is frame 67; managed is frame 64. The different startup
phase must be addressed before using note/sample trajectories as parity evidence.
This music-only case does not cover cancelling an active foreground SFX voice.
No fix or player validation is claimed by this experiment.
