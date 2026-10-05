# Issue 1165 - agent stream 2

GitHub child ticket: [#1239](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1239).

Assignment: **111 files / 226 named table definitions**. [Ownership rules and all streams](lookup-1165-streams.md). Inventory snapshot: 2026-10-04T20:43:25.3086366Z.

## Agent instructions

Work through every entry below under issue 1165. Check current source and review dispositions first. Convert the original mapping into calculation or meaningful cases, or document a concrete impossible/nonsense justification. Preserve behavior and supplied content edits. Independently resolve each value field; checking off a container does not excuse its unresolved payloads.

Edit only source files listed here and this stream checklist/report. Read other files as needed. Ask the coordinator to assign any additional dependency or new source/test file before writing it. Route shared review-ledger, project, verification-entry-point and master-inventory changes through the coordinator. Do not stage, commit or publish other agents' work. Use focused confirmation of identified changes, not exploratory test discovery.

For each completed entry, record the conversion or precise retention evidence, changed files and focused confirmation. Report cross-stream dependencies by path and required contract. Report pending entries honestly; definition counts are not effort estimates.

## Exclusive source files

| File | Definitions |
| --- | ---: |
| [csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/GunshipLiftoffArtworkCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/GunshipLiftoffArtworkCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/KagoVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/KagoVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Assets/KraidHeadTilemapAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/KraidHeadTilemapAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Assets/PauseWireframePresentation.cs](../csharp/src/SuperMetroid.Core/Assets/PauseWireframePresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/ProjectileFrameBindingCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/ProjectileFrameBindingCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/ProjectileTrailCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/ProjectileTrailCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/ProjectileTrailVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/ProjectileTrailVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs](../csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/YardVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/YardVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Audio/ExtractedAudioAssetCatalog.cs](../csharp/src/SuperMetroid.Core/Audio/ExtractedAudioAssetCatalog.cs) | 6 |
| [csharp/src/SuperMetroid.Core/Game/BombTorizoAttackDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/BombTorizoAttackDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/ChozoCarryMotionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ChozoCarryMotionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CrawlerAnimationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CrawlerAnimationDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CrawlerSpeedDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CrawlerSpeedDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/DeadTorizoInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DeadTorizoInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs) | 8 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoInitialInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoInitialInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoJumpLandingInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoJumpLandingInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbCollisionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftTurnInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftTurnInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoStunnedInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoStunnedInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoSuperMissileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoSuperMissileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GunshipDustInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GunshipDustInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/GunshipInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GunshipInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/HZoomerInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/HZoomerInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs) | 11 |
| [csharp/src/SuperMetroid.Core/Game/NinjaSpacePirateInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NinjaSpacePirateInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/NorfairLavaJumpDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NorfairLavaJumpDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/NorfairLavaJumperInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NorfairLavaJumperInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/NorfairRioInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NorfairRioInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/ProjectileTrailDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs) | 10 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.KraidGrowth.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.KraidGrowth.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Pickups.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Pickups.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PipeBugs.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PipeBugs.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.WreckedShipGhost.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.WreckedShipGhost.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/SbugInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SbugInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SbugMovementDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SbugMovementDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs](../csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoBellyPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoBellyPaletteFxProgramMechanicsDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombLeft.cs](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombLeft.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombRight.cs](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombRight.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenLeft.cs](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenLeft.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenRight.cs](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenRight.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.Shared.cs](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.Shared.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TorizoInstructionVramTransferDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionVramTransferDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TorizoJumpBackCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoJumpBackInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoLandingDustInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoLandingDustInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TorizoSonicBoomInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TorizoSonicBoomInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/ViolaInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ViolaInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/WaverAnimationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/WaverAnimationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/WaverInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/WaverInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/YardDirectionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/YardDirectionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/YardInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/YardInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/YardTurnDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/YardTurnDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ZeroInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ZeroInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs](../csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs) | 1 |

## Work checklist

### csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs

- [ ] **ChozoAndTubeColorCatalog.tubeCracks** ([L22](../csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs#L22)) - installed stock table. Original/default payload behind ChozoAndTubeColorCatalog.tubeCracks. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **ChozoAndTubeColorCatalog.wreckedShip** ([L23](../csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs#L23)) - installed stock table. Original/default payload behind ChozoAndTubeColorCatalog.wreckedShip. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **ChozoAndTubeColorCatalog.lowerNorfair** ([L24](../csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs#L24)) - installed stock table. Original/default payload behind ChozoAndTubeColorCatalog.lowerNorfair. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/ChozoCarryMotionDefinitions.cs

- [ ] **ChozoCarryMotionDefinitions.Magnitudes** ([L15](../csharp/src/SuperMetroid.Core/Game/ChozoCarryMotionDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChozoCarryMotionDefinitions.YOffsets** ([L26](../csharp/src/SuperMetroid.Core/Game/ChozoCarryMotionDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs

- [ ] **ChozoStatueInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ChozoStatueInstructionProgramDefinitions.PresentationWords** ([L85](../csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs#L85)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs

- [x] **ChozoTourianDustInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChozoTourianDustInstructionProgramDefinitions.PresentationWords** ([L63](../csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs#L63)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrawlerAnimationDefinitions.cs

- [x] **CrawlerAnimationDefinitions.InitialFamilies** ([L48](../csharp/src/SuperMetroid.Core/Game/CrawlerAnimationDefinitions.cs#L48)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CrawlerAnimationDefinitions.SurfaceSpecies** ([L81](../csharp/src/SuperMetroid.Core/Game/CrawlerAnimationDefinitions.cs#L81)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrawlerSpeedDefinitions.cs

- [ ] **CrawlerSpeedDefinitions.Speeds** ([L16](../csharp/src/SuperMetroid.Core/Game/CrawlerSpeedDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs

- [x] **CrystalFlashColorCatalog.body** ([L14](../csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs#L14)) - installed stock table. Original/default payload behind CrystalFlashColorCatalog.body. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CrystalFlashColorCatalog.bubble** ([L15](../csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs#L15)) - installed stock table. Original/default payload behind CrystalFlashColorCatalog.bubble. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Audio/ExtractedAudioAssetCatalog.cs

- [ ] **ExtractedAudioAssetCatalog.streams** ([L14](../csharp/src/SuperMetroid.Core/Audio/ExtractedAudioAssetCatalog.cs#L14)) - installed stock table. Original/default payload behind ExtractedAudioAssetCatalog.streams. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **ExtractedAudioAssetCatalog.sampleBanks** ([L15](../csharp/src/SuperMetroid.Core/Audio/ExtractedAudioAssetCatalog.cs#L15)) - installed stock table. Stock bank/source-number selection and loop-entry source mapping only. ManagedPcmSample owns the waveform payload; do not count waveform copies or sample object references again.
- [ ] **ExtractedAudioAssetCatalog.instrumentBanks** ([L16](../csharp/src/SuperMetroid.Core/Audio/ExtractedAudioAssetCatalog.cs#L16)) - installed stock table. Original/default payload behind ExtractedAudioAssetCatalog.instrumentBanks. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **ExtractedAudioAssetCatalog.musicBanks** ([L18](../csharp/src/SuperMetroid.Core/Audio/ExtractedAudioAssetCatalog.cs#L18)) - installed stock table. Original/default payload behind ExtractedAudioAssetCatalog.musicBanks. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **ExtractedAudioAssetCatalog.soundPrograms** ([L19](../csharp/src/SuperMetroid.Core/Audio/ExtractedAudioAssetCatalog.cs#L19)) - installed stock table. Original/default payload behind ExtractedAudioAssetCatalog.soundPrograms. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **ExtractedAudioAssetCatalog.soundLibraries** ([L20](../csharp/src/SuperMetroid.Core/Audio/ExtractedAudioAssetCatalog.cs#L20)) - installed stock table. Original/default payload behind ExtractedAudioAssetCatalog.soundLibraries. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GunshipLiftoffArtworkCatalog.cs

- [x] **GunshipLiftoffTransferDefinitions.Entries** ([L31](../csharp/src/SuperMetroid.Core/Assets/GunshipLiftoffArtworkCatalog.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GunshipDustInstructionProgramDefinitions.cs

- [x] **GunshipDustInstructionProgramDefinitions.Programs** ([L40](../csharp/src/SuperMetroid.Core/Game/GunshipDustInstructionProgramDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GunshipInstructionProgramDefinitions.cs

- [x] **GunshipInstructionProgramDefinitions.Words** ([L38](../csharp/src/SuperMetroid.Core/Game/GunshipInstructionProgramDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GunshipInstructionProgramDefinitions.PresentationWords** ([L54](../csharp/src/SuperMetroid.Core/Game/GunshipInstructionProgramDefinitions.cs#L54)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HZoomerInstructionProgramDefinitions.cs

- [x] **HZoomerInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/HZoomerInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **HZoomerInstructionProgramDefinitions.PresentationWords** ([L53](../csharp/src/SuperMetroid.Core/Game/HZoomerInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/KagoVisualDefinitions.cs

- [ ] **KagoVisualDefinitions.Frames / literal at L15** ([L15](../csharp/src/SuperMetroid.Core/Assets/KagoVisualDefinitions.cs#L15)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs

- [x] **KagoBugProjectileInstructionProgramDefinitions.Words** ([L53](../csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KagoBugProjectileInstructionProgramDefinitions.PresentationWords** ([L80](../csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs#L80)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs

- [x] **KagoInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KagoInstructionProgramDefinitions.PresentationWords** ([L25](../csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs

- [ ] **KraidColorCatalog.roomBackdrop** ([L25](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L25)) - installed stock table. Original/default payload behind KraidColorCatalog.roomBackdrop. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **KraidColorCatalog.initialTarget** ([L26](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L26)) - installed stock table. Original/default payload behind KraidColorCatalog.initialTarget. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **KraidColorCatalog.health** ([L27](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L27)) - installed stock table. Original/default payload behind KraidColorCatalog.health. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **KraidColorCatalog.secondary** ([L28](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L28)) - installed stock table. Original/default payload behind KraidColorCatalog.secondary. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **KraidColorCatalog.deathArm** ([L29](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L29)) - installed stock table. Original/default payload behind KraidColorCatalog.deathArm. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/KraidHeadTilemapAtlas.cs

- [ ] **KraidHeadTilemapAtlas.words** ([L12](../csharp/src/SuperMetroid.Core/Assets/KraidHeadTilemapAtlas.cs#L12)) - installed stock table. Original/default payload behind KraidHeadTilemapAtlas.words. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs

- [ ] **KraidArmCollisionDefinitions.Phase0** ([L13](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Phase1** ([L16](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Phase2** ([L19](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Phase3** ([L22](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Phase4** ([L25](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Phase5** ([L28](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Phase6** ([L31](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Phase7** ([L34](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Phase8** ([L37](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Phase9** ([L40](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KraidArmCollisionDefinitions.Geometry** ([L64](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.KraidGrowth.cs

- [ ] **RoomEnemySystem.FinishKraidGrowth / lintTimers** ([L157](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.KraidGrowth.cs#L157)) - method-local definition. Three fixed lint spawn delays indexed by part ordinal. Replace the stored delays with their native timing rule or semantic part cases.

### csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs

- [ ] **KraidRoomPlmDrawDefinitions.All** ([L115](../csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs#L115)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NinjaSpacePirateInstructionProgramDefinitions.cs

- [ ] **NinjaSpacePirateInstructionProgramDefinitions.Words** ([L56](../csharp/src/SuperMetroid.Core/Game/NinjaSpacePirateInstructionProgramDefinitions.cs#L56)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NinjaSpacePirateInstructionProgramDefinitions.PresentationWords** ([L137](../csharp/src/SuperMetroid.Core/Game/NinjaSpacePirateInstructionProgramDefinitions.cs#L137)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.cs

- [x] **NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Durations** ([L39](../csharp/src/SuperMetroid.Core/Game/NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Definitions** ([L42](../csharp/src/SuperMetroid.Core/Game/NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NorfairLavaJumpDefinitions.cs

- [ ] **NorfairLavaJumpDefinitions.InitialVerticalVelocities** ([L11](../csharp/src/SuperMetroid.Core/Game/NorfairLavaJumpDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NorfairLavaJumperInstructionProgramDefinitions.cs

- [ ] **NorfairLavaJumperInstructionProgramDefinitions.Words** ([L33](../csharp/src/SuperMetroid.Core/Game/NorfairLavaJumperInstructionProgramDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NorfairLavaJumperInstructionProgramDefinitions.PresentationWords** ([L60](../csharp/src/SuperMetroid.Core/Game/NorfairLavaJumperInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs

- [x] **NorfairPipeBugInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **NorfairPipeBugInstructionProgramDefinitions.PresentationWords** ([L36](../csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NorfairRioInstructionProgramDefinitions.cs

- [ ] **NorfairRioInstructionProgramDefinitions.Words** ([L31](../csharp/src/SuperMetroid.Core/Game/NorfairRioInstructionProgramDefinitions.cs#L31)) - factory-built stock table. Stored NorfairRioInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **NorfairRioInstructionProgramDefinitions.PresentationWords** ([L33](../csharp/src/SuperMetroid.Core/Game/NorfairRioInstructionProgramDefinitions.cs#L33)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.cs

- [ ] **OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Durations** ([L43](../csharp/src/SuperMetroid.Core/Game/OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Definitions** ([L46](../csharp/src/SuperMetroid.Core/Game/OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs

- [x] **OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorOffsets** ([L39](../csharp/src/SuperMetroid.Core/Game/OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs

- [ ] **PauseBackdropPresentation.areas** ([L10](../csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs#L10)) - installed stock table. Original/default payload behind PauseBackdropPresentation.areas. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PauseBackdropPresentation.buttons** ([L11](../csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs#L11)) - installed stock table. Original/default payload behind PauseBackdropPresentation.buttons. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs

- [x] **PauseEquipmentBaseDefinitions.equipmentLabelRegions** ([L17](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PauseEquipmentBaseDefinitions.reserveRegions** ([L29](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs

- [ ] **PauseEquipmentBasePresentation.tilemap** ([L10](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs#L10)) - installed stock table. Original/default payload behind PauseEquipmentBasePresentation.tilemap. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs

- [ ] **PauseEquipmentLabelPresentation.labels** ([L12](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs#L12)) - installed stock table. Original/default payload behind PauseEquipmentLabelPresentation.labels. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PauseEquipmentLabelPresentation.blank** ([L13](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs#L13)) - installed stock table. Original/default payload behind PauseEquipmentLabelPresentation.blank. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs

- [ ] **PauseReserveUiPresentation.labels** ([L10](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L10)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.labels. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **PauseReserveUiPresentation.digits** ([L12](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L12)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.digits. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **PauseReserveUiPresentation.arrowOffsets** ([L13](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L13)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.arrowOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PauseReserveUiPresentation.arrowFrames** ([L16](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L16)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.arrowFrames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseWireframePresentation.cs

- [ ] **PauseWireframePresentation.frames** ([L8](../csharp/src/SuperMetroid.Core/Assets/PauseWireframePresentation.cs#L8)) - installed stock table. Original/default payload behind PauseWireframePresentation.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Pickups.cs

- [ ] **RoomEnemySystem.EnemyDropAccumulatorOrder** ([L29](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Pickups.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs

- [ ] **PipeBugVisualDefinitions.Brinstar** ([L18](../csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PipeBugVisualDefinitions.Norfair** ([L37](../csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PipeBugVisualDefinitions.Yellow** ([L51](../csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs#L51)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs

- [x] **PipeBugDefinitions.BrinstarInstructionLists** ([L35](../csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PipeBugDefinitions.StrongBrinstarInstructionLists** ([L47](../csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PipeBugs.cs

- [ ] **RoomEnemySystem.RunNorfairPipeBugSamusWait / staggerTargets** ([L453](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PipeBugs.cs#L453)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.RunNorfairPipeBugSamusWait / staggerFunctions** ([L455](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PipeBugs.cs#L455)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Assets/ProjectileFrameBindingCatalog.cs

- [ ] **ProjectileFrameBindingCatalog.sprites** ([L13](../csharp/src/SuperMetroid.Core/Assets/ProjectileFrameBindingCatalog.cs#L13)) - installed stock table. Original/default payload behind ProjectileFrameBindingCatalog.sprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteCatalog.cs

- [ ] **ProjectileSpriteCatalog.frames** ([L10](../csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteCatalog.cs#L10)) - installed stock table. Original/default payload behind ProjectileSpriteCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteDefinitions.cs

- [ ] **ProjectileSpriteDefinitions.NativePointers** ([L12](../csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs

- [ ] **ProjectileTrailAtlas.tiles** ([L8](../csharp/src/SuperMetroid.Core/Assets/ProjectileTrailAtlas.cs#L8)) - installed stock table. Original/default payload behind ProjectileTrailAtlas.tiles. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/ProjectileTrailCatalog.cs

- [ ] **ProjectileTrailCatalog.attributes** ([L10](../csharp/src/SuperMetroid.Core/Assets/ProjectileTrailCatalog.cs#L10)) - installed stock table. Original/default payload behind ProjectileTrailCatalog.attributes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/ProjectileTrailVisualDefinitions.cs

- [x] **ProjectileTrailVisualDefinitions.Frames** ([L10](../csharp/src/SuperMetroid.Core/Assets/ProjectileTrailVisualDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs

- [ ] **ProjectileTrailCoordinateDefinitions.ReachableAdjacentCode** ([L215](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs#L215)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ProjectileTrailCoordinateDefinitions.Pointers** ([L222](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs#L222)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by CreatePointers(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **ProjectileTrailCoordinateDefinitions.Frames** ([L223](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs#L223)) - factory-built stock table. Stored FrozenDictionary<int, Offset> initialized by CreateFrames(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/ProjectileTrailDefinitions.cs

- [x] **ProjectileTrailDefinitions.ReachableSelectors** ([L24](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs

- [x] **ProjectileTrailProgramDefinitions.Ends** ([L10](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ProjectileTrailProgramDefinitions.LeftMoves** ([L12](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ProjectileTrailProgramDefinitions.RightMoves** ([L14](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ProjectileTrailProgramDefinitions.FourTickFrames** ([L16](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SbugInstructionProgramDefinitions.cs

- [x] **SbugInstructionProgramDefinitions.Words** ([L38](../csharp/src/SuperMetroid.Core/Game/SbugInstructionProgramDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SbugInstructionProgramDefinitions.PresentationWords** ([L58](../csharp/src/SuperMetroid.Core/Game/SbugInstructionProgramDefinitions.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SbugMovementDefinitions.cs

- [x] **SbugMovementDefinitions.FacingInstructionLists** ([L12](../csharp/src/SuperMetroid.Core/Game/SbugMovementDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SbugMovementDefinitions.ActivationFunctions** ([L28](../csharp/src/SuperMetroid.Core/Game/SbugMovementDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs

- [ ] **ScrollBoundaryCamera.TrackMovedSamusHorizontally / facingRightOffsets** ([L172](../csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs#L172)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **ScrollBoundaryCamera.TrackMovedSamusHorizontally / facingLeftOffsets** ([L173](../csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs#L173)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs

- [ ] **TorizoInstructionVramArtworkDefinitions.Pages** ([L51](../csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs#L51)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BombTorizoAttackDefinitions.cs

- [ ] **BombTorizoAttackDefinitions.SwipePlacements** ([L17](../csharp/src/SuperMetroid.Core/Game/BombTorizoAttackDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **BombTorizoAttackDefinitions.ExplosionPlacements** ([L37](../csharp/src/SuperMetroid.Core/Game/BombTorizoAttackDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs

- [ ] **BombTorizoMovementDefinitions.PostureX** ([L15](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **BombTorizoMovementDefinitions.PostureY** ([L18](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **BombTorizoMovementDefinitions.WalkVelocities** ([L25](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTorizoInstructionProgramDefinitions.cs

- [ ] **DeadTorizoInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/DeadTorizoInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs

- [x] **DeadTorizoVramTransferDefinitions.Even** ([L28](../csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DeadTorizoVramTransferDefinitions.Odd** ([L39](../csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs

- [ ] **GoldenTorizoAwakeningCollisionDefinitions.Frames** ([L22](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoAwakeningCollisionDefinitions.SeatedLow** ([L33](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoAwakeningCollisionDefinitions.SeatedMid** ([L35](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoAwakeningCollisionDefinitions.SeatedHigh** ([L37](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoAwakeningCollisionDefinitions.Initial** ([L39](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoAwakeningCollisionDefinitions.Rising** ([L41](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoAwakeningCollisionDefinitions.StandingMid** ([L43](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoAwakeningCollisionDefinitions.StandingHigh** ([L45](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoAwakeningInstructionProgramDefinitions.Words** ([L22](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningInstructionProgramDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWords** ([L95](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningInstructionProgramDefinitions.cs#L95)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs

- [x] **GoldenTorizoEggInstructionProgramDefinitions.Words** ([L37](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoEggInstructionProgramDefinitions.PresentationWords** ([L100](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs#L100)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoEyeBeamInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWords** ([L60](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoInitialInstructionProgramDefinitions.cs

- [x] **GoldenTorizoInitialInstructionProgramDefinitions.Words** - Semantic tile upload/stance/lock/wake/pose/sleep dispatch; calculate instruction positions including the seven-byte DMA payload and visual operand gap. No independent timing sequence remains in this one-pose-then-sleep entry.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoJumpLandingInstructionProgramDefinitions.cs

- [x] **GoldenTorizoJumpLandingInstructionProgramDefinitions.Words** - Four facing/forward-foot cases choose named orb/sonic attack entries and return to the opposite moving leg; calculate each five-word control sequence and its addresses.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbCollisionDefinitions.cs

- [ ] **GoldenTorizoLeftFootOrbCollisionDefinitions.Frames** ([L14](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbCollisionDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWords** ([L52](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbInstructionProgramDefinitions.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbCollisionDefinitions.cs

- [ ] **GoldenTorizoLeftOrbCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoLeftOrbCollisionDefinitions.Body** ([L29](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbCollisionDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoLeftOrbInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWords** ([L69](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbInstructionProgramDefinitions.cs#L69)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftTurnInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoLeftTurnInstructionProgramDefinitions.Words** ([L29](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftTurnInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoLeftTurnInstructionProgramDefinitions.PresentationWords** ([L44](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftTurnInstructionProgramDefinitions.cs#L44)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbCollisionDefinitions.cs

- [ ] **GoldenTorizoRightOrbCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoRightOrbCollisionDefinitions.Body** ([L23](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbCollisionDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoRightOrbInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoRightOrbInstructionProgramDefinitions.PresentationWords** ([L44](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbInstructionProgramDefinitions.cs#L44)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicCollisionDefinitions.cs

- [ ] **GoldenTorizoRightSonicCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoRightSonicCollisionDefinitions.Body** ([L38](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicCollisionDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoRightSonicInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoRightSonicInstructionProgramDefinitions.PresentationWords** ([L88](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicInstructionProgramDefinitions.cs#L88)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs

- [ ] **GoldenTorizoRightwardCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoRightwardCollisionDefinitions.StandingBody** ([L28](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoRightwardCollisionDefinitions.SteppingBody** ([L30](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoRightwardInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoRightwardInstructionProgramDefinitions.PresentationWords** ([L104](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardInstructionProgramDefinitions.cs#L104)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoStunnedInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoStunnedInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoStunnedInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoSuperMissileInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoSuperMissileInstructionProgramDefinitions.Words** ([L28](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoSuperMissileInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWords** ([L77](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoSuperMissileInstructionProgramDefinitions.cs#L77)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkDefinitions.cs

- [ ] **GoldenTorizoWalkDefinitions.Velocities** ([L7](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkDefinitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs

- [ ] **GoldenTorizoWalkingCollisionDefinitions.Frames** ([L14](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoWalkingCollisionDefinitions.StandingBody** ([L28](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoWalkingCollisionDefinitions.SteppingBody** ([L30](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoWalkingInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWords** ([L92](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingInstructionProgramDefinitions.cs#L92)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs

- [ ] **RoomEnemySystem.LoadTorizoSharedPaletteRows / rowEleven** ([L273](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L273)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.LoadTorizoSharedPaletteRows / rowFifteen** ([L278](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L278)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.LoadBombTorizoPalette / rowNine** ([L294](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L294)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.LoadBombTorizoPalette / rowTen** ([L299](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L299)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.LoadGoldenTorizoBasePalette / rowNine** ([L316](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L316)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.LoadGoldenTorizoBasePalette / rowTen** ([L321](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L321)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.LoadTorizoDeathPalette / rowNine** ([L332](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L332)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.LoadTorizoDeathPalette / rowTen** ([L337](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L337)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.LoadGoldenTorizoFinalPalette / rowNine** ([L348](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L348)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.LoadGoldenTorizoFinalPalette / rowTen** ([L353](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L353)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs

- [ ] **RoomEnemySystem.DeadTorizoInitialGraphicsCopies** ([L22](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadTorizoColumnWordOffsets** ([L40](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadTorizoColumnMinimumY** ([L43](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoBellyPaletteFxProgramMechanicsDefinitions.cs

- [x] **TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameDurations** ([L24](../csharp/src/SuperMetroid.Core/Game/TorizoBellyPaletteFxProgramMechanicsDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoBellyPaletteFxProgramMechanicsDefinitions.Definitions** ([L26](../csharp/src/SuperMetroid.Core/Game/TorizoBellyPaletteFxProgramMechanicsDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs

- [x] **TorizoChozoOrbInstructionProgramDefinitions.Words** ([L29](../csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoChozoOrbInstructionProgramDefinitions.PresentationWords** ([L76](../csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs

- [ ] **TorizoCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoCollisionDefinitions.Hitboxes** ([L124](../csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs#L124)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs

- [ ] **TorizoExplosionInstructionProgramDefinitions.Words** ([L28](../csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoExplosionInstructionProgramDefinitions.PresentationWords** ([L86](../csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs#L86)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs

- [x] **TorizoExplosiveSwipeInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWords** ([L29](../csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftCollisionDefinitions.cs

- [ ] **TorizoFallingLeftCollisionDefinitions.Components** ([L13](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoFallingLeftCollisionDefinitions.Body** ([L20](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftCollisionDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftInstructionProgramDefinitions.cs

- [ ] **TorizoFallingLeftInstructionProgramDefinitions.Words** ([L30](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoFallingLeftInstructionProgramDefinitions.PresentationWords** ([L47](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftInstructionProgramDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombLeft.cs

- [ ] **TorizoInstructionProgramDefinitions.BombLeft** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombLeft.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombRight.cs

- [ ] **TorizoInstructionProgramDefinitions.BombRight** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombRight.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.cs

- [ ] **TorizoInstructionProgramDefinitions.PresentationWords** ([L406](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.cs#L406)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenLeft.cs

- [ ] **TorizoInstructionProgramDefinitions.GoldenLeft** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenLeft.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenRight.cs

- [ ] **TorizoInstructionProgramDefinitions.GoldenRight** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenRight.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.Shared.cs

- [ ] **TorizoInstructionProgramDefinitions.Shared** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.Shared.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionVramTransferDefinitions.cs

- [ ] **TorizoInstructionVramTransferDefinitions.Entries** ([L17](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionVramTransferDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoJumpBackCollisionDefinitions.cs

- [ ] **TorizoJumpBackCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoJumpBackCollisionDefinitions.Body** ([L20](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackCollisionDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoJumpBackInstructionProgramDefinitions.cs

- [ ] **TorizoJumpBackInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoJumpBackInstructionProgramDefinitions.PresentationWords** ([L74](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackInstructionProgramDefinitions.cs#L74)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftCollisionDefinitions.cs

- [ ] **TorizoJumpBackLeftCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoJumpBackLeftCollisionDefinitions.Body** ([L20](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftCollisionDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftInstructionProgramDefinitions.cs

- [ ] **TorizoJumpBackLeftInstructionProgramDefinitions.Words** ([L15](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftInstructionProgramDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoJumpBackLeftInstructionProgramDefinitions.PresentationWords** ([L71](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftInstructionProgramDefinitions.cs#L71)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoLandingDustInstructionProgramDefinitions.cs

- [ ] **TorizoLandingDustInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/TorizoLandingDustInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoLandingDustInstructionProgramDefinitions.PresentationWords** ([L39](../csharp/src/SuperMetroid.Core/Game/TorizoLandingDustInstructionProgramDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoSonicBoomInstructionProgramDefinitions.cs

- [ ] **TorizoSonicBoomInstructionProgramDefinitions.Words** ([L30](../csharp/src/SuperMetroid.Core/Game/TorizoSonicBoomInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoSonicBoomInstructionProgramDefinitions.PresentationWords** ([L67](../csharp/src/SuperMetroid.Core/Game/TorizoSonicBoomInstructionProgramDefinitions.cs#L67)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs

- [x] **UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.Durations** ([L39](../csharp/src/SuperMetroid.Core/Game/UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ViolaInstructionProgramDefinitions.cs

- [x] **ViolaInstructionProgramDefinitions.Words** ([L28](../csharp/src/SuperMetroid.Core/Game/ViolaInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ViolaInstructionProgramDefinitions.PresentationWords** ([L52](../csharp/src/SuperMetroid.Core/Game/ViolaInstructionProgramDefinitions.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/WaverAnimationDefinitions.cs

- [x] **WaverAnimationDefinitions.InstructionLists** ([L23](../csharp/src/SuperMetroid.Core/Game/WaverAnimationDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/WaverInstructionProgramDefinitions.cs

- [x] **WaverInstructionProgramDefinitions.Words** ([L28](../csharp/src/SuperMetroid.Core/Game/WaverInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **WaverInstructionProgramDefinitions.PresentationWords** ([L40](../csharp/src/SuperMetroid.Core/Game/WaverInstructionProgramDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.WreckedShipGhost.cs

- [x] **RoomEnemySystem.WreckedShipGhostFlickerDurations** ([L123](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.WreckedShipGhost.cs#L123)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.WreckedShipGhostSpawnOffsets** ([L130](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.WreckedShipGhost.cs#L130)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.WreckedShipGhostPalette** ([L139](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.WreckedShipGhost.cs#L139)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs

- [x] **WreckedShipGhostInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **WreckedShipGhostInstructionProgramDefinitions.PresentationWords** ([L31](../csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/YardVisualDefinitions.cs

- [ ] **YardVisualDefinitions.Groups** ([L17](../csharp/src/SuperMetroid.Core/Assets/YardVisualDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/YardDirectionDefinitions.cs

- [x] **YardDirectionDefinitions.Directions** ([L28](../csharp/src/SuperMetroid.Core/Game/YardDirectionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **YardDirectionDefinitions.AirborneInstructions** ([L61](../csharp/src/SuperMetroid.Core/Game/YardDirectionDefinitions.cs#L61)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/YardInstructionProgramDefinitions.cs

- [ ] **YardInstructionProgramDefinitions.Words** ([L95](../csharp/src/SuperMetroid.Core/Game/YardInstructionProgramDefinitions.cs#L95)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/YardTurnDefinitions.cs

- [x] **YardTurnDefinitions.OrdinaryTurns** ([L18](../csharp/src/SuperMetroid.Core/Game/YardTurnDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **YardTurnDefinitions.SuppressedTurns** ([L43](../csharp/src/SuperMetroid.Core/Game/YardTurnDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ZeroInstructionProgramDefinitions.cs

- [x] **ZeroInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/ZeroInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ZeroInstructionProgramDefinitions.PresentationWords** ([L53](../csharp/src/SuperMetroid.Core/Game/ZeroInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

## Agent handoff

- Completed conversions: pending.
- Justified retained entries: pending.
- Confirmation results: pending.
- Cross-stream dependencies and proposed shared-file patches: pending.
- Remaining entries: all unchecked entries above.


### Batch 1: palette mechanics (implementation ready; coordinator confirmation pending)

- Removed `UpperCrateriaEscapeRedFlashPaletteFxProgramMechanicsDefinitions.Durations`: the fourteen durations are the integer triangle `abs(7-frame)+1`, totaling 63 ticks.
- Removed `OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorOffsets`: each color address advances past its duration, preceding colors, and the inline commands before colors 3 and 7.
- Removed `TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameDurations`: six records alternate three-record halves, each holding its first endpoint for 10 ticks and the next two records for 8 ticks (52 total).
- Removed `TorizoBellyPaletteFxProgramMechanicsDefinitions.Definitions`: an immutable indexed view dispatches by Bomb/Golden owner; first frame follows the four-word setup and loop follows six ten-byte records. No reconstructed table is cached.
- Unsupported color-retention prose was removed from the upper-Crateria and Torizo files. Independent BGR555 payload derivation remains pending; this batch grants no retention exception.
- Added `Program.LookupStream2.cs`, method `VerifyLookupStream2PaletteMechanics(ISnesAddressSpace rom)`. Coordinator should wire a guarded `--lookup-stream2-palette-mechanics` command, load the supported ROM and validate its SHA256 as with existing lookup commands, then call this method. It confirms the changed mechanics against original bank 8D words, all color offsets, cycle totals, owner dispatch/enumeration and rejected bounds. This is confirmation of identified conversions, not gameplay discovery.
- No builds or tests run by worker; coordinator owns shared build outputs. No custom-resource loading behavior changes: live color words remain excluded from mechanics resolution.
- Remaining scope: all other unchecked entries and independent color payloads. These four definitions await passing coordinator confirmation before their checkboxes are marked complete.
### Batch 1 confirmed, including Yard dispatch

Verification build passed (1431 existing warnings, zero errors). `--lookup-stream2-palette-mechanics` passed all original mechanics/address/dispatch/bound checks. `--lookup-stream2-yard-directions` passed 48 direction words, 24 duplicated airborne-list words, initialization/turn/detach/contact/shot handoffs, 48 turn words, four suppression aliases and 24 outside/inside production branches.

`YardDirectionDefinitions` now dispatches physical surface states directly to their named animation/callback operations, computes sign/axis property bits and adjacent opposite direction, and dispatches airborne facing. `YardTurnDefinitions` directly dispatches ordinary and suppressed callbacks by movement state; seven-pixel directional lookahead and zero suppressed lookahead retain every native field. Native mismatched callback identities are preserved, not corrected. Four dispatch tables removed; no remaining payload or retention exception in those two files.

Eight named definitions in this batch are now complete. BGR555 color payloads and every other unchecked entry remain required. Earlier 'confirmation pending' notes above describe the pre-verification handoff; this result supersedes them.
### Batch 2: Crystal Flash body color payload

Resolved all 100 stock body colors in `CrystalFlashColorCatalog.body`. Original $91:DC00 records select ten body palettes from $9B:96C0 onward. Each has transparent RGB5(0,0,14), and all nine visible colors are equal greys. The opening frame is neutral16; subsequent greys follow the four-frame triangular pulse 19,23,27,23, ending at19. `CalculateBody` implements the pulse and channel packing. The loader compares every supplied field before discarding the stock rows; any independent edit retains the supplied palette. Runtime resolution and CGRAM application now calculate stock colors without a cached body table.

Verification build passed (1432 warnings, zero errors). `--lookup-stream2-crystal-body` passed all 100 original RGB5 words, actual CGRAM writes, every independently edited body cell with preservation of all other cells, and bounds. All 36 original bubble colors were also confirmed unchanged. No gameplay search was run.

The bubble payload remains unresolved, independently of completed body colors. Static source inspection shows a rotated six-step white-to-pink ramp for the first five frames; the final frame leading color is white instead of the expected rotated ramp value. This is evidence to investigate, not a retention justification. No bubble cells are marked complete or exempted here.

Nine named definitions complete in stream2; 217 remain required.
### Batch 3: Kraid health-color interpolation (partial payload resolution)

Original health and secondary sources at $A7:B3D3/$B513 are identical nine-by-sixteen palettes. Their first bands are saturated white flashes. In normal bands1..8, nearest-integer per-channel interpolation between the first/last bands reproduces every word except two interior samples of color6; the transparent slot switches from the first to final backdrop after the first normal band. `KraidColorCatalog` now computes those channels, stores only endpoints and exact deviations, and shares an identical secondary source. Independent custom edits remain exact, including different secondary colors and edits to endpoints/flash entries. Content identity serializes the resulting colors in the original order.

This is partial resolution: endpoint artwork and the two deviating original color6 samples remain required, and neither `health` nor `secondary` is checked off. No irregularity-based retention exception has been introduced. The other three Kraid source palettes remain unchanged and required.

Added `--lookup-stream2-kraid-ramps` to check all336 original colors, all288 independent health/secondary cell edits with preservation of every other source, exact content identity, shared stock selection, the two residual samples, and bounds. Confirmation pending.
Batch 3 confirmation passed: Verification build (1432 warnings, zero errors); --lookup-stream2-kraid-ramps passed all original336 colors, all288 independent edited cells, source independence, canonical identity, residual accounting and bounds. Both Kraid palette entries remain unchecked for their explicitly identified residual data. Stream2 completion count remains nine.

### Batch 4: Waver and HZoomer executable animation programs

Removed both mechanics-word and presentation-operand-address tables in `WaverInstructionProgramDefinitions` and `HZoomerInstructionProgramDefinitions` (four definitions). Waver emits two steady frame/sleep programs and two four-frame spin/completion/sleep programs. HZoomer emits four surface loops, each selecting its movement axis, displaying five three-tick frames, and branching back after setup. Program word addresses, durations, callbacks, branches, sleeps and interleaved presentation-operand addresses now follow these structures; no regenerated table is stored. Actual sprite artwork remains a separate scope.

Verification build passed (1432 warnings, zero errors). `--waver-instruction-mechanics` passed all16 native mechanics words, both steady/spinning production programs, completion callbacks and sleeps with every original mechanics and ten visual-selector reads blocked. `--hzoomer-instruction-program-definitions` passed all36 native mechanics words, all four real surface loops/movement callbacks and twenty visual selectors. Thirteen named definitions complete in stream2; 213 remain required, including the explicitly pending Kraid endpoints/deviations and Crystal Flash bubble.
### Batch 5: crawler/Sbug program structures and semantic animation dispatch

Removed eleven named definitions: mechanics and visual-operand tables for Viola, Zero and Sbug; Sbug facing and activation selectors; Waver facing/spin selector; crawler initial-family and surface-species selectors. Viola uses four axis-setting entries into a fourteen-frame shared loop, Zero uses four six-frame axis loops, and Sbug uses eight four-frame directional loops. Instruction and operand addresses follow those structures. Sbug facing follows the twenty-byte program stride (including native odd-selector folding); activation, crawler families/species and Waver flags dispatch by their existing semantic domains.

Verification build passed (1432 warnings, zero errors). Focused `--sbug-instruction-mechanics`, `--zero-instruction-program-definitions`, `--viola-instruction-program-definitions`, `--lookup-stream2-crawler-animations` and `--waver-instruction-mechanics` all passed. They confirm original mechanics/selector values and actual initialization, movement callback, loop, spin and surface-handoff behavior with original source reads blocked.

The old Zero verifier expected24 live visual-pointer reads, obsolete after the earlier visual migration. With coordinator-assigned ownership, it now asserts zero such reads and independently compares every24 compiled selector to its original ROM operand; production was not changed to satisfy a stale test. All40 native mechanics words and all four real loops pass.

Twenty-four named definitions complete in stream2; 202 remain required. Kraid endpoint/deviation payloads and Crystal Flash bubble remain expressly unfinished, without a retention exception.
### Batch 6: Norfair phase timing and ghost appearance

Removed the sixteen-word Norfair duration table using its symmetric distance-to-end formula, and the ghost's seventeen-word flicker schedule using paired intervals in four stages followed by the terminator. Ghost spawn positions calculate a 64-pixel movement grid with the native upper-right approach spawning level with Samus. Definitions live in the dedicated `WreckedShipGhostAppearanceDefinitions` catalog; runtime preserves odd flicker-offset folding, interval/terminator ordering, malformed-state rejection, wrapped position arithmetic and unrelated property flags.

Source inspection identified an existing literal typo: upper-right Y was -64, while pinned NTSC J/U v1.0 ROM SHA256 `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72` contains zero at $A8:9AB2. Pinned disassembly agrees, and $A8:9DB9-$9DD4 adds movement classes and directly adds that word to Samus Y. The coordinator authorized correcting this row within the changed mapping. Exact native mapping and produced coordinates are confirmed; the player-facing position correction awaits player validation.

Verification build passed (1221 warnings, zero errors). `--lookup-stream2-ghost-norfair` passed all18 native spawn fields, all17 native flicker intervals, actual production wrapped positions and phase/timer installation, countdown/visibility/terminal behavior, odd offset folding, rejected malformed states, all64 Norfair duration operands and four116-tick cycles. No gameplay discovery was used. Ghost and Norfair color payloads remain independently required; unsupported irregularity-based retention prose was removed from the Norfair color-address documentation.

Twenty-seven named definitions complete in stream2; 199 remain required.
### Batch 7: Crystal Flash rotating bubble ramp (partial payload resolution)

Replaced the stock six-by-six bubble matrix with a calculated rotating ramp and exact sparse supplied deviations. Each frame rotates six levels; red stays31 and green/blue interpolate31 to25 across five intervals. This reproduces35 native words. The remaining original frame5/color0 white at $9B:9774 is stored as one explicit residual, and `bubble` remains unchecked. Independent custom edits remain exact; body edits and calculations remain independent.

Consumer inspection establishes a uniform six-frame, five-tick cycle at $91:DBA0-$DBBD and direct six-word copying to sprite palette6 colorsA..F at $91:DC88-$DCAE, also reflected by `SamusCrystalFlashState`. There is no special phase branch that establishes a functional rule for the exceptional white sample. No invented seam rule or retention exception has been introduced.

Verification build passed (1436 warnings, zero errors). Expanded `--lookup-stream2-crystal-body` passed all100 body and36 bubble original colors, actual CGRAM writes, every independently edited body/bubble cell, preservation of every other supplied cell, bounds, and stock residual count1. Twenty-seven named definitions remain complete;199 remain required, including this now-isolated residual.

### Batch 8: Bomb Torizo explosion placement and Chozo carry bob

Removed Bomb Torizo's six explosion-placement records. The two body sites select gut or face distances; their centered/right/left entries select the signed horizontal offset. Original facing adjustment, odd-parameter folding, centered padding entries and range errors remain exact. The separate hand-swipe trajectory remains required.

Removed Chozo's carried-Samus Y table. The acquisition/release poses place Samus32 and25 pixels above the statue, then the held pose is23 pixels above; the two four-pose stride cycles bob by a triangular0..2 pixels. Source instruction traversal $AA:E4C1-$E54D establishes repeated stride poses; $AA:E55B-$E573 reverses the acquisition poses on release. The unsupported complexity-based magnitude-retention comment was removed; independent velocity magnitudes remain required.

Verification build passed (1436 warnings, zero errors). `--lookup-stream2-body-placements` passed34 original Bomb Torizo geometry words,22 production swipe spawns and18 bounded explosion spawns with original geometry reads forbidden; the existing statue-walking proof passed96 original Chozo fields and actual movement/carried-Samus coordinates, including fractional arithmetic and invalid selectors. Its unchanged neighboring Golden Torizo39 byte windows also pass. An earlier invocation accidentally used the stale binary while build remained active and entered the default verifier, which stopped on a missing startup resource; no changes or task selection were based on that unrelated failure. The correctly rebuilt focused invocation passes.

Twenty-nine named definitions complete in stream2;197 remain required.
### Batch 9: Pipe Bug animation dispatch and Norfair loop structure

Removed both normal/strong Brinstar animation selector tables through semantic species/facing/shooting cases. Removed Norfair Pipe Bug mechanics and presentation-address tables: each facing half contains an eight-frame two-tick rising loop followed by a six-frame one-tick flight loop, each terminating with goto to its first record. Frame/control/visual operand addresses calculate from that program layout, without materializing a replacement table.

Verification build passed (1436 warnings, zero errors). `--lookup-stream2-pipe-programs` passed all eight native selectors and eight actual production handoffs with original selectors forbidden, plus all36 native Norfair mechanics words and four complete production loops with original mechanics and28 visual-selector reads forbidden. Thirty-three named definitions complete in stream2;193 remain required.
### Batch 10: reserve-screen digit glyphs and arrow geometry

Stock digit words now calculate as $0804+digit, matching the ADC operands at $82:8FB6/$8FC1/$8FCB. Stock arrow positions calculate the eight-cell vertical stem at byte$102 with64-byte stride and two-cell horizontal arm at byte$302, matching $82:AE07-$AE41. Loaders compare every supplied word/coordinate before selecting calculation; independent custom glyph fields, anchors, cell positions and palette indices remain exact. No stock digit/offset array is retained or rebuilt. Labels and animated arrow colors remain separately required.

Verification build passed (1436 warnings, zero errors). `--lookup-stream2-reserve-geometry` passed10 stock glyphs across all three positions,10 stock arrow cells, actual tilemap writes, all20 independent glyph/position edits, edited digit anchors and arrow palettes, and preservation of every unrelated tile/priority/flip field. Static native-instruction inspection establishes the source arithmetic; confirmation does not search gameplay. Thirty-five named definitions complete in stream2;191 remain required.
### Batch 11: gunship takeoff transfer geometry

Removed the five transfer records and their independently stored First..Fifth values. The computed immutable view advances source addresses by1024 bytes from $94:C800 and VRAM destinations by512 words from$7600; the contiguous typed asset identity advances with the same frame. All original Length/index/enumeration consumers remain supported without storing or rebuilding a transfer table. Coordinator-assigned `Program.EnemyTileArtwork.GunshipLiftoff.cs` changed only its explicit view type; its full existing artwork/rebinding proof remains intact.

Verification build passed (25 incremental warnings, zero errors); ResourceAudit restore/build passed (zero warnings/errors), confirming audit consumers compile. `--lookup-stream2-gunship-transfers` passed all10 original source/destination words, five typed asset identities, actual production queued source/destination fields, fifth-transfer engine phase transition, enumeration and bounds. No full artwork/whole-game test was claimed for this transfer-layout change.

Thirty-six named definitions complete in stream2;190 remain required. Work rotates to stream5 by coordinator instruction; no unchecked stream2 entry is exempted, completed or abandoned. In particular Kraid endpoint/deviation payloads, the single Crystal Flash bubble residual, ghost/Norfair color payloads, Chozo velocity magnitudes, Torizo swipe trajectory and reserve labels/arrow colors remain explicit required work.
### Batch 12: Dead Torizo DMA geometry and explosive swipe sequence

Removed both Dead Torizo descriptor arrays. Each phase calculates six of twelve ten-tile body rows with their clipped left/right bounds, source tile stride and VRAM row stride, then the corresponding sand strip. Phase parity and every live-WRAM source are preserved. The calculated view supports existing indexed/enumerated consumers without storing descriptors; assigned artwork verifier changes only its explicit view binding. Removed swipe mechanics and visual-address arrays: one packed sound instruction precedes five five-tick frames and deletion.

Verification build passed (1436 full-build warnings,25 incremental warnings, zero errors); ResourceAudit build passed without warnings/errors. New `--lookup-stream2-dead-torizo-transfers` passed all56 native descriptor fields, both terminators, fourteen actual queued transfers, phase advancement/parity, enumeration and bounds. `--torizo-explosive-swipe-instruction-mechanics` passed all7 native mechanics words, actual producer and exact25-frame lifetime,5 native compiled selectors and zero forbidden reads. Its initial run reached the obsolete visual assertion (expected5 reads, actual0); the coordinator-assigned verifier now checks zero reads plus exact native compiled operands, retaining all producer/lifetime checks. No production behavior changed for this fixture correction. The full Dead Torizo artwork test was preserved but not run for this descriptor-only change. Coordinator must refresh the DMA source review hash at integration.

Forty stream2 named definitions complete;186 remain required. Variable Torizo explosion timing, separate artwork and all previously pending palette residuals remain required.
### Batch 13: pause equipment ownership geometry

Removed equipment-label and reserve-region arrays. Membership now calculates directly from native label rows/columns: five beam rows, separated suit/misc groups and three boots rows; reserve labels occupy two seven-cell rows and digits retain their independent anchor. The nine-cell Plasma footprint, including the native VAR overrun, remains exact. Wireframe and arrow ownership calculations are preserved.

Verification build passed (1436 full-build warnings,25 incremental warnings, zero errors). `--lookup-stream2-pause-ownership` passed all1024 tilemap cells and both adjacent out-of-range values against all16 original destination words, checking equipment, total-live and noninventory ownership separately. The first reference fixture omitted the$3800 WRAM tilemap base; source inspection corrected that address normalization before the successful run. No production change was made in response to the fixture mistake. This proves the unchanged ownership masks used during custom-resource rebinding without requiring a full menu playthrough.

Forty-two stream2 named definitions complete;184 remain required.
### Batch 14: tube-crack palette partial derivation

The tube palette at$AA:E2DD contains two identical16-color halves. Colors8..15 interpolate RGB5(31,27,29) to(0,0,1) by seven nearest-integer steps. Those ramp values and the repeated half now calculate; only the first eight independent colors remain as seeds. Supplied resources preserve every independent edit through exact per-color deviations, including edits to one half without changing the other. The selected-content hash uses a temporary serialization buffer; no regenerated runtime palette is cached.

Verification build passed (1437 warnings, zero errors). `--lookup-stream2-tube-ramp` passed32 native colors, actual CGRAM writes, all32 independent color edits with unchanged other cells and statue palettes, canonical identity preservation and bounds. Stock content retains eight seed colors and zero deviations.

This is a partial conversion. `ChozoAndTubeColorCatalog.tubeCracks` remains unchecked: the eight seed color choices still require derivation or concrete impossible/nonsense evidence. Neither duplication nor the interpolation establishes an exception for those choices. The two statue palettes also remain required. Counts remain42 complete and184 required.
### Batch 15: Torizo Chozo-orb control structure

Removed mechanics and presentation-address tables. Left/right flight shares an85-tick pose/goto structure; wall and shot impacts share property setup and five four-tick frames, with the shot path then selecting Bomb/Golden drop headers. Floor impact calculates its six durations4..9 after explicit property/sound setup. Program and visual operand addresses derive from those layouts without rebuilt tables.

Verification build passed (1437 full-build warnings,25 incremental warnings, zero errors). `--torizo-chozo-orb-instruction-mechanics` passed all40 original words, four actual producer loops, exact wall/floor lifetimes, both shot/drop paths,18 original compiled sprite operands and zero forbidden reads. The first run reached the obsolete visual assertion (expected18 reads, actual0); coordinator-assigned verification now checks zero reads and all18 exact native selectors while preserving every producer/lifetime/drop assertion. No production workaround was used.

Forty-four stream2 named definitions complete;182 remain required. Palette residuals and unrelated artwork remain explicitly pending.
### Batch 16: gunship entrance/hull and liftoff-dust programs

Removed hull/entrance mechanics and visual-address arrays. Opening preserves its40-tick entry and24-tick intermediate holds, then accelerates8..4; closing reverses the same transition durations. Both static hulls calculate one-tick poses followed by sleep. Removed all six dust program records and their nested duration arrays: paired sets of three shapes share calculated program lengths/entry addresses, and each duration is its shape's initial hold plus min(frame/2,2). No duration/program lookup is rebuilt.

Verification build passed (1437 full-build warnings,25 incremental warnings, zero errors). `--gunship-instruction-mechanics` passed28 native mechanics words, five actual hull/pad program paths, both initializer identities and22 native selectors. `--gunship-dust-instruction-mechanics` passed76 native mechanics words, all six actual producers/frame loops/deletions and46 native selectors. Both initially reached obsolete visual-read counts (expected22/46, actual0); assigned verifiers now assert zero reads and every exact native compiled operand while retaining all existing behavior checks. Corrected the hull check's old console wording after the successful execution; assertions are unchanged.

Forty-seven stream2 named definitions complete;179 remain required. Separate pixels and independent palette/trajectory residuals remain pending.
### Batch 17: projectile trail selector semantic dispatch, partial

Removed the78 authored left/right selector words. Selection now dispatches on named Wave/Ice/Spazer/Plasma combinations, charge state, missile/super-missile and Spazer SBA variants. Invalid simultaneous Spazer/Plasma combinations retain empty trails. The original39-word table overlap remains exact for the complete low-six-bit selector domain.

The remaining25 words are physical observations of code bytes at$90:B657-$B688, after the right table. Pinned source identifies those bytes as Spawn projectile trail itself, including opcodes, native addresses and relative branches. The final$A96B combines RTL at$B687 with the LDA-immediate opcode at$B688; its operand lies outside the window. This residual is pending coordinator review for a narrow nonsense exception; it is not declared resolved here. No native assembler/code-layout model has been introduced.

Verification build passed (1437 full-build warnings,25 incremental warnings, zero errors). New `--lookup-stream2-trail-selectors` runs only the existing selector proof's relevant assertions: all103 reachable native words, every64 actual spawn selection, original timers/origin and bounds, with the full selector ROM window forbidden. It does not run the unrelated coordinate catalog checks. The original broader verifier remains unchanged.

Counts remain47 complete and179 required; `ProjectileTrailDefinitions.ReachableSelectors` remains unchecked until its residual disposition is accepted.
### Batch 18: Golden Torizo egg program structure

Removed mechanics and visual-address tables. Paired facing programs calculate their bounce48-tick sleep, three four-tick pre-hatch poses, shared property handoff, hatched four-pose six-tick loop and five-pose break. Explicit controls preserve sound, movement callback, shared hatch target and deletion; the final left/right break hold remains10/8 ticks. All offsets derive from program layout, including packed sound bytes.

Verification build passed (1437 full-build warnings,25 incremental warnings, zero errors). `--golden-torizo-egg-instruction-mechanics` passed all53 original private mechanics words, both actual producers through bounce/hatch/charge/floor-break, the shared shot-break program,26 original compiled visual operands and zero forbidden reads. The first run reached the obsolete visual assertion (expected26 reads, actual0); assigned verification now checks zero reads and exact native selectors while preserving the existing complete producer/state/lifetime assertions.

Forty-nine stream2 named definitions complete;177 remain required. Pending palette seeds/residuals and adjacent trail code observations retain their explicit unresolved status.
### Batch 19: Kago body and bug program structure

Removed both body mechanics/visual-address tables: two four-pose loops share a twenty-byte layout, with ten-tick idle and three-tick post-hit holds. Removed bug mechanics/visual-address tables through landed/falling/jumping/shot program cases, shared indefinite hold loops and a five-frame four-tick shot sequence. Explicit controls preserve idle/jump callbacks, palette reset, drop creation and shared deletion.

Verification build passed (1437 full-build warnings,25 incremental warnings, zero errors). `--kago-instruction-mechanics` passed12 native mechanics words, actual slow/post-hit loops and bug-spawning handoff,8 compiled native selectors and zero reads. `--kago-bug-projectile-instruction-mechanics` passed23 native mechanics words, the actual producer, landed/falling/jump paths, complete shot/drop/deletion sequence,11 compiled native selectors and zero reads. Its first run reached the obsolete visual-read assertion (expected11, actual0); the assigned verifier now asserts zero reads and all11 exact native selectors while retaining every behavioral assertion.

Fifty-three stream2 named definitions complete;173 remain required. Palette residuals and adjacent trail code observations remain unresolved pending their specific review.
### Batch 20: Chozo/Tourian dust program structure

Removed the mechanics and visual-address arrays. Footsteps and spike explosions share random placement, fixed-duration frame runs and deletion. Tourian dust calculates its initial64-cycle counter, reset/randomize controls, four two-tick poses, decrement/branch and deletion. Native packed X/Y radius bytes remain explicit placement arguments rather than per-record tables. No replacement program table is built.

Verification build passed (1437 full-build warnings,25 incremental warnings, zero errors). `--chozo-tourian-dust-instruction-mechanics` passed all31 native mechanics words, actual footstep and Tourian producers, alternate explosion initialization, positions, counted-loop/deletion behavior,14 exact native compiled visual selectors and zero runtime ROM reads. Initial execution passed the behavioral assertions and reached the obsolete expected14 visual reads versus actual0; the assigned verifier now confirms zero reads and all native operands, retaining every behavior check.

Fifty-five stream2 definitions complete;171 remain required. Unresolved independent payloads and code-byte observations remain pending.
### Pending ghost palette source review

The16 target words at$A8:99AC are copied unchanged by$A8:9B9B-$9BA9, then every RGB component advances independently toward its target through$A8:9E88-$9F4E. This establishes transport/fading, not irreducibility. Static decoding of pinned$B1:A600's1024 bytes against all three actual OAM compositions at$A8:9E46/$9E5C/$9E72 shows only palette slots2..8 used by visible pixels. Counts across the three frames are: slot2=6/6/6,3=170/170/171,4=266/260/256,5=5/5/5,6=31/31/29,7=83/79/87,8=117/123/122. Slot0 is transparent; slots1 and9..15 are unused throughout all32 source tiles. The shared head uses tiles0..3/16..19; the three lower-body compositions together use every remaining tile. Render inspection associates2/5 with skull highlights,3/4 with shadow/outline and6/7/8 with intermediate skull/body shades. All visible colors have equal red/green components, but their shade levels and blue offsets are not a uniform ramp.

All16 words exactly match Kago's palette at$A8:AAFE. The first nine also match Yapping Maw at$A8:9F4F, whose last seven are zero. Thus the ghost's unused red/orange tail is shared palette content rather than eye or body colors; an earlier proposed color-role inference was incorrect. All target words still participate in component fading, so unused sprite slots have not been dropped. Independent review of the shared artistic shade choices and any derivable subset remains pending. No retention exception or completion is claimed.
### Batch 21: ghost floating program structure

Removed the ghost mechanics and visual-address tables. Three sixteen-tick poses calculate their four-byte stride, followed by goto and the first-pose target. The actual initializer and looping behavior are unchanged.

Verification build passed (1437 full-build warnings,25 incremental warnings, zero errors). `--wrecked-ship-ghost-instruction-mechanics` passed all5 native words, the actual initializer, complete floating loop,3 exact native compiled selectors and zero runtime ROM reads. Initial execution passed program behavior before failing the obsolete expected3 reads versus actual0; assigned verification now asserts zero reads and all original operands, preserving runtime and rejection checks.

Fifty-seven stream2 named definitions complete;169 remain required. The ghost palette remains pending as documented above.
### Batch 22: projectile-trail program layout and fall cadence

Removed four mechanics membership tables and the42-entry visual record-address table. Ice trails contain six initial poses before their first one-pixel fall, two more poses before the next fall, then a fall after each remaining short pose. Seventeen poses and ten commands determine each terminator; the final ice pose holds four ticks. Wave/missile trails each contain four four-tick poses. A calculated indexed/enumerated view derives every address without caching a regenerated table. Independent artwork payloads and the previously documented adjacent-code selector residual remain pending.

Verification build passed (1437 full-build warnings,25 incremental warnings, zero errors); ResourceAudit build passed without warnings/errors. New `--lookup-stream2-trail-programs` passed all67 native mechanics words,42 independently walked visual addresses, rejected gaps/odd addresses/wrong banks, every frame of all four paired production sequences with frozen frames and sibling-directed Y movement, view bounds and the existing typed mutable-memory alias. The initial copied legacy reference path incorrectly ran production without installed artwork and attempted the retired raw sprite-word fallback at$90:B4CD; it failed as expected. The reference now decodes native ROM instructions independently while the actual side runs the compiled walker with a catalog and forbidden cartridge bus. No production workaround or broader artwork sweep was added.

Sixty-two stream2 named definitions complete;164 remain required. Rotating to stream5 by coordinator instruction; all unresolved palette/trajectory/artwork/code-byte entries remain explicitly required.
### Accepted mixed disposition: adjacent native trail code observations

Independent coordinator review accepted a narrow nonsense exception for exactly25 words at$90:B657-$B688. The native right selector base$B609 and low-six-bit index admit these observations beyond its39 actual entries. Pinned source identifies PHB, native operands and branches through RTL followed by the first LDA opcode byte; the final word combines two instructions. All25 values were confirmed against the supported ROM SHA25612B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72. Managed behavior has no equivalent native instruction encoding/relocation layout; implementing an assembler or re-encoding those instructions would only reconstruct the same observed bytes.

The exception covers only these physical code-byte observations. The first78 left/right selector mappings remain calculated semantic beam/effect dispatch. The existing `--lookup-stream2-trail-selectors` proof already passed all103 native words and every64 actual spawn selection with source reads forbidden; this follow-up changes documentation/disposition only and does not rerun unrelated checks. Neither the separate coordinate catalog's code window nor any palette residual receives an exception here.

Stream2 now has63 resolved definitions:62 converted/removed and1 mixed conversion with narrowly justified retention.163 remain required. This is explicitly not full conversion of the selector definition.
### Batch 23: trail beam-family and directional pointer dispatch

Removed the174-word frozen pointer dictionary and its factory. Native$9B:A418/A42A/A43C dispatches the beam flags; meaningful cases now choose default, Wave, Ice-Spazer, Ice-Plasma or SBA geometry using the existing flag domain. Direction families use the existing ten-direction domain, merged vertical facings, shared opposite Wave axes and packed four-byte coordinate sequence widths. No calculated pointer table is rebuilt or cached. All870 coordinate records and the separate28 adjacent code bytes remain pending.

Verification build passed1437 warnings/zero errors. Existing --projectile-trail-coordinates passed every174 native pointer word, all catalog bytes and odd/wrapped word reads, plus6600 actual beam/charged/SBA/missile spawns asserting all four native output positions with coordinate ROM reads forbidden. Existing196608 operand boundary assertions also passed, retaining live-memory and open-bus behavior. Sixty-four stream2 definitions resolved:63 converted/removed and1 mixed trail selector.162 remain required.

### Batch 24: regular trail coordinate waveforms, partial payload

Removed170 of870 stored four-coordinate records. Both default sequences calculate zero. Uncharged Wave derives its16-frame signed oscillation by reflection around the peak; cardinal ramps start at8 then step4, diagonal ramps step4 twice then2 twice. Charged cardinal Wave repeats each phase twice and caps its symmetric spread at16. Cardinal Ice-Spazer and unused SBA sequences use8-pixel linear spread; cardinal Wave-Ice-Spazer uses4-pixel spread capped at16 with mirrored return. Actual reads calculate these values directly; the pending dictionary contains only the700 remaining records.

Verification build passed1437 warnings/zero errors. Reran --projectile-trail-coordinates: all native870 coordinate records,174 pointers, bounded observations,6600 actual spawns with all four positions and196608 operand-boundary assertions pass without coordinate ROM reads. Frames remains unchecked for its700 pending records; no residual or adjacent-code exemption is claimed. Counts remain64 resolved/162 required.

## Golden Torizo initial/landing control batch

Pinned AA:C9CB-C9E1 and CDAF-CDD6 establish these control-flow cases. Removed both stored Words lists without recreating arrays. `--lookup-stream2-golden-control` confirms allseven initial words with an independent native-width cursor, all20 landing words via the existing focused verifier, exact installed initial sprite selector, lookup values, both-byte ownership, and adjacent/index boundaries. Build1437 existing warnings/zero errors. No remaining animation-hold payload was exempted: these lists install a single static pose or dispatch attack/return operations. Integrated stream checkpoint:65 converted, one mixed trail selector,160 required. Other worker batches remain queued for review.
