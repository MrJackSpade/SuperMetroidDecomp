# Enemy visual program completeness audit

Implemented for #1166 on `fix/enemy-visual-selector-v040`, based on `v0.4.0`.
This is a source/definition audit of the missing-selector and missing-composition
failure class. It does not run gameplay, replay input, open saves, or read a ROM.

## Result

After the fixes in `75a75487`, the expanded audit covers **169 program owners and
5,800 visual references**, with **zero missing resources and zero unresolved
owners**. References include shared/repeated operands; they are not 5,800 unique
artwork frames. No additional production omission was found by the expanded pass.

The three omissions were Kzan's selector and ordinary composition, Polyp's
selector and ordinary composition, and Polyp rock's selector inventory and
installed projectile composition. All are fixed. The same singular declaration
shape also occurs in Dead Torizo, whose specialized resolver and artwork already
worked. Its generator entry was added for inventory completeness.

## Independent inventories

The earlier generator and resource audit both began with plural presentation
metadata, which excluded singular constants and custom layouts. The new audit:

1. Discovers every `InstructionProgramDefinitions` owner from parsed Core source,
   regardless of whether it declares presentation metadata.
2. Independently adds definition owners called by the two common instruction-word
   readers. A renamed or unconventional owner must receive a disposition too.
   Unbound interpreter calls fail analysis.
3. Enumerates both indexed and singular visual declarations, split Skree/Metaree
   families, corpse programs, typed Ceres Baby operands, and shared projectile
   records. The Baby's palette operands are not mistaken for sprite operands.
4. Independently enumerates compiled mechanics words. A low-valued word followed
   by an absent word exposes an interleaved visual operand, including at the end
   of a catalog. Thus deleting a presentation declaration does not remove that
   dependency from analysis. Constant-count catalogs and split families are
   included; an unknown mechanics enumeration is an error.
5. Handles Mother Brain's body, falling tubes, hand-beam body and head layouts
   explicitly. All six head regions are decoded statically with explicit command
   widths; branches are not executed and dormant lists are included. Unknown
   commands, region overruns, or changed custom source invalidate the adapter.
6. Checks all compiled selector targets against ordinary/extended installed
   composition declarations. Projectile operands follow the actual installed
   binding/direct-sprite distinction and the bank-$86 versus bank-$8D boundary.
   Native empty frames are explicitly accounted for.
7. Includes typed custom Ceres elevator projectile records and Kraid head BG2
   records. Ceres's explicit source switch supplies its finite record domain.
   Kraid's importer and installation admission derive the required tilemap key
   set from its typed frame records; the reviewed importer source is guarded.

The five control-only owners are common projectile deletion, Golden Torizo's
eye-beam/landing/stunned programs, and the live Tourian entrance statue programs.
Their inspected source is fingerprinted. A changed file revokes the disposition;
these are not permanent name-based exemptions. The seven common interpreter,
word-reader and visual-routing methods have token fingerprints, so a changed
routing rule cannot keep using an obsolete audit assumption.

Missing selectors, missing artwork, unknown shapes, ambiguous banks, and revoked
source contracts all fail the command. The JSON report records owner/shape/count
rows, operand dependencies and findings. A clean result means completeness of
these compiled definition dependencies under the reviewed interpreter contracts.
It does not prove arbitrary corrupted cursor values valid, native correctness of
all data, animation timing, room behavior, or whole-game visual parity.

## Run and enforcement

```powershell
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -- --enemy-visual-audit-self-check
dotnet run --project csharp/src/SuperMetroid.ResourceAudit -- --enemy-visual-program-audit . out/enemy-visual-program-audit.json
dotnet build csharp/src/SuperMetroid.ResourceAudit -p:RunEnemyVisualProgramAudit=true
```

Build from the same checkout being audited. The dedicated workflow runs on Core,
extraction, or audit changes and uploads its JSON report. Tagged Windows packaging
also runs the gate; the release publish job requires that packaging job to pass.

Focused negative fixtures confirm discovery of single/unknown/renamed owners,
independent detection of undeclared and terminal operands, missing-selector and
missing-art rejection, bank isolation, projectile binding semantics, and source
disposition revocation. These fixtures confirm the new audit contract; they are
not gameplay defect searches.

The earlier `--single-frame-enemy-visuals` regression reproduced Kzan's actual
exception before the fix and now confirms all three corrected programs, exact
native OAM, and prior artwork override inheritance. Player confirmation remains
pending. No gameplay change beyond `75a75487` was required by this expanded pass.

## Program inventory

The following snapshot is generated from the successful audit's owner records.
Zero-word custom layouts use their explicit typed or linear-decoding adapters.

| Program owner | Bank | Shape | Visual references | Mechanics words/records |
| --- | --- | --- | ---: | ---: |
| AlcoonFireballInstructionProgramDefinitions | 86 | indexed | 4 | 6 |
| AlcoonInstructionProgramDefinitions | A8 | indexed | 44 | 68 |
| AtomicInstructionProgramDefinitions | A8 | indexed | 24 | 32 |
| BeetomInstructionProgramDefinitions | A8 | indexed | 32 | 48 |
| BlueBrinstarFaceBlockInstructionProgramDefinitions | A8 | indexed | 7 | 10 |
| BombTorizoDormantInstructionProgramDefinitions | AA | indexed | 1 | 6 |
| BombTorizoDroolInstructionProgramDefinitions | 86 | indexed | 7 | 19 |
| BombTorizoStatueInstructionProgramDefinitions | 86 | indexed | 32 | 96 |
| BotwoonInstructionProgramDefinitions | B3 | indexed | 25 | 74 |
| BotwoonProjectileInstructionProgramDefinitions | 86 | indexed | 46 | 73 |
| BoulderInstructionProgramDefinitions | A6 | indexed | 16 | 20 |
| BoyonInstructionProgramDefinitions | A2 | indexed | 10 | 18 |
| BrinstarPipeBugInstructionProgramDefinitions | B3 | indexed | 44 | 60 |
| BullInstructionProgramDefinitions | A8 | indexed | 8 | 16 |
| CacatacInstructionProgramDefinitions | A2 | indexed | 24 | 56 |
| CacatacProjectileInstructionProgramDefinitions | 86 | indexed | 10 | 20 |
| CeresBabyInstructionProgramDefinitions | A6 | typed-sprites-and-palettes | 20 | 43 |
| CeresDoorInstructionProgramDefinitions | A6 | indexed | 33 | 97 |
| CeresElevatorArrivalDefinitions | 86 | typed-projectile-records | 3 | 6 |
| CeresFallingDebrisInstructionProgramDefinitions | 86 | indexed | 2 | 4 |
| CeresRidleyProjectileInstructionProgramDefinitions | 86 | indexed | 26 | 42 |
| CeresSteamInstructionProgramDefinitions | A6 | indexed | 36 | 68 |
| ChootInstructionProgramDefinitions | A2 | indexed | 5 | 11 |
| ChozoStatueInstructionProgramDefinitions | AA | indexed | 52 | 166 |
| ChozoTourianDustInstructionProgramDefinitions | 86 | indexed | 14 | 31 |
| CommonEnemyProjectileInstructionProgramDefinitions | 86 | control-only | 0 | 1 |
| CrocomireInstructionProgramDefinitions | A4 | indexed | 236 | 434 |
| CrocomireProjectileInstructionProgramDefinitions | 86 | indexed | 13 | 22 |
| CrocomireTongueInstructionProgramDefinitions | A4 | indexed | 9 | 14 |
| DachoraInstructionProgramDefinitions | A7 | indexed | 81 | 110 |
| DeadSidehopperInstructionProgramDefinitions | A9 | indexed | 11 | 16 |
| DeadTorizoInstructionProgramDefinitions | A9 | single | 1 | 2 |
| DeadTourianCorpseInstructionProgramDefinitions | A9 | corpse-programs | 8 | 16 |
| DownwardGateProjectileInstructionProgramDefinitions | 86 | indexed | 9 | 28 |
| DragonFireballInstructionProgramDefinitions | 86 | indexed | 8 | 16 |
| DragonInstructionProgramDefinitions | A2 | indexed | 16 | 26 |
| DraygonInstructionProgramDefinitions | A5 | indexed | 250 | 524 |
| DraygonProjectileInstructionProgramDefinitions | 86 | indexed | 27 | 38 |
| ElevatorInstructionProgramDefinitions | A3 | indexed | 2 | 4 |
| EnemyDeathInstructionProgramDefinitions | 86 | indexed | 31 | 66 |
| EnemyPickupInstructionProgramDefinitions | 86 | indexed | 16 | 30 |
| EnemyProjectileInstructionMechanicsDefinitions | 86 | typed-projectile-frames | 246 | 307 |
| EscapeDachoraInstructionProgramDefinitions | B3 | indexed | 43 | 119 |
| EscapeEtecoonInstructionProgramDefinitions | B3 | indexed | 30 | 63 |
| EtecoonInstructionProgramDefinitions | A7 | indexed | 45 | 68 |
| EvirInstructionProgramDefinitions | A8 | indexed | 49 | 67 |
| EyeDoorProjectileInstructionProgramDefinitions | 86 | indexed | 11 | 19 |
| EyeDoorSweatInstructionProgramDefinitions | 86 | indexed | 4 | 8 |
| FakeKraidInstructionProgramDefinitions | A6 | indexed | 24 | 48 |
| FakeKraidProjectileInstructionProgramDefinitions | 86 | indexed | 3 | 6 |
| FallingSparkInstructionProgramDefinitions | 86 | indexed | 14 | 17 |
| FirefleaInstructionProgramDefinitions | A3 | indexed | 52 | 54 |
| FlyInstructionProgramDefinitions | A2 | indexed | 4 | 6 |
| FuneNamiheFireballInstructionProgramDefinitions | 86 | indexed | 6 | 10 |
| FuneNamiheInstructionProgramDefinitions | A8 | indexed | 38 | 62 |
| GoldenTorizoAwakeningInstructionProgramDefinitions | AA | indexed | 21 | 69 |
| GoldenTorizoEggInstructionProgramDefinitions | 86 | indexed | 26 | 53 |
| GoldenTorizoEyeBeamAttackInstructionProgramDefinitions | AA | control-only | 0 | 41 |
| GoldenTorizoEyeBeamInstructionProgramDefinitions | 86 | indexed | 17 | 28 |
| GoldenTorizoInitialInstructionProgramDefinitions | AA | indexed | 1 | 7 |
| GoldenTorizoJumpLandingInstructionProgramDefinitions | AA | control-only | 0 | 20 |
| GoldenTorizoLeftFootOrbInstructionProgramDefinitions | AA | indexed | 10 | 23 |
| GoldenTorizoLeftOrbInstructionProgramDefinitions | AA | indexed | 20 | 46 |
| GoldenTorizoLeftTurnInstructionProgramDefinitions | AA | indexed | 2 | 12 |
| GoldenTorizoRightOrbInstructionProgramDefinitions | AA | indexed | 10 | 23 |
| GoldenTorizoRightSonicInstructionProgramDefinitions | AA | indexed | 40 | 66 |
| GoldenTorizoRightwardInstructionProgramDefinitions | AA | indexed | 12 | 82 |
| GoldenTorizoStunnedInstructionProgramDefinitions | AA | control-only | 0 | 28 |
| GoldenTorizoSuperMissileInstructionProgramDefinitions | 86 | indexed | 24 | 43 |
| GoldenTorizoWalkingInstructionProgramDefinitions | AA | indexed | 10 | 70 |
| GrowingShutterInstructionProgramDefinitions | A2 | indexed | 4 | 8 |
| GunshipDustInstructionProgramDefinitions | 86 | indexed | 46 | 76 |
| GunshipInstructionProgramDefinitions | A2 | indexed | 22 | 28 |
| HibashiInstructionProgramDefinitions | A6 | indexed | 24 | 50 |
| HopperInstructionProgramDefinitions | A3 | indexed | 40 | 96 |
| HorizontalShutterInstructionProgramDefinitions | A2 | indexed | 1 | 2 |
| HZoomerInstructionProgramDefinitions | A3 | indexed | 20 | 36 |
| KagoBugProjectileInstructionProgramDefinitions | 86 | indexed | 11 | 23 |
| KagoInstructionProgramDefinitions | A8 | indexed | 8 | 12 |
| KiHunterAcidSpitInstructionProgramDefinitions | 86 | indexed | 19 | 27 |
| KiHunterInstructionProgramDefinitions | A8 | indexed | 59 | 84 |
| KraidArmInstructionProgramDefinitions | A7 | indexed | 57 | 66 |
| KraidFootInstructionProgramDefinitions | A7 | indexed | 106 | 193 |
| KraidHeadInstructionDefinitions | A7 | typed-bg2-records | 21 | 28 |
| KraidLintInstructionProgramDefinitions | A7 | indexed | 2 | 4 |
| KraidNailInstructionProgramDefinitions | A7 | indexed | 8 | 10 |
| KraidRockProjectileInstructionProgramDefinitions | 86 | indexed | 7 | 12 |
| KzanInstructionProgramDefinitions | A6 | single | 1 | 2 |
| LowerNorfairRioInstructionProgramDefinitions | A2 | indexed | 32 | 51 |
| MagdolliteInstructionProgramDefinitions | A8 | indexed | 53 | 134 |
| MagdolliteLavaInstructionProgramDefinitions | 86 | indexed | 2 | 7 |
| MamaTurtleInstructionProgramDefinitions | A2 | indexed | 75 | 117 |
| MaridiaLargeSnailInstructionProgramDefinitions | A2 | indexed | 60 | 84 |
| MetroidInstructionProgramDefinitions | A3 | indexed | 25 | 31 |
| MochtroidInstructionProgramDefinitions | A3 | indexed | 8 | 12 |
| MorphBallEyeInstructionProgramDefinitions | A8 | indexed | 36 | 46 |
| MotherBrainBabyInstructionProgramDefinitions | A9 | indexed | 9 | 12 |
| MotherBrainBodyInstructionProgramDefinitions | A9 | specialized-layout | 120 | 276 |
| MotherBrainFallingTubeInstructionDefinitions | A9 | specialized-layout | 5 | 0 |
| MotherBrainGlassInstructionProgramDefinitions | 86 | indexed | 68 | 85 |
| MotherBrainHandBeamBodyInstructionDefinitions | A9 | specialized-layout | 15 | 0 |
| MotherBrainHandBeamInstructionProgramDefinitions | 86 | indexed | 21 | 25 |
| MotherBrainHeadInstructionProgramDefinitions | A9 | specialized-layout | 121 | 0 |
| MotherBrainTopTubeInstructionProgramDefinitions | 86 | indexed | 4 | 8 |
| MotherBrainTurretInstructionProgramDefinitions | 86 | indexed | 21 | 49 |
| MultiviolaInstructionProgramDefinitions | A2 | indexed | 14 | 16 |
| NinjaSpacePirateInstructionProgramDefinitions | B2 | indexed | 140 | 308 |
| NoobTubeProjectileInstructionProgramDefinitions | 86 | indexed | 90 | 207 |
| NorfairLavaJumperInstructionProgramDefinitions | A2 | indexed | 14 | 23 |
| NorfairPipeBugInstructionProgramDefinitions | B3 | indexed | 28 | 36 |
| NorfairRioInstructionProgramDefinitions | A2 | indexed | 34 | 65 |
| NuclearWaffleInstructionProgramDefinitions | A6 | indexed | 12 | 14 |
| NuclearWaffleProjectileInstructionProgramDefinitions | 86 | indexed | 12 | 14 |
| OwtchInstructionProgramDefinitions | A2 | indexed | 6 | 12 |
| PhantoonInstructionProgramDefinitions | A7 | indexed | 27 | 58 |
| PhantoonProjectileInstructionProgramDefinitions | 86 | indexed | 31 | 58 |
| PlatformInstructionProgramDefinitions | A3 | indexed | 32 | 56 |
| PolypInstructionProgramDefinitions | A2 | single | 1 | 2 |
| PolypRockInstructionProgramDefinitions | 86 | single | 1 | 2 |
| PowampInstructionProgramDefinitions | A8 | indexed | 12 | 18 |
| PowampSpikeInstructionProgramDefinitions | 86 | indexed | 3 | 6 |
| PuyoInstructionProgramDefinitions | A2 | indexed | 17 | 28 |
| RidleyInstructionProgramDefinitions | A6 | indexed | 86 | 221 |
| RinkaInstructionProgramDefinitions | A2 | indexed | 18 | 26 |
| RioInstructionProgramDefinitions | A2 | indexed | 24 | 32 |
| RipperInstructionProgramDefinitions | A2 | indexed | 24 | 36 |
| RoomSpriteObjectInstructionProgramDefinitions | B4 | indexed | 471 | 552 |
| SaveStationElectricityInstructionProgramDefinitions | 86 | indexed | 8 | 13 |
| SbugInstructionProgramDefinitions | A3 | indexed | 32 | 48 |
| SciserInstructionProgramDefinitions | A3 | indexed | 16 | 32 |
| ShaktoolInstructionProgramDefinitions | AA | indexed | 15 | 110 |
| ShaktoolProjectileInstructionProgramDefinitions | 86 | indexed | 8 | 18 |
| SharedCrawlerInstructionProgramDefinitions | A3 | indexed | 20 | 36 |
| ShitroidInstructionProgramDefinitions | A9 | indexed | 30 | 35 |
| SkreeMetareeInstructionProgramDefinitions | A3 | two-families | 22 | 40 |
| SkreeMetareeParticleInstructionProgramDefinitions | 86 | indexed | 2 | 6 |
| SkulteraInstructionProgramDefinitions | A3 | indexed | 22 | 32 |
| SpacePirateProjectileInstructionProgramDefinitions | 86 | indexed | 42 | 58 |
| SparkInstructionProgramDefinitions | A8 | indexed | 26 | 33 |
| SporeSpawnInstructionProgramDefinitions | A5 | indexed | 41 | 116 |
| SporeSpawnProjectileInstructionProgramDefinitions | 86 | indexed | 17 | 28 |
| StokeInstructionProgramDefinitions | A2 | indexed | 12 | 26 |
| StokeProjectileInstructionProgramDefinitions | 86 | indexed | 2 | 4 |
| TorizoChozoOrbInstructionProgramDefinitions | 86 | indexed | 18 | 40 |
| TorizoExplosionInstructionProgramDefinitions | 86 | indexed | 15 | 53 |
| TorizoExplosiveSwipeInstructionProgramDefinitions | 86 | indexed | 5 | 7 |
| TorizoFallingLeftInstructionProgramDefinitions | AA | indexed | 1 | 14 |
| TorizoInstructionProgramDefinitions | AA | indexed | 564 | 1761 |
| TorizoJumpBackInstructionProgramDefinitions | AA | indexed | 8 | 52 |
| TorizoJumpBackLeftInstructionProgramDefinitions | AA | indexed | 8 | 52 |
| TorizoLandingDustInstructionProgramDefinitions | 86 | indexed | 8 | 16 |
| TorizoSonicBoomInstructionProgramDefinitions | 86 | indexed | 11 | 31 |
| TourianEntranceStatueInstructionProgramDefinitions | AA | control-only | 0 | 3 |
| TourianStatueProjectileInstructionProgramDefinitions | 86 | indexed | 28 | 57 |
| VerticalShutterInstructionProgramDefinitions | A2 | indexed | 5 | 8 |
| ViolaInstructionProgramDefinitions | A3 | indexed | 14 | 30 |
| WalkingSpacePirateInstructionProgramDefinitions | B2 | indexed | 50 | 92 |
| WallSpacePirateInstructionProgramDefinitions | B2 | indexed | 42 | 150 |
| WaverInstructionProgramDefinitions | A3 | indexed | 10 | 16 |
| WorkRobotInstructionProgramDefinitions | A8 | indexed | 227 | 367 |
| WorkRobotLaserInstructionProgramDefinitions | 86 | indexed | 7 | 9 |
| WreckedShipGhostInstructionProgramDefinitions | A8 | indexed | 3 | 5 |
| YappingMawBodyProjectileInstructionProgramDefinitions | 86 | indexed | 2 | 4 |
| YappingMawInstructionProgramDefinitions | A8 | indexed | 52 | 96 |
| YardInstructionProgramDefinitions | A3 | indexed | 112 | 328 |
| YellowPipeBugInstructionProgramDefinitions | B3 | indexed | 16 | 24 |
| ZebetiteInstructionProgramDefinitions | A6 | indexed | 10 | 20 |
| ZeroInstructionProgramDefinitions | A3 | indexed | 24 | 40 |
| ZoaInstructionProgramDefinitions | A3 | indexed | 12 | 26 |
