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

- [ ] **CeresDestructionActorLayout.placements** ([L12](../csharp/src/SuperMetroid.Core/Assets/CeresDestructionActorLayout.cs#L12)) - installed stock table. Original/default payload behind CeresDestructionActorLayout.placements. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

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

- [ ] **CeresFlightActorLayout.placements** ([L12](../csharp/src/SuperMetroid.Core/Assets/CeresFlightActorLayout.cs#L12)) - installed stock table. Original/default payload behind CeresFlightActorLayout.placements. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs

- [ ] **CeresFlightArtworkCatalog.Mode7Characters** ([L24](../csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs#L24)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.
- [ ] **CeresFlightArtworkCatalog.Mode7Maps** ([L26](../csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs#L26)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.
- [ ] **CeresFlightArtworkCatalog.ObjectCharacters** ([L27](../csharp/src/SuperMetroid.Core/Assets/CeresFlightArtworkCatalog.cs#L27)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.

### csharp/src/SuperMetroid.Core/Assets/CeresFlightPalette.cs

- [ ] **CeresFlightPalette.nativeBytes** ([L10](../csharp/src/SuperMetroid.Core/Assets/CeresFlightPalette.cs#L10)) - installed stock table. Original/default payload behind CeresFlightPalette.nativeBytes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresRevealActorLayout.cs

- [ ] **CeresRevealActorLayout.placements** ([L13](../csharp/src/SuperMetroid.Core/Assets/CeresRevealActorLayout.cs#L13)) - installed stock table. Original/default payload behind CeresRevealActorLayout.placements. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/CeresBabyInstructionProgramDefinitions.cs

- [ ] **CeresBabyInstructionProgramDefinitions.Words** ([L49](../csharp/src/SuperMetroid.Core/Game/CeresBabyInstructionProgramDefinitions.cs#L49)) - factory-built stock table. Stored CeresBabyInstructionMechanicsWord[] initialized by CreateWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/CeresCinematicLightPaletteFxProgramMechanicsDefinitions.cs

- [ ] **CeresCinematicLightPaletteFxProgramMechanicsDefinitions.All** ([L103](../csharp/src/SuperMetroid.Core/Game/CeresCinematicLightPaletteFxProgramMechanicsDefinitions.cs#L103)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresDoorInitializationDefinitions.cs

- [x] **CeresDoorInitializationDefinitions.Definitions** ([L84](../csharp/src/SuperMetroid.Core/Game/CeresDoorInitializationDefinitions.cs#L84)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresDoorInstructionProgramDefinitions.cs

- [ ] **CeresDoorInstructionProgramDefinitions.Words** ([L156](../csharp/src/SuperMetroid.Core/Game/CeresDoorInstructionProgramDefinitions.cs#L156)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresDoorInstructionProgramDefinitions.PresentationWords** ([L269](../csharp/src/SuperMetroid.Core/Game/CeresDoorInstructionProgramDefinitions.cs#L269)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresDoorQuakeDefinitions.cs

- [ ] **CeresDoorQuakeDefinitions.XOffsets** ([L14](../csharp/src/SuperMetroid.Core/Game/CeresDoorQuakeDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs

- [x] **CeresEscapeVramTransferDefinitions.Records** ([L30](../csharp/src/SuperMetroid.Core/Game/CeresEscapeVramTransferDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresFallingDebrisInstructionProgramDefinitions.cs

- [ ] **CeresFallingDebrisInstructionProgramDefinitions.Words** ([L34](../csharp/src/SuperMetroid.Core/Game/CeresFallingDebrisInstructionProgramDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresFallingDebrisInstructionProgramDefinitions.PresentationWords** ([L53](../csharp/src/SuperMetroid.Core/Game/CeresFallingDebrisInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs

- [ ] **CeresMode7TransferDefinitions.PlatformLight** ([L32](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.PlatformDark** ([L36](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Baby0** ([L40](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Baby1** ([L45](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Baby2** ([L50](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Wing0** ([L55](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresMode7TransferDefinitions.Wing1** ([L64](../csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs

- [ ] **CeresSteamCollisionDefinitions.DirectionBases** ([L25](../csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresSteamCollisionDefinitions.DirectionLists** ([L27](../csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresSteamCollisionDefinitions.Frames** ([L35](../csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs#L35)) - factory-built stock table. Stored Dictionary<ushort, CeresSteamCollisionComponent[]> initialized by BuildFrames(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **CeresSteamCollisionDefinitions.Lists** ([L38](../csharp/src/SuperMetroid.Core/Game/CeresSteamCollisionDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresSteamDefinitions.cs

- [x] **CeresSteamDefinitions.Initializations** ([L53](../csharp/src/SuperMetroid.Core/Game/CeresSteamDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresSteamInstructionProgramDefinitions.cs

- [ ] **CeresSteamInstructionProgramDefinitions.Words** ([L89](../csharp/src/SuperMetroid.Core/Game/CeresSteamInstructionProgramDefinitions.cs#L89)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresSteamInstructionProgramDefinitions.PresentationWords** ([L164](../csharp/src/SuperMetroid.Core/Game/CeresSteamInstructionProgramDefinitions.cs#L164)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresDoor.cs

- [ ] **RoomEnemySystem.CeresDoorRumbleOffsets** ([L34](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresDoor.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CinematicGlowPaletteFxProgramMechanicsDefinitions.cs

- [ ] **CinematicGlowPaletteFxProgramMechanicsDefinitions.Definitions** ([L33](../csharp/src/SuperMetroid.Core/Game/CinematicGlowPaletteFxProgramMechanicsDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **CrocomireBridgeFragmentDefinitions.XPositions** ([L19](../csharp/src/SuperMetroid.Core/Game/CrocomireBridgeFragmentDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **DeadMonsterRottingDefinitions.RotationRows** ([L9](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadMonsterRottingDefinitions.Transfers** ([L19](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadMonsterRottingDefinitions.TorizoHitboxRecords** ([L34](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadMonsterRottingDefinitions.SandDestinations** ([L47](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadMonsterRottingDefinitions.SandSources** ([L49](../csharp/src/SuperMetroid.Core/Game/DeadMonsterRottingDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadSidehopperInstructionProgramDefinitions.cs

- [ ] **DeadSidehopperInstructionProgramDefinitions.Words** ([L35](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperInstructionProgramDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadSidehopperInstructionProgramDefinitions.PresentationWords** ([L51](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperInstructionProgramDefinitions.cs#L51)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadSidehopperLaunchDefinitions.cs

- [ ] **DeadSidehopperLaunchDefinitions.Vertical** ([L7](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperLaunchDefinitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadSidehopperLaunchDefinitions.Horizontal** ([L9](../csharp/src/SuperMetroid.Core/Game/DeadSidehopperLaunchDefinitions.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs

- [ ] **DeadTourianCorpseDefinitions.Zoomer** ([L11](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadTourianCorpseDefinitions.Ripper** ([L22](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadTourianCorpseDefinitions.Skree** ([L32](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseInstructionProgramDefinitions.cs

- [ ] **DeadTourianCorpseInstructionProgramDefinitions.Programs** ([L41](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DeadTourianCorpseInstructionProgramDefinitions.Words** ([L44](../csharp/src/SuperMetroid.Core/Game/DeadTourianCorpseInstructionProgramDefinitions.cs#L44)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs

- [ ] **RoomEnemySystem.DeadSidehopperInitialGraphicsCopies** ([L19](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadSidehopperColumnWordOffsets0** ([L40](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadSidehopperColumnMinimumY0** ([L43](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadSidehopperColumnWordOffsets2** ([L46](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadSidehopperColumnMinimumY2** ([L49](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadSidehopper.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs

- [ ] **RoomEnemySystem.DeadZoomerProfile** ([L26](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadRipperProfile** ([L53](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.DeadSkreeProfile** ([L75](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.DeadTourianCorpses.cs#L75)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DownwardGateProjectileInstructionProgramDefinitions.cs

- [ ] **DownwardGateProjectileInstructionProgramDefinitions.Words** ([L24](../csharp/src/SuperMetroid.Core/Game/DownwardGateProjectileInstructionProgramDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DownwardGateProjectileInstructionProgramDefinitions.PresentationWords** ([L56](../csharp/src/SuperMetroid.Core/Game/DownwardGateProjectileInstructionProgramDefinitions.cs#L56)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EvirVisualDefinitions.cs

- [ ] **EvirVisualDefinitions.Frames / literal at L17** ([L17](../csharp/src/SuperMetroid.Core/Assets/EvirVisualDefinitions.cs#L17)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/EvirInstructionProgramDefinitions.cs

- [ ] **EvirInstructionProgramDefinitions.Words** ([L34](../csharp/src/SuperMetroid.Core/Game/EvirInstructionProgramDefinitions.cs#L34)) - factory-built stock table. Stored EvirInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **EvirInstructionProgramDefinitions.PresentationWords** ([L35](../csharp/src/SuperMetroid.Core/Game/EvirInstructionProgramDefinitions.cs#L35)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs

- [ ] **HudTileAtlas.transfer** ([L8](../csharp/src/SuperMetroid.Core/Assets/HudTileAtlas.cs#L8)) - installed stock table. Original/default payload behind HudTileAtlas.transfer. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/HudReserveLayout.cs

- [ ] **HudReserveLayout.TileIndices** ([L11](../csharp/src/SuperMetroid.Core/Game/HudReserveLayout.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/KiHunterAcidSpitInstructionProgramDefinitions.cs

- [ ] **KiHunterAcidSpitInstructionProgramDefinitions.Words** ([L24](../csharp/src/SuperMetroid.Core/Game/KiHunterAcidSpitInstructionProgramDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KiHunterAcidSpitInstructionProgramDefinitions.PresentationWords** ([L55](../csharp/src/SuperMetroid.Core/Game/KiHunterAcidSpitInstructionProgramDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/KiHunterInstructionProgramDefinitions.cs

- [ ] **KiHunterInstructionProgramDefinitions.Words** ([L41](../csharp/src/SuperMetroid.Core/Game/KiHunterInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **KiHunterInstructionProgramDefinitions.PresentationWords** ([L95](../csharp/src/SuperMetroid.Core/Game/KiHunterInstructionProgramDefinitions.cs#L95)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramGeneratedDefinitions.cs

- [ ] **LibraryBackgroundProgramDefinitions.programs** ([L8](../csharp/src/SuperMetroid.Core/Rooms/LibraryBackgroundProgramGeneratedDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MagdollitePaletteCycle.cs

- [ ] **MagdollitePaletteCycle.frames** ([L20](../csharp/src/SuperMetroid.Core/Assets/MagdollitePaletteCycle.cs#L20)) - installed stock table. Original/default payload behind MagdollitePaletteCycle.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/MagdolliteInstructionProgramDefinitions.cs

- [ ] **MagdolliteInstructionProgramDefinitions.Words** ([L57](../csharp/src/SuperMetroid.Core/Game/MagdolliteInstructionProgramDefinitions.cs#L57)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MagdolliteInstructionProgramDefinitions.PresentationWords** ([L172](../csharp/src/SuperMetroid.Core/Game/MagdolliteInstructionProgramDefinitions.cs#L172)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MagdolliteLavaInstructionProgramDefinitions.cs

- [ ] **MagdolliteLavaInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/MagdolliteLavaInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MagdolliteLavaInstructionProgramDefinitions.PresentationWords** ([L37](../csharp/src/SuperMetroid.Core/Game/MagdolliteLavaInstructionProgramDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MagdollitePhaseDefinitions.cs

- [x] **MagdollitePhaseDefinitions.Phases** ([L20](../csharp/src/SuperMetroid.Core/Game/MagdollitePhaseDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MapLandmarkLayout.cs

- [ ] **MapLandmarkLayout.points** ([L9](../csharp/src/SuperMetroid.Core/Assets/MapLandmarkLayout.cs#L9)) - installed stock table. Original/default payload behind MapLandmarkLayout.points. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs

- [ ] **MapObjectTileArtwork.otherCharacters** ([L14](../csharp/src/SuperMetroid.Core/Assets/MapObjectTileArtwork.cs#L14)) - installed stock table. Original/default payload behind MapObjectTileArtwork.otherCharacters. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MapPaletteCycle.cs

- [ ] **MapPaletteCycle.durations** ([L9](../csharp/src/SuperMetroid.Core/Assets/MapPaletteCycle.cs#L9)) - installed stock table. Original/default payload behind MapPaletteCycle.durations. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
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

- [ ] **RoomEnemySystem.EyeMountXOffsets** ([L161](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L161)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.EyeMountYOffsets** ([L162](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L162)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.EyeMountInstructionLists** ([L164](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L164)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.EyeBeamRedCycle** ([L174](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L174)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.EyeBeamGreenCycle** ([L177](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MorphBallEye.cs#L177)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs

- [ ] **NoobTubeProjectileInstructionProgramDefinitions.ShardPrograms** ([L21](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NoobTubeProjectileInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs#L23)) - factory-built stock table. Stored NoobTubeProjectileInstructionMechanicsWord[] initialized by BuildWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **NoobTubeProjectileInstructionProgramDefinitions.PresentationWords** ([L24](../csharp/src/SuperMetroid.Core/Game/NoobTubeProjectileInstructionProgramDefinitions.cs#L24)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

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

- [ ] **PhantoonCasualFlameDefinitions.Pattern / literal at L9** ([L9](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L9)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **PhantoonCasualFlameDefinitions.Pattern / literal at L10** ([L10](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L10)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **PhantoonCasualFlameDefinitions.Pattern / literal at L11** ([L11](../csharp/src/SuperMetroid.Core/Game/PhantoonCasualFlameDefinitions.cs#L11)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
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
- [ ] **PhantoonInstructionProgramDefinitions.PresentationWords** ([L95](../csharp/src/SuperMetroid.Core/Game/PhantoonInstructionProgramDefinitions.cs#L95)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs

- [ ] **PhantoonPatternDefinitions.EyeInstructions** ([L11](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonPatternDefinitions.FirstRainColumns** ([L37](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs#L37)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonPatternDefinitions.ShotEyeMarkers** ([L40](../csharp/src/SuperMetroid.Core/Game/PhantoonPatternDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs

- [ ] **PhantoonProjectileInstructionProgramDefinitions.Words** ([L45](../csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonProjectileInstructionProgramDefinitions.PresentationWords** ([L107](../csharp/src/SuperMetroid.Core/Game/PhantoonProjectileInstructionProgramDefinitions.cs#L107)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonSoundDefinitions.cs

- [ ] **PhantoonSoundDefinitions.Materialization** ([L10](../csharp/src/SuperMetroid.Core/Game/PhantoonSoundDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs

- [ ] **PhantoonTimerDefinitions.VulnerableWindow** ([L9](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonTimerDefinitions.EyeClosed** ([L11](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PhantoonTimerDefinitions.RainHiding** ([L13](../csharp/src/SuperMetroid.Core/Game/PhantoonTimerDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PolypInstructionProgramDefinitions.cs

- [ ] **PolypInstructionProgramDefinitions.Words** ([L15](../csharp/src/SuperMetroid.Core/Game/PolypInstructionProgramDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PolypRockInstructionProgramDefinitions.cs

- [ ] **PolypRockInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/PolypRockInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [ ] **SpacePirateProjectileInstructionProgramDefinitions.PresentationWords** ([L82](../csharp/src/SuperMetroid.Core/Game/SpacePirateProjectileInstructionProgramDefinitions.cs#L82)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs

- [ ] **TourianStatueColorCatalog.baseColors** ([L20](../csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs#L20)) - installed stock table. Original/default payload behind TourianStatueColorCatalog.baseColors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TourianStatueColorCatalog.statueColors** ([L21](../csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs#L21)) - installed stock table. Original/default payload behind TourianStatueColorCatalog.statueColors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TourianStatueColorCatalog.eyeColors** ([L22](../csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs#L22)) - installed stock table. Original/default payload behind TourianStatueColorCatalog.eyeColors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TourianStatueColorCatalog.greyColors** ([L23](../csharp/src/SuperMetroid.Core/Assets/TourianStatueColorCatalog.cs#L23)) - installed stock table. Original/default payload behind TourianStatueColorCatalog.greyColors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/TourianEntranceStatueInstructionProgramDefinitions.cs

- [ ] **TourianEntranceStatueInstructionProgramDefinitions.InitialPrograms** ([L24](../csharp/src/SuperMetroid.Core/Game/TourianEntranceStatueInstructionProgramDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TourianEntranceStatueInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/TourianEntranceStatueInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs

- [ ] **TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.Definitions** ([L34](../csharp/src/SuperMetroid.Core/Game/TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.cs

- [ ] **TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.Definitions** ([L49](../csharp/src/SuperMetroid.Core/Game/TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianStatueGreyPaletteFxProgramMechanicsDefinitions.cs

- [ ] **TourianStatueGreyPaletteFxProgramMechanicsDefinitions.Definitions** ([L30](../csharp/src/SuperMetroid.Core/Game/TourianStatueGreyPaletteFxProgramMechanicsDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianStatueProjectileInstructionProgramDefinitions.cs

- [ ] **TourianStatueProjectileInstructionProgramDefinitions.Words** ([L35](../csharp/src/SuperMetroid.Core/Game/TourianStatueProjectileInstructionProgramDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **TourianStatueProjectileInstructionProgramDefinitions.PresentationWords** ([L75](../csharp/src/SuperMetroid.Core/Game/TourianStatueProjectileInstructionProgramDefinitions.cs#L75)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TourianStatueUnlockDefinitions.cs

- [ ] **TourianStatueUnlockDefinitions.EyePositions** ([L15](../csharp/src/SuperMetroid.Core/Game/TourianStatueUnlockDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs

- [ ] **WalkingSpacePirateInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **WalkingSpacePirateInstructionProgramDefinitions.PresentationWords** ([L72](../csharp/src/SuperMetroid.Core/Game/WalkingSpacePirateInstructionProgramDefinitions.cs#L72)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rendering/XrayRoomDisplayRules.cs

- [ ] **XrayRoomDisplayRules.ExcludedBossIds** ([L24](../csharp/src/SuperMetroid.Core/Rendering/XrayRoomDisplayRules.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs

- [ ] **XrayOverlayVisualCatalog.itemMetatiles** ([L29](../csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs#L29)) - installed stock table. Original/default payload behind XrayOverlayVisualCatalog.itemMetatiles. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **XrayOverlayVisualCatalog.rooms** ([L43](../csharp/src/SuperMetroid.Core/Rooms/XrayRevealVisualCatalog.cs#L43)) - installed stock table. Original/default payload behind XrayOverlayVisualCatalog.rooms. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.YappingMaw.cs

- [ ] **RoomEnemySystem.YappingMawHeldSamusOffsets** ([L134](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.YappingMaw.cs#L134)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/YappingMawBodyProjectileInstructionProgramDefinitions.cs

- [ ] **YappingMawBodyProjectileInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/YappingMawBodyProjectileInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **YappingMawBodyProjectileInstructionProgramDefinitions.PresentationWords** ([L32](../csharp/src/SuperMetroid.Core/Game/YappingMawBodyProjectileInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/YappingMawInstructionProgramDefinitions.cs

- [ ] **YappingMawInstructionProgramDefinitions.Words** ([L47](../csharp/src/SuperMetroid.Core/Game/YappingMawInstructionProgramDefinitions.cs#L47)) - factory-built stock table. Stored YappingMawInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **YappingMawInstructionProgramDefinitions.PresentationWords** ([L49](../csharp/src/SuperMetroid.Core/Game/YappingMawInstructionProgramDefinitions.cs#L49)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Assets/ZebetiteColorCatalog.cs

- [ ] **ZebetiteColorCatalog.frames** ([L16](../csharp/src/SuperMetroid.Core/Assets/ZebetiteColorCatalog.cs#L16)) - installed stock table. Original/default payload behind ZebetiteColorCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs

- [ ] **ZebetiteDefinitions.Generations** ([L20](../csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ZebetiteDefinitions.BigHealthInstructionLists** ([L40](../csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ZebetiteDefinitions.LinkedHealthInstructionLists** ([L53](../csharp/src/SuperMetroid.Core/Game/ZebetiteDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ZebetiteInstructionProgramDefinitions.cs

- [ ] **ZebetiteInstructionProgramDefinitions.Programs** ([L46](../csharp/src/SuperMetroid.Core/Game/ZebetiteInstructionProgramDefinitions.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ZebetiteInstructionProgramDefinitions.Words** ([L60](../csharp/src/SuperMetroid.Core/Game/ZebetiteInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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