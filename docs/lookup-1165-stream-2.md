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

- [x] **ChozoAndTubeColorCatalog.tubeCracks** ([L22](../csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs#L22)) - installed stock table. Original/default payload behind ChozoAndTubeColorCatalog.tubeCracks. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **ChozoAndTubeColorCatalog.wreckedShip** ([L23](../csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs#L23)) - installed stock table. Original/default payload behind ChozoAndTubeColorCatalog.wreckedShip. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **ChozoAndTubeColorCatalog.lowerNorfair** ([L24](../csharp/src/SuperMetroid.Core/Assets/ChozoAndTubeColorCatalog.cs#L24)) - installed stock table. Original/default payload behind ChozoAndTubeColorCatalog.lowerNorfair. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/ChozoCarryMotionDefinitions.cs

- [x] **ChozoCarryMotionDefinitions.Magnitudes** ([L15](../csharp/src/SuperMetroid.Core/Game/ChozoCarryMotionDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChozoCarryMotionDefinitions.YOffsets** ([L26](../csharp/src/SuperMetroid.Core/Game/ChozoCarryMotionDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs

- [x] **ChozoStatueInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChozoStatueInstructionProgramDefinitions.PresentationWords** ([L85](../csharp/src/SuperMetroid.Core/Game/ChozoStatueInstructionProgramDefinitions.cs#L85)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs

- [x] **ChozoTourianDustInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChozoTourianDustInstructionProgramDefinitions.PresentationWords** ([L63](../csharp/src/SuperMetroid.Core/Game/ChozoTourianDustInstructionProgramDefinitions.cs#L63)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrawlerAnimationDefinitions.cs

- [x] **CrawlerAnimationDefinitions.InitialFamilies** ([L48](../csharp/src/SuperMetroid.Core/Game/CrawlerAnimationDefinitions.cs#L48)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CrawlerAnimationDefinitions.SurfaceSpecies** ([L81](../csharp/src/SuperMetroid.Core/Game/CrawlerAnimationDefinitions.cs#L81)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrawlerSpeedDefinitions.cs

- [x] **CrawlerSpeedDefinitions.Speeds** ([L16](../csharp/src/SuperMetroid.Core/Game/CrawlerSpeedDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs

- [x] **CrystalFlashColorCatalog.body** ([L14](../csharp/src/SuperMetroid.Core/Assets/CrystalFlashColorCatalog.cs#L14)) - installed stock table. Original/default payload behind CrystalFlashColorCatalog.body. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **CrystalFlashColorCatalog.bubble** - Mixed conversion/justified retention:35 rotating RGB5 ramp words calculate; only independently painted frame5/color0 white at9B:9774 remains supplied. Independent source/consumer review approved this one sample; see disposition below. Not fully converted.

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

- [x] **KagoVisualDefinitions.Frames / literal at L15** ([L15](../csharp/src/SuperMetroid.Core/Assets/KagoVisualDefinitions.cs#L15)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs

- [x] **KagoBugProjectileInstructionProgramDefinitions.Words** ([L53](../csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KagoBugProjectileInstructionProgramDefinitions.PresentationWords** ([L80](../csharp/src/SuperMetroid.Core/Game/KagoBugProjectileInstructionProgramDefinitions.cs#L80)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs

- [x] **KagoInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KagoInstructionProgramDefinitions.PresentationWords** ([L25](../csharp/src/SuperMetroid.Core/Game/KagoInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs

- [x] **KraidColorCatalog.roomBackdrop** ([L25](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L25)) - installed stock table. Original/default payload behind KraidColorCatalog.roomBackdrop. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **KraidColorCatalog.initialTarget** ([L26](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L26)) - installed stock table. Original/default payload behind KraidColorCatalog.initialTarget. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **KraidColorCatalog.health** ([L27](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L27)) - installed stock table. Original/default payload behind KraidColorCatalog.health. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **KraidColorCatalog.secondary** ([L28](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L28)) - installed stock table. Original/default payload behind KraidColorCatalog.secondary. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **KraidColorCatalog.deathArm** ([L29](../csharp/src/SuperMetroid.Core/Assets/KraidColorCatalog.cs#L29)) - installed stock table. Original/default payload behind KraidColorCatalog.deathArm. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/KraidHeadTilemapAtlas.cs

- [ ] **KraidHeadTilemapAtlas.words** ([L12](../csharp/src/SuperMetroid.Core/Assets/KraidHeadTilemapAtlas.cs#L12)) - installed stock table. Original/default payload behind KraidHeadTilemapAtlas.words. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs

- [x] **KraidArmCollisionDefinitions.Phase0** ([L13](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Phase1** ([L16](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Phase2** ([L19](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Phase3** ([L22](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Phase4** ([L25](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Phase5** ([L28](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Phase6** ([L31](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Phase7** ([L34](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Phase8** ([L37](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Phase9** ([L40](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KraidArmCollisionDefinitions.Geometry** ([L64](../csharp/src/SuperMetroid.Core/Game/KraidArmCollisionDefinitions.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.KraidGrowth.cs

- [x] **RoomEnemySystem.FinishKraidGrowth / lintTimers** ([L157](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.KraidGrowth.cs#L157)) - method-local definition. Three fixed lint spawn delays indexed by part ordinal. Replace the stored delays with their native timing rule or semantic part cases.

### csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs

- [ ] **KraidRoomPlmDrawDefinitions.All** ([L115](../csharp/src/SuperMetroid.Core/Rooms/KraidRoomPlmDrawDefinitions.cs#L115)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NinjaSpacePirateInstructionProgramDefinitions.cs

- [x] **NinjaSpacePirateInstructionProgramDefinitions.Words** ([L56](../csharp/src/SuperMetroid.Core/Game/NinjaSpacePirateInstructionProgramDefinitions.cs#L56)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **NinjaSpacePirateInstructionProgramDefinitions.PresentationWords** - CONVERTED: visual operand addresses derive from native command widths and paired action composition; independent timing/spawn/palette choices remain under Words.

### csharp/src/SuperMetroid.Core/Game/NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.cs

- [x] **NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Durations** ([L39](../csharp/src/SuperMetroid.Core/Game/NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Definitions** ([L42](../csharp/src/SuperMetroid.Core/Game/NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NorfairLavaJumpDefinitions.cs

- [ ] **NorfairLavaJumpDefinitions.InitialVerticalVelocities** ([L11](../csharp/src/SuperMetroid.Core/Game/NorfairLavaJumpDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NorfairLavaJumperInstructionProgramDefinitions.cs

- [x] **NorfairLavaJumperInstructionProgramDefinitions.Words** ([L33](../csharp/src/SuperMetroid.Core/Game/NorfairLavaJumperInstructionProgramDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **NorfairLavaJumperInstructionProgramDefinitions.PresentationWords** ([L60](../csharp/src/SuperMetroid.Core/Game/NorfairLavaJumperInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs

- [x] **NorfairPipeBugInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **NorfairPipeBugInstructionProgramDefinitions.PresentationWords** ([L36](../csharp/src/SuperMetroid.Core/Game/NorfairPipeBugInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NorfairRioInstructionProgramDefinitions.cs

- [x] **NorfairRioInstructionProgramDefinitions.Words** ([L31](../csharp/src/SuperMetroid.Core/Game/NorfairRioInstructionProgramDefinitions.cs#L31)) - factory-built stock table. Stored NorfairRioInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **NorfairRioInstructionProgramDefinitions.PresentationWords** ([L33](../csharp/src/SuperMetroid.Core/Game/NorfairRioInstructionProgramDefinitions.cs#L33)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.cs

- [x] **OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Durations** ([L43](../csharp/src/SuperMetroid.Core/Game/OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Definitions** ([L46](../csharp/src/SuperMetroid.Core/Game/OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs

- [x] **OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.ColorOffsets** ([L39](../csharp/src/SuperMetroid.Core/Game/OldTourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs

- [ ] **PauseBackdropPresentation.areas** ([L10](../csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs#L10)) - installed stock table. Original/default payload behind PauseBackdropPresentation.areas. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PauseBackdropPresentation.buttons** ([L11](../csharp/src/SuperMetroid.Core/Assets/PauseBackdropPresentation.cs#L11)) - installed stock table. Original/default payload behind PauseBackdropPresentation.buttons. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs

- [x] **PauseEquipmentBaseDefinitions.equipmentLabelRegions** ([L17](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PauseEquipmentBaseDefinitions.reserveRegions** ([L29](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBaseDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs

- [x] **PauseEquipmentBasePresentation.tilemap** ([L10](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentBasePresentation.cs#L10)) - installed stock table. Original/default payload behind PauseEquipmentBasePresentation.tilemap. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs

- [x] **PauseEquipmentLabelPresentation.labels** ([L12](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs#L12)) - installed stock table. Original/default payload behind PauseEquipmentLabelPresentation.labels. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **PauseEquipmentLabelPresentation.blank** ([L13](../csharp/src/SuperMetroid.Core/Assets/PauseEquipmentLabelPresentation.cs#L13)) - installed stock table. Original/default payload behind PauseEquipmentLabelPresentation.blank. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs

- [x] **PauseReserveUiPresentation.labels** ([L10](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L10)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.labels. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **PauseReserveUiPresentation.digits** ([L12](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L12)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.digits. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **PauseReserveUiPresentation.arrowOffsets** ([L13](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L13)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.arrowOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **PauseReserveUiPresentation.arrowFrames** ([L16](../csharp/src/SuperMetroid.Core/Assets/PauseReserveUiPresentation.cs#L16)) - installed stock table. Original/default payload behind PauseReserveUiPresentation.arrowFrames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/PauseWireframePresentation.cs

- [x] **PauseWireframePresentation.frames** ([L8](../csharp/src/SuperMetroid.Core/Assets/PauseWireframePresentation.cs#L8)) - installed stock table. Original/default payload behind PauseWireframePresentation.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Pickups.cs

- [x] **RoomEnemySystem.EnemyDropAccumulatorOrder** ([L29](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Pickups.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs

- [x] **PipeBugVisualDefinitions.Brinstar** ([L18](../csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PipeBugVisualDefinitions.Norfair** ([L37](../csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PipeBugVisualDefinitions.Yellow** ([L51](../csharp/src/SuperMetroid.Core/Assets/PipeBugVisualDefinitions.cs#L51)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs

- [x] **PipeBugDefinitions.BrinstarInstructionLists** ([L35](../csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PipeBugDefinitions.StrongBrinstarInstructionLists** ([L47](../csharp/src/SuperMetroid.Core/Game/PipeBugDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PipeBugs.cs

- [x] **RoomEnemySystem.RunNorfairPipeBugSamusWait / staggerTargets** ([L453](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PipeBugs.cs#L453)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.RunNorfairPipeBugSamusWait / staggerFunctions** ([L455](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PipeBugs.cs#L455)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Assets/ProjectileFrameBindingCatalog.cs

- [ ] **ProjectileFrameBindingCatalog.sprites** ([L13](../csharp/src/SuperMetroid.Core/Assets/ProjectileFrameBindingCatalog.cs#L13)) - installed stock table. Original/default payload behind ProjectileFrameBindingCatalog.sprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteCatalog.cs

- [ ] **ProjectileSpriteCatalog.frames** ([L10](../csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteCatalog.cs#L10)) - installed stock table. Original/default payload behind ProjectileSpriteCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteDefinitions.cs

- [x] **ProjectileSpriteDefinitions.NativePointers** ([L12](../csharp/src/SuperMetroid.Core/Assets/ProjectileSpriteDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **ScrollBoundaryCamera.TrackMovedSamusHorizontally / facingRightOffsets** ([L172](../csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs#L172)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **ScrollBoundaryCamera.TrackMovedSamusHorizontally / facingLeftOffsets** ([L173](../csharp/src/SuperMetroid.Core/Game/ScrollBoundaryCamera.cs#L173)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs

- [x] **TorizoInstructionVramArtworkDefinitions.Pages** ([L51](../csharp/src/SuperMetroid.Core/Assets/TorizoInstructionVramArtwork.cs#L51)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BombTorizoAttackDefinitions.cs

- [ ] **BombTorizoAttackDefinitions.SwipePlacements** ([L17](../csharp/src/SuperMetroid.Core/Game/BombTorizoAttackDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **BombTorizoAttackDefinitions.ExplosionPlacements** ([L37](../csharp/src/SuperMetroid.Core/Game/BombTorizoAttackDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs

- [x] **BombTorizoMovementDefinitions.PostureX** ([L15](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **BombTorizoMovementDefinitions.PostureY** ([L18](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **BombTorizoMovementDefinitions.WalkVelocities** ([L25](../csharp/src/SuperMetroid.Core/Game/BombTorizoMovementDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTorizoInstructionProgramDefinitions.cs

- [x] **DeadTorizoInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/DeadTorizoInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs

- [x] **DeadTorizoVramTransferDefinitions.Even** ([L28](../csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DeadTorizoVramTransferDefinitions.Odd** ([L39](../csharp/src/SuperMetroid.Core/Game/DeadTorizoVramTransferDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs

- [x] **GoldenTorizoAwakeningCollisionDefinitions.Frames** ([L22](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoAwakeningCollisionDefinitions.SeatedLow** ([L33](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoAwakeningCollisionDefinitions.SeatedMid** ([L35](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoAwakeningCollisionDefinitions.SeatedHigh** ([L37](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoAwakeningCollisionDefinitions.Initial** ([L39](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoAwakeningCollisionDefinitions.Rising** ([L41](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoAwakeningCollisionDefinitions.StandingMid** ([L43](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoAwakeningCollisionDefinitions.StandingHigh** ([L45](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningCollisionDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoAwakeningInstructionProgramDefinitions.cs

- [x] **GoldenTorizoAwakeningInstructionProgramDefinitions.Words** - PARTIAL: composed69 native controls, sequential movement indexes, increasing sitting holds and halving upload holds. Independent hold scales/transitions and loop counts remain REQUIRED.
- [x] **GoldenTorizoAwakeningInstructionProgramDefinitions.PresentationWords** - CONVERTED: pose/function/branch/DMA record widths calculate all21 visual operand addresses; native selectors and selected physical frames confirmed.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs

- [x] **GoldenTorizoEggInstructionProgramDefinitions.Words** ([L37](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoEggInstructionProgramDefinitions.PresentationWords** ([L100](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEggInstructionProgramDefinitions.cs#L100)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.cs

- [x] **GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs

- [x] **GoldenTorizoEyeBeamInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWords** ([L60](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoEyeBeamInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoInitialInstructionProgramDefinitions.cs

- [x] **GoldenTorizoInitialInstructionProgramDefinitions.Words** - Semantic tile upload/stance/lock/wake/pose/sleep dispatch; calculate instruction positions including the seven-byte DMA payload and visual operand gap. No independent timing sequence remains in this one-pose-then-sleep entry.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoJumpLandingInstructionProgramDefinitions.cs

- [x] **GoldenTorizoJumpLandingInstructionProgramDefinitions.Words** - Four facing/forward-foot cases choose named orb/sonic attack entries and return to the opposite moving leg; calculate each five-word control sequence and its addresses.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbCollisionDefinitions.cs

- [x] **GoldenTorizoLeftFootOrbCollisionDefinitions.Frames** ([L14](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbCollisionDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbInstructionProgramDefinitions.cs

- [x] **GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoLeftFootOrbInstructionProgramDefinitions.PresentationWords** ([L52](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftFootOrbInstructionProgramDefinitions.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbCollisionDefinitions.cs

- [x] **GoldenTorizoLeftOrbCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoLeftOrbCollisionDefinitions.Body** ([L29](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbCollisionDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbInstructionProgramDefinitions.cs

- [x] **GoldenTorizoLeftOrbInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWords** ([L69](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftOrbInstructionProgramDefinitions.cs#L69)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftTurnInstructionProgramDefinitions.cs

- [x] **GoldenTorizoLeftTurnInstructionProgramDefinitions.Words** ([L29](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftTurnInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoLeftTurnInstructionProgramDefinitions.PresentationWords** ([L44](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoLeftTurnInstructionProgramDefinitions.cs#L44)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbCollisionDefinitions.cs

- [x] **GoldenTorizoRightOrbCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoRightOrbCollisionDefinitions.Body** ([L23](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbCollisionDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbInstructionProgramDefinitions.cs

- [x] **GoldenTorizoRightOrbInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoRightOrbInstructionProgramDefinitions.PresentationWords** ([L44](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightOrbInstructionProgramDefinitions.cs#L44)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicCollisionDefinitions.cs

- [x] **GoldenTorizoRightSonicCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoRightSonicCollisionDefinitions.Body** ([L38](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicCollisionDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicInstructionProgramDefinitions.cs

- [x] **GoldenTorizoRightSonicInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoRightSonicInstructionProgramDefinitions.PresentationWords** ([L88](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightSonicInstructionProgramDefinitions.cs#L88)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs

- [x] **GoldenTorizoRightwardCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoRightwardCollisionDefinitions.StandingBody** ([L28](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoRightwardCollisionDefinitions.SteppingBody** ([L30](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardCollisionDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardInstructionProgramDefinitions.cs

- [x] **GoldenTorizoRightwardInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoRightwardInstructionProgramDefinitions.PresentationWords** ([L104](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoRightwardInstructionProgramDefinitions.cs#L104)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoStunnedInstructionProgramDefinitions.cs

- [x] **GoldenTorizoStunnedInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoStunnedInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoSuperMissileInstructionProgramDefinitions.cs

- [x] **GoldenTorizoSuperMissileInstructionProgramDefinitions.Words** ([L28](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoSuperMissileInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWords** ([L77](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoSuperMissileInstructionProgramDefinitions.cs#L77)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkDefinitions.cs

- [x] **GoldenTorizoWalkDefinitions.Velocities** ([L7](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkDefinitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs

- [x] **GoldenTorizoWalkingCollisionDefinitions.Frames** ([L14](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoWalkingCollisionDefinitions.StandingBody** ([L28](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoWalkingCollisionDefinitions.SteppingBody** ([L30](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingCollisionDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingInstructionProgramDefinitions.cs

- [x] **GoldenTorizoWalkingInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWords** ([L92](../csharp/src/SuperMetroid.Core/Game/GoldenTorizoWalkingInstructionProgramDefinitions.cs#L92)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs

- [x] **RoomEnemySystem.LoadTorizoSharedPaletteRows / rowEleven** ([L273](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L273)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.LoadTorizoSharedPaletteRows / rowFifteen** ([L278](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L278)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.LoadBombTorizoPalette / rowNine** ([L294](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L294)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.LoadBombTorizoPalette / rowTen** ([L299](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L299)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.LoadGoldenTorizoBasePalette / rowNine** ([L316](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L316)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.LoadGoldenTorizoBasePalette / rowTen** ([L321](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L321)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.LoadTorizoDeathPalette / rowNine** ([L332](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L332)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.LoadTorizoDeathPalette / rowTen** ([L337](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L337)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.LoadGoldenTorizoFinalPalette / rowNine** ([L348](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L348)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.LoadGoldenTorizoFinalPalette / rowTen** ([L353](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.BombTorizo.cs#L353)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs

- [x] **RoomEnemySystem.DeadTorizoInitialGraphicsCopies** ([L22](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.DeadTorizoColumnWordOffsets** ([L40](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.DeadTorizoColumnMinimumY** ([L43](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTorizo.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoBellyPaletteFxProgramMechanicsDefinitions.cs

- [x] **TorizoBellyPaletteFxProgramMechanicsDefinitions.FrameDurations** ([L24](../csharp/src/SuperMetroid.Core/Game/TorizoBellyPaletteFxProgramMechanicsDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoBellyPaletteFxProgramMechanicsDefinitions.Definitions** ([L26](../csharp/src/SuperMetroid.Core/Game/TorizoBellyPaletteFxProgramMechanicsDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs

- [x] **TorizoChozoOrbInstructionProgramDefinitions.Words** ([L29](../csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoChozoOrbInstructionProgramDefinitions.PresentationWords** ([L76](../csharp/src/SuperMetroid.Core/Game/TorizoChozoOrbInstructionProgramDefinitions.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs

- [x] **TorizoCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoCollisionDefinitions.Hitboxes** ([L124](../csharp/src/SuperMetroid.Core/Game/TorizoCollisionDefinitions.cs#L124)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs

- [x] **TorizoExplosionInstructionProgramDefinitions.Words** ([L28](../csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoExplosionInstructionProgramDefinitions.PresentationWords** ([L86](../csharp/src/SuperMetroid.Core/Game/TorizoExplosionInstructionProgramDefinitions.cs#L86)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs

- [x] **TorizoExplosiveSwipeInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoExplosiveSwipeInstructionProgramDefinitions.PresentationWords** ([L29](../csharp/src/SuperMetroid.Core/Game/TorizoExplosiveSwipeInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftCollisionDefinitions.cs

- [x] **TorizoFallingLeftCollisionDefinitions.Components** ([L13](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoFallingLeftCollisionDefinitions.Body** ([L20](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftCollisionDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftInstructionProgramDefinitions.cs

- [x] **TorizoFallingLeftInstructionProgramDefinitions.Words** ([L30](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoFallingLeftInstructionProgramDefinitions.PresentationWords** ([L47](../csharp/src/SuperMetroid.Core/Game/TorizoFallingLeftInstructionProgramDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombLeft.cs

- [x] **TorizoInstructionProgramDefinitions.BombLeft** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombLeft.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombRight.cs

- [x] **TorizoInstructionProgramDefinitions.BombRight** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.BombRight.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.cs

- [x] **TorizoInstructionProgramDefinitions.PresentationWords** ([L406](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.cs#L406)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenLeft.cs

- [x] **TorizoInstructionProgramDefinitions.GoldenLeft** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenLeft.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenRight.cs

- [x] **TorizoInstructionProgramDefinitions.GoldenRight** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.GoldenRight.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.Shared.cs

- [x] **TorizoInstructionProgramDefinitions.Shared** ([L7](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionProgramDefinitions.Shared.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoInstructionVramTransferDefinitions.cs

- [ ] **TorizoInstructionVramTransferDefinitions.Entries** ([L17](../csharp/src/SuperMetroid.Core/Game/TorizoInstructionVramTransferDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoJumpBackCollisionDefinitions.cs

- [x] **TorizoJumpBackCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoJumpBackCollisionDefinitions.Body** ([L20](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackCollisionDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoJumpBackInstructionProgramDefinitions.cs

- [x] **TorizoJumpBackInstructionProgramDefinitions.Words** ([L18](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackInstructionProgramDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoJumpBackInstructionProgramDefinitions.PresentationWords** ([L74](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackInstructionProgramDefinitions.cs#L74)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftCollisionDefinitions.cs

- [x] **TorizoJumpBackLeftCollisionDefinitions.Frames** ([L13](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftCollisionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoJumpBackLeftCollisionDefinitions.Body** ([L20](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftCollisionDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftInstructionProgramDefinitions.cs

- [x] **TorizoJumpBackLeftInstructionProgramDefinitions.Words** ([L15](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftInstructionProgramDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoJumpBackLeftInstructionProgramDefinitions.PresentationWords** ([L71](../csharp/src/SuperMetroid.Core/Game/TorizoJumpBackLeftInstructionProgramDefinitions.cs#L71)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoLandingDustInstructionProgramDefinitions.cs

- [x] **TorizoLandingDustInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/TorizoLandingDustInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoLandingDustInstructionProgramDefinitions.PresentationWords** ([L39](../csharp/src/SuperMetroid.Core/Game/TorizoLandingDustInstructionProgramDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TorizoSonicBoomInstructionProgramDefinitions.cs

- [x] **TorizoSonicBoomInstructionProgramDefinitions.Words** ([L30](../csharp/src/SuperMetroid.Core/Game/TorizoSonicBoomInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TorizoSonicBoomInstructionProgramDefinitions.PresentationWords** ([L67](../csharp/src/SuperMetroid.Core/Game/TorizoSonicBoomInstructionProgramDefinitions.cs#L67)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [x] **RoomEnemySystem.WreckedShipGhostPalette** - MIXED COMPLETE: calculated channels plus23 narrowly reviewed categorical paint/transparent-payload inputs; see complete source packet below.

### csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs

- [x] **WreckedShipGhostInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **WreckedShipGhostInstructionProgramDefinitions.PresentationWords** ([L31](../csharp/src/SuperMetroid.Core/Game/WreckedShipGhostInstructionProgramDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/YardVisualDefinitions.cs

- [x] **YardVisualDefinitions.Groups** ([L17](../csharp/src/SuperMetroid.Core/Assets/YardVisualDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/YardDirectionDefinitions.cs

- [x] **YardDirectionDefinitions.Directions** ([L28](../csharp/src/SuperMetroid.Core/Game/YardDirectionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **YardDirectionDefinitions.AirborneInstructions** ([L61](../csharp/src/SuperMetroid.Core/Game/YardDirectionDefinitions.cs#L61)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/YardInstructionProgramDefinitions.cs

- [x] **YardInstructionProgramDefinitions.Words** ([L95](../csharp/src/SuperMetroid.Core/Game/YardInstructionProgramDefinitions.cs#L95)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

## Crystal Flash bubble: independently reviewed narrow disposition

Coordinator review accepted only frame5/color0 at9B:9774 as an independent painted color choice. Pinned9B:96D4-977E and supported ROM SHA25612B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72 confirm the final row bytesFF7F BF77 7F6F 5F6B 3F67 FF7F. Its leading white7FFF differs from calculated7BDF. Pinned91:DBA0-DBBD advances every five ticks uniformly;91:DC88-DCAE copies six words into sprite palette6 colorsA-F. There is no phase-specific operation from which to derive the extra white. Encoding its exact frame/color/value as a formula exception would merely restate this independent painted choice.

The other35 words remain calculated rotating ramp values. The exception does not cover any other palette or permit a reconstructed stock table. Custom edits remain independent sparse supplied content. Existing --lookup-stream2-crystal-body confirmation already passed all136 native body/bubble colors, actual CGRAM writes, all independent edits and stock residual countone. This commit changes comments/disposition only; no executable behavior or confirmation scope changed.

Integrated Stream2 checkpoint:65 converted and two mixed conversion/justified-retention definitions;159 required. Other queued worker conversions are not counted here.

## Norfair Pipe Bug formation rank and role dispatch

Removed both runtime local arrays in RunNorfairPipeBugSamusWait. Physical member order is center, upper-near, upper-far, lower-near, lower-far; release counters derive from center104 plus eight ticks per signed vertical rank. Named semantic role cases select the existing native post-rise functions. Dedicated PipeBugDefinitions documents native setup operandsB3:8C64-8CA4 and NTSC/PAL distinction; runtime simply applies the catalog operations. No table is regenerated or cached.

Build1437 existing warnings/zero errors. New --lookup-stream2-pipe-bug-formation confirms allten native immediate operands, actual five-member setup facing both directions, exact stored release counters/functions, rising programs and preserved leader-only instruction/loop timer resets, plus invalid member bounds. No gameplay discovery. Integrated stream checkpoint: 67 converted, two mixed definitions, 157 required.

## Enemy drop probability-column dispatch

Replaced the six-entry return-kind array with named semantic column cases in coordinator-granted EnemyDropSelectionDefinitions. Native86:F25E-F263 maps small energy, big energy, missiles, no-drop, Super Missiles and Power Bombs in that probability-column order. Only the final selected identity changes representation; both minor/major accumulator loops, probabilities, resource eligibility, critical-health hysteresis and RNG behavior remain unchanged. Stream3 probability definitions were not edited.

Build1437 existing warnings/zero errors. New --lookup-stream2-drop-selection confirms allsix native return bytes and six actual cumulative selections using the native first probability record; invokes only existing selection-rule assertions for zero RNG reroll,30..49health hysteresis and full-resource eligibility, plus invalid column bounds. No unrelated pickup collision or gameplay cases ran. Integrated stream checkpoint: 68 converted, two mixed definitions, 156 required.

## Kraid lint initial policy dispatch

Removed the three-entry local delay array in FinishKraidGrowth. Coordinator-approved KraidLintInitializationDefinitions names the top/middle/bottom slot identities and expresses their distinct initial launch delays as semantic cases. This is per-part policy, not a mathematically derived timing sequence: pinned A7:AE2F-AE47 separately selects offsets0080/00C0/0100 and sources A916/A918/A91A containing288/160/64. Native EnableKraidLints installs alignment and LintProduce continuation; B923-B93E aligns and decrements each independent countdown until that continuation begins.

Build1437 existing warnings/zero errors. New --lookup-stream2-kraid-lint-initialization confirms allthree native timer values, actual FinishKraidGrowth slot/reset/continuation writes, and512 actual alignment/countdown steps including each exact transition tick, plus invalid slot bounds. No unrelated gameplay confirmation. Integrated stream checkpoint: 69 converted, two mixed definitions, 155 required.

## Kago frame catalog geometry

Frames() yields its three installed identities directly from the consecutive native four-part OAM record width: two-byte count plus four five-byte parts, beginning at A8:ABDA. Removed the factory literal array; no replacement stock array is generated or cached by this definition. Artwork contents remain independently accounted for.

Production compilation passed; the first verifier build exposed an incorrect new reference to catalog All, corrected to its actual Frames API. Final verifier build25 existing warnings/zero errors. --lookup-stream2-kago-frame-geometry passes allthree independent native instruction selectors and four-part headers, exact names, enumeration count and membership in the installed catalog. Integrated stream checkpoint: 70 converted, two mixed definitions, 154 required.

## Pipe Bug visual phase and OAM geometry

Removed all88 stored visual address/frame pairs across Brinstar, Norfair and yellow variants. Eight-record rising cycles open/close five poses; six-record normal flight cycles skip intermediate pose2. Strong Brinstar and yellow variants use four-record three-pose opening/closing cycles. Timed records occupy four bytes, terminal goto four bytes, and each native sprite composition contains a two-byte count plus one five-byte OAM part. Facing/role groups determine the native frame identity, including strong Brinstar's shooting-before-rising OAM order opposite its program order. Named source constants retain exact native identities. No lookup is regenerated/cached, and independent program timings/artwork are untouched.

Build1437 existing warnings/zero errors. New --lookup-stream2-pipe-bug-visual-geometry runs the existing Brinstar/Norfair/Yellow confirmations unchanged:120 native controls, all16 actual variant programs through complete loops, all88 native selectors and source-read guards. Additional scoped proof confirms all88 one-part native OAM headers and rejects adjacent mechanics/unaligned words. Integrated stream checkpoint: 73 converted, two mixed definitions, 151 required.

## Horizontal camera target modes

Coordinator-approved HorizontalCameraTargetDefinitions replaces both stored facing arrays with named normal/boss/left-edge/right-edge cases. Normal targets are32 pixels either side of the256-pixel viewport center; edge targets sit32 pixels from their respective edges. Boss mode retains its explicit64/80 facing policy. Native90:963F/9647 confirms all eight values; callers88:8347 restore normal, KraidA7:A9E4 and CrocomireA4:8ABA select boss, CrocomireA4:97F3 selects right-edge. No native assignment was found for index4; its name describes only its proven coordinate effect, not an invented historical purpose.

The existing native XOR/reversal rules, accepted context domain, integer-movement gate, speed arithmetic and scroll-boundary consumers are unchanged. Build1437 existing warnings/zero errors. --lookup-stream2-horizontal-camera-targets confirms all eight native offsets and32 actual combinations of mode/facing/normal-knockback-moonwalk-acceleration reversal, exact target positions, unchanged movement speed and invalid-context rejection. Integrated stream checkpoint: 75 converted, two mixed definitions, 149 required.

## Torizo named artwork-page dispatch

Coordinator-approved semantic catalog dispatch replaces the eight stored page references with mutually exclusive named artwork roles. The calculated indexed view preserves the manifest/hash order: shared death, statue crumble, left/right attack, Golden awakening, Golden left/right attack, Chozo debris. Source/extent/filename definitions remain on their dedicated named catalog members; separate pixel payloads remain REQUIRED and receive no exception.

Root integration build1444 existing warnings/zero errors. --lookup-stream2-torizo-page-dispatch checks all eight exact ordered definitions and enumeration/index agreement, decodes each native page into the actual installed atlas representation, confirms complete-page and last-byte resolution, rejects overruns/empty ranges and invalid indexes, and compares canonical content hash to the original ordered native bytes. Integrated stream checkpoint: 76 converted, two mixed definitions, 148 required.

### Bomb Torizo movement reflection integration
Posture X and walking motion now calculate opposite-facing signs from eight left-facing posture displacements and ten walking values. Native AA:C3EE-C41D/C440-C46F and C4BD-C4E4/C532-C559 confirm reflection and identical standing/sitting and normal/faceless sources. The eight base X values, eight Y values and ten walking values remain required. PostureX, PostureY and WalkVelocities stay unchecked; no choreography exception or completion-count change.

Root Verification build: 1445 warnings, zero errors. --lookup-stream2-torizo-movement passed all 88 native words, 32 real posture applications, 20 real walking consumers and invalid offsets with all six source tables forbidden.

### Reserve arrow pulse integration
The two 32-frame pulses now calculate mirrored RGB5 interpolation from four endpoint colors: phase=min(frame,31-frame), channel=floor((start*(15-phase)+end*phase)/15). Sparse deviations preserve independently supplied edits without caching whole ramps. Native color6 blue phases 5/10 are 5/11 instead of 6/12, red phase 14 is 19 instead of 18; color11 blue phases 5/10/14 are 3/7/10 instead of 4/8/11. These six deviations repeat in the mirrored half. Superseded by the complete reserve-arrow review below: four deviations calculate through the shared finite-precision quantizer, both other magnitudes calculate as prior-shade holds, and only the documented paint/hold choices receive narrow retention.

Root Verification build: 1445 warnings, zero errors. --lookup-stream2-reserve-arrow-colors passed all 64 native colors, 192 independent channel edits including endpoints, solid/animated CGRAM writes, frame wrapping and exactly 12 stock residual cells. ResourceAudit build: zero warnings/errors; source hash refreshed.

### Projectile trail appearance integration
Calculated consecutive tile identities, common priority/no-flip fields and family palette selection for two ice sides, wave and missile. The 42 stock attribute words are no longer stored; independently supplied edits remain exact sparse overrides. Native 90:B4CB/B52D/B58F/B5A1 confirms the original attributes. Ice pose boundaries at records 4/8/16 remain unresolved inputs; attributes stays unchecked without exception or completion-count change.

Root Verification build: 1445 warnings, zero errors. --lookup-stream2-trail-appearance passed 42 native attributes, 252 independent field edits, zero stock overrides, invalid frame rejection, 67 native mechanics words and all four actual paired-trail OAM/lifetime/freeze/movement cases with mutable alias checks. ResourceAudit build: zero warnings/errors; source hash refreshed.

### Chozo statue structural integration
Calculated 166 control positions and 52 visual addresses with shared acquire/rise/breathe/release sequences and Wrecked Ship carrying strides. Native AA:E39D-E428/E457-E57E establishes command widths, movement-index progression and loop labels. Independent acquisition/stride/per-scene holds, four footstep offsets and repeat counts 4/5/16 remain required; Words stays unchecked. Only PresentationWords is complete. Integrated checkpoint: 77 converted, two mixed, 147 required.

Root Verification build: 1445 warnings, zero errors. --lookup-stream2-chozo-layout passed 166 native controls, 52 independently decoded visual addresses and installed selectors, exact native list endpoints, bounds and allocation assertions.

### Norfair lava-jumper structural integration
Calculated 23 control positions and 14 visual addresses from hidden pose/sleep, seven-pose jump/callback/sleep and follower startup/counting loop. Native A2:BE3C-BE85 confirms the instruction widths and targets. Independent jump holds 1/5/9/7/3/10/1 remain required; Words stays unchecked without exemption. Integrated checkpoint: 78 converted, two mixed, 146 required.

Root Verification build: 1445 warnings, zero errors. --lookup-stream2-lava-jumper-layout passed 23 native controls, 14 independently decoded addresses/selectors, all three production programs, parent/follower initialization, rise/completion handshake, exact boundary, source-read guards and allocations.

### Norfair Rio structural integration
Calculated 65 control positions and 34 visual addresses from shared four-pose loops and six/eight-pose callback transitions. Native A2:C0F1-C1B6 establishes record widths and ordering. Idle holds 13/18, flight holds 6/5/8/6 and the per-pose follower-attachment callback sequence remain required. Named callback cases preserve the sequence but do not justify its independent offsets. Words stays unchecked without exemption. Integrated checkpoint: 79 converted, two mixed, 145 required.

Root Verification build: 1445 warnings, zero errors. --norfair-rio-instruction-program-definitions passed 65 native words, seven actual parent/flame programs, eleven follower/completion callbacks, 34 exact compiled selectors, initializer selections, source-read guards, boundaries and allocations.

## Integrated Torizo explosion layout

Calculated 53 control positions and 15 visual addresses from the low-health, large-death and smoke phase structures, including the packed sound byte. Native 86:A3CB-A455 confirms record widths, counted back-edges and deletion. Only PresentationWords is complete. Words remains required: small holds 2/2/3/3/2, large holds 4/6/5/5/5/6, smoke hold 8, random spread masks/biases and repeat counts 3/2 have no exception.

Root confirmation: Verification build passed with 1445 warnings and zero errors. --torizo-explosion-instruction-mechanics confirms 53 native words, the actual low-health producer's three cycles, both probabilistic death paths, exact jitter/lifetimes, 15 native compiled selectors, source-read guards, bounds and allocations. Integrated stream checkpoint: 80 converted, two mixed, 144 required. Overall: 504 converted, 15 justified retained/mixed, 609 pending.

## Integrated crawler speed ramps

CrawlerSpeedDefinitions now calculates quarter-pixel ramps in place of 32 stored magnitudes. Both pinned native copies A3:E5F0 and A3:CCA2 agree. Speeds remains unchecked: gaps at parameters 14/16, skipped-step totals 1/4, the penultimate eight-pixel magnitude and terminal zero remain unresolved independent choices. No exception is claimed.

Root confirmation: Verification build passed with 1445 warnings and zero errors. --lookup-stream2-crawler-ramps confirms both 32-word native copies, 384 actual crawler/Yard velocity resets and parameter-bound rejection. Stream counts remain 80 converted, two mixed, 144 required; overall counts remain 504 converted, 15 justified retained/mixed, 609 pending.

## Integrated Dead Torizo crop geometry

ColumnWordOffsets is complete: each 32-byte 4bpp tile advances 16 words. InitialGraphicsCopies and ColumnMinimumY remain unchecked. Their address arithmetic and clip thresholds derive from one shared crop, but the top/middle/lower silhouette boundaries and source placement still need a disposition; no artwork exception is granted.

Pinned A9:DE18-DEBF supplies twelve native MVN descriptors; E272/E38B supplies the ten column offsets and clipping limits. Root Verification build passed with 1445 warnings and zero errors. --lookup-stream2-dead-torizo-geometry confirms all descriptors and limits, actual installed-art staging, and 192 actual row copy/move operations against independent native operands, comparing all 4096 work-buffer bytes. Stream checkpoint: 81 converted, two mixed, 143 required. Overall: 505 converted, 15 justified retained/mixed, 608 pending.

### Root integration: Golden Torizo eye-beam layouts

Wall, floor and flight phases calculate pose/control positions, including the floor program's packed sound byte and damage-enable operation. Explosion waits form a unit-step progression. All17 presentation addresses are complete. Words remains required for wall4, landing8, flight1, explosion start4 and increment1, and the selected phase2 damage-enable placement. No retention exception is added.

Root build passed (1445 warnings, zero errors). `--golden-torizo-eye-beam-instruction-mechanics` passes28 native controls, both real producers, collision-to-impact selection, exact wall/floor lifetimes, disabled floor loop, frame-specific damage transition,17 exact native installed selectors with zero live reads, boundaries and allocation checks. The fixture's obsolete live-read expectation was corrected without weakening behavior assertions. Inventory:533 converted,15 justified retained/mixed,580 pending.

## Golden Torizo awakening layout (partial timing payload)

Composed the69 controls and21 visual addresses atAA:C9E2-CACD from fall, sit, seated-upload, stand and color-handoff operations. Native timed poses occupy4 bytes, word instructions/operands2, and DMA records9 including their separately owned7-byte descriptors. Sitting movement indexes traverse4/2/0; standing traverses0..10 in word steps. Sitting holds3/4/5 increment once per phase, upload holds32/16/8 halve. No mechanics or presentation-address roster is rebuilt/cached.

Independent hold scales and transitions1/3/48/32/4/32/12/8/4/16, upload repeat2 and color iterations16 remain REQUIRED; Words is unchecked. PresentationWords resolves. Build1437 existing warnings/zero errors; new guarded --lookup-stream2-golden-awakening-layout calls the existing bounded native confirmation unchanged: all69 unique control words and byte ownership,8 DMA operations,21 exact compiled visual selectors,7 selected physical frames and9 hitbox lists pass. This batch confirms the changed layout contract without gameplay discovery. Stream checkpoint86resolved:83converted,two mixed definitions,one justified coordinate-code window;140required.

### Pause equipment blank cells

Converted the complete nine-cell blank placeholder at82:C01A-C02B into destination clearing. Both ordinary uncollected labels and the beam slots discarded during Hyper mode now clear their actual width. Only nonzero explicitly edited cells are stored; serialization/validation and source-byte content hashes are unchanged. Independent label glyphs and placements remain required under the separate labels entry.

Focused `--lookup-stream2-equipment-blank` confirms all nine native zero words, zero stored stock cells, each of nine independent custom-cell edits, exact content hashes, and20 actual ordinary/Hyper inventory tilemap writes including all untouched surrounding bytes. Build1437existing warnings/zero errors; focused run passed. Stream2 now87resolved (84converted, two mixed conversion/justified-retention definitions, one justified-retained definition)/139required. No new exception.

### Pause equipment label composition and placement

Granted PauseEquipmentLabelDefinitions now catalogs the exact packed text fragments observed in B6:8000-BFFF artwork and82:BF32-C018 label sources. Semantic label cases compose contiguous CHARG/E, ICE, WAVE, SPAZER, PLASMA, VARIA/sharedSUIT, GRAVITY/sharedSUIT, MORPHIN/G-B/AL/L, BOMBS, SPRING BALL, SCREW ATTACK, HI-JUMP BOOTS, SPACE JUMP, SPEED BOOSTER and HYPER/empty-endcap fragments. This is meaningful text/atlas selection, not phase-index value recitation. Palette2, markerFF and padded text backgroundD4 are common; native Hyper uses its distinct12F endcap. The independently designed pixels are still interface artwork, not calculated or exempted by these label references. Static `lookup1165-equipment-glyphs.png` diagnostic shows the exact named fragments for review.

Stock destinations derive from category columns4/21, first rows16/9/19, successive item rows and the two-row suit/misc gap at82:C06C-C086. Runtime label objects calculate stock words/positions when used and store only explicit cell/placement edits; no stock byte arrays are rebuilt or cached. Unknown labels/cell indices throw. Native Plasma's nine-word copy continues into four Varia words exactly as before.

`--lookup-stream2-equipment-labels` confirms all115native glyph words and15placements, each106ordinary and five consumed Hyper cell edits through actual patch/inventory writes, disabled recoloring, one independently moved Charge label, exact Plasma/Varia overlap, source hashes and bounds. The four unused Hyper trailing words are also compared with native stock. `--lookup-stream2-equipment-blank` still passes after the shared provider change. Production build1437existing warnings/zero errors; final verifier build25/zero. Stream2 now88resolved (85converted, two mixed conversion/justified-retention definitions, one justified-retained definition)/138required. No new exception.

### Dead Torizo crop derived from immutable stock composition

Following independent coordinator source/artwork review, the former LeftColumn/EndColumn silhouette formula and hardcoded source-origin delta are removed. Granted Assets/DeadTorizoStationaryCompositionDefinitions provides the immutable native25-part composition: three large head/shoulder parts, five rows of four body parts, a small rear ankle and a large rear foot. InitialCopy now derives source bounds, row tile union, packed destination width/origin and byte length directly from those parts. ColumnMinimumY takes the earliest covered row in each packed source column. No replacement crop table or generated cache exists; source/display geometry remains separate.

The exact independently chosen composition inputs are NOT exempted or silently relocated outside the inventory. They remain REQUIRED under [Stream3 EnemySpritemapCatalog.frames](lookup-1165-stream-3.md), specifically frame `dead_torizo_stationary_a9_d6e2` at A9:D6E2-D760: head tile/origin109/(-8,-52)/three parts, body128/(-16,-36)/four-by-five parts, ankle197/(-24,20)/small, foot1A6/(-32,28)/large, and their native ordering. Stock display/frame ownership stays with Stream3; this Stream2 immutable subset is the functional source for physical corpse staging. No artwork-retention approval is claimed. The two duplicated crop/clip definitions can complete by deriving from that explicitly tracked composition source.

Focused `--lookup-stream2-dead-torizo-geometry` passes all25native part tile/size/X/Y values, exact97tile coverage vs12MVNs, all10native column displacements/clip limits, actual installed-art staging,192actual row copy/move operations and input bounds. It binds an independently edited one-part display with different origin/tile/size, confirms actual edited OAM, and still obtains the exact native staging buffer. Build1437existing warnings/zero errors. Stream2 now90resolved (87converted, two mixed conversion/justified-retention definitions, one justified-retained definition)/136required; underlying OAM artwork remains required in its original Stream3 entry.

### Reserve label composition and placement

Granted PauseReserveUiDefinitions catalogs semantic text runs at82:BF06-BF30: MODE119-11B with palette1/priority, shared MANUAL146-149 with palette7/priority, RESERVE TANK080-086 and AUTO156-159 with palette7/priority. Mode composes its prefix with the shared Manual suffix; the two long-label anchors share a column and consecutive rows at82:C068/C06A, while manual/auto replace the suffix atModeCell. The provider calculates stock words and destinations directly and stores only independently edited cells/anchors. No unexplained fallback or cached stock label array remains. Independent glyph pixel design and arrow palette endpoints/channel deviations remain REQUIRED separately; no artistic exemption follows from text composition.

`--lookup-stream2-reserve-labels` passes22native words/four destinations,22independent cell edits,56actual ordinary/attribute-preserving/moved label patches and invalid-name/short-destination/index bounds. This specifically preserves82:AB47's old attributes while replacing only the character during mode changes. Build1437existing warnings/zero errors. Stream2 now91resolved (88converted, two mixed conversion/justified-retention definitions, one justified-retained definition)/135required.

### Pause ankle/toe strips and exact remaining inputs (partial)

Added regular Varia ankle17B/18B, Hi-Jump collar179-origin rows, regular upper-foot19E/19F, regular toe1AD-1AF, and regular outer-foot1EE/1FE. Extended existing Power leg and Hi-Jump toe pieces through their remaining native cells, retaining their priority differences. There are266calculated glyph references,19left/17right residual words, and25named source-piece origins. All origins, chosen placements/extents, attribute choices and residual words remain REQUIRED under the same original frames entry; no exemption is asserted.

Exact residual coordinates below are zero-based `(column,row)=native word` within each8x17frame. They are the complete stored stock residual set confirmed by the focused check, not a subset:

- PowerSuit left: `(2,6)=2594, (1,9)=25C9, (3,13)=05D7, (2,14)=258F, (0,15)=258E`.
- PowerSuitHiJump left: `(2,6)=2594, (1,9)=25C9, (1,13)=0579, (2,13)=258C, (3,13)=05D7, (1,14)=0589, (3,15)=059B`.
- VariaSuit left: `(1,9)=25C9, (1,11)=05E9, (1,12)=05F9, (3,13)=05D7, (0,15)=258E`.
- VariaSuitHiJump left: `(1,9)=25C9, (3,15)=059B`.
- All four right sides: `(6,9)=0000, (7,15)=259D`, plus `(7,14)=F955` for PowerSuit and `E955` for the other three.
- Additional VariaSuit right: `(4,11)=45EB, (6,11)=65E9, (4,12)=45FB, (6,12)=65F9, (7,16)=25FF`.

Native82:B20C-B256 selects one complete frame solely from EquippedItems&0101, then copies17rows of8words. Managed PauseMenuState.WriteSamusWireframe uses PauseEquipmentRules.WireframeIndex followed by the same bounded patch operation. The display is static line artwork, including the asymmetric cannon and equipment connector strokes shown in `lookup1165-pause-wireframes.png`; there is no animation/angle/velocity phase controlling individual cells. This bounds the remaining artwork-role review but does not itself approve retention of every selected value.

`--lookup-stream2-wireframe-mirrors` passes266native glyph calculations, all544complete native words, exact19left/17right residual membership/values, four actual stock patches,20independent edits and bounds. Build1437existing warnings/zero errors. Stream2 remains91resolved/135required.

Root integration: combines25d70f98,66814a6a,17961551,159a07c0. Viewed the four source wireframes; root Verification build1445warnings/0errors and focused544word/266glyph/exact36residual/20edit checks pass; ResourceAudit0/0. All25piece origins, placements, attributes and36residuals remain required; no aggregate completion or retention. Master551converted/16retained-mixed/561pending unchanged.

### Integrated equipment template geometry (partial)

Panel borders, semantic label runs, SAMUS heading/ornaments, reserve arrow/gauge and shared Power Suit piece geometry now calculate the template instead of caching1024words. Root viewed the native template and reviewed the operations. All selected panel bounds, placements, styles, glyph origins, shared wireframe composition and23residual words remain REQUIRED under tilemap; no exemption or completion.

Root --lookup-stream2-equipment-base-geometry passes all1024native words, exact23residual membership/values,8independent edits,18actual rebinds preserving live ownership/arrow palette, mutable output independence and bounds. Verification1445warnings/0errors; ResourceAudit0/0. Audit pins both new label/wireframe dependencies. Worker97c52ce1c22e4fbadc22caf83de042182c50d8f5. Aggregate551converted/16retained-mixed/561pending unchanged.

### Pause backdrop frame, area lettering and control strips (partial)

Granted PauseBackdropDefinitions now calculates the rectangular border on rows5-24, repeated inset/outer fills, centered uppercase area names from the contiguous A-Z atlas, and the L/MAP, EXIT/START, SAMUS/R control glyph strips. Native82:8EDA-8F1A copies B6:E000's1024word page and its lower512words atB6:E400 into the mutable button state;82:9428's area pointer selects the twelve-word name patch at page cell170. The existing semantic Ceres area displays COLONY. Calculation shares these native geometric/text operations without caching complete pages. Actual VRAM writes calculate one word at a time; CreateButtonTilemap returns the mutable state buffer. Independent edits to any area or button cell retain their original separate ownership.

Both original areas/buttons entries remain REQUIRED. Selected frame/control placements, glyph origins, palette/priority styles and the following three distinct source differences remain explicit pending inputs: nativeB6:E150 at(column8,row5)=68BE reverses one header-bar tile;82:9681 gives Crateria's final A palette2 instead of the other letters'palette6, appearing at(column19,row5)=2830;B6:E6BA at(column29,row26)=2887 selects a separate lower R-button glyph rather than the regular packed-row continuation286D. The two common differences occur in all seven area images, the final-A difference only in Crateria, and the R glyph also occurs independently in the button template:15area residual cells and1button residual. No generation rationale or retention approval is asserted for these choices.

The decoded native image is `C:\Users\SERVIC~1\AppData\Local\Temp\lookup1165-pause-backdrop.png`: a rectangular map frame with centered area text and named lower controls. It establishes the source-art roles, not permission to exempt chosen values. `--lookup-stream2-backdrop-geometry` calls VerifyLookupStream2BackdropGeometry inside guarded Main. It passes7680direct native words, exact15area/1button residual membership and values,53actual full-page VRAM loads with untouched surrounding memory,ten independent area/control edits and invalid destination/area bounds with no partial write. Initial build1438warnings/zero errors; removed the newly introduced argument-guard warning using existing Ensure helper, final production+verifier incremental1222warnings/zero errors. Focused confirmation passes again. Stream2 remains91resolved/135required.

Root integrated5f90fd61fb9446c4d94eb0cf6a9cf99725be023c: viewed native backdrop, Verification1445warnings/0errors, focused7680native words/exact15area+1button residuals/53VRAM loads/10edits/bounds pass;ResourceAudit0/0. Aggregate551converted/16retained-mixed/561pending unchanged; both entries remain pending.

### Integrated environmental owner catalogs

Four Norfair heat/palette owners and two Old Tourian railings/panels owners use calculated definition/program/loop offsets and semantic CGRAM placements. Both Definitions entries complete; accent Durations and colors remain required. Root six native bindings,292mechanics words,16phase bytes,complete/repeating cycles,live colors,enumeration/bounds pass; Verification1445warnings/0errors. Worker67cbb0ed71ea4a75e452ab09799cb786123fcd9c.


## Integrated projectile sprite identity geometry

ProjectileSpriteDefinitions.NativePointers is converted (d30b18a514,6df4c64b0,5f7affb9f). All417 addresses calculate from named directional/variant groups, two-byte headers and five-byte OAM parts. Core/paired-lobe/spread geometry and triangular startup extents include unselected physical records; no literal residual address table or expanded cached pointer array remains. Span-based flare callers retain their separate required identities.

Selected startup stages1/3/6/7 axial and2/4/8/10 diagonal remain independently required under ProjectileFrameBindingCatalog.sprites. Chosen part counts,ordered footprints and artwork remain required under ProjectileSpriteCatalog.frames. These dependencies are explicit in the master JSON; no selection or artwork exception is claimed.

Root Verification build1445 warnings/zero errors; --lookup-stream2-projectile-identity-geometry passes the417 identity union from805 native selectors,48 selected/unselected physical startup headers through93:F5E2,417 extracted native-vs-installed OAM draws,independent edited composition/ownership,54 flare selectors and bounds. ResourceAudit source hashes refreshed. Inventory556 converted/19 retained-mixed/553 pending.


## Integrated partial Power beam composition calculation

54825296a calculates eight centered one-tile Power compositions at93:A24D-A27E using triangular three-glyph phases and native reflection phases. Stock matching uses immutable calculated SpriteComposition views; any supplied field difference preserves independently copied parts. The aggregate ProjectileSpriteCatalog.frames stays unchecked: glyph origin30,palette6,priority2,pixels and every other chosen OAM design remain required.

Root Verification build1445 warnings/zero errors; focused projectile identity/composition check confirms exactly eight calculated views,417 native identities/OAM draws,48 startup records,independent Power edit/ownership,54 flare selectors and bounds. ResourceAudit builds0warnings/0errors; current shared SpriteComposition source is included in the projectile/flare closure. No new retained exception or count change:556 converted/19 retained-mixed/553 pending.

## Integrated beam geometry and visual phase selection

Partial chain67ad12808,a705f29d4,792ff33e2,8b47c7ae8,5ba2cbc61,df144411a,d5f56dd57,ff94b789b:84 Power/Ice/Wave compositions now use calculated indexed parts, including centered glyphs, reflected quads and signed lobes.160 beam,missile,bomb,effect frame selections derive compass order and sequential/oscillating phase traversal. Independently supplied composition and selector edits take precedence.

Both aggregate entries remain pending. Required inputs include645 selectors,Wave axial distances8/13/15/16,diagonal ratio3/4,EC3E's part ordering,other chosen geometry,glyph/palette/priority/pixels and timing/radii in their own owners. The diagonal relation is exact arithmetic, not a claim of trigonometric provenance. Identity record geometry still depends on the separately tracked selected footprint sizes.

Root Verification build1445 warnings/zero errors. --lookup-stream2-projectile-identity-geometry passes417 native identities/OAM draws,48 physical startup records,84 calculated views,independent quadrant/Wave edits and ownership,54 flare selectors,bounds. --lookup-stream2-power-direction-bindings passes805 native operands,exact645 residual membership,160 actual projectile handler/edit paths and14 actual bomb handler paths with timing/flow/radii preserved. ResourceAudit build0/0. Inventory unchanged558 converted/20 retained-mixed/550 pending.
## Integrated missile/effect geometry and charged-Wave selection

Partial chain abd61b470,753ce6da5,c44ec7ea2,4f45daf9c,98d427139 adds twenty calculated compositions: four axial Super Missiles, eight diagonal Missiles/Super Missiles, four Bombs and four beam-explosion quads. Total calculated compositions:104. Charged Wave/IceWave travel-axis and phase traversal adds122 selections, including two invisible lead-ins; total282 calculated selectors and523 required residuals. Eight reversed final axial choices remain explicitly pending at8F8F/8F97,9097/909F,91CB/91D3,92D3/92DB.

Palette fields derive from actual CGRAM destinations208/224 and sixteen-color OBJ rows, cross-checked against82:E13E-E148 and90:ACDE-ACE8. Colors, glyphs, priorities, diagonal pivots, chosen corner/order policies and other geometry remain required; neither aggregate entry closes.

Root Verification build1446warnings/zero errors. Focused projectile identity/composition check passes417native identities/OAM draws,48startup records,104calculated views,independent tail/quadrant/Wave edits,54flare selectors and bounds. Frame-selection check passes805native operands,282actual handler/edit paths,14bomb handler paths and exact523residual membership. ResourceAudit build0warnings/0errors. Inventory unchanged558converted/21retained-mixed/549pending.
## Integrated Wave lobes and Spazer composition geometry

574b83b08,bf2311055,afa0d08af,542d3c899,93cb310c0 add65calculated compositions:16horizontal charged-Wave lobes,8Spazer seed poses,16diagonal spreads,19axial spreads and6horizontal charged strips. Shared Spazer coordinates were already integrated from0f569e5c3. Total calculated views169; selectors remain282calculated/523required.

Chosen lobe/corner/lane ordering,spacing4 and8/13/15/16,diagonal ratio3/4,origin(-14,0),strip length4,glyphs,priority,pixels and selected footprint lengths remain required. Initial-right D84E and final irregular diagonal spreads remain supplied and pending. Repeated lanes,adjacent cells and native reflections calculate without claiming these choices are resolved. Both aggregate entries remain pending; inventory558converted/21retained-mixed/549pending unchanged.

Root Verification1447warnings/zero errors. Focused identity/composition check passes417native identities and actual OAM draws,805selector union,48physical startup records,exact169calculated views,independent edits/ownership,54flare selectors and bounds. ResourceAudit0warnings/0errors; projectile and flare closures include the shared coordinate catalog.
## Integrated charged vertical Spazer and startup geometry

c348d68aa and e228a7c5d calculate22additional compositions: four later charged vertical column layouts,twelve axial startup strips and six ordinary diagonal startup pair layouts. Total calculated views191. Cell adjacency,centering and reflected near-edge placement calculate; shortening a diagonal from two pairs to one preserves its center through a half-cell shift.

Chosen edge-relative/traversal policies,distances,lengths,glyphs,priority,shared origin(-14,0),pixels and other independent inputs remain required. Initial charged vertical DA3A/DA50 and four charged diagonal startup layouts remain supplied/pending. No aggregate closure or new exemption; inventory unchanged558converted/21retained-mixed/549pending.

Root Verification1447warnings/zero errors. Focused identity/composition check passes417native identities and actual OAM draws,805selector union,48physical startup records,exact191calculated views,independent edits/ownership,54flare selectors and bounds. ResourceAudit0warnings/0errors; affected projectile/flare source hashes refreshed.
## Integrated complete projectile selector decomposition

ab9368cb1,901abfe5b,e2fc8e520,ba20587f7,07f4e9e7,3385ef53e,dee04f273 and cd3b99a38 calculate the remaining523selector operands. All805stock operands now calculate with zero stored stock selector entries. This does not complete ProjectileFrameBindingCatalog.sprites: independent growth stages,axis/variant choices,near-center pose policy,capped DownLeft sweep and final axial parity reversal remain required. Semantic directions,record strides and outward/return phases supply the implemented mapping; independent supplied edits remain overrides.

0c43a7eb7 and efa40ce90 add17composition views: eight Plasma startup cores,four charged diagonal Spazer startup/endcaps and five horizontal PlasmaWave Short strips. Total208calculated compositions. Selected glyphs,footprints,lengths,spacing,lobe order,priority,pixels and remaining irregular geometry remain required. Vertical Short asymmetric offsets were not assumed to reflect the horizontal layout.

Root Verification1450warnings/zero errors; --lookup-stream2-power-direction-bindings passes805native operands,805actual handler/edit paths,preserved timing/control/radius and14bomb paths. --lookup-stream2-projectile-identity-geometry passes417native OAM draws,805selector union,48startup headers,208views,independent edits,54flare bindings and bounds. ResourceAudit0warnings/zero errors with all shared source dependencies refreshed. No new exception or aggregate closure;560converted/21retained-mixed/547pending unchanged.


### Integrated Ninja program composition (7173d035b)

Twenty facing-paired action programs calculate308mechanics words and140visual operand addresses through command/pose widths. Right second claw throw retains its distinct sound/first-pose ordering. Root build1452warnings/0errors;308native mechanics,20production programs,claw/palette/sound/function callbacks,140selectors,strict ordering/bounds pass with source reads forbidden. Fixture now supplies exact installed gold-Pirate palette;blank graphics serve only the instruction fixture and imply no rendering validation. PresentationWords complete;Words timing/launch/sound/palette choices remain required. Master596/21/511.


## Complete ghost target-palette disposition packet (independent review requested)

Selected original entry: RoomEnemySystem.WreckedShipGhostPalette, now implemented by WreckedShipGhostAppearanceDefinitions.PaletteColor. This packet accounts for every original word/channel and every remaining source input. It proposes a narrowly mixed disposition, not an exemption for palettes generally.

### Exact source and consumers

Supported unheadered ROM SHA256:12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72. Palette_Coven occupies A8:99AC-99CB. All16 words also occur at Palette_Kago A8:AAFE-AB1D; slots0..8 also occur at Palette_YappingMaw A8:9F4F-9F60. These are one shared set of paint choices, not three independent exception requests.

A8:9B9B-9BA9 copies exactly32 bytes to the target palette after the white flash. A8:9E88-9F4E reads each target word and independently increments/decrements each RGB5 component by one until equal;9BAD-9BBB combines the returned change count with the phase timer. No consumer interprets the palette index as light intensity, angle, time, material parameters or a generating seed. The OBJ pixel index directly selects a categorical color. Managed RunWreckedShipGhostBrightening at RoomEnemySystem.WreckedShipGhost.cs347 copies PaletteColor to all16 state.TargetPalette slots; StepWreckedShipGhostPaletteTowardTarget at467-483 preserves every component and count. All target slots remain observable through TargetPalette and CGRAM. No unused slot is discarded.

### Complete word and channel accounting

|Slot|Native word|RGB5|Observed source-art role|Calculated channel dependence|
|---|---|---|---|---|
|0|3800|0,0,14|Transparent source index; copied/faded payload only|R=G=0; independent B14|
|1|57FF|31,31,21|Kago brightest bone/skull highlight; unused by ghost|G=R; independent B21|
|2|42F7|23,23,16|Ghost/Kago olive highlight|G=R; B=max(0,R-7)|
|3|0929|9,9,2|Ghost/Kago dark olive contour|G=R; B=max(0,R-7)|
|4|00A5|5,5,0|Ghost/Kago deepest olive shadow|G=R; B=max(0,R-7)|
|5|4F5A|26,26,19|Ghost/Kago bright olive highlight|G=R; B=max(0,R-7)|
|6|36B5|21,21,13|Ghost/Kago intermediate skull/body paint|G=R; independent B13|
|7|2610|16,16,9|Ghost/Kago intermediate skull/body paint|G=R; B=max(0,R-7)|
|8|1DCE|14,14,7|Ghost/Kago intermediate skull/body paint|G=R; B=max(0,R-7)|
|9|01DF|31,14,0|Kago orange cavity highlight|B=0; independent R/G|
|10|001F|31,0,0|Kago bright red cavity|G=B=0|
|11|0018|24,0,0|Kago middle red cavity|G=B=0|
|12|000A|10,0,0|Kago dark red cavity|G=B=0|
|13|06B9|25,21,1|Kago bug ochre/gold highlight|Independent R/G/B|
|14|00EA|10,7,0|Kago bug brown shade|G=R-3; B=0|
|15|0045|5,2,0|Kago bug deeper brown shade|G=R-3; B=0|

The final brown hue dependency removes two stored green levels7/2 in favor of one chosen separation3. Remaining23 scalar inputs are exactly: OliveLevels1..8=[31,23,9,5,26,21,16,14] (8); WarmRed9..15=[31,31,24,10,25,10,5] (7); orange/ochre WarmGreen9/13=[14,21] (2); BackdropBlue14,OliveBlueReduction7,BrightOliveBlue21,MiddleOliveBlue13,OchreBlue1,BrownGreenReduction3 (6). Branch membership itself records the selected color-family assignments and is part of the proposed bounded paint-content disposition. The nine ghost-visible inputs are seven levels2..8,tint7 and slot6blue13; fourteen inputs concern copied slots unused by the ghost. No further functional intensity axis exists in the direct pixel-label consumer; equal numbers in unrelated color families do not establish shared shading inputs.

### Source-art evidence and narrow nonsense rationale

Ghost B1:A600-A9FF contains32 tiles. Native A8:9E46/9E5C/9E72 comprises four16x16 parts, shared upper tiles100/102 and lower pairs104/106,108/10A,10C/10E. Only opaque indices2..8 appear. Frame counts:2=6/6/6,3=170/170/171,4=266/260/256,5=5/5/5,6=31/31/29,7=83/79/87,8=117/123/122. Native Kago B1:AE00-B5FF uses every opaque label1..15; raw tile counts0..15=[1344,38,63,426,918,45,156,237,312,28,75,110,208,36,63,37]. ABDA/ABF0/AC06 composes the olive bone/skull and red/orange cavities. Bug OAM8D:8458/845F/8466 uses tiles128/129/12A; A8:ABD3 calls SpawnEnemyProjectileY_ParameterA_XGraphics, whose86:8030..8036 ORs source palette and VRAM tile bits; managed RoomEnemySystem.KagoBugs.cs74 preserves that inherited selection, providing the gold/brown role for13..15.

Static images reviewed: C:/Users/SERVIC~1/AppData/Local/Temp/lookup1165-ghost-palette.png and lookup1165-kago-palette-tiles.png. These show painted categorical regions and distinguish ghost-unused warm slots from visible ghost colors. The remaining22 visible-source paint inputs and their family assignments specify the content of that artwork: replacing them with an interpolated palette, inferred lighting model or function of slot number would choose different highlight/shadow/hue colors because the consumer supplies only categorical pixel labels. A function encoding the exact label-to-selected-shade choices would merely restate those inputs. This is the requested nonsense boundary for those exact paint choices, after shared channel/tint calculations, not a claim based on size, performance, complexity or missing provenance.

Slot0B14 has a separate proposed disposition. Transparent OBJ pixels never select its visible RGB, so no visual color-generation rule determines14. The exact word nevertheless enters the native16-word copy and independent component fade, and changes the target/CGRAM/change-count state. Generating a replacement value from visible art would invent a payload absent from that art; deleting it would change the copied/faded source contract. Retain only this one chosen transparent payload level under the same bounded data-content rationale, separately from visible paint. No arbitrary unused-memory preservation is requested.

Requested decision: approve these22 categorical paint/tint inputs plus one transparent target-payload level, together with their exact family membership, as narrow nonsense content. Keep all shared channel calculations. The original entry is mixed converted/justified-retained only if this complete packet is approved; until then it remains unchecked. Confirmation covers all16 native/shared words, actual target installation,32 actual component-fade steps including completion counts, every source pixel label and domains. It does not search gameplay or claim player validation.

Packet confirmation: build1,437 existing warnings/zero errors; --lookup-stream2-ghost-palette passes all native/shared targets, actual target copy and32 actual fade steps, exact ghost slot membership and all16 Kago source-pixel counts. No direct ResourceAudit dependency found for WreckedShipGhostAppearanceDefinitions.cs. Pending independent decision; counts unchanged95/131.

Transparent-slot consumer anchor: SnesObjRenderer.cs246 skips colorIndex0 before accessing CGRAM. Native Kago initializer InitAI_EnemyProjectile_KagoBug86:D088 handles position/state; inherited palette selection belongs to the separate native spawner86:8030..8036 invoked atA8:ABD3, not that initializer.

### Ghost target palette complete: approved mixed disposition

Coordinator independently reviewed both diagnostic images, all23 specified inputs/native words, the full component-fade consumer and transparent-index renderer skip. Approved only RoomEnemySystem.WreckedShipGhostPalette as mixed converted/justified-retained:22 material paint/tint choices and exact family membership are categorical artwork content; slot0blue14 is separately retained as exact copied/faded target data with no visual generating rule. All shared-channel calculations remain. This approval does not cover Kago/Yapping palette containers, sprite geometry, or any separate ghost timing/state definitions.

Removed pending wording from the owned palette definition and recorded the bounded rationale beside each input group. Corrected two pre-existing mojibake plus/minus XML phrases in the owned ghost runtime to ASCII; no runtime behavior changed. Confirmation remains the just-passed1,437-warning/zero-error build and focused16-target/shared-source,actual copy,32-step fade/count,pixel-usage/domain checks. Stream2 now96 resolved /130 required; this is one completed mixed entry, not a fully converted palette or an expansion of retained scope.
Root integration: viewed both native ghost/Kago images,checked native32-byte target copy and component fade plus renderer transparency.1452warnings/0errors;16native/shared words,actual target installation,32component fade steps/counts,pixel-label usage/bounds pass. Runtime preexisting plus/minus XML preserved. Master596/23/509.

## Complete old-Tourian accent duration disposition packet (review requested)

Selected entry: OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Durations only. Native orange-railings program8D:FBC1-FC5E and yellow-panel programFC5F-FCFC have identical15record timing. DefinitionFFDD/FFE1 uses no-op initial functionC685; each program has one color-index setup, fifteen timed three-color records each endingDone, and an unconditionalGoto to its first record. There is no conditional event, random sample or palette-value-dependent branch in these two loops.

Each program repeats its same five three-color tuples three times. Orange tuples are35AD/1CE7/0C63,29D0/150A/0885,1E14/114D/08A7,0E37/096F/04A8,025A/0192/00CA. Yellow tuples are28C8/2484/1C61,398E/296B/1549,4A74/2E52/1230,5739/3318/0B18,67FF/43FF/03FF. These demonstrate neutral-to-tint sweeps; this timing packet does not request retention of their color content.

|Pass|Frames|Exact native holds|Neutral hold|Extra tick on tint phase|
|---|---|---|---|---|
|First|0..4|16,1,1,2,1|16|3|
|Second|5..9|2,1,1,1,1|2|None|
|Third|10..14|32,2,1,1,1|32|1|

All thirty duration operands (fifteen in each program) are accounted for by this table. The implementation directly selects the pass/tint phase from frame/5 and frame%5. It stores three authored pass descriptors (NeutralHold,ExtendedTintPhase) and returns the neutral hold, two native ticks for the selected extended tint, otherwise one native tick. It does not construct/cache a replacement fifteen-value lookup. The authored three-pass structure, holds16/2/32 and selected extra-tick locations3/none/1 remain explicitly pending until this complete disposition is approved; this is a decomposition of content, not proof those choices were mathematically generated.

Native consumer8D:C552 decrements the instruction timer each tick;C56A installs the next positive duration;C571-C591 writes colors and stores the next record pointer;C595 terminates that record; the program'sGoto restarts it. Managed RoomPaletteFxSystem.Step185-190,ExecuteProgram374-376 and WritePaletteRecord preserve this contract. Equal native RGB tuples are held16,2 and32ticks at the neutral phase, and the same tint tuples receive1 or2ticks on different passes. Thus RGB/color phase alone does not determine duration. No program input distinguishes these occurrences except their authored place in the flicker sequence. A function encoding pass-specific numeric holds and exceptions would restate this same chosen cadence; deriving different holds from brightness/interpolation would change its visible rhythm. This is a bounded authored flicker-content rationale, not a blanket timing exemption or an argument from sequence size/complexity.

Requested narrow mixed disposition: preserve this exact three-pass cadence (neutral holds and extra-tick placements) as nonsense-to-regenerate authored animation content, while calculating the common five-phase record structure and single/double-tick repeats. The 64tick period is the sum of these exact native holds. This does not exempt any separate color palette, other escape flash duration sequence, room event or gameplay timer.

Build:1,437 existing warnings, zero errors. New guarded --lookup-stream2-tourian-accent-cadence confirms all30 native durations,90 repeated color words,existing68mechanics words and130actual per-tick CGRAM states including both loop handoffs with mechanics ROM reads blocked. Original duration lower/upper IndexOutOfRange domains are preserved. No gameplay probing. Counts remain96resolved/130required until independent approval.

### Old-Tourian accent durations complete: approved mixed disposition

Coordinator independently read native FBC1-FCFC and C54A-C596 and reviewed the real-tick confirmation. Approved only the exact three authored flicker passes, neutral holds16/2/32 and extra-tick tint phases3/none/1 as animation choreography: the repeated identical palette states have independent temporal choices, so generation without those choices would invent a different rhythm. Common five-phase record structure and single/double-tick repeats remain calculated. No palette-color or other timer exemption follows.

Renamed RequiredCadence to AuthoredCadence and replaced pending code comments with the exact bounded source rationale. Prior passing build/focused confirmation applies to unchanged behavior; this final change is field naming and disposition documentation. Stream2 now97 resolved /129 required. Durations is mixed complete, not wholly derived.
Coordinator confirmation: integrated build1452 warnings/zero errors; focused30 native durations,90 repeated colors,68 controls and130 actual tick/CGRAM states passed. Master596 converted/24 retained-mixed/508 pending.

## Complete reserve-arrow color disposition
### Pixel roles and actual source consumer

Equipment template B6:E800 uses ten arrow cells:129,161,193,225,257,289,321,353,385,386. They select tile14C(top arrowhead),15C(seven stem cells),16C(corner),16F(right terminal) from the pause atlasB6:A000. Exactly640 source pixels comprise280 transparent0,198 fixed border4,74 bevel-color6 and88 bevel-color11. Native82:AE01-AE45 changes these cells to palette6; unrelated border pixels remain unchanged by the two sequences. The two animated slots color opposite bevel edges of the same arrow. Source rendering at phases0/5/14/15 shows gold/brown edges fading into light/dark greys; the start swaps the normal solid-edge colors as noted above.

Image: C:/Users/SERVIC~1/AppData/Local/Temp/lookup1165-reserve-arrow-native.png. Guarded --lookup-stream2-reserve-arrow-artwork exports only the selected native tilemap/character data, not gameplay. The198 fixed index4 pixels are shown in diagnostic grey; only slots6/11 use the native sequence colors. No claim about the unrelated border's native color is made.

## Reserve-arrow shared quantizer and complete revised disposition packet

The initial six-channel retention proposal was withdrawn and removed. Review identified a shared finite-precision fade calculation that explains four of those channel crossings; they are calculated, not paint exceptions. No historical authoring tool is asserted. This is a directly reproducible common ramp operation matching native channel data.

For each mirrored phase0..14, start a binary32 remaining fraction at1 and repeatedly subtract binary32(1/15), rounding to binary32 after every subtraction. Amount is binary32(1-remaining). Multiply the endpoint delta by amount with an explicit binary32 rounding before adding the start and rounding again, then truncate to the RGB5 channel. Phase15 emits the exact chosen endpoint. Code uses binary64 intermediate arithmetic followed by explicit binary32 conversions at each boundary; multiplication rounding cannot be elided by an FMA with the subsequent addition. All16 phases, both ramps and all channels use the same rule. It constructs no lookup or cache.

|Phase|Remaining binary32|Amount binary32|Bright blue before truncation|Dark blue before truncation|
|---|---|---|---|---|
|5|0.6666666865348816|0.3333333134651184|5.999999523162842|3.999999761581421|
|10|0.3333333730697632|0.6666666269302368|11.999999046325684|7.999999523162842|

Thus native bright5/11 and dark3/7 follow from the same quantizer, rather than four selected channel values. Plain forward per-channel float32/float64 accumulation fails to reproduce all four. Reverse normalized accumulation in either precision reproduces these interior crossings. We choose explicit binary32 operations for a deterministic contract; no claim about the unavailable source tool is needed.

Only two native deviations remain from that common ramp, both at the penultimate phase14: bright red19 repeats phase13 red19 (unheld value18), and dark blue10 repeats phase13 blue10 (unheld value11). Code derives these values by using the preceding phase for exactly those channels, so no selected channel magnitude remains stored. The held shade derives from the ramp's endpoints and quantizer. Independent supplied phase13 edits do not implicitly edit phase14: every frame is compared separately against the stock operation, preserving independently supplied colors.

Approved narrow retained choices: the existing shared solid colors039E=(30,28,0) and0156=(22,10,0), neutral endpoint levels18 and12, and the exact penultimate bright-red/dark-blue hold policy. The original entry is mixed complete after independent coordinator review. All blue crossing values, every other intermediate channel, the held magnitudes, repeated neutral channels, reversed solid-role starts and mirrored second half now calculate. There is no residual color table and stock has zero unexplained overrides.

The source/artwork/consumer evidence above remains applicable: native82:AD29-AD4A samples NMI&31 and uniformly copies the two colors; the ten source arrow cells use74 bright-bevel and88 dark-bevel pixels. The start/end colors specify the categorical gold/brown-to-grey material appearance. The two held channels form a chosen one-frame shade dwell just before the neutral endpoint, not a lighting measurement or state-driven channel change. Native frame14 bright word4253 has red19, matching preceding3E73 red19, while dark frame14 word296C has blue10, matching preceding296D blue10; frame15 endpoints4A52/318C follow. These exact two shade-dwell assignments define source animation content. Independent coordinator review approved this bounded source-content rationale; failure of alternate arithmetic alone is not the exception argument.

Confirmation after implementation: build1,437 warnings/zero errors; --lookup-stream2-reserve-arrow-colors passes64 exact native words through actual CGRAM writes,192 independent frame-channel edits,6 independent solid-color edits,wrapping/solid-mode and zero stock overrides. No new gameplay or discovery test was added. PauseReserveUiPresentation.cs is the only changed production file; MenuClosedPresentationContractDefinitions owns its audit hash. Counts are98 resolved/128 required after final independent review. No other arrow/glyph/geometry entry inherits this disposition.
### Final independent approval

Coordinator reviewed the explicit-rounding generator, native arrow strip and uniform82:AD31-AD43 consumer. Approved only shared gold/brown source paint039E/0156, neutral levels18/12 and the exact penultimate bright-red/dark-blue shade hold as categorical bevel-color and pulse content. All four integral-crossing deviations calculate; both held magnitudes calculate from the prior shade. This is mixed conversion/justified retention, not a completely mathematical palette. Removed obsolete pending and six-value retention claims. No additional behavior changed after the passing focused confirmation above.

Root build1452warnings/zero errors;ResourceAudit0/0;64native CGRAM colors,192frame edits,6solid edits and zero stock overrides pass. Master598converted/27retained-mixed/503pending.

## Chozo stride magnitudes: bounded source decomposition in progress

Selected original entry: ChozoCarryMotionDefinitions.Magnitudes. It is now mixed complete after final root confirmation below. Native AA:E630-E66F contains32 signed8.8 words: stationary acquisition/release phases, two repetitions of whole-pixel stride2/3/14/8, then the opposite-sign half. The actual instruction AA:E5D8-E62F executes movement once when each pose is selected: X receives the signed displacement, downward collision receives its absolute magnitude, slopes align afterward, and Samus receives the resulting statue position plus the same record's hand offsets. Thus these are pose-transition displacements, not continuously applied frame velocities. Managed ProcessChozoStatueMovement preserves that contract.

Wrecked Ship AA:E4C1-E527 selects B,4,5,6,7,8,9,A cyclically, with motion indices16/08/0A/0C/0E/10/12/14 hex; the loop repeats sixteen times, then E529-E553 adds B,4,5,6. Timed pose holds are8/11/8/6, but do not multiply the displacement. Lower Norfair uses the reflected acquisition poses only; all32 public records must still preserve exact values and invalid-domain behavior.

The same visible planted-foot tiles170/171 occur in poses4/5/6/7 at foot-left X=-31/-28/-14/-7, Y=27/28/30/29. Native composition starts are AA:E943/E9AE/EA1E/EA8E, keys chozo_statue_aa_e943/e9ae/ea1e/ea8e. Applying cumulative body displacement to those X anchors gives -33/-33/-33/-34 relative to the pre-stride body origin. Hence the pose5 and pose6 displacements3 and14 exactly preserve the planted foot's horizontal location; phase7 has an explicit extra one-pixel left shift beyond the foot-anchor delta7. Phase4 displacement2 occurs across the transfer between differently drawn feet and is not implied by those two adjacent planted-foot equalities. Poses8/9/A/B swap the legs' palette/foreground roles while reusing these positions and movement magnitudes.

No blanket animation exception is proposed. The source supports deriving3/14 from canonical stock pose geometry, but the current editable EnemySpritemapCatalog.TryGet/TryGetDisplay arrays must not become gameplay inputs. Stream3 confirms there is no immutable Chozo geometry owner yet; its ChozoStatueVisualDefinitions registers identities only. Coordinator rejected adding duplicate literal anchors. Current main already supplies the immutable EnemySpritemapParts calculated view (4aebb0a5e/f98a6eaa); the earlier isolated-tree API assessment was stale. Coordinator granted a new shared stock-footprint owner plus a narrow loader hook. Its independent shape inputs remain under Stream3's existing EnemySpritemapCatalog.frames obligation. The transfer2 and extra-pixel plant adjustment received the bounded approval documented below; root confirmation passed as recorded below.
### Root-based Chozo implementation candidate

Prepared csharp/test-temp/chozo-stride-candidate against current main, per coordinator instruction, because this isolated branch predates the indexed enemy-part API. ChozoStrideGeometryDefinitions.cs owns the four stock left-foot origins and calculates the adjacent tile171 origin by adding one8-pixel tile. Its FootParts view removes both native X fields from each of the eight matched pose records, retaining all other source flags/Y/attributes/order; independent supplied X differences keep their own unmodified view. The same immutable owner supplies carry distances3/14 and the seven-pixel final foot-origin difference. No editable display catalog becomes a gameplay input. These four geometry inputs remain required in Stream3.frames and are not a new orphaned residual.

EnemySpritemapCatalog.cs candidate adds only the Chozo Compile hook immediately after existing BabyMetroidSpriteParts.Compile; Stream3 confirmed no conflicting loader work. ChozoCarryMotionDefinitions.cs calculates repeated/signed/stationary mapping, including transfer advance2 and final anchor-delta7 plus chosen slide1. Coordinator independently approved only those two movement-policy values as selected pose-transition trajectory. They specify how far the actor advances during the change of supporting foot and the one-pixel shift of the planted foot before the next transfer. Generating a different shift would change the authored pose-motion trajectory; no physics equation, native acceleration state or duration integration produces them. This rationale is offered for exact bounded review, not failure-to-fit exemption.

Candidate verifier method VerifyLookupStream2ChozoFootGeometry appends to the current main Program.LookupStream2.cs and replaces only the existing --lookup-stream2-chozo-strides call. It confirms all174 native parts in eight actual installed views,16 independent supplied foot-X edits,complete content hash/order and existing32-row native movement/actual carry assertions. It also confirms edited display anchors do not alter gameplay3/14. The method invokes VerifyCompiledStatueWalking, preserving the preexisting Torizo and Chozo movement assertions. The root owns candidate application/build; no passing result is claimed yet. No broad API migration or unowned production mutation occurred. All candidates are stable for coordinator review.
### Chozo final source disposition approval and integration dependencies

Coordinator independently reviewed the complete movement/source packet and approved only transfer advance2 and final planted-foot slide1 as the selected pose-transition trajectory. They are not continuous velocities or a general exemption for physical tuning. The planted3/14 and final anchor-delta7 derive from shared immutable stock foot geometry; all four origins -31/-28/-14/-7 remain explicitly REQUIRED under Stream3 EnemySpritemapCatalog.frames. Native AA:E959/E9C4/EA34/EAA4 directly confirm foot170 X words C3E1/C3E4/C3F2/C3F9 with attribute2370; adjacent foot171 is one8-pixel tile to the right. Helper geometry/view and carry have one canonical source of those X inputs, and preserve all editable display differences independently. No wider artwork, timing or motion exemption is granted.

Candidate snapshots refreshed from current main after approval. Dependency order: existing Hardware/EnemySpritemapParts.cs plus existing BabyMetroidSpriteParts.cs (4aebb0a5e/f98a6eaa); new Assets/ChozoStrideGeometryDefinitions.cs; one-line EnemySpritemapCatalog loader hook; owned Game/ChozoCarryMotionDefinitions.cs; append VerifyLookupStream2ChozoFootGeometry to Program.LookupStream2.cs and invoke it under existing --lookup-stream2-chozo-strides. Root build/integration passed as recorded below.

ResourceAudit exact dependencies: EnemyDisplayArtworkClosedContractDefinitions.cs references EnemySpritemapCatalog.cs in both the plain enemy artwork and extended enemy artwork closed records (lines9/20 in reviewed root). Refresh both catalog source hashes and add ChozoStrideGeometryDefinitions.cs as a transitive provider dependency alongside existing EnemySpritemapParts/BabyMetroidSpriteParts. EnemyDisplayArtworkContractChecks.cs line83 also names the provider for static contract analysis, without an embedded hash. No current ResourceAudit source-hash reference to ChozoCarryMotionDefinitions.cs was found. Existing parts/Baby source hashes do not change. No ledger files edited by this worker.
### Chozo completed root confirmation

Coordinator applied the candidate to current main and added a dedicated early --lookup-stream2-chozo-strides branch (the earlier branch existed only in this isolated tree). Root build passed with1,452 existing warnings/zero errors; ResourceAudit reported0/0. Focused confirmation passed174 native parts,eight calculated views,16 independent foot-X edits,canonical hash/drawing order,39 preserved Torizo byte windows,96 Chozo native words and52 actual movement/carry calls. Downward collision promotion and carried-Samus state effects remain covered by the existing real production calls.

Magnitudes is mixed complete, retaining ONLY foot-transfer advance2 and final planted-foot slide1 as authored pose-transition trajectory. The four immutable foot origins -31/-28/-14/-7 remain REQUIRED under Stream3 EnemySpritemapCatalog.frames, identified by the eight chozo_statue_aa_e943/e9ae/ea1e/ea8e/eafe/eb69/ebd9/ec49 compositions. They were centralized, not exempted. All other shape/style/order/glyph inputs remain within that existing obligation. Stream2 now99 resolved/127 required. No user playtesting or whole-game validation is claimed.
Root complete disposition integrated;master598converted/31retained-mixed/499pending.

## Golden Torizo walking: complete planted-foot derivation proposal

Next bounded original entry is GoldenTorizoWalkDefinitions.Velocities. No retention is proposed for its20 movement words. Native AA:D54D-D599 performs one whole-pixel movement when the instruction selects a pose, preserving fractional position, collision/turn handling and slope alignment. The ten left walking extended frames AA:A4FA+34*phase have four8-byte components; component2 has zero offset and owns the body/feet. Right-facing words are the opposite sign. Complete odd byte-index windows remain part of the public domain0..38 and will compose adjacent calculated little-endian words.

|Phase|Extended frame|Body OAM|Floor tile160 anchors by palette|Left displacement|
|---|---|---|---|---|
|0|A4FA|9672|palette2 X10 at967E; palette1 X-37 at968D|-5|
|1|A51C|8FD4|palette1 X-37 at8FE0|0|
|2|A53E|9044|palette1 X-32 at905F|-5|
|3|A560|90AF|palette1 X-13 at90CA|-19|
|4|A582|911A|palette1 X3 at9135|-16|
|5|A5A4|96EC|palette1 X10 at96F8; palette2 X-37 at9720|-7|
|6|A5C6|918A|palette2 X-37 at91B4|0|
|7|A5E8|91FA|palette2 X-30 at9233|-7|
|8|A60A|9265|palette2 X-13 at929E|-17|
|9|A62C|92D0|palette2 X5 at930E|-18|

All addresses above are bankAA and directly confirmed against the supported ROM. Floor strip tiles160/161/162 share Y40, with X adjacency8. Palette1/2 identifies the two legs; phases0/5 plant both feet. In the ordinary transitions, the movement cancels the change in the existing planted foot's local X. At phase0 the outgoing palette2 foot moves from5 to10, yielding displacement-5; at phase5 outgoing palette1 moves3 to10, yielding-7. Thus the transfer magnitudes also derive, with no selected advance or slide residual as in the separate Chozo entry.

For example, starting before phase0 at bodyX0 gives planted palette1 world X=-42 through phases0/1/2/3/4 and the outgoing foot in5: cumulative bodyX -5/-5/-10/-29/-45/-52 plus footX -37/-37/-32/-13/3/10. The new palette2 plant stays at worldX-89 through5/6/7/8/9/next0: bodyX -52/-52/-59/-76/-94/-99 plus footX -37/-37/-30/-13/5/10. This directly accounts for every magnitude rather than treating numerical differences as authored velocities.

Proposed shared immutable foot owner centralizes seven existing geometry choices -37/-32/-13/3/-30/5/10 and calculates three/six foot-X fields in matching body compositions. Those seven chosen origins remain REQUIRED under Stream3 EnemyExtendedFrameCatalog.frames; they are not exempted or duplicated as a new untracked movement table. Root currently exposes extended component Parts as ReadOnlyMemory even though plain EnemySpritemapParts supports calculated views. Requested narrow binding adaptation: EnemyExtendedFrameCatalog component/loader; RoomEnemySystem draw call; Program.BossDisplayBindings.Stock/Oracle; Program.EnemyExtendedLegacyOverrides; CrocomireSkeletonResourceChecks. Stream3 confirms no conflicting loader work and will retain the seven geometry inputs in its checklist. No production edits before coordinator grant.
### Golden Torizo root-based implementation candidate

Coordinator granted the new shared helper and exact narrow binding paths. Stable candidates are under csharp/test-temp/torizo-stride-candidate, built from current main files rather than the older isolated branch. GoldenTorizoStrideGeometryDefinitions owns seven immutable source origins and matches the third component of only the ten left-walking frames. Each matched body's three/six floor-tile X fields calculate from the shared anchor and8-pixel adjacency; its other parts and every foot flag/Y/attribute/order input remain preserved. Independent X/tile/palette mismatches retain the supplied view. The geometry basis is now shared with GoldenTorizoWalkDefinitions rather than duplicated as20 motion words. All seven origins remain REQUIRED under Stream3 EnemyExtendedFrameCatalog.frames; no artwork retention approval is claimed.

Extended component Parts binds to the already existing immutable EnemySpritemapParts view. Exact binding-only consumers are RoomEnemySystem's extended OAM call, Program.BossDisplayBindings.Stock/Oracle, Program.EnemyExtendedLegacyOverrides and CrocomireSkeletonResourceChecks. The native oracle merely wraps its independently decoded owned array; no expectation/behavior assertion was removed. No projectile catalog migration occurs.

Append VerifyLookupStream2GoldenTorizoFootGeometry to current root Program.LookupStream2.cs and invoke it under an early --lookup-stream2-golden-torizo-strides branch. It confirms all20 derived native words and39 little-endian windows,220 body parts/40 actual native components,ten calculated views,36 independently edited foot-X fields,display remapping,canonical hash and exact packed OAM,then invokes the existing actual movement/carry proof. This is identified-conversion confirmation only. Root application/build is pending; the original velocity entry remains unchecked until it passes.

Audit dependencies: EnemyDisplayArtworkClosedContractDefinitions and NativeDisplaySelectorClosedContractDefinitions contain EnemyExtendedFrameCatalog source hashes. Refresh both catalog hashes; the artwork provider gains GoldenTorizoStrideGeometryDefinitions as a transitive dependency. The selector contract remains GetDisplayPointer mapping availability. RoomEnemySystem is statically analyzed by CompiledEnemyDisplayAudit without an embedded file hash for this binding-only call change. No existing GoldenTorizoWalkDefinitions hash dependency was found. Candidate LF hashes: catalog D658FAC93CBEA1A4697CEB0D0C5C02E935272372673A2C2290DEE18B3679B25B; helper B82370AED8024E53A741A8C09D0E7E700C0FB4FBB0B1061CC2EAAEBA9CB5BA36; walk D00A22CE772403BCE6E79A5BD5A2609DC4B2E8E4B2D5F3B112C40C7418839075. Coordinator owns ledger updates and publication.
### Golden Torizo complete root confirmation

Root independently reviewed native AA:D54D-D5C1,body component bindings and shared geometry calculation. Verification1452 warnings/0 errors;Audit0/0. All20 derived native words,39byte windows,220body parts/40components,10calculated views,36foot-X edits,canonical hash/display remapping/packed OAM and existing52actual movement/carry calls pass. Velocities is converted; master599converted/34retained-mixed/495pending. Seven origins remain required artwork content, with no movement-table residual.


## Pause wireframe complete composition-source packet (approved, implemented)

Selected next original entry is PauseWireframePresentation.frames. Coordinator independently reviewed and approved the bounded composition disposition; implementation and confirmation follow below. The four source diagrams are the Power/Power-HiJump/Varia/Varia-HiJump patches82:D521/D631/D741/D851. Native82:B20C-B256 masks EquippedItems with0101, selects one complete figure, and copies seventeen rows of eight words to equipment BG1 at byte1D8 with64-byte row stride. It does not evaluate individual limb angles, animation phases, lighting or per-cell equipment physics. Managed PauseEquipmentRules.WireframeIndex and PauseMenuState.WriteSamusWireframe preserve exactly that selection/patch contract.

### Complete remaining field accounting

The25 existing atlas origins are named concrete image pieces, not unexplained velocity/color sequences: helmet1B3; Power/Varia shoulder1BC/1C0; arm170; Power/Varia torso1A4/182; Varia chest1E0; Power/Varia hip1C4/1CA; Power/Varia leg1B6/1E9; HiJump foot198; Power chest left/right1EC/17C; lower chest left1F0; Power/Varia lower chest1FC/1F2; cannon inner/outer178/186; HiJump outer foot19C; regular Varia ankle17B; HiJump collar179; regular foot upper19E; regular toe1AD; regular outer foot1EE. Each chosen source identity and each source piece's selected placement/extent remains an explicit content input in PauseWireframeDefinitions.TryStockTile. Within-piece16-character atlas-row progressions,8-column destination geometry, bilateral reflection and wholly empty background already calculate. The chosen memberships in that bounded method define the diagram's helmet/chest/arms/hips/legs/boots; they are not a generated anatomical model.

Further functional sharing identified before requesting disposition: (1) Power torso tile194 at(2,6) is the prior atlas row of the existing1A4 torso strip. Direct B6:A000 pixel decode shows the green abdominal contour continuing through194/1A4/1B4. (2) Tile19D at(7,15) in all variants is the second character of the existing19C outer-foot/diagonal-connector strip. Its white indexE crosses rows0..2 at columns2..0, continuing into19C rows3..6 at columns7..2; bottom pixels also continue the green outline. (3) Varia regular tile1FF at(7,16) is the adjacent lower-right character of the existing1EE/1FE outer-foot strip. (4) Varia regular inner-knee tile1D7 at(3,13) reuses the Power leg-tip glyph instead of introducing a second selected glyph. These relationships must calculate in the final implementation, not be retained as mismatches.

After those derivations, only five additional isolated glyph identities remain: hand-tip1C9 at(1,9) in all variants; regular ankle18F at Power(2,14); regular toe18E at(0,15) in both regular-boot variants; HiJump inner collar18C at Power-HiJump(2,13); diagonal connector155 at(7,14) in all variants. The cannon-side cell(6,9) is deliberately empty rather than a reflection of the opposite hand tip. These identities/placements describe actual selected hand/boot/connector artwork. Their outlines and endpoints are visible in C:/Users/SERVIC~1/AppData/Local/Temp/lookup1165-pause-wireframes.png, which root previously reviewed. This packet does not excuse other pause artwork or the independent glyph pixel payload.

The thirteen priority differences are separate from glyph selection and must not retain a full word containing a calculable glyph:
- Power: clear priority at(3,13).
- Power-HiJump: clear at(1,13),(3,13),(1,14),(3,15).
- Varia: clear at(1,11),(1,12),(3,13),(4,11),(4,12), and set at(6,11),(6,12) where mirrored left data had it clear.
- Varia-HiJump: clear at(3,15).

All other common figure attributes use palette1 with priority; mirrored pieces toggle horizontal flip while keeping other attributes. The diagonal connector has both flips and priority, with palette6 in Power and Varia/Hi-Jump and palette2 in Power/Hi-Jump and Varia. This last palette identity needs its own bounded data-role disposition: direct source tile155 uses only transparent0,black4 andwhiteD. Native B6:F000 palette2 and6 both map4 to0000 andD to7FFF, so the stock connector pixels are identical; the copied BG1 words F955/E955 still retain different palette identities and can differ under independently edited palettes. It is not justified as visibly different stock paint.

### Narrow complete disposition proposal

Retain only the named30 source-piece/glyph identities, chosen composition placements/footprints and variant piece selections, common/exceptional layering policy including the exact13 priority memberships, the one cannon-side blank membership, and connector palette identity6/2. These specify a hand-drawn categorical equipment diagram and its layering metadata. Their arrangement selects particular source image contours for helmet/shoulders/torso/limbs/cannon/boots; producing a different silhouette or guide-line arrangement would invent a different diagram, while encoding the same part placements as an unconstrained function would restate that content. The generic copy consumer does not turn these coordinates into a physical body model. The connector's visually redundant stock palette identity is separately retained as bounded copied composition metadata that preserves independent palette edits, not as invisible art paint. No exception is sought for the derived row strides, within-piece glyph addresses, bilateral symmetry, common pieces, runtime inventory rule or any other table.

Proposed implementation makes the full stock word operation explicit using the already named pieces plus the five isolated glyph identities, including the calculated strip additions and shared knee. It calculates attributes separately from glyphs and records only independent supplied word differences; no stock full-frame cache or unexplained default remains. Existing four-patch/all544-word and independent-edit confirmations will verify that exact changed contract once implemented. The original entry is now mixed complete following coordinator review and focused confirmation.
### Pause wireframe completion and confirmation

The approved thirty-piece composition now produces every stock word through StockWord. The preceding torso row, shared inner knee, adjacent outer-foot glyphs, atlas strips and mirrored cells calculate. The thirteen priority memberships, cannon-side blank and connector palette policy express only the approved specific diagram content. The runtime retains no stock residual word or generated frame cache; its dictionary contains independent supplied overrides only. The existing strip-only API remains unchanged for the separately pending equipment-base entry.

The first full native comparison caught an incorrect membership in this packet: Varia/Hi-Jump also uses connector palette six. Direct ROM D60F/D71F/D82F/D93F gives F955/E955/E955/F955, now represented exactly and corrected above. Stock pixels remain equal for the connector's used indices in palettes two/six; the distinction is copied metadata observable with independent palette edits, not different visible paint.

Verification build passed1437 warnings/zero errors initially and1222/zero on incremental rebuild. --lookup-stream2-wireframe-mirrors passed544 exact calculated native words,266 preserved strip references,zero stock overrides,four actual full patches,28 independent glyph/flip/palette/priority edits and untouched-page/bounds checks. No gameplay search was performed. One additional original entry is mixed complete:100resolved/126required in this isolated report, excluding the Golden Torizo candidate still awaiting root confirmation. Glyph pixels and other pause-art entries remain independently accounted.
### Golden Torizo integrated completion

Coordinator published5466ba35a after the complete root confirmation packet passed. GoldenTorizoWalkDefinitions.Velocities is complete, bringing this report to101resolved/125required after the wireframe completion. The seven canonical foot origins remain required under Stream3 artwork; this completion does not exempt them.

## Equipment-base complete source packet (approved, complete)

Selected original entry PauseEquipmentBasePresentation.tilemap is the static1024word B6:E800 page. Native82:8F2C-8F3A DMA-copies exactly0800bytes into7E:3800. Subsequent equipment labels, reserve labels/digits, live arrow palettes and the selected wireframe patch own their established footprints. Managed PauseMenuState constructor/reset uses CreateTilemap; MapPresentation refresh uses RebindBeforeInventoryRefreshInto/RebindBaseInto to preserve those live owners. Native82:AE01-AE89 changes only arrow palette bits. This is a static composed interface, not a generated physical scene.

Source-art image C:/Users/SERVIC~1/AppData/Local/Temp/lookup1165-equipment-base.png shows five labeled border panels, the SAMUS header, a reserve arrow/gauge, and white guide lines connecting suit/misc/boots/beam panels to the central body. The complete136-cell body footprint equals the already reviewed Power figure, so it now calls that immutable StockWord operation rather than repeating any of its artwork choices.

Inventory panel bounds derive existing semantic label placement/width: border one cell outside first/last label rows and left/right label columns. Beam(3,15)-(9,21), Suit(20,8)-(30,11), Misc(20,12)-(30,17), Boots(20,18)-(30,22) no longer exist as independent rectangle inputs. Their title start is centered using(Left+Right-TitleWidth+1)/2. Border sides/corners/horizontals/title breaks calculate by edge position/reflection; chosen source glyphs140/141/142/143 remain concrete panel artwork. The supplied image shows their purple/grey rounded outline; their pixel patterns remain separately required artwork.

Complete remaining composition choices requested for narrow review: Supply rectangle(2,9)-(11,13) with four-glyph centered title107..10A,palette3,low priority; inventory title runs BEAM F9..FB,palette6; SUIT F6..F8,palette2; MISC1B0..1B2,palette2; BOOTS A0..A2,palette3,all priority. SAMUS uses13A..13F centered across columns13..18 on row5,with BE bars at9/10/21/22,BD reflected endcaps11/20 and solid-background1 at12/19. Header row4 columns12..30 uses transparent glyph0,palette2/priority. These choose the visible title/ornament arrangement and style, rather than further derivable movement or lighting data.

Reserve composition: column1 arrow fromrow4 through12,tip14C,stem15C,turn16C,join16F at(2,12); four repeated empty-gaugeFC cells x3..6 and endFE atx7,row12; palette7. Arrow tip/join/end use priority,stem/turn/gauge normally low priority,with first stem(row5) high priority. This last priority is separately copied layering metadata; the native glow consumer preserves it while altering only palette. Supply interior uses solid glyph1/palette2, except live reserve cells overlaid later. Selected gauge length, arrow/header placement and source glyph identities are explicit composition content.

All eight remaining guide cells decompose into a diagonal atlas strip145/155 (155=145+16) and border attachment154. Decoded145 has white indexD at(row5,x7),(6,6),(7,5);155 continues D at(0,4),(1,3),(2,2),(3,1),(4,0). Beam's two shifted strip copies occupy x=panel.Right+2 at y=panel.Top..Top+1 and x=Right+1 at y=Top+1..Top+2; the latter uses palette6,former2. Border attachment154 at(Right,Top+2) usespalette6. Suit/Misc use reflected154 at(Left,Top+2); Boots uses both flips at(Left,Bottom-1). These produce exact native guide endpoints, with independent placement/flip/palette policy as specific diagram content. Glyph154 itself is the panel edge plus a white attachment, not a calculated number merely because adjacent to155.

Six transparent padding words0800 atx31/y4 and14..18 are separately bounded copied metadata. Source glyph0 is all transparent pixels; these words preserve palette2 with priorityclear despite identical stock appearance to empty0. Their exact membership/metadata can affect independently edited glyphs/palettes and therefore cannot silently be dropped; no visible-paint rationale is claimed. Similarly the header's transparent priority words and first-stem priority are accounted as copied composition attributes.

No broad artwork exemption is requested. The specific visible panel/header/arrow/gauge/guide layout, selected glyph identities/style, and bounded copied metadata above are the entire requested content disposition. Generating a different placement or text ornament would invent a different interface; atlas runs, border geometry, label-relative rectangles, centered titles, guide adjacency/reflection and the shared Power figure all calculate. Independent resource words remain editable without influencing neighboring cells or immutable stock geometry. Implementation has removed unexplained fallback words; focused full1024native/edit/live-rebind confirmation is in progress. Entry stays required until independent review and passing confirmation. Supply bounds now also reuse MODE/RESERVE TANK immutable label anchors and width, retaining only chosen two-cell left/bottom padding; its title centers by the same arithmetic as the inventory titles. Initial full confirmation passed1024words/zerooverrides/eight edits/eighteen actual rebinds after1437warning/zeroerror build. No disposition approval is assumed.
 Coordinator independently viewed the page and reviewed the packet/source/consumers, approving only this exact title/border/header/arrow/gauge/guide composition, selected supply padding2 and bounded transparent/priority metadata. All identified geometry/sharing calculates. Final SupplyPanel refinement passed1197warning/zeroerror build and repeated1024native/zerooverride/eightedit/eighteenrebind confirmation. Entry mixed complete;102resolved/124required after integrated Golden Torizo and both pause entries. PauseClosedContractDefinitions needs refreshed hashes for PauseEquipmentBasePresentation and both PauseEquipmentBaseDefinitions references. No glyph-pixel or other pause-entry exemption follows.

Root integration: Root Verification1452existing warnings/0errors;Audit0/0;544wireframe native words/266strip references/4actual patches/28edits and1024equipment words/zero stock overrides/8edits/18actual live rebinds pass. Master 600 converted/45 retained-mixed/483 pending.

### Root completion: Yard semantic visual groups

`YardVisualDefinitions.Groups` now dispatches directly by the existing named bank-A3 instruction entry boundaries. Its38 stored entry/name records represented crawling, outside/inside turns, hiding/hidden and airborne program roles. Descending named cases preserve the exact containing-program mapping without a replacement group table; frame ordinal resets on the actual semantic group change. The existing112 compiled presentation operands still produce104 distinct ordered sprite identities. This is the semantic case conversion authorized by the ticket; no artwork, program timing or movement data inherits a retention decision.

Confirmation: full Verification build1457warnings/0errors, focused proof rebuild25/0. The pre-change group mapping combined with all112 pinned ROM pointer words produces the104-record bank/pointer/name/order SHA2562E09CB87CB8C4106F823834D08281C250FB4294981A52F4FE9F83BD983732954. Dedicated --lookup-stream2-yard-groups confirms the new production Frames() output matches that complete identity. No gameplay or unrelated test search. No ResourceAudit hash closure references this source.

Root integration: Root full build1457warnings/0errors then focused proof rebuild25/0;104complete native bank/pointer/name/order identities match the pre-change registry hash across38semantic group roles. Master 606 converted/98 retained-mixed/424 pending.

## Single-thread batch - enemy instruction programs

This stream's instruction-program entries (`*InstructionProgramDefinitions.Words` and
`.PresentationWords`, plus program-specific stores) are resolved together across all streams.
The method, exactness evidence, disposition of frame durations and the per-program table are in
[lookup-1165-instruction-programs.md](lookup-1165-instruction-programs.md). Stored word tables are
now semantic `InstructionProgramLayout` items; already-calculated programs keep their code with
the reviewed disposition recorded on each owner. Checkboxes above are ticked for the entries this
batch resolved.
