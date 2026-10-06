# Issue 1165 - agent stream 5

GitHub child ticket: [#1242](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1242).

Assignment: **113 files / 225 named table definitions**. [Ownership rules and all streams](lookup-1165-streams.md). Inventory snapshot: 2026-10-04T20:43:25.3086366Z.

## Agent instructions

Work through every entry below under issue 1165. Check current source and review dispositions first. Convert the original mapping into calculation or meaningful cases, or document a concrete impossible/nonsense justification. Preserve behavior and supplied content edits. Independently resolve each value field; checking off a container does not excuse its unresolved payloads.

Edit only source files listed here and this stream checklist/report. Read other files as needed. Ask the coordinator to assign any additional dependency or new source/test file before writing it. Route shared review-ledger, project, verification-entry-point and master-inventory changes through the coordinator. Do not stage, commit or publish other agents' work. Use focused confirmation of identified changes, not exploratory test discovery.

For each completed entry, record the conversion or precise retention evidence, changed files and focused confirmation. Report cross-stream dependencies by path and required contract. Report pending entries honestly; definition counts are not effort estimates.

## Exclusive source files

| File | Definitions |
| --- | ---: |
| [csharp/src/SuperMetroid.Core/Assets/CeresDestructionActorLayout.cs](../csharp/src/SuperMetroid.Core/Assets/CeresDestructionActorLayout.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CeresDestructionArtworkCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/CeresDestructionArtworkCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CeresDestructionSpritePresentation.cs](../csharp/src/SuperMetroid.Core/Assets/CeresDestructionSpritePresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs](../csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CeresFlightActorLayout.cs](../csharp/src/SuperMetroid.Core/Assets/CeresFlightActorLayout.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/CeresFlightPalette.cs](../csharp/src/SuperMetroid.Core/Assets/CeresFlightPalette.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CeresRevealActorLayout.cs](../csharp/src/SuperMetroid.Core/Assets/CeresRevealActorLayout.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.Bank86.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.Bank86.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA2.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA2.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA3.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA3.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA4.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA4.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA5.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA5.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA6.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA6.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA7.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA7.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA8.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA8.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA9.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA9.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankAA.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankAA.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB2.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB2.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB3.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB3.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB4.Definitions.cs](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB4.Definitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Assets/CrocomireMeltingArtwork.cs](../csharp/src/SuperMetroid.Core/Assets/CrocomireMeltingArtwork.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/CrocomireSkeletonTransferDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/CrocomireSkeletonTransferDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/DachoraColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/DachoraColorCatalog.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/DeadTourianCorpseVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/DeadTourianCorpseVisualDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/EvirVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/EvirVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MagdollitePaletteCycle.cs](../csharp/src/SuperMetroid.Core/Assets/MagdollitePaletteCycle.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MapLandmarkLayout.cs](../csharp/src/SuperMetroid.Core/Assets/MapLandmarkLayout.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs](../csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MapPaletteCycle.cs](../csharp/src/SuperMetroid.Core/Assets/MapPaletteCycle.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/MapScreenPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/MapScreenPresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MapStaticPalettes.cs](../csharp/src/SuperMetroid.Core/Assets/MapStaticPalettes.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/MapStationLayout.cs](../csharp/src/SuperMetroid.Core/Assets/MapStationLayout.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MorphBallEyeVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/MorphBallEyeVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Assets/ZebetiteColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/ZebetiteColorCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs) | 12 |
| [csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs](../csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs](../csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/CeresBabyInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresBabyInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CeresCinematicLightPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresCinematicLightPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CeresDoorInitializationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresDoorInitializationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CeresDoorInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresDoorInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CeresDoorQuakeDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresDoorQuakeDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CeresFallingDebrisInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresFallingDebrisInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs) | 7 |
| [csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Game/CeresSteamDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresSteamDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CeresSteamInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresSteamInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CinematicGlowPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CinematicGlowPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CrocomireBodyCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CrocomireBodyCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CrocomireBridgeFragmentDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CrocomireBridgeFragmentDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CrocomireInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CrocomireInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CrocomireRumbleDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CrocomireRumbleDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/DachoraInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DachoraInstructionProgramDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Game/DeadSidehopperInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/DeadSidehopperLaunchDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperLaunchDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/DownwardGateProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DownwardGateProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EvirInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EvirInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/HudReserveLayout.cs](../csharp/src/SuperMetroid.Core/Game/HudReserveLayout.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/KiHunterAcidSpitInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/KiHunterAcidSpitInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/KiHunterInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/KiHunterInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MagdolliteInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MagdolliteInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MagdolliteLavaInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MagdolliteLavaInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MagdollitePhaseDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MagdollitePhaseDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MorphBallEyeInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MorphBallEyeInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileRomData.cs](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileRomData.cs) | 6 |
| [csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs) | 7 |
| [csharp/src/SuperMetroid.Core/Game/PhantoonInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PhantoonInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/PhantoonSoundDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PhantoonSoundDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/PolypInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PolypInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/PolypRockInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PolypRockInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RinkaInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RinkaInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresDoor.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresDoor.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CrocomireArena.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CrocomireArena.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Rinka.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Rinka.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.YappingMaw.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.YappingMaw.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SharedCrawlerInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SharedCrawlerInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SlopeSpeedDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SlopeSpeedDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SpacePirateCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SpacePirateCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SpacePirateProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SpacePirateProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TourianEntranceStatueInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TourianEntranceStatueInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TourianStatueGreyPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TourianStatueGreyPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/TourianStatueProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TourianStatueProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TourianStatueUnlockDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TourianStatueUnlockDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/YappingMawBodyProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/YappingMawBodyProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/YappingMawInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/YappingMawInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/ZebetiteInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ZebetiteInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Rendering/XrayRoomDisplayRules.cs](../csharp/src/SuperMetroid.Core/Rendering/XrayRoomDisplayRules.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramGeneratedDefinitions.cs](../csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramGeneratedDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs](../csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs) | 2 |

## Work checklist

### csharp/src/SuperMetroid.Core/Assets/CeresDestructionActorLayout.cs

- [x] **CeresDestructionActorLayout.placements** ([L12](../csharp/src/SuperMetroid.Core/Assets/CeresDestructionActorLayout.cs#L12)) - installed stock table. Original/default payload behind CeresDestructionActorLayout.placements. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresDestructionArtworkCatalog.cs

- [ ] **CeresDestructionArtworkCatalog.CeresMaps** ([L26](../csharp/src/SuperMetroid.Core/Assets/CeresDestructionArtworkCatalog.cs#L26)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.

### csharp/src/SuperMetroid.Core/Assets/CeresDestructionSpritePresentation.cs

- [x] **CeresDestructionSpritePresentation.frames** - MIXED: nineteen prior layouts are calculated; four star sheets calculate common rendering fields and grid scale, retaining only 29 ordered decorative position/glyph triples under the independently reviewed nonsense exception below. Pixel data is separate.

### csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs

- [ ] **CeresDoorVisualCatalog.normal** ([L24](../csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs#L24)) - installed stock table. Original/default payload behind CeresDoorVisualCatalog.normal. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresDoorVisualCatalog.escape** ([L25](../csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs#L25)) - installed stock table. Original/default payload behind CeresDoorVisualCatalog.escape. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresDoorVisualCatalog.animation** ([L26](../csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs#L26)) - installed stock table. Original/default payload behind CeresDoorVisualCatalog.animation. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **CeresDoorVisualCatalog.mode7DoorFrames** ([L27](../csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs#L27)) - installed stock table. Original/default payload behind CeresDoorVisualCatalog.mode7DoorFrames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs

- [x] **CeresEscapeOverlayTilemapDefinitions.Definitions** - CONVERTED: named source-page cases preserve addresses, ranges, names and order. Independent pixels and overlay words remain required.
- [x] **CeresEscapeOverlayTilemapCatalog.pages** ([L78](../csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs#L78)) - installed stock table. Original/default payload behind CeresEscapeOverlayTilemapCatalog.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs

- [x] **CeresEscapeTileArtworkDefinitions.Pages** - CONVERTED: named source-page cases preserve addresses, ranges, names and order. Independent pixels and overlay words remain required.

### csharp/src/SuperMetroid.Core/Assets/CeresFlightActorLayout.cs

- [x] **CeresFlightActorLayout.placements** ([L12](../csharp/src/SuperMetroid.Core/Assets/CeresFlightActorLayout.cs#L12)) - installed stock table. Original/default payload behind CeresFlightActorLayout.placements. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs

- [ ] **CeresFlightArtworkCatalog.Mode7Characters** ([L24](../csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs#L24)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.
- [ ] **CeresFlightArtworkCatalog.Mode7Maps** ([L26](../csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs#L26)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.
- [ ] **CeresFlightArtworkCatalog.ObjectCharacters** ([L27](../csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs#L27)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.

### csharp/src/SuperMetroid.Core/Assets/CeresFlightPalette.cs

- [ ] **CeresFlightPalette.nativeBytes** ([L10](../csharp/src/SuperMetroid.Core/Assets/CeresFlightPalette.cs#L10)) - installed stock table. Original/default payload behind CeresFlightPalette.nativeBytes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresRevealActorLayout.cs

- [x] **CeresRevealActorLayout.placements** ([L13](../csharp/src/SuperMetroid.Core/Assets/CeresRevealActorLayout.cs#L13)) - installed stock table. Original/default payload behind CeresRevealActorLayout.placements. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/CeresBabyInstructionProgramDefinitions.cs

- [x] **CeresBabyInstructionProgramDefinitions.Words** ([L49](../csharp/src/SuperMetroid.Core/Game/CeresBabyInstructionProgramDefinitions.cs#L49)) - factory-built stock table. Stored CeresBabyInstructionMechanicsWord[] initialized by CreateWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/CeresCinematicLightPaletteFxProgramMechanicsDefinitions.cs

- [x] **CeresCinematicLightPaletteFxProgramMechanicsDefinitions.All** ([L103](../csharp/src/SuperMetroid.Core/Game/CeresCinematicLightPaletteFxProgramMechanicsDefinitions.cs#L103)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresDoorInitializationDefinitions.cs

- [x] **CeresDoorInitializationDefinitions.Definitions** ([L84](../csharp/src/SuperMetroid.Core/Game/CeresDoorInitializationDefinitions.cs#L84)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresDoorInstructionProgramDefinitions.cs

- [x] **CeresDoorInstructionProgramDefinitions.Words** ([L156](../csharp/src/SuperMetroid.Core/Game/CeresDoorInstructionProgramDefinitions.cs#L156)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CeresDoorInstructionProgramDefinitions.PresentationWords** ([L269](../csharp/src/SuperMetroid.Core/Game/CeresDoorInstructionProgramDefinitions.cs#L269)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresDoorQuakeDefinitions.cs

- [x] **CeresDoorQuakeDefinitions.XOffsets** ([L14](../csharp/src/SuperMetroid.Core/Game/CeresDoorQuakeDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs

- [x] **CeresEscapeVramTransferDefinitions.Records** ([L30](../csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresFallingDebrisInstructionProgramDefinitions.cs

- [x] **CeresFallingDebrisInstructionProgramDefinitions.Words** ([L34](../csharp/src/SuperMetroid.Core/Game/CeresFallingDebrisInstructionProgramDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CeresFallingDebrisInstructionProgramDefinitions.PresentationWords** ([L53](../csharp/src/SuperMetroid.Core/Game/CeresFallingDebrisInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs

- [ ] **CeresMode7TransferDefinitions.PlatformLight** ([L32](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.PlatformDark** ([L36](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Baby0** ([L40](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Baby1** ([L45](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Baby2** ([L50](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Wing0** ([L55](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Wing1** ([L64](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs

- [x] **CeresSteamCollisionDefinitions.DirectionBases** ([L25](../csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CeresSteamCollisionDefinitions.DirectionLists** ([L27](../csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CeresSteamCollisionDefinitions.Frames** ([L35](../csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs#L35)) - factory-built stock table. Stored Dictionary<ushort, CeresSteamCollisionComponent[]> initialized by BuildFrames(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **CeresSteamCollisionDefinitions.Lists** ([L38](../csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresSteamDefinitions.cs

- [x] **CeresSteamDefinitions.Initializations** ([L53](../csharp/src/SuperMetroid.Core/Game/CeresSteamDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresSteamInstructionProgramDefinitions.cs

- [x] **CeresSteamInstructionProgramDefinitions.Words** ([L89](../csharp/src/SuperMetroid.Core/Game/CeresSteamInstructionProgramDefinitions.cs#L89)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CeresSteamInstructionProgramDefinitions.PresentationWords** ([L164](../csharp/src/SuperMetroid.Core/Game/CeresSteamInstructionProgramDefinitions.cs#L164)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresDoor.cs

- [x] **RoomEnemySystem.CeresDoorRumbleOffsets** ([L34](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresDoor.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CinematicGlowPaletteFxProgramMechanicsDefinitions.cs

- [x] **CinematicGlowPaletteFxProgramMechanicsDefinitions.Definitions** ([L33](../csharp/src/SuperMetroid.Core/Game/CinematicGlowPaletteFxProgramMechanicsDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.Bank86.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.Bank86** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.Bank86.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA2.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankA2** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA2.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA3.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankA3** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA3.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA4.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankA4** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA4.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA5.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankA5** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA5.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA6.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankA6** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA6.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA7.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankA7** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA7.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA8.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankA8** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA8.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA9.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankA9** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankA9.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankAA.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankAA** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankAA.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB2.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankB2** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB2.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB3.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankB3** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB3.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB4.Definitions.cs

- [ ] **CompiledEnemyVisualSelectors.BankB4** ([L7](../csharp/src/SuperMetroid.Core/Assets/CompiledEnemyVisualSelectors.BankB4.Definitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs

- [ ] **CrocomireColorCatalog.fightBody** ([L24](../csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs#L24)) - installed stock table. Original/default payload behind CrocomireColorCatalog.fightBody. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CrocomireColorCatalog.initialWall** ([L25](../csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs#L25)) - installed stock table. Original/default payload behind CrocomireColorCatalog.initialWall. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CrocomireColorCatalog.initialProjectile** ([L26](../csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs#L26)) - installed stock table. Original/default payload behind CrocomireColorCatalog.initialProjectile. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CrocomireColorCatalog.skeletonArm** ([L27](../csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs#L27)) - installed stock table. Original/default payload behind CrocomireColorCatalog.skeletonArm. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CrocomireColorCatalog.wallSpikes** ([L28](../csharp/src/SuperMetroid.Core/Assets/CrocomireColorCatalog.cs#L28)) - installed stock table. Original/default payload behind CrocomireColorCatalog.wallSpikes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CrocomireMeltingArtwork.cs

- [ ] **CrocomireMeltingArtwork.firstTilemap** ([L25](../csharp/src/SuperMetroid.Core/Assets/CrocomireMeltingArtwork.cs#L25)) - installed stock table. Original/default payload behind CrocomireMeltingArtwork.firstTilemap. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CrocomireMeltingArtwork.secondTilemap** ([L26](../csharp/src/SuperMetroid.Core/Assets/CrocomireMeltingArtwork.cs#L26)) - installed stock table. Original/default payload behind CrocomireMeltingArtwork.secondTilemap. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CrocomireSkeletonTransferDefinitions.cs

- [ ] **CrocomireSkeletonTransferDefinitions.Entries** ([L22](../csharp/src/SuperMetroid.Core/Assets/CrocomireSkeletonTransferDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrocomireBodyCollisionDefinitions.cs

- [ ] **CrocomireBodyCollisionDefinitions.Frames** ([L24](../csharp/src/SuperMetroid.Core/Game/CrocomireBodyCollisionDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CrocomireBodyCollisionDefinitions.HitboxLists** ([L78](../csharp/src/SuperMetroid.Core/Game/CrocomireBodyCollisionDefinitions.cs#L78)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrocomireBridgeFragmentDefinitions.cs

- [x] **CrocomireBridgeFragmentDefinitions.XPositions** ([L19](../csharp/src/SuperMetroid.Core/Game/CrocomireBridgeFragmentDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrocomireInstructionProgramDefinitions.cs

- [ ] **CrocomireInstructionProgramDefinitions.Words** ([L300](../csharp/src/SuperMetroid.Core/Game/CrocomireInstructionProgramDefinitions.cs#L300)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CrocomireInstructionProgramDefinitions.PresentationWords** ([L745](../csharp/src/SuperMetroid.Core/Game/CrocomireInstructionProgramDefinitions.cs#L745)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrocomireRumbleDefinitions.cs

- [ ] **CrocomireRumbleDefinitions.Definitions** ([L33](../csharp/src/SuperMetroid.Core/Game/CrocomireRumbleDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CrocomireArena.cs

- [ ] **RoomEnemySystem.PublishCrocomireBridgeCollapsePlms / dustPositions** ([L83](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CrocomireArena.cs#L83)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Assets/DachoraColorCatalog.cs

- [ ] **DachoraColorCatalog.normal** ([L21](../csharp/src/SuperMetroid.Core/Assets/DachoraColorCatalog.cs#L21)) - installed stock table. Original/default payload behind DachoraColorCatalog.normal. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **DachoraColorCatalog.speed** ([L22](../csharp/src/SuperMetroid.Core/Assets/DachoraColorCatalog.cs#L22)) - installed stock table. Original/default payload behind DachoraColorCatalog.speed. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **DachoraColorCatalog.shine** ([L23](../csharp/src/SuperMetroid.Core/Assets/DachoraColorCatalog.cs#L23)) - installed stock table. Original/default payload behind DachoraColorCatalog.shine. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/DachoraInstructionProgramDefinitions.cs

- [ ] **DachoraInstructionProgramDefinitions.Sources** ([L60](../csharp/src/SuperMetroid.Core/Game/DachoraInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DachoraInstructionProgramDefinitions.Words** ([L78](../csharp/src/SuperMetroid.Core/Game/DachoraInstructionProgramDefinitions.cs#L78)) - factory-built stock table. Stored DachoraInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **DachoraInstructionProgramDefinitions.PresentationWords** ([L79](../csharp/src/SuperMetroid.Core/Game/DachoraInstructionProgramDefinitions.cs#L79)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Assets/DeadTourianCorpseVisualDefinitions.cs

- [ ] **DeadTourianCorpseVisualDefinitions.Entries** ([L19](../csharp/src/SuperMetroid.Core/Assets/DeadTourianCorpseVisualDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadTourianCorpseVisualDefinitions.SidehopperSelectors** ([L31](../csharp/src/SuperMetroid.Core/Assets/DeadTourianCorpseVisualDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs

- [x] **DeadMonsterRottingDefinitions.RotationRows** ([L9](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DeadMonsterRottingDefinitions.Transfers** ([L19](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadMonsterRottingDefinitions.TorizoHitboxRecords** ([L34](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DeadMonsterRottingDefinitions.SandDestinations** ([L47](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DeadMonsterRottingDefinitions.SandSources** ([L49](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadSidehopperInstructionProgramDefinitions.cs

- [ ] **DeadSidehopperInstructionProgramDefinitions.Words** ([L35](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperInstructionProgramDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadSidehopperInstructionProgramDefinitions.PresentationWords** ([L51](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperInstructionProgramDefinitions.cs#L51)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadSidehopperLaunchDefinitions.cs

- [ ] **DeadSidehopperLaunchDefinitions.Vertical** ([L7](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperLaunchDefinitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadSidehopperLaunchDefinitions.Horizontal** ([L9](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperLaunchDefinitions.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs

- [x] **DeadTourianCorpseDefinitions.Zoomer** ([L11](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DeadTourianCorpseDefinitions.Ripper** ([L22](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DeadTourianCorpseDefinitions.Skree** ([L32](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseInstructionProgramDefinitions.cs

- [x] **DeadTourianCorpseInstructionProgramDefinitions.Programs** ([L41](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DeadTourianCorpseInstructionProgramDefinitions.Words** ([L44](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseInstructionProgramDefinitions.cs#L44)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs

- [x] **RoomEnemySystem.DeadSidehopperInitialGraphicsCopies** ([L19](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.DeadSidehopperColumnWordOffsets0** ([L40](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.DeadSidehopperColumnMinimumY0** ([L43](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.DeadSidehopperColumnWordOffsets2** ([L46](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.DeadSidehopperColumnMinimumY2** ([L49](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs

- [ ] **RoomEnemySystem.DeadZoomerProfile** ([L26](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadRipperProfile** ([L53](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadSkreeProfile** ([L75](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs#L75)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DownwardGateProjectileInstructionProgramDefinitions.cs

- [x] **DownwardGateProjectileInstructionProgramDefinitions.Words** ([L24](../csharp/src/SuperMetroid.Core/Game/DownwardGateProjectileInstructionProgramDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DownwardGateProjectileInstructionProgramDefinitions.PresentationWords** ([L56](../csharp/src/SuperMetroid.Core/Game/DownwardGateProjectileInstructionProgramDefinitions.cs#L56)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EvirVisualDefinitions.cs

- [ ] **EvirVisualDefinitions.Frames / literal at L17** ([L17](../csharp/src/SuperMetroid.Core/Assets/EvirVisualDefinitions.cs#L17)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/EvirInstructionProgramDefinitions.cs

- [x] **EvirInstructionProgramDefinitions.Words** ([L34](../csharp/src/SuperMetroid.Core/Game/EvirInstructionProgramDefinitions.cs#L34)) - factory-built stock table. Stored EvirInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **EvirInstructionProgramDefinitions.PresentationWords** ([L35](../csharp/src/SuperMetroid.Core/Game/EvirInstructionProgramDefinitions.cs#L35)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs

- [ ] **HudTileAtlas.transfer** ([L8](../csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs#L8)) - installed stock table. Original/default payload behind HudTileAtlas.transfer. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/HudReserveLayout.cs

- [x] **HudReserveLayout.TileIndices** ([L11](../csharp/src/SuperMetroid.Core/Game/HudReserveLayout.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/KiHunterAcidSpitInstructionProgramDefinitions.cs

- [ ] **KiHunterAcidSpitInstructionProgramDefinitions.Words** ([L24](../csharp/src/SuperMetroid.Core/Game/KiHunterAcidSpitInstructionProgramDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **KiHunterAcidSpitInstructionProgramDefinitions.PresentationWords** ([L55](../csharp/src/SuperMetroid.Core/Game/KiHunterAcidSpitInstructionProgramDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/KiHunterInstructionProgramDefinitions.cs

- [ ] **KiHunterInstructionProgramDefinitions.Words** ([L41](../csharp/src/SuperMetroid.Core/Game/KiHunterInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KiHunterInstructionProgramDefinitions.PresentationWords** ([L95](../csharp/src/SuperMetroid.Core/Game/KiHunterInstructionProgramDefinitions.cs#L95)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramGeneratedDefinitions.cs

- [ ] **LibraryBackgroundProgramDefinitions.programs** ([L8](../csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramGeneratedDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MagdollitePaletteCycle.cs

- [ ] **MagdollitePaletteCycle.frames** - PARTIAL: native A8:AC2E-AC34 glow colors rotate left one slot per phase; calculate all four phases from four independently supplied seed colors and preserve arbitrary later-row edits sparsely. The four independent seed colors remain REQUIRED, with no claimed artistic exemption. --lookup-stream5-magdollite-pulse confirms all16 native colors,48 independent RGB edits, actual CGRAM, canonical identity, bounds and zero stock residual rows. Root integration build1444 existing warnings/zero errors; integrated counts71 converted,one mixed,153 required unchanged.

### csharp/src/SuperMetroid.Core/Game/MagdolliteInstructionProgramDefinitions.cs

- [ ] **MagdolliteInstructionProgramDefinitions.Words** ([L57](../csharp/src/SuperMetroid.Core/Game/MagdolliteInstructionProgramDefinitions.cs#L57)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MagdolliteInstructionProgramDefinitions.PresentationWords** ([L172](../csharp/src/SuperMetroid.Core/Game/MagdolliteInstructionProgramDefinitions.cs#L172)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MagdolliteLavaInstructionProgramDefinitions.cs

- [x] **MagdolliteLavaInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/MagdolliteLavaInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MagdolliteLavaInstructionProgramDefinitions.PresentationWords** ([L37](../csharp/src/SuperMetroid.Core/Game/MagdolliteLavaInstructionProgramDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MagdollitePhaseDefinitions.cs

- [x] **MagdollitePhaseDefinitions.Phases** ([L20](../csharp/src/SuperMetroid.Core/Game/MagdollitePhaseDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MapLandmarkLayout.cs

- [ ] **MapLandmarkLayout.points** ([L9](../csharp/src/SuperMetroid.Core/Assets/MapLandmarkLayout.cs#L9)) - installed stock table. Original/default payload behind MapLandmarkLayout.points. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs

- [ ] **MapObjectTileArtwork.otherCharacters** ([L14](../csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs#L14)) - installed stock table. Original/default payload behind MapObjectTileArtwork.otherCharacters. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MapPaletteCycle.cs

- [x] **MapPaletteCycle.durations** ([L9](../csharp/src/SuperMetroid.Core/Assets/MapPaletteCycle.cs#L9)) - installed stock table. Original/default payload behind MapPaletteCycle.durations. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MapPaletteCycle.colors** ([L10](../csharp/src/SuperMetroid.Core/Assets/MapPaletteCycle.cs#L10)) - installed stock table. Original/default payload behind MapPaletteCycle.colors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MapScreenPresentation.cs

- [ ] **MapScreenPresentation.pages** ([L10](../csharp/src/SuperMetroid.Core/Assets/MapScreenPresentation.cs#L10)) - installed stock table. Original/default payload behind MapScreenPresentation.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MapStaticPalettes.cs

- [ ] **MapStaticPalettes.fileSelect** ([L10](../csharp/src/SuperMetroid.Core/Assets/MapStaticPalettes.cs#L10)) - installed stock table. Original/default payload behind MapStaticPalettes.fileSelect. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MapStaticPalettes.pause** ([L10](../csharp/src/SuperMetroid.Core/Assets/MapStaticPalettes.cs#L10)) - installed stock table. Original/default payload behind MapStaticPalettes.pause. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MapStaticPalettes.world** ([L11](../csharp/src/SuperMetroid.Core/Assets/MapStaticPalettes.cs#L11)) - installed stock table. Original/default payload behind MapStaticPalettes.world. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MapStationLayout.cs

- [ ] **MapStationLayout.points** ([L9](../csharp/src/SuperMetroid.Core/Assets/MapStationLayout.cs#L9)) - installed stock table. Original/default payload behind MapStationLayout.points. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs

- [ ] **MapTileAtlas.planar** ([L8](../csharp/src/SuperMetroid.Core/Assets/MapTileAtlas.cs#L8)) - installed stock table. Original/default payload behind MapTileAtlas.planar. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs

- [x] **MapLandmarkDefinitions.crateriaBosses** ([L12](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.brinstarBosses** ([L14](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.norfairBosses** ([L16](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.wreckedShipBosses** ([L18](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.maridiaBosses** ([L20](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.ceresBosses** ([L22](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.crateriaElevators** ([L28](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.brinstarElevators** ([L32](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.norfairElevators** ([L35](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.wreckedShipElevators** ([L37](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.maridiaElevators** ([L39](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MapLandmarkDefinitions.tourianElevators** ([L41](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs

- [x] **MapScrollControls.Buttons** ([L16](../csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs

- [ ] **MapStationDiscoveryRules.missile** ([L16](../csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapStationDiscoveryRules.energy** ([L22](../csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapStationDiscoveryRules.map** ([L33](../csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MorphBallEyeVisualDefinitions.cs

- [ ] **MorphBallEyeVisualDefinitions.Frames / literal at L15** ([L15](../csharp/src/SuperMetroid.Core/Assets/MorphBallEyeVisualDefinitions.cs#L15)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/MorphBallEyeInstructionProgramDefinitions.cs

- [ ] **MorphBallEyeInstructionProgramDefinitions.Words** ([L41](../csharp/src/SuperMetroid.Core/Game/MorphBallEyeInstructionProgramDefinitions.cs#L41)) - factory-built stock table. Stored MorphBallEyeInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **MorphBallEyeInstructionProgramDefinitions.PresentationWords** ([L43](../csharp/src/SuperMetroid.Core/Game/MorphBallEyeInstructionProgramDefinitions.cs#L43)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs

- [x] **RoomEnemySystem.EyeMountXOffsets** ([L161](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L161)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.EyeMountYOffsets** ([L162](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L162)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.EyeMountInstructionLists** ([L164](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L164)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.EyeBeamRedCycle** ([L174](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L174)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.EyeBeamGreenCycle** ([L177](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L177)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs

- [x] **NoobTubeProjectileInstructionProgramDefinitions.ShardPrograms** ([L21](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NoobTubeProjectileInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs#L23)) - factory-built stock table. Stored NoobTubeProjectileInstructionMechanicsWord[] initialized by BuildWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **NoobTubeProjectileInstructionProgramDefinitions.PresentationWords** ([L24](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs#L24)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileRomData.cs

- [ ] **NoobTubeProjectileRomData.ShardXOffsetWords** ([L7](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileRomData.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NoobTubeProjectileRomData.ShardYOffsetWords** ([L9](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileRomData.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NoobTubeProjectileRomData.ShardXVelocityWords** ([L11](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileRomData.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NoobTubeProjectileRomData.ShardYVelocityWords** ([L13](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileRomData.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NoobTubeProjectileRomData.BubbleXOffsetWords** ([L14](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileRomData.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NoobTubeProjectileRomData.BubbleYOffsetWords** ([L15](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileRomData.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs

- [ ] **PhantoonColorCatalog.healthBands** ([L21](../csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs#L21)) - installed stock table. Original/default payload behind PhantoonColorCatalog.healthBands. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **PhantoonColorCatalog.fadeOut** ([L22](../csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs#L22)) - installed stock table. Original/default payload behind PhantoonColorCatalog.fadeOut. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PhantoonColorCatalog.powerOn** ([L23](../csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs#L23)) - installed stock table. Original/default payload behind PhantoonColorCatalog.powerOn. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs

- [x] **PhantoonCasualFlameDefinitions.Pattern / literal at L9** ([L9](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L9)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **PhantoonCasualFlameDefinitions.Pattern / literal at L10** ([L10](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L10)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **PhantoonCasualFlameDefinitions.Pattern / literal at L11** ([L11](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L11)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **PhantoonCasualFlameDefinitions.Pattern / literal at L12** ([L12](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L12)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs

- [x] **PhantoonCollisionDefinitions.Point** ([L30](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonCollisionDefinitions.FullBody** ([L31](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonCollisionDefinitions.EyeOnly** ([L32](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonCollisionDefinitions.DoublePoint** ([L34](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonCollisionDefinitions.PointHitboxes** ([L37](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonCollisionDefinitions.FullBodyHitboxes** ([L40](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonCollisionDefinitions.EyeOnlyHitboxes** ([L48](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L48)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonInstructionProgramDefinitions.cs

- [ ] **PhantoonInstructionProgramDefinitions.Words** ([L57](../csharp/src/SuperMetroid.Core/Game/PhantoonInstructionProgramDefinitions.cs#L57)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonInstructionProgramDefinitions.PresentationWords** ([L95](../csharp/src/SuperMetroid.Core/Game/PhantoonInstructionProgramDefinitions.cs#L95)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs

- [x] **PhantoonPatternDefinitions.EyeInstructions** ([L11](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonPatternDefinitions.FirstRainColumns** ([L37](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonPatternDefinitions.ShotEyeMarkers** - retained (nonsense): eight arbitrary random-bucket marker assignments only ([L40](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs

- [ ] **PhantoonProjectileInstructionProgramDefinitions.Words** ([L45](../csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonProjectileInstructionProgramDefinitions.PresentationWords** ([L107](../csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs#L107)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonSoundDefinitions.cs

- [x] **PhantoonSoundDefinitions.Materialization** ([L10](../csharp/src/SuperMetroid.Core/Game/PhantoonSoundDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs

- [x] **PhantoonTimerDefinitions.VulnerableWindow** ([L9](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonTimerDefinitions.EyeClosed** ([L11](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonTimerDefinitions.RainHiding** ([L13](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PolypInstructionProgramDefinitions.cs

- [x] **PolypInstructionProgramDefinitions.Words** ([L15](../csharp/src/SuperMetroid.Core/Game/PolypInstructionProgramDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PolypRockInstructionProgramDefinitions.cs

- [x] **PolypRockInstructionProgramDefinitions.Words** - Single-pose/sleep controls and visual operand locations calculate from program widths; see Polyp-rock/Yapping Maw batch below.

### csharp/src/SuperMetroid.Core/Game/RinkaInstructionProgramDefinitions.cs

- [ ] **RinkaInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/RinkaInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RinkaInstructionProgramDefinitions.PresentationWords** ([L40](../csharp/src/SuperMetroid.Core/Game/RinkaInstructionProgramDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Rinka.cs

- [ ] **RoomEnemySystem.RinkaSpawnResources** ([L86](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Rinka.cs#L86)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SharedCrawlerInstructionProgramDefinitions.cs

- [ ] **SharedCrawlerInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/SharedCrawlerInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SharedCrawlerInstructionProgramDefinitions.PresentationWords** ([L54](../csharp/src/SuperMetroid.Core/Game/SharedCrawlerInstructionProgramDefinitions.cs#L54)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SlopeSpeedDefinitions.cs

- [ ] **SlopeSpeedDefinitions.Multipliers** ([L13](../csharp/src/SuperMetroid.Core/Game/SlopeSpeedDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SpacePirateCollisionDefinitions.cs

- [ ] **SpacePirateCollisionDefinitions.Frames** ([L15](../csharp/src/SuperMetroid.Core/Game/SpacePirateCollisionDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SpacePirateCollisionDefinitions.Lists** ([L150](../csharp/src/SuperMetroid.Core/Game/SpacePirateCollisionDefinitions.cs#L150)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SpacePirateProjectileInstructionProgramDefinitions.cs

- [ ] **SpacePirateProjectileInstructionProgramDefinitions.Words** ([L39](../csharp/src/SuperMetroid.Core/Game/SpacePirateProjectileInstructionProgramDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SpacePirateProjectileInstructionProgramDefinitions.PresentationWords** ([L82](../csharp/src/SuperMetroid.Core/Game/SpacePirateProjectileInstructionProgramDefinitions.cs#L82)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs

- [ ] **TourianStatueColorCatalog.baseColors** ([L20](../csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs#L20)) - installed stock table. Original/default payload behind TourianStatueColorCatalog.baseColors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TourianStatueColorCatalog.statueColors** ([L21](../csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs#L21)) - installed stock table. Original/default payload behind TourianStatueColorCatalog.statueColors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TourianStatueColorCatalog.eyeColors** ([L22](../csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs#L22)) - installed stock table. Original/default payload behind TourianStatueColorCatalog.eyeColors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TourianStatueColorCatalog.greyColors** ([L23](../csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs#L23)) - installed stock table. Original/default payload behind TourianStatueColorCatalog.greyColors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/TourianEntranceStatueInstructionProgramDefinitions.cs

- [x] **TourianEntranceStatueInstructionProgramDefinitions.InitialPrograms** ([L24](../csharp/src/SuperMetroid.Core/Game/TourianEntranceStatueInstructionProgramDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TourianEntranceStatueInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/TourianEntranceStatueInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs

- [ ] **TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.Definitions** ([L34](../csharp/src/SuperMetroid.Core/Game/TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.cs

- [x] **TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.Definitions** ([L49](../csharp/src/SuperMetroid.Core/Game/TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianStatueGreyPaletteFxProgramMechanicsDefinitions.cs

- [x] **TourianStatueGreyPaletteFxProgramMechanicsDefinitions.Definitions** ([L30](../csharp/src/SuperMetroid.Core/Game/TourianStatueGreyPaletteFxProgramMechanicsDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianStatueProjectileInstructionProgramDefinitions.cs

- [ ] **TourianStatueProjectileInstructionProgramDefinitions.Words** ([L35](../csharp/src/SuperMetroid.Core/Game/TourianStatueProjectileInstructionProgramDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **TourianStatueProjectileInstructionProgramDefinitions.PresentationWords** ([L75](../csharp/src/SuperMetroid.Core/Game/TourianStatueProjectileInstructionProgramDefinitions.cs#L75)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianStatueUnlockDefinitions.cs

- [ ] **TourianStatueUnlockDefinitions.EyePositions** ([L15](../csharp/src/SuperMetroid.Core/Game/TourianStatueUnlockDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs

- [ ] **WalkingSpacePirateInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **WalkingSpacePirateInstructionProgramDefinitions.PresentationWords** ([L72](../csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs#L72)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rendering/XrayRoomDisplayRules.cs

- [x] **XrayRoomDisplayRules.ExcludedBossIds** - Restored native equality-branch control flow from CheckIfXrayShouldShowAnyBlocks ($91:D158-D172), replacing the membership array with the same five conditional cases. Existing focused room-rule verifier confirms every room/boss word, Fireflea precedence, and color-math settings via --lookup-stream5-xray-room-rules. Root build 1444 existing warnings, zero errors. Integrated stream checkpoint: 61 converted, one mixed, 163 required.

### csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs

- [ ] **XrayOverlayVisualCatalog.itemMetatiles** ([L29](../csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs#L29)) - installed stock table. Original/default payload behind XrayOverlayVisualCatalog.itemMetatiles. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **XrayOverlayVisualCatalog.rooms** ([L43](../csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs#L43)) - installed stock table. Original/default payload behind XrayOverlayVisualCatalog.rooms. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.YappingMaw.cs

- [x] **RoomEnemySystem.YappingMawHeldSamusOffsets** - Calculate the radius16 Manhattan diamond in eight clockwise steps; each step exchanges eight pixels between components. Granted YappingMawRomData owns native A8:A0A7-A0C6 identity/formula; both actual runtime consumers call it. --lookup-stream5-yapping-maw-offsets confirms16native words, eight actual held-offset callbacks,256 angle-quantized BeginExtension state writes (including wrap), and unchanged invalid-direction bounds. Build1433 existing warnings/zero errors. Integrated stream checkpoint57resolved(56converted,one mixed)/168required.

### csharp/src/SuperMetroid.Core/Game/YappingMawBodyProjectileInstructionProgramDefinitions.cs

- [x] **YappingMawBodyProjectileInstructionProgramDefinitions.Words** - Single-pose/sleep controls and visual operand locations calculate from program widths; see Polyp-rock/Yapping Maw batch below.
- [x] **YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWords** - Single-pose/sleep controls and visual operand locations calculate from program widths; see Polyp-rock/Yapping Maw batch below.

### csharp/src/SuperMetroid.Core/Game/YappingMawInstructionProgramDefinitions.cs

- [ ] **YappingMawInstructionProgramDefinitions.Words** ([L47](../csharp/src/SuperMetroid.Core/Game/YappingMawInstructionProgramDefinitions.cs#L47)) - factory-built stock table. Stored YappingMawInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **YappingMawInstructionProgramDefinitions.PresentationWords** ([L49](../csharp/src/SuperMetroid.Core/Game/YappingMawInstructionProgramDefinitions.cs#L49)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Assets/ZebetiteColorCatalog.cs

- [ ] **ZebetiteColorCatalog.frames** - PARTIAL: all eight rows now calculate mirrored four-step componentwise linear interpolation with integer truncation, confirmed against all16 native colors at A6:FD87-FDA6. Stock stores zero frame residuals; independent custom colors use sparse edits. The independent endpoint choices RGB(31,2,0), (23,1,0), shared peak(31,0,0) remain REQUIRED pending functional/art evidence. --lookup-stream5-zebetite-pulse confirms16 native colors,48 independent RGB edits, actual CGRAM, unchanged canonical identity, and frame bounds. Root build1444 existing warnings/zero errors; ResourceAudit0 warnings/errors after source closure refresh. Integrated counts unchanged:71 converted,one mixed,153 required.

### csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs

- [x] **ZebetiteDefinitions.Generations** ([L20](../csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ZebetiteDefinitions.BigHealthInstructionLists** ([L40](../csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ZebetiteDefinitions.LinkedHealthInstructionLists** ([L53](../csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ZebetiteInstructionProgramDefinitions.cs

- [x] **ZebetiteInstructionProgramDefinitions.Programs** ([L46](../csharp/src/SuperMetroid.Core/Game/ZebetiteInstructionProgramDefinitions.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ZebetiteInstructionProgramDefinitions.Words** ([L60](../csharp/src/SuperMetroid.Core/Game/ZebetiteInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

## Agent handoff

- Completed conversions: pending.
- Justified retained entries: pending.
- Confirmation results: pending.
- Cross-stream dependencies and proposed shared-file patches: pending.
- Remaining entries: all unchecked entries above.


### Batch 1: Ceres initialization/DMA and Magdollite phases

- `CeresDoorInitializationDefinitions.Definitions`: semantic dispatch on the seven named door variants replaces the paired main-function/instruction table. Both fields preserve native aliases.
- `CeresSteamDefinitions.Initializations`: semantic dispatch on directional and rotating-elevator variants replaces both initialization fields; rotating variants reuse the corresponding directional animation and select the transform callback.
- `MagdollitePhaseDefinitions.Phases`: threshold is 16 times phase; the initial two records share visible phase zero, then each phase advances a six-byte frame/sleep program and eight-pixel overlay displacement.
- `CeresEscapeVramTransferDefinitions.Records`: calculated indexed records replace all 19 descriptors. Japanese tilemap rows are packed at their original widths and mapped to 32-word VRAM rows; timer sprites use consecutive blocks; five warning-character strips share source data between object/background VRAM; three door blocks advance by 512 source bytes and 256 destination words. No reconstructed descriptor table is cached. Original pixels remain separately required.
- Added `Program.LookupStream5.cs` and local `--lookup-stream5-initialization` wiring. This confirms all 53 original initialization/phase words, every changed field and rejected domains. Extended the assigned DMA proof only for `IReadOnlyList` API compatibility; its original descriptor and actual DMA-queue assertions remain.
- Existing `VerifyCeresDoorInitializationDefinitions` production fixture cannot currently run standalone: its actor lacks installed Ceres door visuals now required by production. It fails before the changed dispatch is exercised. No production behavior was changed to accommodate that fixture; dedicated native-word checks cover the changed dispatch contract. Other existing fixture production behavior is not claimed as verified.
- Confirmation results: pending. No retention exceptions granted; all other checklist entries remain required.
Batch 1 confirmation passed: Verification build (1431 existing warnings, zero errors); ResourceAudit build (zero warnings/errors); `--lookup-stream5-initialization` (53 native words and rejected domains); `--ceres-escape-transfers` (all 19 original records, three terminators, both installed timer lists and Japanese overlay with descriptor reads blocked). Four named definitions are complete. These results supersede the pending confirmation note above; the older standalone door-production fixture limitation remains as recorded. Other 221 named definitions remain required.
### Batch 2: cinematic/Tourian palette entry catalogs

Replaced `CeresCinematicLightPaletteFxProgramMechanicsDefinitions.All`, `CinematicGlowPaletteFxProgramMechanicsDefinitions.Definitions`, `TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.Definitions`, and `TourianStatueGreyPaletteFxProgramMechanicsDefinitions.Definitions` with immutable indexed calculation/semantic-dispatch views. The first three dispatch by the actual light/glow/level owner. Statue definition/entry addresses advance by their four/eight-byte record layouts, CGRAM rows advance by palette size while skipping row eight, and only the final statue entry falls through into the shared program. These views do not cache a regenerated table.

Added `--lookup-stream5-palette-entries`: checks all eleven definition list operands against original bank 8D, indexed/enumerated order and bounds, and every mechanics result over the full pointer domain against the pinned ROM with exact expected coverage. This verifies the changed entry dispatch routes into the original controls. All live BGR555 color payloads remain independently required; no retention disposition is granted. Confirmation pending.
Batch 2 confirmed: Verification build passed (1431 existing warnings, zero errors). `--lookup-stream5-palette-entries` passed all eleven entry identities, 44 Ceres-light mechanics words, 64 glow words, 50 shared-red words and 31 statue-grey words, enumeration and bounds. Four more named definitions complete, eight total in stream5; 217 entries remain required. No color payload was exempted or marked complete by these metadata conversions.
### Batch 3: installed Ceres actor placements

Removed the three stock placement arrays for the approach, destruction and Zebes-reveal scenes. Defaults now resolve through the existing named actor initializer cases: large/small debris, moving/stationary vortex, station, rear stars, planet/title and the calculated two-by-two reveal star-sheet grid. No second stock mapping or regenerated cache remains. Every supplied identity/X/Y is checked before selecting defaults; any independent coordinate edit keeps the complete supplied layout. Canonical selected-content identity remains byte-for-byte equivalent.

Verification build passed (25 incremental warnings, zero errors; full production rebuild1431 warnings). `--lookup-stream5-actor-layouts` passed all14 native actor placements/28 original coordinate operands, all28 independent X/Y edits with all other fields preserved, unchanged canonical identities and both bounds. The stationary-vortex fixture initially addressed the LDA opcode at$8B:BFA5; source inspection corrected the fixture to its immediate operand at$BFA6 before the successful run. No production workaround was made. This confirmation covers the changed layout/indexer contract, not a gameplay playthrough.

Eleven named definitions complete in stream5;214 remain required. Color/Mode7/artwork payloads are not exempted by actor-layout dispatch.
### Batch 4: Ceres steam/debris program structure and unused HUD mapping

Removed steam mechanics/visual-address tables: all four directions share the52-byte activation-wait, hidden64-tick hold, seven three-tick active-frame and return-branch structure. Removed falling-debris mechanics/visual-address tables: light/dark each display one one-tick pose then sleep. Calculated words preserve exact instruction, branch, duration and presentation-address fields without cached sequences. Separate artwork remains required; the old single-pose/size-based retention prose was removed.

Static search of all csharp/src found `HudReserveLayout` only at its declaration; removed the unused type and its six-cell table. Compiler confirmation shows no consumer depends on it. Unsupported opaque-generator/complexity retention claims were also removed from Crocomire rumble and bridge-fragment documentation; those two data mappings remain required and unchecked.

Verification build passed (1431 warnings, zero errors). `--ceres-steam-instruction-mechanics` passed68 original mechanics words and all four actual activation/hide/show cycles. `--ceres-debris-instruction-mechanics` passed4 original mechanics words, both actual producers, terminal sleeps and shared deletion. Both initially reached obsolete visual-read assertions (36/2 expected versus0 actual) after all changed program assertions passed. With coordinator-assigned ownership, the verifiers now assert zero reads and independently compare all36/2 compiled selectors with original ROM operands. Production was not changed for these stale assertions. The final steam console wording was corrected to describe the now-passing zero-read check.

Sixteen named definitions complete in stream5;209 remain required.
### Batch 5: Evir animation and projectile regeneration structure

Removed both generated-and-cached Evir word tables. Both facing halves calculate their six-frame body and seventeen-frame arm loops, preserving the final arm rest. The normal projectile displays one pose then sleeps; the regeneration program explicitly starts the horizontal offset, sets an eight-step timer, emits the spit sound, advances the offset through eight-tick records, then finishes after its16-tick final pose and sleeps. Mechanics and interleaved visual operand addresses now calculate directly.

Verification build passed (1431 warnings, zero errors). `--evir-instruction-program-definitions` passed all67 original mechanics words, all six actual body/arm/projectile programs, complete regeneration callbacks and49 compiled sprite selectors. Eighteen named definitions complete in stream5;207 remain required. The separate Crocomire destruction-order retention review is pending and is not counted complete.
### Batch 6: Crocomire destruction order — mixed conversion and justified retention

**Resolved by mixed conversion/justified retention, not fully converted.** Floor X coordinates now calculate as $0710+16*(column-1). Only the eleven-column destruction order `8,3,9,4,11,6,10,7,1,5,2` remains stored. This order specifies the effect choreography itself: native $A4:9136-$9153 advances a byte cursor by2 and passes the selected X directly to the identical projectile initializer, stopping at22. It does not select order by direction, geometry, RNG or another input. $86:9286-$92B9 fixes Y187, X velocity0 and graphics$0400; each fragment independently receives Y velocity(RNG&63)+64. That random velocity does not determine which column crumbles next. Replacing the authored order with an invented shuffle would change the visual destruction sequence, rather than reconstruct any functional relation. The coordinator reviewed both pinned native consumers and accepted the narrow nonsense exception for this permutation only.

Verification build passed (1431 warnings, zero errors). `--lookup-stream5-crocomire-order` passed every original word at $A4:9156-$916B, actual produced X order `0780,0730,0790,0740,07B0,0760,07A0,0770,0710,0750,0720`, all eleven allocations, Y/substate behavior and cursor cutoff with original position reads forbidden. The fixture's fixed RNG gives Y velocity$74; this is fixture evidence, not a universal native velocity claim. Invalid odd/post-end cursors remain rejected.

Also corrected an incomplete comment replacement from batch4: the remaining unsupported `no clearer lossless generator` line in Crocomire rumble documentation is now removed. Rumble remains required and unchecked.

Nineteen stream5 definitions resolved:18 converted/removed and1 mixed conversion with narrowly justified retention.206 remain required. No other payload receives this exception.
### Batch 7: Magdollite lava and downward gate programs

Removed both mechanics and presentation-address tables in each program. Magdollite's two directional poses calculate their one-tick/sleep records; its shot branch explicitly calls drops then follows shared deletion. Downward gates calculate four equal movement stages in each direction and explicitly encode velocity installation, movement callback installation, closed sleep, callback clearing and deletion. No equivalent table is rebuilt or cached.

Verification build passed (1431 full-build warnings,25 incremental warnings, zero errors). `--magdollite-lava-instruction-mechanics` passed all7 private native words, both actual directional producers, shot/drop/deletion and2 native compiled visual operands. `--downward-gate-projectile-instruction-mechanics` passed all28 native words, both actual producers, exact four-stage close/open positions, sleep/wake behavior, deletion and9 native compiled visual operands. Both first runs reached obsolete live-visual-read assertions after their changed mechanics/runtime checks passed. With coordinator-granted ownership, those assertions now require zero cartridge reads and compare every compiled selector with its native operand. Producer, movement, drop and deletion assertions remain intact.

Twenty-three stream5 definitions resolved:22 converted/removed and1 mixed conversion with narrowly justified retention.202 remain required.
### Batch 8: Ceres steam physical frame layout

Removed direction bases, per-direction hitbox-list pointer arrays, generated frame keys and the generated single-component dictionary. The28 ten-byte extended frames calculate consecutive pointers; each direction has seven frames, with the first five selecting consecutive fourteen-byte hitbox records and the final two selecting the shared empty list. Every component retains zero X/Y displacement. Calculated indexed/enumerable views retain no reconstructed table. Three inventoried definitions converted; individual rectangle shapes in Lists remain required.

Verification build passed (1431 full-build warnings,25 incremental warnings, zero errors). The existing `--verify-ceres-steam-collision` first failed in its obsolete reference fixture: a fake enemy$FFFF attempted to enter a removed ROM fallback. With coordinator approval, its reference side now directly decodes the original frame/rectangle words and signed16 touch/shot bounds. No production workaround was added. The corrected focused check passed all28 native component records,21 hitbox lists and4512 existing boundary comparisons through the actual compiled walker, with zero cartridge reads on that path. These cases confirm the changed mapping; rectangle retention has not been justified by their pass.

Twenty-six stream5 definitions resolved:25 converted/removed and1 mixed conversion with narrowly justified retention.199 remain required.
### Batch 9: Ceres door control programs

Removed the97-word mechanics table. Both normal facing programs share the same30-word control structure, relocating local branches by their82-byte program length. Closing/opening phases calculate four five-tick frames; Ridley's introduction calculates its four two-tick transition frames and explicitly dispatches visibility, ownership and boss-alive handoff controls. The overlay and Mode7 wall actors share one-time initialization followed by one-frame loops; the invisible wall preserves its separate escape-status branch. No equivalent mechanics array is rebuilt.

Verification build passed (1431 warnings, zero errors). `--ceres-door-instruction-mechanics` passed all97 original mechanics words, all seven actual control variants, their branch/visibility/handoff behavior and all33 existing sprite selectors with ROM reads forbidden. PresentationWords stores actual artwork selector values and remains independently required; this control conversion does not grant it a retention exception.

Twenty-seven stream5 definitions resolved:26 converted/removed and1 mixed conversion with narrowly justified retention.198 remain required.
### Batch 10: corpse-rotting tile rows, sand strips and live DMA

Removed the five stored rotation-row sequences: each corpse selects its tile width/row count and offsets calculate by32 bytes per tile. Removed both sixteen-entry sand maps: eight two-byte words per strip, with separate$120 destination/$200 source strip strides. Replaced ten DMA descriptor arrays with calculated live-WRAM row views: each corpse/parameter selects its staging region, row width and VRAM destination; subsequent rows advance the staging width and one32-tile VRAM row. Sidehopper partial first rows retain their clipped source/destination alignment. No pixels or reconstructed descriptor tables are cached. The asymmetric Torizo touch rectangles remain required.

Verification build passed (1431 full-build warnings,25 incremental warnings, zero errors); ResourceAudit build passed without warnings/errors. New `--lookup-stream5-corpse-geometry` invokes the existing native corpse-metadata proof: all30 rotation offsets,32 sand offsets,32 descriptors/all128 fields and10 native zero terminators match. Added collection confirmation checks indexed/enumerated order, audit flattening and rejected indices/layouts. The existing rectangle field comparison still passes but does not justify its retention. ResourceAudit's source review hash requires coordinator update at integration.

Thirty-one stream5 definitions resolved:30 converted/removed and1 mixed conversion with narrowly justified retention.194 remain required.
### Batch 11: Ceres elevator palette fade, partial payload derivation

The eight six-color rows at$A6:F871-$F8EC mirror four phases. Relative to phase1, each channel follows clamp(seed +5*(1-phase),0,31), except four independent phase colors. The catalog now calculates the mirrored ramp, retaining six phase1 seeds and only four first-half residuals; independent reverse-half resource edits remain separate exact deviations. Content identity serializes a temporary six-color row rather than retaining or rebuilding a runtime table. Setup palettes, tiles and Mode7 maps remain unchanged.

Verification build passed (1432 warnings, zero errors). New `--lookup-stream5-ceres-door-ramp` passed all48 native colors through actual CGRAM writes, all48 independent color edits without propagation to other rows/cells, unchanged normal/escape colors, exact canonical identities and row bounds. Stock data retains six seeds, four phase residuals and no reverse-row deviations. Native$A6:F850 uses frame bits3..5 to select these rows uniformly.

This is partial: `CeresDoorVisualCatalog.animation` remains unchecked. Seeds at$A6:F881 and residuals$F877/$F879/$F87B plus$F8A9 still need derivation or specific impossible/nonsense evidence. The last residual is red14 instead of the ramp's15; no semantic explanation is invented for it. Counts remain31 resolved (30 converted/removed,1 mixed narrowly justified retention) and194 required.
### Batch 12: Phantoon rain-gap geometry and eye-direction dispatch

Removed the first-rain-column table: the nine rain X columns span48..208 at20-pixel spacing, and every body rain placement lies exactly on one column. Starting at the next column then wrapping eight flames leaves the gap at the body. The calculated view preserves indexing/enumeration without rebuilding a table. Removed the eye-selector array in favor of semantic directional program cases, including native unreachable code5's downward selection. Independent shot-marker choices remain required.

Verification build passed (1432 warnings, zero errors). New `--lookup-stream5-phantoon-rain` confirms eight native first columns, all eight actual rain populations with exact stagger/order/Y and the gap at the body's X, nine native eye selectors and all eight actual eye-direction handoffs, with original tables forbidden. Bounds are confirmed. It uses the existing focused producer fixture narrowed to one RNG high byte; it does not run the unrelated65536 shot-marker cases.

Thirty-three stream5 definitions resolved:32 converted/removed and1 mixed narrowly justified retention.192 remain required. Ceres palette seeds/residuals remain explicitly pending.
### Batch 13: tube-break program layouts and partial timing derivation

Removed ten shard start pointers and all90 stored visual addresses. Nine shard programs occupy36 bytes; the ninth omits reflected operands and occupies32 bytes. Their calculated view preserves every producer selection. Removed the rebuilt207-word mechanics array: shard counted flicker/movement/deletion controls share one layout, crack opening accelerates12/10/8 then holds6, its falling phase holds7 with a16-tick tail, and bubbles calculate their flying/falling holds. Five irregular final crack flicker durations at$86:D407-$D417 remain an explicit residual sequence2,3,6,9,8. No replacement mechanics/visual table is cached.

Verification build passed (1432 full-build warnings,25 incremental warnings, zero errors). `--noob-tube-projectile-instruction-mechanics` passed all207 original mechanics words, seventeen actual burst producers (one crack,ten shards,six bubbles), all counted lifecycles, shared shot deletion,90 exact native compiled sprite operands and zero runtime reads. Initial execution passed behavior and reached the obsolete expected90 visual reads versus actual0; assigned verification now confirms zero and every original operand without weakening lifetime/producer assertions.

Thirty-five stream5 definitions resolved:34 converted/removed and1 mixed narrowly justified retention.190 remain required. Only ShardPrograms and PresentationWords are checked off. Words remains required for its five timing residuals; the six placement/velocity arrays and Ceres palette residuals remain pending independently.
### Batch 14: Phantoon uniform mouth schedules and sound sequence

Removed the three uniform casual-flame pattern arrays. A calculated view retains the count header and180-tick cooldown; burst ranks1..3 contain2*rank+1 flames and16*rank holds, selected in the native middle/short/long order. Pattern3 shares header/cooldown geometry but retains its seven irregular intervals as explicitly pending data. The coordinator-assigned runtime file changes only its explicit sequence binding. Removed the three consecutive materialization sound IDs in favor of base$79 plus the bounded cycle index.

Verification build passed (1432 warnings, zero errors). New `--lookup-stream5-phantoon-schedules` invokes the existing focused schedule/sound proofs:30 original schedule words,65536 masked selector inputs,8192 frame-exact reverse-countdown/mouth transitions, all three original sound IDs and two actual callback cycles, without the original tables. These checks confirm the changed schedules; they do not grant an exception to pattern3.

Thirty-nine stream5 definitions resolved:38 converted/removed and1 mixed narrowly justified retention.186 remain required. The fourth casual-flame pattern, crack flicker intervals, scatter arrays and independent palette residuals remain pending.
### Batch 15: dead Tourian corpse configurations and sleep programs

Removed Zoomer/Ripper/Skree variant record arrays. Eight16-byte configurations share ordinal instruction/record layout. Zoomer/Ripper variants share regular64-byte work areas,18-byte DMA lists and paired callback/init routine strides; Skree uses its128-byte work areas,34-byte DMA lists and separate callback/init strides. Rotation-list selection follows species and wrap offsets derive from row tile width minus12. Named catalog bases carry original addresses and identities. Removed the eight program pointers and sixteen mechanics rows: every program is a six-byte one-tick pose followed by sleep.

Verification build passed (1432 full-build warnings;1217 after the scoped fixture update; zero errors). Both existing focused checks initially failed because their actual initializer fixtures omitted mandatory installed corpse artwork. With coordinator-assigned verifier ownership, they now receive only the exact native shared$B7:C000 sheet through RoomCharacterAtlas and the existing partial-artwork fixture catalog; its unused required palette is synthetic black. No production fallback was added. Corrected `--dead-tourian-corpse-definitions` passed16 native selectors,64 configuration words,eight derived wrap offsets and all eight actual initializers with metadata reads forbidden. `--dead-tourian-corpse-instruction-mechanics` passed16 original control words,eight actual initializer-to-sleep paths and eight native compiled visual selectors with zero reads. Every existing assertion remains intact.

Forty-four stream5 definitions resolved:43 converted/removed and1 mixed narrowly justified retention.181 remain required. Pending timing/artwork/palette exceptions are unchanged.
### Batch 16: dead Sidehopper staging and pixel-column geometry

Removed the five copy/column tables. Each silhouette has five staged tile rows of five tiles; the native atlas has sixteen tiles per row. The alternate silhouette starts nine atlas tiles right and twenty-five staged tiles later. Its top row uses two columns, while variant zero uses the complementary last three. These dimensions calculate all copy spans, planar word offsets and missing-top-row clipping without rebuilding a table. Any nonzero graphics variant preserves the existing alternate-silhouette selection.

Verification build passed (1432 warnings, zero errors). New --lookup-stream5-sidehopper-geometry compares all ten native MVN source/destination/length triples at$A9:DEC1/DF08 and all ten unrolled column operands/clips at$A9:E468/E564. Actual installed-artwork initialization matches every staged byte. Actual copy/move methods match independent native-operand reference results at pixel rows0,7,8,37,38, covering missing top tiles, tile-row wrap, final copying and clearing; the nonzero full-word variant also passes. Collection bounds pass. This is focused confirmation of the replaced geometry, with no gameplay discovery.

Forty-nine stream5 definitions resolved:48 converted/removed and1 mixed narrowly justified retention.176 remain required. No new retention exception is claimed; pending timing/artwork/palette evidence remains pending.

### Batch 17: Ceres baby control-program layout

Removed the cached43-word mechanics array and its literal-building factory. The initial program calculates two conditional four-pose groups with ten-tick holds. The expressive program calculates twelve callback/pose pairs with duration2+abs(frame-4), followed by named palette-reset and conditional/unconditional loop controls. No replacement table is built or retained; native pointers derive from these instruction widths.

Verification build passed (1432 warnings, zero errors). Existing --ceres-baby-instruction-mechanics passed all43 native control words, twenty compiled spritemap selectors, thirteen palette selectors and the complete actual production animation loop. Its existing save/load and branch assertions remain intact.

Fifty stream5 definitions resolved:49 converted/removed and1 mixed narrowly justified retention.175 remain required. No new retention exception is claimed.

### Batch 18: map-highlight origin timing and mirrored palette phases

Removed the stock duration array: the native loop-origin record holds15 ticks and each transition holds3. Independent duration edits remain exact sparse deviations. Native$82:C10C records explicitly traverse phases0..7..1; the sixteen-color rows at$82:A987 match that traversal. The catalog calculates the reverse six rows from eight retained phase rows, with exact independent resource edits. It never rebuilds or caches the full stock cycle. Custom cycles of other lengths preserve all supplied rows.

Verification build passed (1432 warnings, zero errors). New --lookup-stream5-map-highlight passes224 native colors through actual CGRAM writes, all14 native timing and phase records, each of224 independently edited colors and14 independently edited durations without propagation, custom frame counts1/15/255, bounds and zero stock deviation entries. The stock catalog retains exactly eight color rows.

Fifty-one stream5 definitions resolved:50 converted/removed and1 mixed narrowly justified retention.174 remain required. MapPaletteCycle.colors stays unchecked: the eight independent sixteen-color phases still require derivation or a specific permitted justification. The mirrored traversal alone does not settle the color payload.

### Batch 19: Morph Ball eye mount directions and beam triangle

Removed three mount tables in favor of named left/right/up/down cases with one-tile directional offsets and matching mount programs. Removed both beam channel tables: their shared intensity is8+abs(phase-8), with red/green COLDATA selector bits retained for the existing raw-byte fade. The coordinator-granted domain catalog records native$A8:90CA/90D2/90DA and$88:EA8B identities. Actual beam cycling still masks and wraps the native low nibble.

Verification build passed (1432 warnings, zero errors). New --lookup-stream5-eye-geometry passes all four native mount tuples, eight actual initializer cases confirming signed offset wrap at zero/65535 and exact programs, all sixteen native red/green/blue records, actual full-beam updates and the phase wrap, and unsupported bounds. No player-facing behavior change is intended.

Fifty-six stream5 definitions resolved:55 converted/removed and1 mixed narrowly justified retention.169 remain required. Other eye animation timing and visual payloads remain pending.

## Polyp-rock and Yapping Maw body program batch

Three definitions converted: Polyp-rock Words and Yapping Maw body Words/PresentationWords. Static pose followed by terminal sleep determines the controls; two facing programs use six-byte spacing. No replacement table is allocated. Pinned bank86 BBD5-BBD9 and EC56-EC60 confirm all six control words and three native sprite operands.

The existing focused Polyp-rock check reproduced a missing compiled selector at86:BBD7 before its first actual pose. Supported ROM bytes01 00 40 93 59 81 and pinned8D:9340 identify EnemyProjSpritemaps_LavaquakeRocks. The granted shared dispatch now delegates only this exact operand to the named catalog selector. --polyp-rock-instruction-mechanics passes both native controls, the actual producer and exact produced native sprite9340, sleep, shared deletion, rejected boundaries and allocation checks, with zero live operand reads. This separate player-facing selector correction is awaiting-player-validation for1242 when integrated.

The Yapping Maw check first confirmed its obsolete expectedtwo live reads versus actualzero. Its corrected --yapping-maw-body-projectile-instruction-mechanics passes four native controls, both exact compiled native selectors, actual both-facing producers/eight links, sleep, deletion, bounds and allocation checks with zero source reads. Runtime stores the exact PresentationOperandAddress and a blank placeholder for draw-time installed artwork; the actual-state assertion respects that documented representation rather than comparing the placeholder to a native target. No production workaround was added for the initially incorrect assertion.

Build passed1433 existing warnings/zero errors; final verifier rebuild25 warnings/zero errors. Integrated checkpoint60resolved (59converted,one mixed bridge),165required. No independent timing or artwork choice received a new exception.

## Ceres door presentation-selector batch

Removed33 stored address/selector tuples. Existing program boundaries determine visual operand addresses; semantic opening/closing phase selection reverses the four transition poses as appropriate. Both facing pose sequences derive their pointers from native OAM records: four inner parts plus four outer parts for closed/first opening, then four inner plus two outer for the other three poses. Two-byte headers and five-byte OAM entries give42/32byte widths and180bytes per facing. Named native identities select the separate elevator overlay and Mode7 wall actors. PinnedA6:F95F-FAC6 confirms that geometry; no nonuniform-gap retention argument remains. Independent part layout/pixels are separate pending artwork entries.

Build passed1433 existing warnings/zero errors. Existing --ceres-door-instruction-mechanics passed all97 native controls,33 exact native selectors and actual seven variants, both ordinary door transitions, Ridley boss handoff and wall loops with ROM reads forbidden. No verifier change or expanded gameplay confirmation was needed. Integrated checkpoint: 60 converted, one mixed, 164 required.

## Tourian entrance statue batch

Both assigned definitions are converted. Pinned bank AA D7A5-D7B9 establishes the ten-byte layout; parameter cases identify the actual statue role. The focused existing verifier first confirmed all three native control words, then exposed its obsolete missing installed-palette fixture. Installed only the extracted native Tourian palette catalog in that fixture; production has no fallback. The corrected `--tourian-entrance-statue-instruction-program-definitions` passes all three actual initializers and delete programs, rejected unused-program boundary, zero forbidden mechanics reads, and allocation confirmation. Root integration build: 1444 existing warnings, zero errors. Integrated checkpoint: 63 converted, one mixed, 161 required.

### Batch 27: Polyp stationary program and missing compiled selector

Removed Polyp's two-row mechanics array: the one-tick pose and terminal sleep derive from the four-byte frame width. Its focused actual initializer/program check exposed a missing compiled visual selector at$A2:B51C, which failed before the first pose. The supported native operand is Spritemap_Polyp$B5FB. Coordinator-granted shared-dispatch ownership adds only the narrow Polyp branch delegating to the named program selector; no opaque lookup table or unrelated dispatch changed.

Root integration Verification build passed (1444 warnings, zero errors). Corrected --polyp-instruction-mechanics passes both native control words, actual initializer-to-sleep execution and exact produced sprite pointer$B5FB against the native operand, with zero presentation reads. Existing rejected-boundary and allocation assertions remain intact. This reproduces and fixes the identified program-contract failure; no gameplay discovery was performed. The player-facing missing-selector correction awaits player validation.

Integrated checkpoint:64 converted,one mixed,160 required. Other timing/palette/artwork residuals are unchanged.

### Phantoon program structural integration

Calculated58 control positions/commands and27 visual operand addresses through named sleep,eye transition,callback,tentacle loop and mouth layouts. PresentationWords is complete. Words remains REQUIRED: eye-transition10,tentacle-pose8 and mouth-preparation5 are still independent dwell inputs,now named explicitly with native addresses. No retention exception and no claim that removing the container resolves timing.

Worker2b31939a's full control-table completion claim is not accepted. The focused verifier retains all actual program/callback,bounds,save/load and allocation assertions; its obsolete27 live visual reads expectation is corrected to zero and all27 installed selectors are independently compared with native operands.

Integrated checkpoint:65 converted,one mixed,159 required.

Root integration Verification build1444 existing warnings/zero errors. --phantoon-instruction-program-definitions passed58 native control words,all19 actual programs,four callbacks,27 exact native compiled selectors with zero presentation reads,bounds,save/load and allocation assertions.

### Pirate projectile structural integration

Calculated58 control positions and42 visual operand addresses through shared facing/program layouts. PresentationWords is complete; Words remains required because the two-tick laser startup dwell has not been derived or acceptably justified. LaserStartupFrames explicitly records this residual. No exemption or complete control-table claim.

Laser programs select immediate directional movement after three startup poses,then per-frame expansion/loop poses. Claws install directional movement and loop eight per-frame poses. Native record widths determine operand positions. Existing verifier preserves actual producers,facings,immediate movement,claw loops,shared deletion,bounds and allocation assertions; all42 selectors now compare directly to native values with zero live presentation reads.

Integrated checkpoint:66 converted,one mixed,158 required.

Root integration Verification build1444 existing warnings/zero errors; --space-pirate-projectile-instruction-mechanics passed58 native controls,both real producers/facings,immediate laser motion,claw loops,shared deletion,42 exact native selectors,zero presentation reads,bounds and allocation assertions.

## Zebetite geometry and health-program batch

Pinned A6:FC03-FC32 and FDCC-FE07 establish alternating large/split geometry and equal pose/sleep programs. Five definitions now calculate directly without stored rows. `--lookup-stream5-zebetite-geometry` invokes existing confirmation:24 generation fields, ten native health selectors, both populations, eight actual initializers,80 health handoffs and both real respawns with migrated sources forbidden. The instruction verifier first confirmed all20 native controls then failed its stale live-spritemap-read expectation. Its scoped correction compares allten compiled selectors and actual selected sprites to native operands, requires zero live reads, and preserves actual health/frame/sleep/bounds/allocation checks. `--zebetite-instruction-program-definitions` passes. Root integration Verification build1444 warnings/zero errors; both focused checks passed. Integrated checkpoint:71 converted,one mixed,153 required.

### Batch 20: independently reviewed Phantoon random-marker exception

Retain only the eight bytes06,06,08,08,06,08,06,08 at$A7:CDA5. Native$A7:DE5C-DE6A calls RNG, masks with7 and assigns the selected byte to Enemy[1].var1. The pinned source marks that eye field as never read. Managed PhantoonCollision writes eye.VariableB; the other Phantoon VariableB consumers use distinct body/mouth/tentacle slots. There is no directional or phase meaning to derive: this is the chosen partition of eight random buckets into exposed marker6 or8. Replacing it with a generic equal-probability choice changes exact RNG-to-state behavior; boolean encoding would merely restate the arbitrary bucket data. Independent coordinator review approved this narrow nonsense justification. The RNG call and state write remain unchanged.

Root integration Verification build passed (1444 existing warnings, zero errors). New --lookup-stream5-phantoon-markers checks the supported ROM SHA25612B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72, all eight native bytes and eight actual bus-free shot reactions. Exact eye markers, selected mouth patterns, one RNG call per shot and16-tick window shortening pass. This does not authorize removing exposed state or retaining any other table.

Integrated checkpoint:71 converted,one mixed,one retained,152 required. No other table receives an exemption.

### Phantoon flame structural integration

Calculated58 control positions/commands and31 visual addresses from named loops,sleep,rest,death,impact,counted fall and drop-tail layouts. PresentationWords is complete. Words remains required: five-tick flame cadence,eight-tick rain impact and four falling repetitions are explicitly named independent inputs without a retention exception.

Existing actual producer/lifetime/program/callback/deletion/bounds/allocation confirmation remains; the obsolete31 live visual reads assertion now checks zero reads and all31 exact native installed selectors. Integrated checkpoint:72 converted,one mixed,one retained,151 required.

Root integration Verification build1444 existing warnings/zero errors; --phantoon-projectile-instruction-mechanics passed58 native controls,both actual producers,every flame program,callbacks,deletion,31 exact native selectors with zero live reads,bounds and allocation checks.

### Morph Ball eye structural integration
Calculated36 visual operand addresses and46 control positions from tracking,paired closing/opening and mount record widths. PresentationWords is complete. Words remains required: eyelid cadence8/48/5,active cadence10 and activation delay32 are independent inputs without approved exceptions. Closing/opening reversal and reused closed-state hold are calculated. Integrated checkpoint73 converted,one mixed,one retained,150 required.

Root Verification build: 1444 existing warnings, zero errors. --morph-ball-eye-instruction-program-definitions passed 46 native controls, eleven actual programs, six initializer roles, 36 native selectors, zero presentation reads, bounds, save/load and allocation assertions. Native A8:8FAC-904E confirms the record layout and independent timing inputs.

### Crocomire rumble envelope integration
Calculated record offsets, signs, timing flags, terminators, approach deltas and regular buildup/decay envelope. Negative amplitudes double every two buildup cycles then halve per decay cycle; positive returns reuse approach deltas. Cooldowns rise by four from eight to sixteen, hold, then halve. The last live hold is three instead of four, and restored-prefix targets four/one remain explicit independent inputs. Definitions remains unchecked with no exception; counts unchanged. Integrated worker commits 68ffc222, 007b64d7 and 154d8fe9 for this file only.

Root Verification build: 1444 existing warnings, zero errors. --lookup-stream5-crocomire-rumble passed all 32 native words and 537 exact production frames with original source reads forbidden. Native A4:98CA-9909 independently confirms the complete sequence.

### Tourian statue palette ramp integration
Base colors 1..8 at AA:D785 interpolate exactly from RGB5 (25,31,9) to black. Grey colors 1..7 at 87:839C interpolate from (24,26,31) to (3,4,4), except color 6 green is 7 instead of 8 at 87:83A8. Both fields remain unchecked: endpoints, outside colors and this deviation have no approved exception. Custom supplied colors remain independently editable through sparse deviations; no complete replacement ramp is cached.
Root Verification build: 1445 warnings, zero errors. --lookup-stream5-statue-ramps passed all 56 native colors, 72 independent RGB edits, actual CGRAM writes, canonical identities, unchanged statue/eye colors and bounds. ResourceAudit build: zero warnings/errors; source hash refreshed. Counts unchanged.

### Zebes star composition: independent review and integration

Native 8C:975E-97F6 contains four fixed decorative compositions with 12, 6, 4 and 7 stars. Native 8B:CD83-CDA1 repeatedly displays each unchanged picture. Initializers 8B:C942-C991 place the four pictures on a two-by-two actor grid with inherited palette 4; 8B:C8B9-C941 translates whole pictures. Individual star coordinates and glyphs do not participate in the motion, completion condition, RNG or another functional calculation. The managed StepZebesActors and installed composition Draw preserve that same separation. Their irregular ordered positions and selected glyphs specify where each drawn star appears and its appearance. Replacing that composition with a new generated star distribution changes the picture; encoding its arbitrary points in a function only restates the same drawing data. This is the user's permitted nonsense exception for seemingly random visual composition, not an argument from size, complexity or performance.

Retain ONLY the 29 ordered grid-X/grid-Y/glyph triples. Calculate eight-pixel coordinate scaling, small sprite size, zero priority, no flips and inherited palette. Full-field matching preserves independently edited compositions exactly. Separate glyph pixels receive no exemption. Earlier independent reviews already resolve the other nineteen compositions; this closes the remaining portion of this inventory definition as mixed conversion/retention.

Ordered triples (glyphs hexadecimal), directly checked against the pinned native records:
Native8C:975E ordered(gridX,gridY,glyph): `(7,-5,40) (8,-3,09) (5,-1,09) (3,-4,09) (-3,7,09) (0,5,08) (-5,5,40) (0,1,AB) (-1,-2,00) (-3,-1,40) (-5,0,09) (-4,-5,00)`.

Native8C:979C ordered(gridX,gridY,glyph): `(0,8,40) (3,5,AB) (-3,1,08) (2,-1,50) (-1,-5,00) (4,-5,09)`.

Native8C:97BC ordered(gridX,gridY,glyph): `(8,-3,09) (1,-6,40) (-4,-3,09) (-5,-8,00)`.

Native8C:97D2 ordered(gridX,gridY,glyph): `(-10,0,09) (2,-1,09) (-4,-1,08) (-3,-6,08) (-1,-4,50) (4,-5,00) (4,-8,08)`.

Integrated checkpoint: 73 converted, two mixed, one retained, 149 required.

Root Verification build: 1445 warnings, zero errors. --lookup-stream5-zebes-star-fields passed 29 native parts, 261 independent field edits, exact ordered low/high OAM, calculated stock backing and canonical identity. ResourceAudit build: zero warnings/errors; interface source hash refreshed.

### KiHunter acid structural integration
Calculated 27 control positions and 19 visual addresses from shared directional introductions, movement handoffs and floor splash. Splash holds 12/10/10/8/8 descend by two every two phases. Native 86:CF34-CF8F confirms the supported layout. Introduction holds 3/3/4/3/1 remain explicit unresolved inputs; Words stays unchecked without an exception. Integrated checkpoint: 74 converted, two mixed, one retained, 148 required.

Root Verification build: 1445 warnings, zero errors. --kihunter-acid-spit-instruction-mechanics passed 27 native controls, both real directional producers, movement handoffs, sleeps, floor impact, full splash, shared deletion, 19 native selectors without ROM reads, bounds and allocation checks.

### Ceres door setup palette sharing integration
Native A6:F4FE-F509 and F51E-F529 share six setup colors (palette slots 9..14). Escape now reads those from normal colors with sparse overrides preserving independent supplied edits. Nine unique escape colors and all independent normal choices remain required. Escape stays unchecked; no exception and no completion-count change.
Root Verification build: 1445 warnings, zero errors. --lookup-stream5-ceres-door-ramp passed 78 native setup/animation colors, 90 setup RGB edits, 48 animation edits, actual CGRAM, zero stock shared-slot overrides and canonical identities. ResourceAudit build: zero warnings/errors; source hash refreshed.

### Yapping Maw structural integration
Calculated 96 control positions and 52 visual addresses from eight 22-byte directional attacks and mirrored cooldown groups. Named directional callbacks preserve diagonal-to-vertical carry behavior. Native A8:9F6F-A096 confirms record widths and ordering. Holds 5/3/80/4 remain explicit unresolved inputs; Words stays unchecked without an exception. Integrated checkpoint: 75 converted, two mixed, one retained, 147 required.

Root Verification build: 1445 warnings, zero errors. --yapping-maw-instruction-program-definitions passed all 96 native controls, fourteen actual attack/cooldown entries, seven carry/sound callbacks, 52 compiled selectors, source-read guards, bounds and allocation assertions.

### Walking Space Pirate structural integration
A stack-only layout cursor calculates 92 control positions and 50 visual addresses: two 12-byte flinches and two 130-byte facing groups comprising walk, three-shot attack and look/turn. Native B2:FB4C-FC67 and named callbacks establish widths and control ordering. Holds 16/10/24/8/32 and shot offsets +8/+2/-8 remain explicit unresolved inputs. Words stays unchecked without exemption. Integrated checkpoint: 76 converted, two mixed, one retained, 146 required.

Root Verification build: 1445 warnings, zero errors. --walking-space-pirate-instruction-mechanics passed 92 native words, eight actual programs, both three-laser attacks with offsets/velocities, all 50 installed frame selections, source-read guards, bounds and zero per-lookup allocation.

### Root integration: Ceres door quake byte decoding

The four-element offset table is removed. A neutral word followed by the native negative-four impulse yields the low/high signed bytes selected by the timer. Native A6:A2FF-A30D indexes A321 in byte increments. Full overlapping words are 0000/FC00/FFFC/FFFF; the retained ninth OAM coordinate bit agrees with signed-byte extension. Phase three consumes A325; only A326-A328 are unread by this caller. This is native byte decoding, not a newly invented oscillation.

The existing fixture now installs its native spritemap before the first actual draw. Root build passed (1445 warnings, zero errors). `--lookup-stream5-door-quake-decoding` confirms four native bytes, all 65536 timer aliases, actual four-phase drawing and editable overlay/source guards, plus 56 wrapped low/high OAM comparisons against full native words. XOffsets is complete with no retention exception. Inventory: 529 converted, 15 justified retained/mixed, 584 pending.

### Root integration: shared Ceres flight palette colors

The full stored CGRAM byte image is replaced with direct color access. Repeated lower/upper palette sections, backdrop/fill relationships and the eight-color RGB5 endpoint ramp calculate selected values. Independent edits remain exact even when they change shared source slots. Byte buffers are produced only for serialization/hash framing; runtime loads calculated colors directly to CGRAM.

`CeresFlightPalette.nativeBytes` remains unchecked: 80 independent colors remain required, with no retention exception. Root build passed (1445 warnings, zero errors). `--lookup-stream5-ceres-flight-palette` confirms all256 native colors,768 independent RGB channel edits, zero stock correction entries, actual CGRAM output, exact byte serialization, extractor round-trip and unchanged full catalog identity. Bounds and short output-buffer rejection also pass. Counts unchanged:531 converted,15 justified retained/mixed,582 pending.

### Root integration: Tourian statue projectile layouts

Timed-pose widths, the packed one-byte sound operand, callback boundaries and repeated actor loops calculate57 control positions and28 presentation addresses. Tail offsets halve8/4/2; the initial magnitude remains required. Only PresentationWords is complete. Words remains pending for eye holds8/8/8/7/7/7/6/6/5/48, splash8, particle3, tail4, soul8, decoration128/statue1911, initial tail displacement8 and particle burst4. No retention exception is granted.

The fixture now installs native eye colors before spawning actors and compares all28 exact native compiled selectors with zero live reads. Root build passed (1445 warnings, zero errors). `--tourian-statue-projectile-instruction-mechanics` confirms all57 native words, eight real actor families, tail Y movement, selectors, guards/bounds and allocation checks. Its step helper forces instruction timers; this is not elapsed-time confirmation. Inventory:532 converted,15 justified retained/mixed,581 pending.

## Integrated Ceres Mode7 transfer views

0be62c045 replaces seven cached descriptor/payload lists with calculated readonly transfer and tile views. Native packed paired-frame source extents,128-column map rows,baby tile adjacency,wing atlas runs and shared cells calculate directly without reconstructing payload buffers. Main's previously integrated BabyFrameForPhase remains intact.

Chosen platform/baby positions and glyphs,six wing regions,run origins/extents and isolated glyphs remain required. All seven aggregate entries remain pending; no new exception or completion;560converted/21retained-mixed/547pending unchanged.

Root Verification1450warnings/zero errors; --lookup-stream5-mode7-transfers passes20native descriptors,170source bytes,all seven terminators and exact low-byte-only VRAM effects/order. Existing source-byte/destination/high-byte preservation assertions remain unchanged. No ResourceAudit source-hash reference exists for the changed production file.

## Integrated semantic map-scroll buttons

a20ecf5ee completes MapScrollControls.Buttons: Left/Right/Up/Down dispatch to the matching named controller buttons. The default caller and equivalent native injection store no copied binding array; independently supplied custom bindings retain copied values. Direction iteration preserves native priority.

Root Verification1450warnings/zero errors and ResourceAudit0warnings/zero errors. --lookup-stream5-map-buttons passes four native masks/IDs,index/direction bounds,zero default storage,actual default/custom four-direction pulses,release,sound boundary and simultaneous-input priority. Audit closure includes the new dispatch dependency. One converted entry;561converted/21retained-mixed/546pending. Only1165 is in-progress.

## Integrated shared crawler instruction layout

79225b8f4 completes SharedCrawlerInstructionProgramDefinitions.PresentationWords: four surface programs derive their visual operand addresses from setup,timed-record and loop widths. Function dispatch,self-loop targets and repeated mechanics layout calculate. Words remains pending for selected pose count5 and hold3; no timing or artwork exception.

Root Verification1450warnings/zero errors; existing --shared-crawler-instruction-program-definitions passes36native words,four enemy families across four surface loops,20visual selections,function/loop publication,source-read guards and allocation checks. No direct ResourceAudit source-hash dependency exists. One converted entry;562converted/21retained-mixed/545pending.

## Integrated Phantoon black target and matched health tint

af8d5bcf0 completes PhantoonColorCatalog.fadeOut: native A7:CA41-CA60 targets RGB zero, consumed by DBB1-DBCA as the destination of its fade interpolation. Stock now stores no fade target values; independently supplied targets remain sparse overrides. Root inspected the native target and consumer.

073dd50cd calculates104non-endpoint health words by RGB5 interpolation toward saturated red with weight(band+8)/15. Sixteen healthy endpoint words,eight deviations and tint range8/15 remain required, separate from independent supplied edits. Deviations are (band,color)=(5,6),(6,6),(3,7),(5,8),(6,8),(6,9),(0,10),(0,11). No rounding exemption or health aggregate closure; power-on colors remain required.

Root Verification1450warnings/zero errors; --lookup-stream5-phantoon-fade passes16native fade targets,128health colors,exact8deviations,zero stock overrides,432independent channel edits,unchanged power colors,canonical hashes and bounds. ResourceAudit0warnings/zero errors; dependency hash and provider description updated. One converted entry;563converted/21retained-mixed/544pending.

## Integrated Phantoon collision roles

c88d48df1 converts Point,FullBody,EyeOnly,DoublePoint,PointHitboxes and EyeOnlyHitboxes. Named body modes select collision lists at the actor origin; inactive components use no-op points, and tentacle frames emit their two native inert components. Eye-only mode reuses the exact eye rectangle from the full-body list. Native references: A7:DEDD-DFFD frame roots,E020point,E02Ebody,E03Ceye component,E06Ceye-only copy.

FullBodyHitboxes remains pending with all five independent rectangles under RequiredBodyBounds. Shared callbacks and eye geometry do not exempt those shapes. Runtime collision/callback code is unchanged.

Root Verification1450warnings/zero errors; --lookup-stream5-phantoon-collision passes22native frames,25components,all seven rectangles/callback pairs,actual full-body touch/shot and eye-only inclusion/exclusion,enumeration and bounds. The granted existing verifier adaptation changes only explicit span bindings. No direct ResourceAudit source-hash reference exists. Six converted entries;569converted/21retained-mixed/538pending.

## Integrated Rinka program layout

789c838a7 completes RinkaInstructionProgramDefinitions.PresentationWords through native setup/timed-record/loop widths. Semantic setup dispatch and returning visible stages calculate repeated mechanics and mirrored pulse holds. Words remains pending for hidden64,seed16,minimum pulse5 and cycle count8; no magnitude or animation exemption.

Root Verification1450warnings/zero errors; existing --rinka-instruction-mechanics passes26native words,18selectors,both actual ordinary/special programs,visibility/offscreen/aim-delay behavior,source guards,bounds and allocation checks. No direct ResourceAudit hash dependency exists. One converted entry;570converted/21retained-mixed/537pending.

## Narrow Phantoon random-bucket ordering review

Root inspected exact native CD41/CD53/CD63 values and consumers D060-D06C,D07C-D088,D5A6-D5B2,D7E7-D7F3. Only the exact bucket ordering is approved authored random-choice content: indices are RNG&7 or first-round(NMI>>1)&3, not a progression through physical or animation phases. Fitting a numerical permutation would merely encode the same selected random policy. This does not exempt duration magnitudes,scale/ratios or timer semantics, and does not close any timer aggregate. Worker implementation and focused confirmation remain queued.

## Integrated Phantoon channel sharing and timer decomposition

Partial color conversion550706e4f,c31b70f91,72cb71cf7: sixteen healthy green channels share red/zero by body/eye role; eight blue channels use clamped subtractive yellow. Only nine differing channels remain from eight exceptional tint words. Twenty-four healthy channel inputs,nine tint deviations,grouping,separation7 and tint8/15 remain REQUIRED. Independent supplied edits stay exact; no artwork exemption.

Partial timer conversion2b3803c2c separates shared duration scales from the exact CD41/CD53/CD63 random-bucket order. Only that opaque selection order has the previously reviewed nonsense justification; quantum15,scales4/2/6 and doubling2 remain REQUIRED. All three timer aggregates stay unchecked.

Root build1,450warnings/0errors;144 native color words,432 independent channel edits,exact residual membership,zero stock overrides,hashes and bounds pass. Timer confirmation compares24native words,196608 actual RNG selections and256 NMI values,including timer writes,phase handoffs and RNG consumption without a cartridge bus. ResourceAudit0/0. Inventory unchanged570converted/21retained-mixed/537pending.

## Integrated Phantoon collision reflection

Partial147760ba0 derives centered body/eye/center X edges as left=-right-1 and reflects the side-tentacle interval around the same pixel center. Five horizontal extents and ten vertical edges remain REQUIRED. Root identified this remaining exact relationship during exception review; no collision-shape exception is granted and FullBodyHitboxes remains unchecked.

Root build1450warnings/0errors;22native frames,25components,seven rectangles/callbacks,actual full-body/eye selection and bounds pass. Inventory573converted/21retained-mixed/534pending unchanged.

## Integrated Wrecked Ship power-on shade ramps

Partial34245036f calculates ten darker RGB5 colors from five selected three-shade runs in BG palettes4/5. Root inspected native A7:DC71-DC8A fade-target consumer and source-art export using actual area tile-table C1C5CF. Row4 slots1-3,4-6,8-10 and row5 slots4-6,8-10 subtract6 per channel; row5 slots1-3 are not a ramp and remain required. All102 starting/unrelated colors,step6 and membership remain REQUIRED; powerOn stays unchecked. Independent supplied edits retain their exact values and canonical hashes.

Root build1450warnings/0errors;256native colors,768independent channel edits,exact residual membership,zero stock overrides and bounds/hash pass. ResourceAudit0/0; contract dependency and description updated. Inventory573converted/21retained-mixed/534pending unchanged.

## Integrated semantic map landmark catalogs

Conversionaee36f9a3 completes six boss-slot and six elevator-destination arrays via named area/boss/connection cases and generated stable anchor IDs. Crateria retains three empty boss slots; Tourian has no boss slots and Ceres has no elevator list. Native82:C759-C7CA destination records and C83B/C89D/C90B/C981/C9DB/CA9B boss lists establish exact ordering. Coordinates,glyphs and artwork remain independent required payloads.

Root build1450warnings/0errors;8native boss slots,17destinations,extraction/schema,23ordered identities,bounds/enumeration and56actual stock/edited ordered OAM/state cases pass. Inventory586converted/21retained-mixed/521pending. No artwork or coordinate exception.


### Integrated Ceres source catalog cases (d1cef8ec3)

Two descriptor arrays are replaced by named WarningText/Doors and Emergency/JapaneseFirst/Second/Third/Fourth cases. Root confirmed exact identities, ordered enumeration, range admission and bounds; the existing native DMA check passes 19 records, three terminators, both timer lists and Japanese overlay with descriptor reads blocked. Verification build 1452 warnings/0 errors, corrected command wiring incremental build 25/0; ResourceAudit 0/0. An initial missing CLI branch fell through to the unrelated default suite and is not confirmation evidence. Overlay words and character pixels remain required; no new retention. Master 595 converted /21 retained-mixed /512 pending.


### Integrated Crocomire shared paint (3b42ed044)

Native A4:B8BD wall slots2..6 repeat one color;B8FD skeleton slots2..6 repeat7..11 and share body1/7;two17-word initial transfers overlap next palette startsB8DD/B8FD. Thirteen repeated words derive from61required paint inputs with independently supplied differences. Root inspected pinned source and passed74native colors,222independent channel edits,exact61input basis/zero stock overrides,actual five-band CGRAM,original hash framing and bounds. Verification1452warnings/0errors;Audit0/0. All paint choices/group membership remain required;five entries unchecked;master595/21/512 unchanged.


### Integrated Phantoon role selectors (3db6e0324)

Removed27BankA7 literal selectors in favor of named body modes,opening/closing eyelids,eight compass gazes,mirrored tentacle cycle and mouth pose dispatch. Shared selector count/enumeration/address dispatch include the calculated family;other families and existing unresolved timing constants are preserved. Root inspected native A7:CC41-CCFB;Verification1452warnings/0errors. Focused instruction check passes58mechanics words,19reachable programs,four callbacks,27native selectors with no presentation reads. Shared catalog confirmation passes5069native identities,sorting and unknown-key rejection. Existing verifier commands also execute their preamble checks;no extra defect search was performed. Pose/timing/art requirements remain open;master595/21/512 unchanged. No direct ResourceAudit dependency.


### Integrated slope-family speed dispatch (9503b01ec)

Thirty-two stored multipliers now select seven coefficients through the native square/flat,descending stair,unit/half/third/double/triple-rise families. Root cross-checked SlopeHeightDefinitions and native94:8526/8573 consumer. Existing signed multiplication,orientation and airborne admission remain unchanged. Seven chosen coefficients remain REQUIRED;no trigonometric origin or retention exemption claimed. Root1452warnings/0errors;32native multipliers,8388608signed byte-packed operands,26112BTS/airborne cases and shape bounds pass without ROM reads. No direct audit source dependency. Master595/21/512 unchanged.


### Whole Ceres warning overlay composition review

Selected original entry: CeresEscapeOverlayTilemapCatalog.pages, all55 words. Native A6:C164 spells EMERGENCY. C3F4-C44E spells 自爆装置が、作動しました / ただちに脱出して下さい in two eight-pixel halves per line. B7:DA00 source glyphs, rendered in csharp/test-temp/lookup-phantoon-source/ceres-japanese-native-glyphs.png, establish the twenty-character atlas order 自爆装置が、作動まただちに脱出して下さい; source glyph6 is 作, not the disassembly comment's 発. The exact sentence and font order are chosen language/display content; changing them or inventing a generator without those choices would change the warning's text or select different glyphs. Glyph pixel data remain separately required under CeresEscapeTileArtwork; this proposal does not exempt them.

The remaining geometry now calculates: Latin first tile equals authoritative WarningBackgroundDestination1820 divided by16VRAM words per4bpp8x8tile; the twenty-six-letter alphabet ends at19B; remaining upper Japanese halves fill19C..19F; the main upper row aligns to the next sixteen-tile source-atlas row1A0; lower halves follow at1B0. Page extents are text lengths, and contiguous Japanese source addresses derive from previous row extents. The explicit sixteen-tile packing and two-half character design are source layout choices, not a claim that hardware requires this font layout. English BG palette6 derives from existing AlarmCgramIndex97 /16. Native A6:C19C alarm writes97..99 and therefore colors this text; no independent English palette number remains. Selected priority and Japanese BG palette7 remain exact typography roles. No cached table is regenerated: transient bytes serve only the existing DMA/hash contract, with stock overrides empty.

Consumers: A6:C136-C164 enqueues eighteen bytes to50CB after the graphics uploads; C3B8-C3D4 enqueues Japanese upper/lower rows to528A/52AA/52CA/52EA; C0F5-C104 selects the Japanese overlay based on language, independently of text artwork. Managed QueueCeresEmergencyText and QueueCeresTransferList preserve those queues and installed resolver checks. Every independently supplied tile/style word is preserved without altering neighbors or native source/hash order.

Confirmation after the final geometry change: build1431warnings/zeroerrors, final verifier-only25/zero. Early --lookup-stream5-ceres-overlay-complete passes55 native words,55 independent full-word edits,zero stock overrides,original hashes/bounds,56 actual Emergency producer and installed-transfer-resolution calls,19native transfer records/three terminators and unchanged installed timer/Japanese queue assertions. No gameplay discovery. Coordinator independently viewed the native glyph sheet and approved only the specific wording/font ordering/two-half packing/priority/Japanese-palette choices above as categorical typography content. The original pages entry is mixed complete, with no glyph pixel or RGB exemption.

Integration: root still has the pre-glyph overlay implementation, so use the complete owned CeresEscapeOverlayTilemapCatalog.cs (including prior glyph/sparse-edit implementation) and only WarningBackgroundDestination private-to-internal in CeresEscapeVramTransferDefinitions.cs. Append scoped worker checks and early flag, preserving root Program.cs. Both EnemyArtworkClosedContractDefinitions and CinematicClosedContractDefinitions hash the overlay; add CeresEscapeVramTransferDefinitions and CeresRidleyPaletteRomData to its transitive source dependencies. Native endpoint/color paint remains outside this entry.

Root integration: Root build1452 warnings/0 errors;ResourceAudit0/0. All55 native words,55 independent edits,zero stock overrides,hashes/bounds,56 actual Emergency queue/resolver calls,19 transfer records/three terminators and installed Japanese queue passed. Master 600 converted/61 retained-mixed/467 pending.


### Whole Phantoon figure-eight exposure review

Selected original entry: PhantoonTimerDefinitions.VulnerableWindow only. A7:CD41 eight buckets select60/30/15/30/60/30/15/60 calls. The existing coordinator-approved bucket ordering is an arbitrary RNG assignment, separately retained. The remaining independent timing policy is the shortest fifteen-call exposure and medium/long doubling tiers. A7:D03F-D075 independently installs eye-only body hitbox, clears collision-ignore, centers eye, consumes one RNG word and copies the chosen countdown; the eye-opening sprite animation has already reached this callback. A7:D60D-D65B decrements once per actor call while aiming the eye at Samus. It does not integrate movement, evaluate sprite geometry or derive another state quantity from the timer. At expiry it clears accumulated round damage and either closes the eye/installs invulnerable body/flags rain for the next round, or consumes a shot trigger and selects the separately timed swoop. The latter's60-call setup atD61F is a separate existing scalar, not an exemption or generating owner for these windows.

Approved narrow disposition: shortest15 and exact doubling exposure tiers are selected vulnerability/challenge timing. Changing them changes the player's available hit opportunity even with identical pose/position/shot inputs; a generator without those design choices has no different functional measurement to recover the intended durations. Medium30/long60 calculate from the shared shortest value; no three-magnitude table remains. This does not exempt EyeClosed/RainHiding magnitudes/scales, swoop timing, any other enemy cadence or gameplay parameter.

Confirmation adds only the identified countdown contract to the owned worker verifier. Three native durations and both hit/no-hit expiry branches execute210 actual RunPhantoonEyeTracking calls, asserting every decrement/no-early transition, collision eligibility, unchanged body position, retained damage before expiry, exact damage clear, eye/body program handoff or consumed swoop trigger and original separately loaded hold. Existing focused timer confirmation passes24 native words,196608 RNG selector calls and256 initial NMI buckets without a cartridge bus. First compile rejected a verifier-only nonexistent ushort.Has helper; corrected to direct flag masks before execution. Final verifier build25warnings/zeroerrors and --lookup-stream5-phantoon-timers passes. Coordinator independently readD03F-D075/D60D-D65B and approved only the shortest15-call opportunity, doubling tiers and previously reviewed RNG ordering. VulnerableWindow is mixed complete; no other timer aggregate closes.
Final named exposure-owner build1191warnings/zeroerrors; focused timers and210countdown calls passed again. No ResourceAudit source-hash entry directly references PhantoonTimerDefinitions.cs in current root.

Root integration: Root build1452 warnings/0 errors. After build completion,24 native words,196608 RNG selector calls,256 initial NMI values and210 actual countdown calls passed,including collision eligibility,no early transition,damage clear and exact hit/no-hit handoffs. Master 600 converted/62 retained-mixed/466 pending.


### Complete Ceres light/dark platform glyph strips

CeresDoorVisualCatalog.mode7DoorFrames is mixed complete after independent coordinator native-art and consumer review. Native A6:F918/F91C contains light68/69/69/78 and dark8D/8E/8E/79. The source light/dark strips are cap/interior/interior/cap, with two identical interior tiles per32px strip. Canonical CeresMode7TransferDefinitions.PlatformTile now owns both editable catalog defaults and compiled native transfer glyphs, eliminating duplicated stock anchor storage. Each interior glyph derives from the corresponding selected left cap+1. Light right cap derives from light left+16 (same source-atlas column, following row); dark right cap derives from light right+1 (adjacent phase glyph). Only selected left identities68/8D, exact four-cell topology and phase membership are retained as the specific drawn strip composition. Choosing different endpoints/topology would draw different imagery, not regenerate this content.

Evidence: native C0:E22A decompresses the interleaved map/character allocation; character pixels occupy high bytes, with C2:C104 palette. csharp/test-temp/lookup-phantoon-source/ceres-platform-native-strips.png shows the two selected strips, while ceres-platform-native-atlas.png shows all256 glyphs arranged in the source's coherent sixteen-column packing. Continuous multirow room/pipeline artwork establishes that source geometry beyond these cap values;16 is not presented as a Mode7 hardware requirement. Caps68/78 occupy column8, rows6/7. Coordinator viewed both images and approved the two-anchor reduction. Pixel content, RGB paint, timing and map placement receive no exemption.

Consumer A6:F8F1-F8FF selects phase by NMI&2 and enqueues F904/F90E. Both descriptors transfer four low map bytes to060E; independent high character bytes must survive. Managed RunCeresDoorPaletteAnimation calls the selected editable catalog with matching frame bits, including after arrival projectiles are gone. The constructor retains only independently supplied cell differences, with zero stock overrides. Hashing and DMA use direct canonical operations plus sparse edits and transient transfer buffers; no stock lookup is rebuilt or cached.

Verification: full implementation build1431warnings/zeroerrors, verifier increment25/zero, final cap-row refinement1191/zero. --lookup-stream5-ceres-door-ramp confirms all8 native cells/eight full-byte independent edits, canonical hashes, frame bounds and99 actual actor animation calls (stock plus each edited document, two full four-tick periods and FFFE/FFFF/0000 wrap). Every call checks exact selected map bytes, untouched high character bytes and both neighboring VRAM words. Existing48-color/144-animation-channel/90-setup-channel assertions remain unchanged and pass; those color entries remain required. No gameplay discovery.

Do not close CeresMode7TransferDefinitions.PlatformLight/PlatformDark from this payload approval: their selected map placement(14,12) remains required. Preserve root's additional BabyFrameForPhase helper when applying the narrow CeresMode7TransferDefinitions diff. Integrate the owned catalog platform changes and scoped worker confirmation, keeping any parent changes elsewhere. CinematicClosedContractDefinitions hashes CeresDoorVisualCatalog; refresh it and include CeresMode7TransferDefinitions/CeresDoorVisualRomData in its supporting source closure.

Root integration: Root narrowed main-based candidate preserves existing color work,BabyFrameForPhase and null validation. Build1452 warnings/0 errors;ResourceAudit0/0;eight native cells/eight edits,99 actual actor calls through wrap,high-byte/neighbor preservation,hashes and domains passed. Master 600 converted/63 retained-mixed/465 pending.


### Complete Phantoon closed-eye moving-phase timing

PhantoonTimerDefinitions.EyeClosed is mixed complete after independent coordinator source review. Native CD53 chooses720/60/360/720/360/60/360/720; the already approved random/NMI bucket ordering is preserved. Short60 derives from shared fifteen-call exposure unit times4, medium360 is six short intervals, long720 doubles medium. Those exact relative phase lengths are selected attack pacing, not an inferred physical traversal period.

A7:D07C-D088 selects a later-round bucket with RNG&7; D5A6-D5B2 selects the initial bucket with(NMI>>1)&3, independently of direction initialization. D5E7-D60C updates figure-eight movement and casual flames, then unconditionally decrements the closed-eye timer. On zero/negative it opens the eye at the current path position, clears the rain flag and spawns the spiral. There is no cursor/position/speed completion predicate; trying to infer these durations from the movement table would change the intended opportunity schedule. Coordinator approved only4/6/2 ratios and the reviewed bucket policy. Movement samples, flame cadence, RainHiding timing and separate swoop holds remain required.

Focused proof adds VerifyLookupStream5PhantoonClosedEye to the existing early --lookup-stream5-phantoon-timers flag. It selects each native60/360/720 duration through the real first-round selector, in each independently selected direction; calls the actual moving-phase routine2280times; confirms every timer decrement/no-early opening; then exact EyeOpen instruction/timer, cleared rain flag and48actual spiral flame spawns at the current body coordinates. A valid unexpired casual-flame hold isolates this timer contract while still asserting that the mouth timer advances each call. No bus is installed, no replay/room search or exploratory testing. Build1432warnings/zeroerrors and all focused checks pass, alongside unchanged24native/196608RNG/256NMI and210vulnerability-expiry confirmations. The code-only update after that build changes documentation/disposition, not values or control behavior.

Root integration: Root build1452 warnings/0 errors;2280 actual closed-eye countdown calls at60/360/720 in both directions,exact EyeOpen handoff/rain reset and48 actual spiral spawns passed,along with existing native selector/exposure checks. Master 600 converted/64 retained-mixed/464 pending.


### Complete Phantoon subsequent-rain hidden delay

PhantoonTimerDefinitions.RainHiding is mixed complete. CD63 chooses60/120/30/60/30/60/30/30; shortest30 derives from the shared fifteen-call unit times2, medium/long double each prior tier. Existing reviewed random bucket order is unchanged. Coordinator independently read A7:D7D5-D829 and approved only the selected short-scale2/doubling wait policy: it determines how long the boss remains hidden before the next rain attack, not physical travel or fade progression.

The fade-complete flag gates the first RNG call and countdown assignment atD7E7-D7F3. The hidden phaseD7F7-D829 only decrements and waits. Only upon expiry does a second independent RNG call select the next path cursor/position and spawn rain. There is therefore no next-position distance or fade duration available from which to generate this chosen pause. Native initial rain is a different moving/fading state; its duration, the fade denominator, placement/cursor lookup and per-projectile delays are not exempted by this review.

Guarded --lookup-stream5-phantoon-timers now includes VerifyLookupStream5PhantoonRainWait. Three native30/60/120 waits execute210actual countdown calls. An incomplete fade does not consume RNG or replace the timer; a completed fade selects the exact native wait; before expiry no second RNG, cursor/position change or rain spawn occurs. Expiry performs one separate RNG draw, installs exact nativeCDAD position/cursor for the selected pattern, clears the fade counter, hands off to appearance and launches24real rain projectiles over the three cases. Build1432warnings/zeroerrors and the complete focused timer command pass. Prior210vulnerable and2280closed-eye calls plus24native/196608RNG/256NMI assertions remain passing. Only this three-entry timer family is now complete; no broader combat exception is claimed. Final production changes after confirmation are XML/comments only, with no value/control changes.

Root integration: Root build1452 warnings/0 errors;210 actual rain-wait calls,fade-gated duration selection,no premature RNG/spawns/position changes,exact delayed placement/cursor and24 actual rain projectiles passed,alongside existing timer profiles. Master 600 converted/65 retained-mixed/463 pending.


### Complete Ceres door discrete destruction anchors

RoomEnemySystem.CeresDoorRumbleOffsets is mixed complete after coordinator review of pinned A6:F7DC-F84F and the native footprint diagram. New Game/CeresDoorRumbleGeometryDefinitions.cs calculates the horizontal coordinates as two staggered pairs: shared column step 2, pair width twice that step, second pair shifted one step, initial left column minus two steps. Retained only the selected four-column/interleaved formation and step 2, and vertical placements -8/4/22/12. These are discrete destruction composition choices: four separately spawned effects, not samples of a trajectory. The old unsupported irregularity/arithmetic-progression retention claim was deleted.

Native A1:E95C installs variant 2 at (232,631); its A6:F921 twelve-part overlay contains the impact anchors within the left column. The anchors do not select sprite centers or collision edges. Native F7FE-F820 decrements/wraps the index and adds actor coordinates, then RNG selects the effect animation; 86:E468-E47D copies supplied X/Y directly. Changing the remaining selected impact locations would invent different destruction composition. The exact bounded approval does not cover rumble duration, interval, sound, RNG threshold, overlay geometry/pixels or physical behavior. No replacement XY table or cache is built.

Source diagram: csharp/test-temp/lookup-phantoon-source/ceres-rumble-native-geometry.png (ignored; direct native OAM footprints and anchor coordinates). Guarded source exporter completed successfully. Focused --lookup-stream5-ceres-rumble confirms SHA-verified eight native words and rejected bounds, then 98 actual rumble calls and 20 independent emissions at both native actor origin and a wrapping origin: exact reverse cyclic positions, both RNG animation branches, one RNG call per emission, no intermediate allocation, sound, hide and rotation handoff. Build 1217 warnings / zero errors and focused proof pass. Initial verifier compile used an unavailable Rom property on the abstract bus; corrected the SHA assertion to the actual local cartridge bytes before confirmation. No production workaround or gameplay search. No direct ResourceAudit source-hash reference to RoomEnemySystem.CeresDoor.cs found in current main; new helper is the runtime's new transitive source dependency.

Root integration: Root reviewed native A6:F7DC-F84F and variant2 footprint diagram. Build1452warnings/zeroerrors; fresh terminal-success rebuild and focused proof pass eight native words, bounds,98actual calls,20emissions at native/wrapping origins, reverse order, both animations and exact expiry. Only discrete formation/step2/vertical placements retained. Master 600 converted/68 retained-mixed/460 pending.
