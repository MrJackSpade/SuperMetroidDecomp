# SuperMetroid.AudioNative

This DLL is a deliberately narrow bridge to the cartridge-derived audio implementation in
`vendor/sm/src/spc_player.c` and `vendor/sm/src/snes/dsp.c`. Those files are verbatim copies
from `snesrev/sm` commit `578f90b3cc49557bb70060ad033bb90b8cf8ac50` and implement the
translated Super Metroid SPC sequencer and SNES DSP/BRR mixer; this directory does not replace
either with host-authored approximations.

The bridge exposes allocation, validated cartridge upload streams, APU input-port writes,
and one audio-frame generation call. All game-side queue timing remains in C# so it is easy
to debug. The upstream implementation is MIT licensed; its license is retained at
`vendor/sm/LICENSE.txt` so a fresh clone does not depend on the ignored reference checkout.

The normal `SuperMetroid.Desktop`/`SuperMetroid.Game` build invokes `Build-NativeAudio.ps1`
and copies the x64 DLL to the managed output directory. Building requires Visual Studio's
`Desktop development with C++` workload and the `v145` toolset selected by the project. To
build the bridge directly from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File csharp/native/SuperMetroid.AudioNative/Build-NativeAudio.ps1 -Configuration Debug
```
