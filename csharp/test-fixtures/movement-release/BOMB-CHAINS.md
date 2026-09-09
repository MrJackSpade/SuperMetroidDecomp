# Ordinary bomb-chain parity (#412)

Status: initial short-chain matrix passes. Infinite/ladder chain coverage still
needs work; do not mark the whole issue ready based on these cases alone.

`native-bomb-chain-probe.h` executes unmodified cartridge instructions. The room
is 16 by 32 blocks, floor row 16, walls at columns 0/15, and ceiling at row 0 or
12. Samus begins in grounded Morph Ball at (128,249), zero subpixels/velocity,
99 health, Morph Ball + Bombs equipped, air, cheats off. Both facings are tested.

Every bomb is placed with Shoot input, not injected. The six schedules are a
single frame-zero bomb or three bombs at frames 0, N and 2N, with N in
40/44/48/52/56. Travel input is neutral, Left, or Right for frames 46–49.
The matrix contains 72 cases, 180 frames each, 12,960 total frames. It compares
position/subpixels, pose, bomb direction, vertical and horizontal velocities,
bomb count, and all five slots' type, fuse, position, animation-list pointer,
instruction timer and spritemap. These are explicitly bounded controller tests,
not a route through multiple rooms.

## Reproduced correction

Before the fix, the 24 diagonal/low-ceiling cases each differed for two frames
(48 mismatching frames). At fixture frame 74 the ceiling collision ends bomb
movement. Native retains horizontal speed $0000.3000 while the managed ordinary
no-input fallback cleared it. The native collision transition takes priority over
that fallback, just as for ordinary aerial ceiling collision. Including bomb
vertical collision in the shared post-movement collision transition corrects the
ordering; no bomb impulse, position, or speed table was adjusted.

All 12,960 frames match after the fix. The independent #413 full-fuse matrix also
still matches all 32,000 frames. The native capture was repeated independently
and the two CSV hashes agree:
`B618A4CA01E1B45C402E8110A600F251DB427E80A94BEA819482F2ECFE12695F`.

`bomb-chain-native-capture.zip` preserves that CSV, not ROM/save data. To capture,
include `native-release-probe.h` and this probe in `sm_rtl.c` after the forward
declaration of `StateRecorder`, then temporarily dispatch
`DiagnosticBombChains(romPath, newCsvPath)` before SDL initialization. Restore the
temporary hooks afterwards. Do not overwrite existing captures.

After extracting the archive to a new directory:

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --bomb-chain-comparison-audit "Super Metroid.smc" path/to/bomb-chain-412.csv
```

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native C: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

Remaining acceptance: identify and assert successful repeated airborne chains,
horizontal/ladder traversal and neighboring timing misses, rather than assuming
that issuing three bombs proves an infinite or ladder bomb jump. Preserve the
same explicit frame/input and lifecycle comparisons when expanding the fixture.
