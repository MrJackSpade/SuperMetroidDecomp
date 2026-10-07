# Construction Zone shots crossing the solid row (#1223)

Affected build: `0.4.12-smoke1222+1aedf1186421730fd5ee05ebdbca22691c92e3c9`.

The player's in-progress recording was copied with shared read access, preserving
66,960 inputs without changing the running game or its saves. Replaying installed
content and the recording-owned initial SRAM reproduced four ordinary leftward
beams at input frames 59485, 59679, 59770 and 59860 in Construction Zone ($9F11).
These are no-Wave beams (type $8000), not Wave Beam behavior.

The upper obstruction is row 8: solid columns 4, 5, 10 and 11 surround four
shootable blocks at columns 6..9. Row 7 above them is air. Samus is at Y=139.
Running shots have Y=131 and collision radii 8/4; stopped shots have Y=134.
The running shot therefore touches Y=127..134 (air row 7 and blocking row 8).
The stopped shot touches Y=130..137, entirely within row 8.

## Original cartridge result

`native-construction-shot-probe.h` runs original 65816 code at $94:A23B using
the project's NTSC J/U 1.0 ROM. The existing loader restores the retail bytes
after SnesInit's comparison patches. This is a bounded collision reproduction,
not a full native replay of the 18-minute session. Its constructed terrain uses
only collision types and the same row/column layout; no artwork or ROM data is
included. PLM setup runs, but subsequent PLM animation is outside this probe.

The seed is the actual first shot after input frame 59485:
X=181, Xfraction=61440, Y=131, velocity=-1296 (8.8), radii=8/4.
Each following step applies the native -16 horizontal acceleration and invokes
the original no-Wave horizontal collision routine. All 20 positions, fractions,
Y coordinates and projectile types matched recording frames 59486..59505.
The last result is X=67, fraction=36864, type=$8000, carry clear. All four
shootable-block PLMs are allocated despite the solid blocks on either side.
A control changes only Y to 134: the first step returns carry set and converts
the beam to explosion type $8700, with no shot-block actors allocated.

The native horizontal scanner checks each touched row. The counter begins at
(number of rows minus one) and decrements only for blocking reactions. It kills
the beam only after the counter becomes negative: every touched row must block.
Shootable-block side effects happen during the scan even when its aggregate
collision result is clear. The managed scanner preserves both behaviors.

Cross-checks: pinned upstream-sm/src/sm_94.c BlockShotReactHoriz / 
BlockCollNoWaveBeamHoriz, and pinned disassembly bank_94.asm
BlockShotReaction_Horizontal ($94:A1B5), MoveBeamHorizontally_NoWaveBeam
($94:A23B), especially $94:A2BA..A2C9.

**Conclusion: reproduced native behavior; no production change.** Changing this
to stop on any touched solid block would break cartridge parity.

## Repeat

Use the existing native-release-probe.h integration described in this folder's
README, then apply native-construction-shot-entrypoint.patch to a private native
checkout. Build Release x64 and run:

```powershell
./upstream-sm/build/bin-x64-Release/sm.exe --diagnostic-construction-shot 'Super Metroid.smc'
```

The entrypoint bypasses SDL startup and suppresses explicit error dialogs. The
loader installs the no-fault-dialog policy before loading the cartridge; the
bounded CPU fails to stderr/nonzero exit on instruction-budget exhaustion.

The existing guarded DebugRunner replays the private preserved recording:

```powershell
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0-windows/SuperMetroid.DebugRunner.dll --installed-input-replay RECORDING INSTALLATION 59330 59895 trace.csv
```

Private recording, screenshots, full terrain details, native output and isolated
probe build remain under out/workbook-investigation/1223-*; they are not committed.
The original replay CSV is out/workbook-investigation/shot-block-replay.csv.
