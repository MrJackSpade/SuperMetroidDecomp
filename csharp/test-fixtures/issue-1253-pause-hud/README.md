# Pause HUD location highlight (#1253)

The player's cartridge screenshot shows the yellow/orange square in the small
HUD minimap, not the four-corner position cursor in the large pause map. The first
investigation tested the latter and did not address the reported visual.

`probe.cpp` executes original, unpatched ROM instructions at `$90:A91B`
(UpdateMinimap), followed by 24 calls to `$80:9B44` (HandleHudTilemap). This is the
HUD writer called by paused state `$82:90E8`; that state does not call UpdateMinimap.
The probe isolates these owners, not the full pause transition or a playthrough.

At room-map coordinates (28,1), Samus (512,768), the native center word is `$BC26`
for blink phase 0 and `$A826` for phase 8. Advancing the NMI byte through 24 paused
HUD writes preserves each word. `$90:AB4A..AB59` only ORs `$1C00` into the center
when the last minimap-update frame has bit 3 clear. Pause retains either phase;
it does not force the orange highlight on. `native.csv` records both executions.

The C# `--pause-map-position` check also runs `VerifyPauseHudLocation`: it produces
the HUD through the installed gameplay path, accepts its DMA, enters the paused
dispatcher, and checks 24 HUD/NMI updates for each phase. All opaque center-cell
pixels in the pause compositor and render snapshot are compared with the pinned
ROM's raw 2-bpp characters and pause palette. Captures are written under
`csharp/test-temp/issue-1253-hud/phase-{0,8}.png`. Both phases agree with native.
No production change or resolution of the player's specific discrepancy is claimed.

Reference: [bank $90 minimap routine](https://patrickjohnston.org/bank/90#fA91B),
cross-checked against the pinned local ROM and native source routines. ROM SHA256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Original `upstream-sm/src/snes/cpu.c` SHA256:
`A5D88B0F2E0798482A2CAE9DDDAF602FEC69A8FD26C55B167417C4C92EEC30A6`.

Build `probe.vcxproj` with VS 2026 MSBuild, Release/x64, then run
`csharp/test-temp/issue-1253-pause-hud/probe.exe upstream-sm/sm.smc`.
The guarded native entry point installs the no-dialog policy and catches failures.
`SyncCThrow` permits exceptions from the C-linkage CPU bus callbacks to reach it.
