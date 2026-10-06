# #1165 enemy instruction programs — semantic layout review

Applies to the stored-word instruction program definitions converted onto
`InstructionProgramLayout` (csharp/src/SuperMetroid.Core/Game/InstructionProgramLayout.cs).
Single-thread continuation of streams #1238–#1242; replaces per-stream reporting for these entries.

## What was stored

Each program owned either a dense `Words` array (word values with presentation markers) or
address/value records plus a `PresentationWords` address list, or a factory that rebuilt both at
type initialization (Dachora). Every native address was stored alongside its value.

## Implemented representation

Programs are now written in native order as their instructions:

- `Op(opcode, operands…)` — named handler (bank-local catalog constant: `CommonEnemyInstructionCodes`,
  `EnemyProjectileCodePointers`, family `*InstructionCodes`, or a private constant carrying the pinned
  symbol on its XML summary) and its operands. Branch targets are the program's named entry constants;
  movement and callback operands name the routines they select. Packed byte operands are
  `InstructionWord.Bytes(low, high)`.
- `Frame(duration)` — one timed pose: its duration and an installed-presentation slot.
- `Entry(name)` — asserts the named native entry point is reached at that position; a drifting layout
  fails at type initialization.
- `Origin(address)` / `Skip(bytes)` — gaps owned elsewhere (other programs, packed sound bytes).
- `IndefiniteDuration` (`$7FFF`) names the hold-until-moved frame timer instead of a magic value.

Word addresses, mechanics/presentation classification, counts, mechanics reads, byte ownership,
presentation lookup and entry frame runs (`FramesFrom`) all derive from item order. Nothing is cached
or rebuilt into an address table. Each class declares its literal `Bank` for the static audits.

## Exactness evidence

- The generator (session tool) took word order and mechanics/presentation classification from the
  current stored definitions (reflection dump of all 165 program types), and every word's name from
  the pinned disassembly `dw`/`db` tokens. Each token was validated against the ROM word: numbers by
  value, symbols by their assembled bank-local address, `regional()`/`!FPS` by the NTSC branch.
  Disassembly address-comment typos (e.g. `$A3:C8EA` for `$A3:C8EC`) were resolved by that validation.
- After conversion, a fresh dump of all 165 program types is identical to the pre-conversion dump
  (mechanics addresses/values and presentation slot addresses).
- Each program's existing single original-ROM proof passes unchanged (flags listed in the commit).

## Disposition of frame durations

The only literal data left are frame durations. The native interpreter copies each duration into
the enemy's instruction timer and advances when it expires; no position, speed, health or other
simulation quantity is computed from the value, and the programs that share a pose set choose their
own per-pose holds (e.g. Yard's 7/4/7 turn versus 9/13/9 crawl, Dachora's 5/3/1 running tiers,
Shitroid-style pulses). These are the authored animation and action cadence the parent ticket names
as arbitrary sequencing content. Writing them as a formula or switch would only recite them. Uniform
runs appear as repeated `Frame(n)` items; the `$7FFF` sentinel is named.

Entries: `…InstructionProgramDefinitions.Words` (and Dachora `Sources`) are **mixed
justified-retained** — control calculated, durations retained as cadence.
`…PresentationWords` are **converted** — slot addresses derive from the layout.

## Already-calculated programs (disposition only)

These 36 programs were already expressed as calculated layouts by the earlier streams; their
code is not churned. Their residual frame holds (and a few named action/placement values) carried
"unresolved"/"required" notes. Each owner now records the reviewed disposition beside the value:
authored animation cadence, authored action timing (attack, stagger, launch delays whose relationships
stay calculated), authored repetition counts, or placement attached to drawn poses (e.g. Chozo footstep
spawn points, Walking Pirate barrel heights, Norfair Rio flame offsets as named per-pose dispatch).
Where a short series admits an incidental fit (Skultera 13/10/8/6) the owner says so; that is not a
derivation. Already-derived sequences (KiHunter acid splash ramp, Mother Brain hand-beam stage repeats,
mirrored turns) remain calculated. `Unresolved*` constant names were renamed to their roles.

Programs: ChozoStatue, EnemyDeath, GoldenTorizoAwakening, GoldenTorizoEyeBeam, Hopper, KiHunterAcidSpit, LowerNorfairRio, MamaTurtle, MorphBallEye, MotherBrainBaby, MotherBrainHandBeam, Multiviola, NinjaSpacePirate, NoobTubeProjectile, NorfairLavaJumper, NorfairRio, Phantoon, PhantoonProjectile, Rinka, Rio, Shaktool, ShaktoolProjectile, SharedCrawler, Shitroid, SkreeMetaree, Skultera, SpacePirateProjectile, Spark, SporeSpawnProjectile, Stoke, StokeProjectile, TorizoExplosion, TourianStatueProjectile, WalkingSpacePirate, WorkRobot, YappingMaw.

## Programs on the semantic layout

`∞` is `IndefiniteDuration` ($7FFF).

| Program | Bank | Entries checked | Instructions | Frames | Durations (value × count) |
| --- | --- | ---: | ---: | ---: | --- |
| Crocomire | $A4 | 34 | 189 | 236 | 1×27 2×37 3×12 4×29 5×66 6×3 7×6 8×27 9×2 10×6 16×4 20 32×3 34 48 180 ∞×10 |
| Dachora | $A7 | 15 | 15 | 81 | 1×13 3×12 4×6 5×16 7×16 8×4 10×8 11×4 48×2 |
| DeadSidehopper | $A9 | 6 | 5 | 11 | 1×3 2 4×3 5×3 48 |
| DeadTorizo | $A9 | 2 | 2 | 0 | — |
| Draygon | $A5 | 37 | 130 | 250 | 1×52 2×26 3×24 4×22 5×44 6×46 7×4 8×4 10×12 16×4 21×2 32×8 64×2 |
| Etecoon | $A7 | 20 | 16 | 45 | 1×6 3×8 5×10 6×2 8×9 12×8 32×2 |
| GoldenTorizoEyeBeamAttack | $AA | 3 | 26 | 0 | — |
| GoldenTorizoLeftFootOrb | $AA | 1 | 8 | 10 | 3×8 6×2 |
| GoldenTorizoLeftOrb | $AA | 2 | 16 | 20 | 3×16 6×4 |
| GoldenTorizoLeftTurn | $AA | 2 | 7 | 2 | 8 24 |
| GoldenTorizoRightOrb | $AA | 1 | 8 | 10 | 3×8 6×2 |
| GoldenTorizoRightSonic | $AA | 2 | 14 | 40 | 1×12 3×24 4×4 |
| GoldenTorizoRightward | $AA | 4 | 39 | 12 | 4×8 8×3 24 |
| GoldenTorizoStunned | $AA | 2 | 18 | 0 | — |
| GoldenTorizoSuperMissile | $86 | 5 | 12 | 24 | 2×16 5×6 48×2 |
| GoldenTorizoWalking | $AA | 2 | 32 | 10 | 4×8 8×2 |
| KiHunter | $A8 | 13 | 21 | 59 | 1×11 2×22 6×4 8×10 11×4 16×2 24×2 32×4 |
| Magdollite | $A8 | 21 | 69 | 53 | 1×13 5×28 8×2 13×8 26×2 |
| MaridiaLargeSnail | $A2 | 8 | 20 | 60 | 1×6 2×6 3×6 4×2 7×32 16×6 18×2 |
| MotherBrainTopTube | $86 | 4 | 4 | 4 | 1×4 |
| Ridley | $A6 | 9 | 100 | 86 | 1×12 2×4 3×2 4×8 5×14 6×10 8×18 12×2 16×2 17×4 32×2 48×4 80×2 96×2 |
| RoomSpriteObject | $B4 | 0 | 60 | 471 | 1×122 2×63 3×68 4×34 5×69 6×6 7×4 8×18 9×2 10×62 12 16×18 24 32 48×2 |
| SporeSpawn | $A5 | 5 | 45 | 41 | 1×8 6 7×2 8×19 16×7 208 256 512 768 |
| TorizoFallingLeft | $AA | 3 | 7 | 1 | 5 |
| Torizo.BombLeft | $AA | 16 | 117 | 129 | 1×15 2×12 3×24 4×5 6×32 8×19 10×2 16×9 24×3 32×2 48×2 56×2 72×2 |
| Torizo.BombRight | $AA | 50 | 215 | 165 | 1×25 2×12 3×24 5×34 6×32 8×21 10×2 16×7 24×3 32 56×2 72×2 |
| Torizo.GoldenLeft | $AA | 24 | 102 | 142 | 1×26 3×81 4×14 5 6×8 8×5 12 16×2 32×3 48 |
| Torizo.GoldenRight | $AA | 22 | 202 | 128 | 1×38 2×34 4×20 6×4 8×18 10×4 16×4 24×2 48×4 |
| Torizo.Shared | $AA | 5 | 59 | 0 | — |
| Torizo | $AA | 0 | 0 | 0 | — |
| TorizoJumpBack | $AA | 1 | 22 | 8 | 1×2 5×6 |
| TorizoJumpBackLeft | $AA | 1 | 22 | 8 | 1×2 5×6 |
| TorizoLandingDust | $86 | 2 | 8 | 8 | 4×8 |
| TorizoSonicBoom | $86 | 6 | 14 | 11 | 2×3 3×2 6×4 80×2 |
| WallSpacePirate | $B2 | 8 | 60 | 42 | 1×3 5×8 8×16 9×2 10×12 15 |
| Yard | $A3 | 38 | 108 | 112 | 1×8 3×8 4×16 5×8 7×32 9×16 13×8 16×8 48×8 |
