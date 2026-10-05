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

- [ ] **CeresDestructionSpritePresentation.frames** ([L9](../csharp/src/SuperMetroid.Core/Assets/CeresDestructionSpritePresentation.cs#L9)) - installed stock table. Remaining four Zebes star sheets only: upper-left 8C:975E (12 parts), upper-right 979C (6), lower-left 97BC (4), lower-right 97D2 (7). The other nineteen layouts have specific completed geometry/anchor/atlas dispositions. Pixel data remains separately listed.

### csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs

- [ ] **CeresDoorVisualCatalog.normal** ([L24](../csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs#L24)) - installed stock table. Original/default payload behind CeresDoorVisualCatalog.normal. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresDoorVisualCatalog.escape** ([L25](../csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs#L25)) - installed stock table. Original/default payload behind CeresDoorVisualCatalog.escape. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresDoorVisualCatalog.animation** ([L26](../csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs#L26)) - installed stock table. Original/default payload behind CeresDoorVisualCatalog.animation. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresDoorVisualCatalog.mode7DoorFrames** ([L27](../csharp/src/SuperMetroid.Core/Assets/CeresDoorVisualCatalog.cs#L27)) - installed stock table. Original/default payload behind CeresDoorVisualCatalog.mode7DoorFrames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs

- [ ] **CeresEscapeOverlayTilemapDefinitions.Definitions** ([L41](../csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresEscapeOverlayTilemapCatalog.pages** ([L78](../csharp/src/SuperMetroid.Core/Assets/CeresEscapeOverlayTilemapCatalog.cs#L78)) - installed stock table. Original/default payload behind CeresEscapeOverlayTilemapCatalog.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs

- [ ] **CeresEscapeTileArtworkDefinitions.Pages** ([L21](../csharp/src/SuperMetroid.Core/Assets/CeresEscapeTileArtwork.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [ ] **CeresDoorInstructionProgramDefinitions.PresentationWords** ([L269](../csharp/src/SuperMetroid.Core/Game/CeresDoorInstructionProgramDefinitions.cs#L269)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresDoorQuakeDefinitions.cs

- [ ] **CeresDoorQuakeDefinitions.XOffsets** ([L14](../csharp/src/SuperMetroid.Core/Game/CeresDoorQuakeDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **RoomEnemySystem.CeresDoorRumbleOffsets** ([L34](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresDoor.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [ ] **KiHunterAcidSpitInstructionProgramDefinitions.PresentationWords** ([L55](../csharp/src/SuperMetroid.Core/Game/KiHunterAcidSpitInstructionProgramDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **MapLandmarkDefinitions.crateriaBosses** ([L12](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.brinstarBosses** ([L14](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.norfairBosses** ([L16](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.wreckedShipBosses** ([L18](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.maridiaBosses** ([L20](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.ceresBosses** ([L22](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.crateriaElevators** ([L28](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.brinstarElevators** ([L32](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.norfairElevators** ([L35](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.wreckedShipElevators** ([L37](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.maridiaElevators** ([L39](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapLandmarkDefinitions.tourianElevators** ([L41](../csharp/src/SuperMetroid.Core/Frontend/MapLandmarkDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs

- [ ] **MapScrollControls.Buttons** ([L16](../csharp/src/SuperMetroid.Core/Frontend/MapScrollControls.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs

- [ ] **MapStationDiscoveryRules.missile** ([L16](../csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapStationDiscoveryRules.energy** ([L22](../csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MapStationDiscoveryRules.map** ([L33](../csharp/src/SuperMetroid.Core/Frontend/MapStationDiscoveryRules.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MorphBallEyeVisualDefinitions.cs

- [ ] **MorphBallEyeVisualDefinitions.Frames / literal at L15** ([L15](../csharp/src/SuperMetroid.Core/Assets/MorphBallEyeVisualDefinitions.cs#L15)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/MorphBallEyeInstructionProgramDefinitions.cs

- [ ] **MorphBallEyeInstructionProgramDefinitions.Words** ([L41](../csharp/src/SuperMetroid.Core/Game/MorphBallEyeInstructionProgramDefinitions.cs#L41)) - factory-built stock table. Stored MorphBallEyeInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **MorphBallEyeInstructionProgramDefinitions.PresentationWords** ([L43](../csharp/src/SuperMetroid.Core/Game/MorphBallEyeInstructionProgramDefinitions.cs#L43)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

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
- [ ] **PhantoonColorCatalog.fadeOut** ([L22](../csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs#L22)) - installed stock table. Original/default payload behind PhantoonColorCatalog.fadeOut. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PhantoonColorCatalog.powerOn** ([L23](../csharp/src/SuperMetroid.Core/Assets/PhantoonColorCatalog.cs#L23)) - installed stock table. Original/default payload behind PhantoonColorCatalog.powerOn. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs

- [x] **PhantoonCasualFlameDefinitions.Pattern / literal at L9** ([L9](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L9)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **PhantoonCasualFlameDefinitions.Pattern / literal at L10** ([L10](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L10)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **PhantoonCasualFlameDefinitions.Pattern / literal at L11** ([L11](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L11)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **PhantoonCasualFlameDefinitions.Pattern / literal at L12** ([L12](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L12)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs

- [ ] **PhantoonCollisionDefinitions.Point** ([L30](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonCollisionDefinitions.FullBody** ([L31](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonCollisionDefinitions.EyeOnly** ([L32](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonCollisionDefinitions.DoublePoint** ([L34](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonCollisionDefinitions.PointHitboxes** ([L37](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonCollisionDefinitions.FullBodyHitboxes** ([L40](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonCollisionDefinitions.EyeOnlyHitboxes** ([L48](../csharp/src/SuperMetroid.Core/Game/PhantoonCollisionDefinitions.cs#L48)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonInstructionProgramDefinitions.cs

- [ ] **PhantoonInstructionProgramDefinitions.Words** ([L57](../csharp/src/SuperMetroid.Core/Game/PhantoonInstructionProgramDefinitions.cs#L57)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonInstructionProgramDefinitions.PresentationWords** ([L95](../csharp/src/SuperMetroid.Core/Game/PhantoonInstructionProgramDefinitions.cs#L95)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs

- [x] **PhantoonPatternDefinitions.EyeInstructions** ([L11](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PhantoonPatternDefinitions.FirstRainColumns** ([L37](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonPatternDefinitions.ShotEyeMarkers** ([L40](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs

- [ ] **PhantoonProjectileInstructionProgramDefinitions.Words** ([L45](../csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonProjectileInstructionProgramDefinitions.PresentationWords** ([L107](../csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs#L107)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonSoundDefinitions.cs

- [x] **PhantoonSoundDefinitions.Materialization** ([L10](../csharp/src/SuperMetroid.Core/Game/PhantoonSoundDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs

- [ ] **PhantoonTimerDefinitions.VulnerableWindow** ([L9](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonTimerDefinitions.EyeClosed** ([L11](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonTimerDefinitions.RainHiding** ([L13](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PolypInstructionProgramDefinitions.cs

- [x] **PolypInstructionProgramDefinitions.Words** ([L15](../csharp/src/SuperMetroid.Core/Game/PolypInstructionProgramDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PolypRockInstructionProgramDefinitions.cs

- [x] **PolypRockInstructionProgramDefinitions.Words** - Single-pose/sleep controls and visual operand locations calculate from program widths; see Polyp-rock/Yapping Maw batch below.

### csharp/src/SuperMetroid.Core/Game/RinkaInstructionProgramDefinitions.cs

- [ ] **RinkaInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/RinkaInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RinkaInstructionProgramDefinitions.PresentationWords** ([L40](../csharp/src/SuperMetroid.Core/Game/RinkaInstructionProgramDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Rinka.cs

- [ ] **RoomEnemySystem.RinkaSpawnResources** ([L86](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Rinka.cs#L86)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SharedCrawlerInstructionProgramDefinitions.cs

- [ ] **SharedCrawlerInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/SharedCrawlerInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SharedCrawlerInstructionProgramDefinitions.PresentationWords** ([L54](../csharp/src/SuperMetroid.Core/Game/SharedCrawlerInstructionProgramDefinitions.cs#L54)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [ ] **TourianStatueProjectileInstructionProgramDefinitions.PresentationWords** ([L75](../csharp/src/SuperMetroid.Core/Game/TourianStatueProjectileInstructionProgramDefinitions.cs#L75)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianStatueUnlockDefinitions.cs

- [ ] **TourianStatueUnlockDefinitions.EyePositions** ([L15](../csharp/src/SuperMetroid.Core/Game/TourianStatueUnlockDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs

- [ ] **WalkingSpacePirateInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **WalkingSpacePirateInstructionProgramDefinitions.PresentationWords** ([L72](../csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs#L72)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [ ] **YappingMawInstructionProgramDefinitions.PresentationWords** ([L49](../csharp/src/SuperMetroid.Core/Game/YappingMawInstructionProgramDefinitions.cs#L49)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

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
