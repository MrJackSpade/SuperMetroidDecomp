# Ordinary save/reload cancellation (#430)

`save.csv` covers both verified Flash/Draygon generation orders and facings,
through 351 frames each. At the final checkpoint the original CPU executes
SaveToSram ($81:8000), LoadFromSram ($81:8085), and Samus_Initialize ($91:E00D).
The save adapter maps a disposable 8 KiB LoROM SRAM bank; it does not intercept
or replace the native encoding, checksum, decoding, or initializer.

Saving alone preserves timer 5 and palette owner 7. Loading and initializing
clears both words. Before loading, the probe overwrites live health with zero
and requires the saved health to be restored, so a skipped SRAM load cannot
pass merely because initialization clears the timer.

The port generates the same state using real runtime movement, then uses
SuperMetroidSaveSnapshot.Capture, SuperMetroidSaveRam.SaveSlot/ReadSlot and
SuperMetroidRuntime.InitializeSavedGame. Its disposable save targets the real
Crateria station-zero load record. All 1,404 movement checkpoints and four
save/reload boundaries match. The port also requires inactive Crystal Flash
and Shinespark owners after reload. No production fix was needed.

This is the ordinary persistence/reinitialization boundary, not the save-pod
activation animation, host JSON serialization, or debugger snapshot loading.
No player's SRAM, JSON or debug-state file is read or written. A debugger
snapshot is intentionally not used: it preserves transient emulated state.

Pinned source cross-check: `upstream-sm/src/sm_81.c` saves player words starting
at equipped items, not the transient shine timer; `sm_91.c` Samus_Initialize
clears $0A02..$0E0B, including the retained timer/palette owner. Expected values
come from executing the original ROM routines, not from these annotations.

Generate: `DraygonCrystalAudit/audit.exe "Super Metroid.smc" save`.
Compare: DebugRunner `--flash-save-audit "Super Metroid.smc" <save.csv>`.
ROM identity is pinned in the adjacent fixtures. Normalized LF UTF-8 trace
SHA-256: `A8FEC4FF65CD6ADBD5CFA44EFEFAFF03A5AC18F26CF9E465BB4D08246480CB87`.
