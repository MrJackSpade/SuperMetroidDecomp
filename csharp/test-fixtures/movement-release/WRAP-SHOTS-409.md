# Side-edge Wave addressing (#409): first reproduced defect

The original cartridge uses a linear tile byte offset for the vertical Wave
scan. A horizontal span may cross a row boundary while its center is still in
the room. The port rejected the whole span when either endpoint was outside the
room's X bounds. This prevented both right-edge next-row hits and left-edge
underflow into later room data.

`native-wrap-shot-probe.h` executes original cartridge fire dispatch `$90:B80D`
and projectile processing `$90:AECE`; it does not call the translated collision
implementation. Cross-check: pinned disassembly `MoveBeamVertically_WaveBeam`
at `$94:A3E4`, especially the byte hardware multiply, word offset construction,
center/target-edge screen gates and two-byte span increments. The fix retains
those gates and bounds the final linear allocation offset; it does not let
projectiles access host memory or continue colliding after their center exits.

## Deterministic fixture

Fresh CPU/WRAM or managed address space per case; 64x128 blocks, four by eight
screens, logical allocation 16,384 bytes. Right case: Samus (992,128), pose 7;
left case: (32,128), pose 8. Subpixels and velocities zero. Shoot is held/new only
on frame 0. No movement, equipment cheats, enemies, camera tracking or PLM frame
execution. Fixed camera X=768/right or 0/left, Y=0. Beam words 0,1,5 exercise
plain Power, Wave, and Wave+Spazer through ordinary production spawning and
animation. Each case runs 40 frames, including deletion/empty-slot frames.

The launch-side outer column is solid. The opposite column contains type-four,
BTS-zero shootable-air targets. Each native PLM setup changes its contacted word
from `$4000` to `$0052` synchronously, so all remote writes are directly observable
without inventing a test-only collision hook. Rightward Wave variants first hit
index `$0280` (column 0, row 10) on frame 4; leftward variants hit `$123F`
(column 63, row 72) on frame 3. Plain Power hits no remote target.

Before the fix, 146 of 240 records diverged. Afterward all records match: full
position/subpixels, type/instruction lifetime and every target-column word change.
Assertions also require the exact remote-hit frames and a live projectile at the
reaction. The matrix runs in the default verification suite.

## Reproduction

ROM SHA256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native harness pin: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly pin: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

Temporarily include `native-release-probe.h` then `native-wrap-shot-probe.h` after
`state_recorder` in upstream `sm_rtl.c`. Before SDL initialization, dispatch
`DiagnosticWrapShot(rom, newOutput)` for `--wrap-shot-probe ROM NEW_OUTPUT`, with
explicit SDL error dialogs suppressed in that branch. Force Rebuild Release/x64,
PlatformToolset=v145. The bounded CPU loader reloads the original unpatched ROM.
All temporary hooks were removed after capture; no native game window launched.

Two independent captures and the tracked `wrap-shot-409.csv` agree at SHA256
`DABAFA7074E51468EB5DDA64C14AD87C67B454C4A0969842982F25F9904E045D`.
Only synthetic numeric observations are committed, not ROM or save bytes.

```
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --wrap-shots csharp/test-fixtures/movement-release/wrap-shot-409.csv
```

## Remaining acceptance

#409 remains open: the actual-room integration below complements this native
matrix, but its precise beam-width boundary still needs independent cartridge
comparison, and enemy non-aliasing assertions remain. This fixes the demonstrated
shared addressing defect; it does not claim the entire technique ticket is complete.
Ceiling byte-misalignment/PLM overload belongs to #410 and is not changed here.
The evidence is pinned NTSC only; no PAL claim.

## Actual remote-door integration

`VerifyRetailWrapShotDoors` loads each real room through the production loader,
including its resident PLM population. Terrain and BTS remain unmodified. It
then isolates ordinary projectile production/processing and PLM execution for
40 frames; it does not run Samus movement, enemies, room FX, or camera tracking.
The deterministic stationary launch coordinates are test setup, not a claim of
a controller-only route to the launch point. Shoot is pressed only on frame 0;
all following inputs are neutral. Fresh runtime per room/beam variant.

| Room | Launch X/Y | Target block | Beams | Door hit / all four cells clear |
| --- | --- | --- | --- | --- |
| Landing Site $91F8 | 2260 / 546, down-right | $1561 | Wave+Plasma | 9 / 27 |
| Crocomire $A98D | 2004 / 34, down-right | $0301 | Wave+Plasma | 9 / 27 |
| Green Brinstar shaft $9AD9 | 38 / 1606, down-left | $2981 | Wave+Spazer or Wave | 5 / 23 |

Plain Wave at the first two launch setups does not activate the remote cap;
plain Power fails in all three rooms. The shaft's plain Wave succeeds, so the
width requirement must not be generalized from the two right-edge setups.
Successful cases queue library-three sound 7 and run the actual blue-door
opening list; negatives neither open the target nor queue that sound. Assertions
cover activation frame, the four cleared cap cells at the exact completion
frame, and sound request. These nine managed integration cases pass and run in
the default suite; they are not additional original-CPU trace comparisons.

```
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --wrap-shot-rooms
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --wrap-shot-room-audit "Super Metroid.smc"
```

The search prints room cap locations and the first projectile/PLM candidate.
Its cloned terrain also loads the native room population: omitting that step
left the shaft's resident red-door trigger without an owner and was rejected as
an invalid diagnostic fixture, not hidden by a gameplay exception catch.
