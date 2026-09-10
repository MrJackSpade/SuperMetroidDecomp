# Kago against a falling Kzan pair (#455, partial)

## Independent reference

Original Japan/USA revision-zero ROM, SHA-256
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Host pin `578f90b3cc49557bb70060ad033bb90b8cf8ac50`;
disassembly pin `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

The native header executes original CPU instructions, including initialization,
enemy-list construction `$A0:8EB6`, EnemyMain `$A0:8FD4` (pre-AI collision,
AI, instructions), Samus alpha, movement, animation, pose transitions and timer
epilogue. It does not inject a successful touch callback or manufacture a result.
The bounded headless entrypoint patch suppresses explicit SDL error/warning
dialogs and does not start a game window. Temporary host hooks were removed
after capturing; apply the archived patch and rebuild before reproducing.

## Setup

72 independent cases of 80 frames (5,760 frames): two facings, three controller
patterns, and 12 input delays (0 through 11).

- Synthetic 144x80-block room, solid floor row 32, everything else air.
- Samus X=1024, Y=491 standing or Y=505 in initial Morph Ball; zero fractions,
  correct matching previous pose/direction/movement, animation frame zero/timer
  one, NMI starts at two. Morph Ball only, no beams, 999 health, no cheats/liquids.
- Kzan top `$DFFF` at (1024,448), bottom `$E03F` follows at top Y+12. Original
  header radii, health, bank, AI, and instruction data. Parameters `$0040/$8008`:
  eight-frame wait, four-pixel initial fall speed, 128-pixel travel.
- Top properties `$A800`, bottom `$0900`: retail solidity/invisibility and
  instruction flags plus offscreen processing for deterministic camera isolation.
- Turn: opposite direction held from frame 8+delay through frame 39.
- Morph: Down taps at 8+delay and 12+delay.
- Unmorph: start in ball, Up tap at 8+delay.
- Clear external displacement at the terrain-preparation boundary, then allow
  original enemy carry to publish its actual displacement before beta movement.

The C# side loads the same pair through the production enemy loader and calls
complete `SuperMetroidRuntime.StepFrame` for every input. It compares Samus's
position/fractions, pose, movement type, animation/timer, X base/extra speed,
acceleration/facing, Y speed/direction, charge counter, health, invincibility,
hurt timer and knockback direction, plus top fixed Y/function and bottom Y.

## Reproduced defects and fixes

Before changes: **476 mismatching frames**.

1. Ground-family ball art can survive a hurt trajectory with nonzero vertical
   speed. Runtime wrongly inferred an airborne landing from that speed and ran
   the bounce/landing handler. Native `$90:E637`'s collision table selects the
   no-transition entry for grounded Morph Ball and Spring Ball. Restrict the
   handler to the actual airborne pose family. This removed 448 mismatches.
   Both-facing witness: morph pattern, delay 0, frame 24 retains Y `01F1.A7FF`,
   speed `0000.1C00`, direction down, instead of clearing speed/direction.
   Spring Ball follows the same explicit table rule; this capture directly
   reproduces ordinary Morph Ball, not a Spring Ball variant.
2. Shared pose expansion mixed enemy and block probes. Native first probes
   solid enemies, checks the opposite direction with the expanded radius, then
   separately probes terrain; mixed-side collisions have their own fallback.
   Splitting those stages preserves the native live-fraction write order and
   eliminates the remaining 28 mismatches. Both-facing witness: turn pattern,
   delay 11, frame 26 lands at Y `01EB.FFFF`, not `01EB.0000`.

Adjacent timing assertions: turn delay 2 takes 200 damage on frame 11 without
knockback; delay 3 takes the same damage with knockback. The test is not an
invulnerability cheat. All **5,760 frames now match**, including actor movement.

Two independent native captures have identical SHA-256:
`B213A17EB63EC3A0F7192239D929B737ACDB4BA072E0DA64BFF3881195BE63BC`.
The numeric CSV is in `kago-kzan-native-capture.zip`.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --kago-kzan-audit 'Super Metroid.smc' PATH/kago-kzan-455-v1.csv
```

Additional verification: full Core Verification passed; ordinary Kago contact
(5,400 frames) and all four Quick Drop comparisons (37,852 frames) still match.
Total compared in this change: 49,012 frames. The previous solidity boundary
capture remains an independent low-level fixture, not part of that frame count.

## Remaining issue scope

This is a constructed falling-Kzan interaction, not every possible Kzan approach
or a full playthrough. Kamer controller/carry interactions still require their
own comparison. Do not mark #455 complete or awaiting player validation until
those remaining named actor cases are handled. No production Kago-specific
exception was added: both fixes correct shared native dispatch/order.
