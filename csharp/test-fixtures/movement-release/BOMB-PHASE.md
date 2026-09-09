# Corrected bomb collision phase (#485, #412, #413)

GameState_8 at $82:8B44 calls SamusProjectileInteractionHandler before EnemyMain,
then runs Samus movement/alpha. EnemyMain checks each enemy's bomb overlap before
touch and AI. Sampling after alpha reads a fuse/radius one update too new and
allows input arbitration to precede a bomb interruption incorrectly.

#485's full EnemyMain reproduction exposed both production ordering errors.
It also exposed an error in the older #412/#413 native probes: they called the
correct cartridge instructions at the wrong phase. Their old archives remain
as historical evidence, **not accepted regression oracles**. This document and
`bomb-phase-native-capture.zip` replace those captures; no ROM bytes or player
states were changed. The captured original CPU code itself is unmodified.

## Preserved reproduction

The three probe headers retain the original room geometry, controller schedules,
and compared fields described in BOMB-CHAINS.md and HURT-BOMB.md. Only the overlap
call moves before alpha input/fuse updates. Each capture was independently run
twice; all eight pairs are byte-identical. SHA-256 hashes are enforced by
Verify-BombPhaseMatrix.ps1 rather than accepted from arbitrary matching files.

| Capture suffix | Coverage | Frames |
| --- | --- | ---: |
| bomb-phase-485-0 | Short chains | 12,960 |
| bomb-phase-485-1 | Repeated chains | 64,800 |
| bomb-phase-485-2 | Three-bomb handoffs | 73,440 |
| bomb-phase-485-3 | Horizontal steering | 112,320 |
| bomb-phase-485-4 | Ladder schedules | 21,600 |
| bomb-phase-485-5 | Ceiling steering | 138,240 |
| hurt-phase-485 | Constructed overlap/hurt arbitration | 12,800 |
| live-hurt-phase-485 | Normal placement, full fuse, hurt arbitration | 32,000 |

All 468,160 frames match. This supplements #485's unchanged 7,200-frame capture.
Important corrected observations:

- Horizontal-steering case 0/1/0/70 releases before the ceiling strike, rather
  than on it. Frame75 is stationary ball at Y=$00D7.37FF; frame76 reaches the ceiling.
- Ladder ceiling interruption is frame128, not127. Most tested final landings
  are frame209, not208; left-facing travel2 instead contacts the floor at216,
  simultaneously arms another bomb launch, and retains the falling pose.
- Ceiling-steering case 0/1/0/6 launches vertically at the wall edge, with Y
  $00F7.3FFF at466. The old diagonal-collision assertion does not describe it.
- Full-fuse hurt cases retain 78 launches and subsequent boosts, not84. All
  remaining timing outcomes are compared, not discarded to select successes.

Explicit boundary assertions now reflect these native observations. Sustained
horizontal/ceiling traversal remains unfinished under #412; matching this bounded
matrix does not finish that technique's broader acceptance criteria.

## Re-run

Extract the archive into a new directory. Build DebugRunner, then run from root:

```powershell
dotnet build csharp/src/SuperMetroid.DebugRunner -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File csharp/test-fixtures/movement-release/Verify-BombPhaseMatrix.ps1 -TraceDirectory path/to/extracted
```

To recapture, include native-release-probe.h followed by native-bomb-chain-probe.h,
native-hurt-bomb-probe.h and native-live-hurt-bomb-probe.h after StateRecorder in
sm_rtl.c. Temporarily dispatch DiagnosticBombChainVariant(rom, output, variant0..5),
DiagnosticHurtBomb or DiagnosticLiveHurtBomb before SDL startup. Each output path
must be new. Remove the hooks after capture; no GUI or player save is involved.

ROM Japan/USA rev0 SHA256:
`12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native source: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.
