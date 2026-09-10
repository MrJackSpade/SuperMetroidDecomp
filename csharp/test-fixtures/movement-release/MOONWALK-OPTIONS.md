# Moonwalk options handoff (#467, partial)

This covers the settings portion only. Gameplay entry, interruption, charge
and reversal evidence is recorded separately in MOONWALK.md.

## Reproduced defect

Real frontend inputs selected a saved file, changed Shoot to R, toggled Icon
Cancel and Moonwalk, then continued through both map selection screens.
Before the fix, the resulting runtime reported the old values:
`moonwalk=False, icon=False, shoot=0040`.

`SetupSelectedGame` first copied live options, then overwrote them from SRAM.
`InitializeSavedGame` also restores these fields for direct diagnostic loads.
The frontend now preserves live settings on the Ceres path and reapplies them
after the ordinary saved-room initialization. Direct runtime loads still use
the saved values. This does not write SRAM merely because options were edited.

## Verification

Run `--moonwalk-options-audit ROM` through SuperMetroid.DebugRunner. It uses
disposable in-memory SRAM and actual menu inputs, covering enable and disable,
continuation and abandoning/reselecting the file, all three affected settings,
save encode/decode, and isolation of another slot. Four cases pass after the fix.
No player save or debugger state is read or overwritten.

`native-moonwalk-options-probe.h` uses the actual cartridge CPU, with the shared
retail loader from `native-release-probe.h`. Include both after StateRecorder
and dispatch `DiagnosticMoonwalkOptions(rom)` before SDL for a headless run.
It executes SaveToSRAM ($81:8000), LoadFromSRAM ($81:8085), Right on the Moonwalk
row ($82:F024), and StartGame ($82:EEB4), then saves/reloads the result. SRAM
writes remain in modeled cartridge RAM, not the host save-writing C function.
Its results are:

```
initial=0 abandon=0 continued-and-saved=1
initial=0 abandon=1 continued-and-saved=0
initial=1 abandon=0 continued-and-saved=0
initial=1 abandon=1 continued-and-saved=1
```

The native probe isolates menu/SRAM routine boundaries; it does not claim to
run the complete title-to-room sequence. The C# reproduction does exercise that
frontend sequence. Cartridge source cross-check confirms options reside in
live WRAM after file selection, and StartGame does not reload their old values.

Pinned ROM SHA-256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
upstream-sm: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`, bank 82
GameOptionsMenu_4_StartGame and GameOptionsMenu_8_SpecialSettings.

Reference: https://wiki.supermetroid.run/Moonwalk (settings and abandonment).
