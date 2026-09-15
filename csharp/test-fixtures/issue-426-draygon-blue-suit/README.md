# Draygon death interruption: native Blue Suit comparison

Partial #426. This covers death during an uncrashed spark, the retained-running-
momentum exception, recovery, walking contact damage, Dash cancellation, and
storing/launching another spark. Grab-at-activation and rendered echo-cue coverage
remain open; this is not a claim that the entire issue is complete.

## Reproduction

`csharp/native/TemporaryBlueSuitAudit` executes unmodified Japan/USA 65816 code.
The controller earns Speed Booster on a constructed 144-by-80-block runway with
a floor at row 32. Both facings start with Morph Ball + Speed Booster, 99 energy,
no host cheats, and no injected boost or shinespark state.

All cases run through frame 139, crouch with angle-up on frame 140, jump on 150,
and launch upward. Modes whose remainder modulo four is 0 or 1 keep angle held
before jumping and retain running momentum; modes 2 or 3 release angle during
141..149 and clear momentum while keeping the stored spark. Even modes kill
Draygon at frame 175; odd modes at 180. The interruption occurs after the sampled
Samus phases, so the following frame observes its consequences.

Native invokes the fatal post-damage callback $A5:960D with enemy health zero.
C# loads the retail four-record Draygon population in a separate collision
fixture and sends a constructed lethal charged-Plasma shot through its real eye
hitbox and shot resolver. Frozen EnemyMain prepares the collision index. Samus
is the actual runway runtime object, not a replacement or an edited snapshot.
Boss AI, visuals, and a complete fight are outside this local boundary.

The native alpha entry already executes HUD/grapple cleanup when needed. No
second grapple call is inserted. Each row compares position including subpixels,
pose, animation frame/timer, base/extra speed, boost/contact words, shine/palette
timers, vertical speed and direction. There are 24 cases, each 400 frames.

After landing, frames 340..349 walk without Dash. Modes 0..3 then idle; 4..7
press Dash+forward on 360..369; 8..11 crouch with angle on 360 and jump/launch
again on 370..371. Successful cases retain $0400 after landing, publish contact
damage 1 while walking, replace it with a fresh $0001 run counter on frame 361
when Dash is pressed, or store 179 remaining shine ticks on frame 360 and launch
a second vertical spark. Retained-momentum controls lose Blue Suit after death.

## Defects reproduced and corrected

- The port guarded Draygon's death release with `IsActive` (grabbed), although
  native release is unconditional. It left Samus in the active spark handler.
  Release now restores standing/normal movement while preserving boost and
  independent palette state. Unchanged-pose animation is not restarted.
- Vertical spark movement clamped the stored Y-speed word to 14. Native clamps
  only the temporary movement distance and keeps accumulating wrapped velocity.
- Grapple drop cleanup consumed the whole beta frame and applied its pose early.
  Runtime cleanup now clears speed before movement, queues the pose, permits the
  old normal mover, and commits after animation without a second speed clear.
- Launch's RTS pose-input handler was conflated with movement ownership. Native
  Draygon release leaves that input handler installed until landing restores the
  one-shot auto-jump handler. The port now preserves this independent lock and
  restores ordinary input at landing or the normal crash-finish transition.

The pre-fix run failed at frame 155 on stored vertical speed and at the exact
death frame (175) on pose/handler/velocity. Intermediate comparison exposed the
cleanup-frame and post-release input mismatches; the final 9,600 rows match.

## Run

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --draygon-blue-suit-audit 'Super Metroid.smc' `
  csharp/test-fixtures/issue-426-draygon-blue-suit/death.csv
```

Regenerate with the existing native build script, then run `audit.exe ROM draygon`.
ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
UTF-8/LF-normalized trace SHA-256:
`88E6A9B40CDD192C81BA4012795FA2A6CFF5E247989D773343233D99D112EEEE`.

Only numeric checkpoints and diagnostic code are published. No ROM bytes,
SRAM, player snapshots, movies, or images are included.

Cross-checks: pinned `upstream-sm/src/sm_a5.c` ($A5:960D), `sm_90.c`
($90:E2DE, $90:D1FF), `sm_9b.c` ($9B:C8C5), and `sm_91.c`
($91:FACA, $91:F1EC, $91:EB88), plus the
[Blue Suit technique reference](https://wiki.supermetroid.run/Blue_Suit_Glitch#Draygon).
