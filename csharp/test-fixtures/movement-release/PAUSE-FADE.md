# Cartridge pause-fade timing (#471)

Run `SuperMetroid.DebugRunner --pause-fade-comparison-audit "Super Metroid.smc" pause-fade-471-v1.csv`.
The trace is packaged in `pause-fade-native-capture.zip`; SHA-256:
`E68B593B2CD1D50D14F00ACF7143487ABBB7ED60A8C6D9CA2FBE7B171AE5BED6`.
Two independent native runs produced that hash.

ROM: Japan/USA rev 0, SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native harness: `upstream-sm` at `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `upstream-disassembly` at `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

`native-pause-fade-probe.h` runs the unpatched cartridge CPU. It calls the actual
pause admission routine ($90:EA45), confirms state $0C and both fade words equal
one, then captures $80:8924/$80:894D for each of the four fade directions. The
three other callers initialize the same words at $82:8D2F, $82:A5C9 and $82:937C;
this focused probe reuses that initialization rather than executing their
graphics and pause hooks. It is a recurrence/brightness capture, **not a full
native pause-menu/movement run**. No player saves or cartridge bytes are included.

The C# comparator enters and exits pause through actual frontend inputs, without
assigning dispatcher states, brightness or counters. It compares all 120
published brightness samples and requires all four exact 30-frame boundaries.
Before the fix it failed immediately: cartridge brightness 15, C# brightness 14
on pause-darkening frame zero. C# had omitted the delay counter and changed
brightness every frame. Native changes it every second frame; gameplay continues
on both frames during entry darkening and restoration.

The production fix shares that counter recurrence across all four pause fades.
The core capture integration test also asserts each 30-frame duration while
comparing legacy/captured pixels, Samus state and audio commands. Its controller
slice is now 180 rather than 160 frames to include the complete restored fade.
The 32-case charge-carry frontend baseline still passes, with Start/Down shifted
15 frames earlier to retain the same airborne freeze points. That baseline's
native movement/charge comparison is covered separately by [PAUSE-CHARGE.md](PAUSE-CHARGE.md).

To recapture, include `native-release-probe.h` and this probe in `sm_rtl.c`, then
dispatch `DiagnosticPauseFade(rom, output)` before SDL initialization. Temporary
hooks are removed after capture. The helper restores ROM bytes after `SnesInit`
and suppresses Windows fault dialogs; run it headlessly.
