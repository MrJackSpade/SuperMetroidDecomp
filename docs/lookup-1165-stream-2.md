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
- [ ] **ChozoCarryMotionDefinitions.YOffsets** ([L26](../csharp/src/SuperMetroid.Core/Game/ChozoCarryMotionDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs

- [ ] **ChozoStatueInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ChozoStatueInstructionProgramDefinitions.PresentationWords** ([L85](../csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs#L85)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs

- [ ] **ChozoTourianDustInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ChozoTourianDustInstructionProgramDefinitions.PresentationWords** ([L63](../csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs#L63)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **GunshipLiftoffTransferDefinitions.Entries** ([L31](../csharp/src/SuperMetroid.Core/Assets/GunshipLiftoffArtworkCatalog.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GunshipDustInstructionProgramDefinitions.cs

- [ ] **GunshipDustInstructionProgramDefinitions.Programs** ([L40](../csharp/src/SuperMetroid.Core/Game/GunshipDustInstructionProgramDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GunshipInstructionProgramDefinitions.cs

- [ ] **GunshipInstructionProgramDefinitions.Words** ([L38](../csharp/src/SuperMetroid.Core/Game/GunshipInstructionProgramDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GunshipInstructionProgramDefinitions.PresentationWords** ([L54](../csharp/src/SuperMetroid.Core/Game/GunshipInstructionProgramDefinitions.cs#L54)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HZoomerInstructionProgramDefinitions.cs

- [x] **HZoomerInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/HZoomerInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **HZoomerInstructionProgramDefinitions.PresentationWords** ([L53](../csharp/src/SuperMetroid.Core/Game/HZoomerInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/KagoVisualDefinitions.cs

- [ ] **KagoVisualDefinitions.Frames / literal at L15** ([L15](../csharp/src/SuperMetroid.Core/Assets/KagoVisualDefinitions.cs#L15)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs

- [ ] **KagoBugProjectileInstructionProgramDefinitions.Words** ([L53](../csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KagoBugProjectileInstructionProgramDefinitions.PresentationWords** ([L80](../csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs#L80)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs

- [ ] **KagoInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KagoInstructionProgramDefinitions.PresentationWords** ([L25](../csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **NorfairPipeBugInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NorfairPipeBugInstructionProgramDefinitions.PresentationWords** ([L36](../csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **PauseEquipmentBaseDefinitions.equipmentLabelRegions** ([L17](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PauseEquipmentBaseDefinitions.reserveRegions** ([L29](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs

- [ ] **PauseEquipmentBasePresentation.tilemap** ([L10](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs#L10)) - installed stock table. Original/default payload behind PauseEquipmentBasePresentation.tilemap. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs

- [ ] **PauseEquipmentLabelPresentation.labels** ([L12](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs#L12)) - installed stock table. Original/default payload behind PauseEquipmentLabelPresentation.labels. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PauseEquipmentLabelPresentation.blank** ([L13](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs#L13)) - installed stock table. Original/default payload behind PauseEquipmentLabelPresentation.blank. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs

- [ ] **PauseReserveUiPresentation.labels** ([L10](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L10)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.labels. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PauseReserveUiPresentation.digits** ([L12](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L12)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.digits. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PauseReserveUiPresentation.arrowOffsets** ([L13](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L13)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.arrowOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
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

- [ ] **PipeBugDefinitions.BrinstarInstructionLists** ([L35](../csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PipeBugDefinitions.StrongBrinstarInstructionLists** ([L47](../csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **ProjectileTrailVisualDefinitions.Frames** ([L10](../csharp/src/SuperMetroid.Core/Assets/ProjectileTrailVisualDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs

- [ ] **ProjectileTrailCoordinateDefinitions.ReachableAdjacentCode** ([L215](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs#L215)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ProjectileTrailCoordinateDefinitions.Pointers** ([L222](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs#L222)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by CreatePointers(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **ProjectileTrailCoordinateDefinitions.Frames** ([L223](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailCoordinateDefinitions.cs#L223)) - factory-built stock table. Stored FrozenDictionary<int, Offset> initialized by CreateFrames(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/ProjectileTrailDefinitions.cs

- [ ] **ProjectileTrailDefinitions.ReachableSelectors** ([L24](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs

- [ ] **ProjectileTrailProgramDefinitions.Ends** ([L10](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ProjectileTrailProgramDefinitions.LeftMoves** ([L12](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ProjectileTrailProgramDefinitions.RightMoves** ([L14](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ProjectileTrailProgramDefinitions.FourTickFrames** ([L16](../csharp/src/SuperMetroid.Core/Game/ProjectileTrailProgramDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [ ] **BombTorizoAttackDefinitions.ExplosionPlacements** ([L37](../csharp/src/SuperMetroid.Core/Game/BombTorizoAttackDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs

- [ ] **BombTorizoMovementDefinitions.PostureX** ([L15](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **BombTorizoMovementDefinitions.PostureY** ([L18](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **BombTorizoMovementDefinitions.WalkVelocities** ([L25](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTorizoInstructionProgramDefinitions.cs

- [ ] **DeadTorizoInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/DeadTorizoInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs

- [ ] **DeadTorizoVramTransferDefinitions.Even** ([L28](../csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadTorizoVramTransferDefinitions.Odd** ([L39](../csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **GoldenTorizoEggInstructionProgramDefinitions.Words** ([L37](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoEggInstructionProgramDefinitions.PresentationWords** ([L100](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs#L100)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoEyeBeamInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWords** ([L60](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoInitialInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoInitialInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoInitialInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoJumpLandingInstructionProgramDefinitions.cs

- [ ] **GoldenTorizoJumpLandingInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoJumpLandingInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **TorizoChozoOrbInstructionProgramDefinitions.Words** ([L29](../csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoChozoOrbInstructionProgramDefinitions.PresentationWords** ([L76](../csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs

- [ ] **TorizoCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoCollisionDefinitions.Hitboxes** ([L124](../csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs#L124)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs

- [ ] **TorizoExplosionInstructionProgramDefinitions.Words** ([L28](../csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoExplosionInstructionProgramDefinitions.PresentationWords** ([L86](../csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs#L86)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs

- [ ] **TorizoExplosiveSwipeInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWords** ([L29](../csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **WreckedShipGhostInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **WreckedShipGhostInstructionProgramDefinitions.PresentationWords** ([L31](../csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
