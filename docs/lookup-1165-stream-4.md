# Issue 1165 - agent stream 4

GitHub child ticket: [#1241](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1241).

Assignment: **112 files / 225 named table definitions**. [Ownership rules and all streams](lookup-1165-streams.md). Inventory snapshot: 2026-10-04T20:43:25.3086366Z.

## Agent instructions

Work through every entry below under issue 1165. Check current source and review dispositions first. Convert the original mapping into calculation or meaningful cases, or document a concrete impossible/nonsense justification. Preserve behavior and supplied content edits. Independently resolve each value field; checking off a container does not excuse its unresolved payloads.

Edit only source files listed here and this stream checklist/report. Read other files as needed. Ask the coordinator to assign any additional dependency or new source/test file before writing it. Route shared review-ledger, project, verification-entry-point and master-inventory changes through the coordinator. Do not stage, commit or publish other agents' work. Use focused confirmation of identified changes, not exploratory test discovery.

For each completed entry, record the conversion or precise retention evidence, changed files and focused confirmation. Report cross-stream dependencies by path and required contract. Report pending entries honestly; definition counts are not effort estimates.

## Exclusive source files

| File | Definitions |
| --- | ---: |
| [csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs](../csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/BeamPaletteCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/BeamPaletteCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/BotwoonColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/BotwoonColorCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs) | 8 |
| [csharp/src/SuperMetroid.Core/Assets/CeresRidleyMode7ColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyMode7ColorCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Assets/EndingFontAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/EndingFontAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EndingMode7ArtworkCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/EndingMode7ArtworkCatalog.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/EndingPaletteCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/EndingPaletteCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/FlyVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/FlyVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs) | 9 |
| [csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticePresentation.cs](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticePresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelPresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/NorfairRidleyColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/NorfairRidleyColorCatalog.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/RidleySupplementalVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/RidleySupplementalVisualDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/RoomMetatileAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/RoomMetatileAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/RoomPaletteFxPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/RoomPaletteFxPresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/RoomStaticPalette.cs](../csharp/src/SuperMetroid.Core/Assets/RoomStaticPalette.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Assets/TitleGradientPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/TitleGradientPresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/TitleGraphicsPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/TitleGraphicsPresentation.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Assets/TitlePalettePresentation.cs](../csharp/src/SuperMetroid.Core/Assets/TitlePalettePresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/TitleSpriteDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/TitleSpriteDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Audio/ManagedPcmSample.cs](../csharp/src/SuperMetroid.Core/Audio/ManagedPcmSample.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Audio/ManagedSpcPlayer.Music.cs](../csharp/src/SuperMetroid.Core/Audio/ManagedSpcPlayer.Music.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Frontend/EndingCreditsRomData.cs](../csharp/src/SuperMetroid.Core/Frontend/EndingCreditsRomData.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Frontend/TitleSequenceInstructionDefinitions.cs](../csharp/src/SuperMetroid.Core/Frontend/TitleSequenceInstructionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Frontend/TitleSequenceRomData.cs](../csharp/src/SuperMetroid.Core/Frontend/TitleSequenceRomData.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/BotwoonMovementSampleData.cs](../csharp/src/SuperMetroid.Core/Game/BotwoonMovementSampleData.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/BotwoonNavigationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/BotwoonNavigationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CeresRidleyProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CeresRidleyProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/DraygonBurialEvirDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DraygonBurialEvirDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/DraygonCannonData.cs](../csharp/src/SuperMetroid.Core/Game/DraygonCannonData.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Oam.cs](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Oam.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/DraygonInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DraygonInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/DraygonProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DraygonProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EtecoonInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EtecoonInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EyeDoorEnemyProjectileRomData.cs](../csharp/src/SuperMetroid.Core/Game/EyeDoorEnemyProjectileRomData.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EyeDoorProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EyeDoorProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EyeDoorSweatInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EyeDoorSweatInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GameplayMessageDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GameplayMessageDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs](../csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/KzanInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/KzanInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/MamaTurtleShellContourDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MamaTurtleShellContourDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/PlanetZebesTextPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PlanetZebesTextPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs) | 6 |
| [csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs](../csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs) | 6 |
| [csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Game/RidleyInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RidleyInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs) | 7 |
| [csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemyAuxiliaryDefinitionCatalog.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemyAuxiliaryDefinitionCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemyDefinitionCatalog.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemyDefinitionCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment0.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment0.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment1.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment1.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment0.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment0.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment1.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment1.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment2.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment2.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment3.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment3.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySpawnNameDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySpawnNameDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidley.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidley.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyMode7.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyMode7.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SaveRamLayout.cs](../csharp/src/SuperMetroid.Core/Game/SaveRamLayout.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SaveStationElectricityInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SaveStationElectricityInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SkreeMetareeAnimationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeAnimationDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Game/SkreeMetareeParticleInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeParticleInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SkulteraInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SkulteraInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SporeSpawnCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SporeSpawnCollisionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SporeSpawnInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SporeSpawnInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/StokeInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/StokeInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/StokeProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/StokeProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/WallSpacePirateInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/WallSpacePirateInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ZebesEscapeExplosionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ZebesEscapeExplosionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs](../csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs](../csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Rooms/RoomBackgroundTilemapSourceDefinitions.cs](../csharp/src/SuperMetroid.Core/Rooms/RoomBackgroundTilemapSourceDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Rooms/RoomLevelStreamDefinitions.cs](../csharp/src/SuperMetroid.Core/Rooms/RoomLevelStreamDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs](../csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs](../csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs](../csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs) | 2 |

## Work checklist

### csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs

- [ ] **AreaMapPresentationAsset.cells** ([L10](../csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs#L10)) - installed stock table. Original/default payload behind AreaMapPresentationAsset.cells. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **Area map stock station-reveal plane** ([L11](../csharp/src/SuperMetroid.Core/Assets/AreaMapPresentationAsset.cs#L11)) - installed stock table. Original area-map station bitplanes; AreaMapCartridgeData.StationRevealMaskBytes and installed station masks are views of the same source mapping. Discoverability and reveal-above are calculated from map cells, not separate source tables.

### csharp/src/SuperMetroid.Core/Assets/BeamPaletteCatalog.cs

- [ ] **BeamPaletteCatalog.palettes** ([L11](../csharp/src/SuperMetroid.Core/Assets/BeamPaletteCatalog.cs#L11)) - installed stock table. Original/default payload behind BeamPaletteCatalog.palettes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs

- [ ] **BeamTileAtlas.tiles** ([L8](../csharp/src/SuperMetroid.Core/Assets/BeamTileAtlas.cs#L8)) - installed stock table. Original/default payload behind BeamTileAtlas.tiles. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/BotwoonColorCatalog.cs

- [ ] **BotwoonColorCatalog.health** ([L16](../csharp/src/SuperMetroid.Core/Assets/BotwoonColorCatalog.cs#L16)) - installed stock table. Original/default payload behind BotwoonColorCatalog.health. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/BotwoonMovementSampleData.cs

- [ ] **BotwoonMovementSampleData.PackedSamples** ([L15](../csharp/src/SuperMetroid.Core/Game/BotwoonMovementSampleData.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BotwoonNavigationDefinitions.cs

- [x] **BotwoonNavigationDefinitions.Holes** ([L39](../csharp/src/SuperMetroid.Core/Game/BotwoonNavigationDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs

- [x] **BotwoonWallPlmDrawDefinitions.All** ([L29](../csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs

- [ ] **DraygonColorCatalog.intro** ([L24](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L24)) - installed stock table. Original/default payload behind DraygonColorCatalog.intro. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **DraygonColorCatalog.background** ([L25](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L25)) - installed stock table. Original/default payload behind DraygonColorCatalog.background. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **DraygonColorCatalog.sprite** ([L26](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L26)) - installed stock table. Original/default payload behind DraygonColorCatalog.sprite. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **DraygonColorCatalog.whiteFlash** ([L27](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L27)) - installed stock table. Original/default payload behind DraygonColorCatalog.whiteFlash. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **DraygonColorCatalog.healthBands** ([L28](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L28)) - installed stock table. Original/default payload behind DraygonColorCatalog.healthBands. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/DraygonBurialEvirDefinitions.cs

- [x] **DraygonBurialEvirDefinitions.Entries** ([L14](../csharp/src/SuperMetroid.Core/Game/DraygonBurialEvirDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonCannonData.cs

- [x] **DraygonCannonData.FiringTargets** ([L24](../csharp/src/SuperMetroid.Core/Game/DraygonCannonData.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs

- [x] **DraygonCollisionDefinitions.FirstBody** ([L28](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DraygonCollisionDefinitions.Empty** ([L30](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DraygonCollisionDefinitions.SecondBody** ([L32](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DraygonCollisionDefinitions.FirstBodyHitboxes** ([L35](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DraygonCollisionDefinitions.SecondBodyHitboxes** ([L47](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Oam.cs

- [x] **DraygonCollisionDefinitions.EmptyOamFrames** ([L15](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Oam.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonInstructionProgramDefinitions.cs

- [ ] **DraygonInstructionProgramDefinitions.Words** ([L89](../csharp/src/SuperMetroid.Core/Game/DraygonInstructionProgramDefinitions.cs#L89)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DraygonInstructionProgramDefinitions.PresentationWords** ([L268](../csharp/src/SuperMetroid.Core/Game/DraygonInstructionProgramDefinitions.cs#L268)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonProjectileInstructionProgramDefinitions.cs

- [x] **DraygonProjectileInstructionProgramDefinitions.Words** ([L30](../csharp/src/SuperMetroid.Core/Game/DraygonProjectileInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DraygonProjectileInstructionProgramDefinitions.PresentationWords** ([L73](../csharp/src/SuperMetroid.Core/Game/DraygonProjectileInstructionProgramDefinitions.cs#L73)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EndingFontAtlas.cs

- [ ] **EndingFontAtlas.transfer** ([L6](../csharp/src/SuperMetroid.Core/Assets/EndingFontAtlas.cs#L6)) - installed stock table. Original/default payload behind EndingFontAtlas.transfer. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EndingMode7ArtworkCatalog.cs

- [ ] **EndingMode7SceneArtwork.Map** ([L27](../csharp/src/SuperMetroid.Core/Assets/EndingMode7ArtworkCatalog.cs#L27)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.
- [ ] **EndingMode7SceneArtwork.Characters** ([L28](../csharp/src/SuperMetroid.Core/Assets/EndingMode7ArtworkCatalog.cs#L28)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.
- [ ] **EndingRewardIconArtwork.Transfer** ([L130](../csharp/src/SuperMetroid.Core/Assets/EndingMode7ArtworkCatalog.cs#L130)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.

### csharp/src/SuperMetroid.Core/Assets/EndingPaletteCatalog.cs

- [ ] **EndingPalette.nativeBytes** ([L23](../csharp/src/SuperMetroid.Core/Assets/EndingPaletteCatalog.cs#L23)) - installed stock table. Unresolved ending palette resources only. The reviewed gunship ink/unused-color fields and logo-fade endpoint artwork are excluded; no whole-ending palette exemption.

### csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs

- [ ] **EndingTextPresentation.resultPanel** ([L9](../csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs#L9)) - installed stock table. Original/default payload behind EndingTextPresentation.resultPanel. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **EndingTextPresentation.japaneseSubtitle** ([L11](../csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs#L11)) - installed stock table. Original/default payload behind EndingTextPresentation.japaneseSubtitle. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Frontend/EndingCreditsRomData.cs

- [ ] **EndingCreditsRomData.Motion.PlanetFastPattern** ([L195](../csharp/src/SuperMetroid.Core/Frontend/EndingCreditsRomData.cs#L195)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EndingCreditsRomData.Motion.PlanetSlowPattern** ([L202](../csharp/src/SuperMetroid.Core/Frontend/EndingCreditsRomData.cs#L202)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EtecoonInstructionProgramDefinitions.cs

- [ ] **EtecoonInstructionProgramDefinitions.Words** ([L55](../csharp/src/SuperMetroid.Core/Game/EtecoonInstructionProgramDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EtecoonInstructionProgramDefinitions.PresentationWords** ([L93](../csharp/src/SuperMetroid.Core/Game/EtecoonInstructionProgramDefinitions.cs#L93)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EyeDoorEnemyProjectileRomData.cs

- [x] **EyeDoorEnemyProjectileRomData.ProjectileOriginOffsets** ([L24](../csharp/src/SuperMetroid.Core/Game/EyeDoorEnemyProjectileRomData.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EyeDoorEnemyProjectileRomData.SweatVelocities** ([L30](../csharp/src/SuperMetroid.Core/Game/EyeDoorEnemyProjectileRomData.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EyeDoorProjectileInstructionProgramDefinitions.cs

- [x] **EyeDoorProjectileInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/EyeDoorProjectileInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EyeDoorProjectileInstructionProgramDefinitions.PresentationWords** ([L53](../csharp/src/SuperMetroid.Core/Game/EyeDoorProjectileInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EyeDoorSweatInstructionProgramDefinitions.cs

- [x] **EyeDoorSweatInstructionProgramDefinitions.Words** ([L21](../csharp/src/SuperMetroid.Core/Game/EyeDoorSweatInstructionProgramDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EyeDoorSweatInstructionProgramDefinitions.PresentationWords** ([L34](../csharp/src/SuperMetroid.Core/Game/EyeDoorSweatInstructionProgramDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/FlyVisualDefinitions.cs

- [x] **FlyVisualDefinitions.Frames / literal at L15** ([L15](../csharp/src/SuperMetroid.Core/Assets/FlyVisualDefinitions.cs#L15)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs

- [x] **FlyInstructionProgramDefinitions.Words** ([L15](../csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **FlyInstructionProgramDefinitions.PresentationWords** ([L20](../csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs

- [ ] **GameplayBasePaletteCatalog.initial** ([L9](../csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs#L9)) - installed stock table. Original/default payload behind GameplayBasePaletteCatalog.initial. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayBasePaletteCatalog.commonSprites** ([L10](../csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs#L10)) - installed stock table. Original/default payload behind GameplayBasePaletteCatalog.commonSprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs

- [x] **GameplayHudDefinitions.EnergyTankByteOffsets** ([L47](../csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GameplayHudDefinitions.ItemByteOffsets** ([L53](../csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs

- [ ] **GameplayHudPresentation.template** ([L10](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L10)) - installed stock table. Original/default payload behind GameplayHudPresentation.template. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.topRowTransfer** ([L11](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L11)) - installed stock table. Stock document.TopRow cells serialized as DMA bytes. This is distinct from document.Template; conversion must target the stock cells, not the byte-packing loop.
- [x] **GameplayHudPresentation.healthDigits** ([L12](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L12)) - installed stock table. Original/default payload behind GameplayHudPresentation.healthDigits. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **GameplayHudPresentation.ammoDigits** ([L13](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L13)) - installed stock table. Original/default payload behind GameplayHudPresentation.ammoDigits. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.autoFull** ([L14](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L14)) - installed stock table. Original/default payload behind GameplayHudPresentation.autoFull. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.autoEmpty** ([L15](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L15)) - installed stock table. Original/default payload behind GameplayHudPresentation.autoEmpty. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **GameplayHudPresentation.autoAnchors** ([L16](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L16)) - installed stock table. Original/default payload behind GameplayHudPresentation.autoAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **GameplayHudPresentation.energyTankAnchors** ([L17](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L17)) - installed stock table. Original/default payload behind GameplayHudPresentation.energyTankAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.icons** ([L18](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L18)) - installed stock table. Original/default payload behind GameplayHudPresentation.icons. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs

- [x] **GameplayMessageNoticeDefinitions.MapAndEnergyRegions** ([L23](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GameplayMessageNoticeDefinitions.MissileRegions** ([L29](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GameplayMessageNoticeDefinitions.SaveRegions** ([L35](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticePresentation.cs

- [ ] **GameplayMessageNoticePresentation.notices** ([L13](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticePresentation.cs#L13)) - installed stock table. Original/default payload behind GameplayMessageNoticePresentation.notices. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayMessageNoticePresentation.border** ([L14](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticePresentation.cs#L14)) - installed stock table. Original/default payload behind GameplayMessageNoticePresentation.border. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelPresentation.cs

- [ ] **GameplayMessagePanelPresentation.panels** ([L13](../csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelPresentation.cs#L13)) - installed stock table. Original/default payload behind GameplayMessagePanelPresentation.panels. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayMessagePanelPresentation.border** ([L14](../csharp/src/SuperMetroid.Core/Assets/GameplayMessagePanelPresentation.cs#L14)) - installed stock table. Original/default payload behind GameplayMessagePanelPresentation.border. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs

- [ ] **GameplayMessageTitlePresentation.titles** ([L13](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs#L13)) - installed stock table. Original/default payload behind GameplayMessageTitlePresentation.titles. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayMessageTitlePresentation.Border** ([L25](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageTitlePresentation.cs#L25)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.

### csharp/src/SuperMetroid.Core/Game/GameplayMessageDefinitions.cs

- [x] **GameplayMessageDefinitions.Definitions** ([L13](../csharp/src/SuperMetroid.Core/Game/GameplayMessageDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs

- [x] **GameplayMessageRomData.Buttons.OrderedGlyphs** ([L76](../csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GameplayMessageRomData.Buttons.SpecialGlyphByteOffsets** ([L92](../csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs#L92)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/KzanInstructionProgramDefinitions.cs

- [x] **KzanInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/KzanInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs

- [ ] **LowerNorfairRioInstructionProgramDefinitions.Words** ([L31](../csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs#L31)) - factory-built stock table. Stored LowerNorfairRioInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **LowerNorfairRioInstructionProgramDefinitions.PresentationWords** ([L33](../csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs#L33)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs

- [ ] **MamaTurtleInstructionProgramDefinitions.Words** ([L43](../csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs#L43)) - factory-built stock table. Stored MamaTurtleInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **MamaTurtleInstructionProgramDefinitions.PresentationWords** ([L45](../csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs#L45)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **MamaTurtleInstructionProgramDefinitions.AddBabyCrawl / literal at L202** ([L202](../csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs#L202)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/MamaTurtleShellContourDefinitions.cs

- [ ] **MamaTurtleShellContourDefinitions.Offsets** ([L19](../csharp/src/SuperMetroid.Core/Game/MamaTurtleShellContourDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Audio/ManagedPcmSample.cs

- [ ] **ManagedPcmSample stock PCM sample payloads** ([L8](../csharp/src/SuperMetroid.Core/Audio/ManagedPcmSample.cs#L8)) - installed stock table. Canonical sample/waveform data indexed by sample identity and sample position. Sample-bank dictionaries and loop-entry aliases are references to this payload; authored sound content is not assumed exempt without a concrete disposition.

### csharp/src/SuperMetroid.Core/Audio/ManagedSpcPlayer.Music.cs

- [ ] **SPC default FIR coefficients / fourth** ([L284](../csharp/src/SuperMetroid.Core/Audio/ManagedSpcPlayer.Music.cs#L284)) - confirmed pending logical table. spcFirPresetOwnershipReview: SPC1E32..1E51 contains four independent eight-tap defaults. Imported through SpcAudioAssetExtractor; mutable FIR RAM and later writes are not additional fixed tables.
- [ ] **SPC default FIR coefficients / reverb** ([L284](../csharp/src/SuperMetroid.Core/Audio/ManagedSpcPlayer.Music.cs#L284)) - confirmed pending logical table. spcFirPresetOwnershipReview: SPC1E32..1E51 contains four independent eight-tap defaults. Imported through SpcAudioAssetExtractor; mutable FIR RAM and later writes are not additional fixed tables.
- [ ] **SPC default FIR coefficients / sharp** ([L284](../csharp/src/SuperMetroid.Core/Audio/ManagedSpcPlayer.Music.cs#L284)) - confirmed pending logical table. spcFirPresetOwnershipReview: SPC1E32..1E51 contains four independent eight-tap defaults. Imported through SpcAudioAssetExtractor; mutable FIR RAM and later writes are not additional fixed tables.
- [ ] **SPC default FIR coefficients / smooth** ([L284](../csharp/src/SuperMetroid.Core/Audio/ManagedSpcPlayer.Music.cs#L284)) - confirmed pending logical table. spcFirPresetOwnershipReview: SPC1E32..1E51 contains four independent eight-tap defaults. Imported through SpcAudioAssetExtractor; mutable FIR RAM and later writes are not additional fixed tables.

### csharp/src/SuperMetroid.Core/Game/MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.cs

- [ ] **MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.Definitions** ([L31](../csharp/src/SuperMetroid.Core/Game/MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailCollisionDefinitions.cs

- [x] **MaridiaLargeSnailCollisionDefinitions.ListPointers** ([L33](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailCollisionDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MaridiaLargeSnailCollisionDefinitions.Lists** ([L42](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailCollisionDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionDefinitions.cs

- [x] **MaridiaLargeSnailInstructionDefinitions.InstructionPointers** ([L24](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs

- [ ] **MaridiaLargeSnailInstructionProgramDefinitions.Words** ([L42](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MaridiaLargeSnailInstructionProgramDefinitions.PresentationWords** ([L101](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs#L101)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs

- [ ] **MaridiaElevatubePlmDefinitions.Draw** ([L23](../csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MaridiaElevatubePlmDefinitions.AllDraws** ([L30](../csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PlanetZebesTextPaletteFxProgramMechanicsDefinitions.cs

- [ ] **PlanetZebesTextPaletteFxProgramMechanicsDefinitions.Definitions** ([L45](../csharp/src/SuperMetroid.Core/Game/PlanetZebesTextPaletteFxProgramMechanicsDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs

- [x] **PowerBombFixedColorCatalog.preExplosion** ([L17](../csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs#L17)) - installed stock table. Original/default payload behind PowerBombFixedColorCatalog.preExplosion. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **PowerBombFixedColorCatalog.explosion** ([L18](../csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs#L18)) - installed stock table. Original/default payload behind PowerBombFixedColorCatalog.explosion. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs

- [ ] **CeresRidleyColorCatalog.start** ([L11](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs#L11)) - installed stock table. Original/default payload behind CeresRidleyColorCatalog.start. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresRidleyColorCatalog.eyeFade** ([L12](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs#L12)) - installed stock table. Original/default payload behind CeresRidleyColorCatalog.eyeFade. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresRidleyColorCatalog.bodyFade** ([L13](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs#L13)) - installed stock table. Original/default payload behind CeresRidleyColorCatalog.bodyFade. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresRidleyColorCatalog.health** ([L14](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs#L14)) - installed stock table. Original/default payload behind CeresRidleyColorCatalog.health. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresRidleyColorCatalog.alarm** ([L15](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs#L15)) - installed stock table. Original/default payload behind CeresRidleyColorCatalog.alarm. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresRidleyColorCatalog.retreatBg** ([L16](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs#L16)) - installed stock table. Original/default payload behind CeresRidleyColorCatalog.retreatBg. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresRidleyColorCatalog.retreatShared** ([L17](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs#L17)) - installed stock table. Original/default payload behind CeresRidleyColorCatalog.retreatShared. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **CeresRidleyColorCatalog.baby** ([L18](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyColorCatalog.cs#L18)) - installed stock table. Original/default payload behind CeresRidleyColorCatalog.baby. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CeresRidleyMode7ColorCatalog.cs

- [ ] **CeresRidleyMode7ColorCatalog.rows** ([L11](../csharp/src/SuperMetroid.Core/Assets/CeresRidleyMode7ColorCatalog.cs#L11)) - installed stock table. Original/default payload behind CeresRidleyMode7ColorCatalog.rows. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/NorfairRidleyColorCatalog.cs

- [ ] **NorfairRidleyColorCatalog.initial** ([L18](../csharp/src/SuperMetroid.Core/Assets/NorfairRidleyColorCatalog.cs#L18)) - installed stock table. Original/default payload behind NorfairRidleyColorCatalog.initial. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **NorfairRidleyColorCatalog.reveal** ([L19](../csharp/src/SuperMetroid.Core/Assets/NorfairRidleyColorCatalog.cs#L19)) - installed stock table. Original/default payload behind NorfairRidleyColorCatalog.reveal. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/RidleySupplementalVisualDefinitions.cs

- [x] **RidleySupplementalVisualDefinitions.WingPointers** ([L18](../csharp/src/SuperMetroid.Core/Assets/RidleySupplementalVisualDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleySupplementalVisualDefinitions.TailTipPointers** ([L30](../csharp/src/SuperMetroid.Core/Assets/RidleySupplementalVisualDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresRidleyProjectileInstructionProgramDefinitions.cs

- [x] **CeresRidleyProjectileInstructionProgramDefinitions.Words** ([L80](../csharp/src/SuperMetroid.Core/Game/CeresRidleyProjectileInstructionProgramDefinitions.cs#L80)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CeresRidleyProjectileInstructionProgramDefinitions.PresentationWords** ([L167](../csharp/src/SuperMetroid.Core/Game/CeresRidleyProjectileInstructionProgramDefinitions.cs#L167)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs

- [x] **RidleyAttackChoices.BelowHalfHealth** ([L10](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyAttackChoices.AboveHalfHealth** ([L14](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyAttackChoices.DamageBoosting** ([L18](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyAttackChoices.PogoZone** ([L22](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyAttackChoices.SpinJumping** ([L26](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyAttackChoices.ZeroHealth** ([L30](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs

- [x] **RidleyClawOffsets.X** ([L7](../csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyClawOffsets.Y** ([L10](../csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyClawOffsets.ExistingOutOfRangeWindow** - **retained (nonsense): only six adjacent instruction words** ([L19](../csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs

- [x] **RidleyCollisionDefinitions.FrameKeys** ([L24](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyCollisionDefinitions.ListKeys** ([L29](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyCollisionDefinitions.LeftBase** ([L36](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyCollisionDefinitions.RightBase** ([L39](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyCollisionDefinitions.Frames** ([L43](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyCollisionDefinitions.Lists** ([L58](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs

- [x] **RidleyExplosionDefinitions.SpawnOrder** ([L46](../csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyExplosionDefinitions.PartRecords** ([L66](../csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs#L66)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyExplosionDefinitions.DeathExplosionPlacements** ([L87](../csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs#L87)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyExplosionDefinitions.TailTipInstructionLists** ([L105](../csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs#L105)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyInstructionProgramDefinitions.cs

- [ ] **RidleyInstructionProgramDefinitions.Words** ([L34](../csharp/src/SuperMetroid.Core/Game/RidleyInstructionProgramDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyInstructionProgramDefinitions.PresentationWords** ([L112](../csharp/src/SuperMetroid.Core/Game/RidleyInstructionProgramDefinitions.cs#L112)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs

- [x] **RidleyMovementTargets.DescendingPogoX** ([L7](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyMovementTargets.AscendingPogoX** ([L9](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyMovementTargets.GroundAttackX** ([L11](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyMovementTargets.CarryAnchorX** ([L13](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyMovementTargets.CarryReleaseX** ([L15](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyMovementTargets.HoverDivisorIndexes** ([L17](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyMovementTargets.GrabDivisorIndexes** ([L19](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs

- [x] **RidleyPogoDefinitions.Upward** ([L7](../csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyPogoDefinitions.Downward** ([L9](../csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyPogoDefinitions.Horizontal** ([L12](../csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyPogoDefinitions.Vertical** ([L20](../csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidley.cs

- [ ] **RoomEnemySystem.CreateInitialRidleyTailSegments / distances** ([L916](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidley.cs#L916)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.CreateInitialRidleyTailSegments / angles** ([L917](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidley.cs#L917)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs

- [ ] **RoomEnemySystem.CeresRidleyWingAnimationDeltas** ([L10](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.UpdateRidleyTailDistances / maximumDistances** ([L323](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs#L323)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **RoomEnemySystem.UpdateRidleyTailDistances / neutralDistances** ([L324](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs#L324)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyMode7.cs

- [x] **RoomEnemySystem.TickCeresRidleyMode7Getaway / babyTransferPointers** ([L101](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyMode7.cs#L101)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs

- [x] **RoomEnemySystem.RidleySamusMovementFlags** ([L15](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.RidleyTailTouchesTerrain / segmentIndexes** ([L672](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs#L672)) - method-local definition. Five indexed joint selections 6 through 2; calculate the reverse traversal directly.
- [x] **RoomEnemySystem.RidleyTailTouchesTerrain / yOffsets** ([L673](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs#L673)) - method-local definition. Tip probe offset 16 followed by four joint offsets 18; express the tip/joint distinction directly.

### csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs

- [ ] **RoomBackgroundTilemapAtlas.transfer** ([L11](../csharp/src/SuperMetroid.Core/Assets/RoomBackgroundTilemapAtlas.cs#L11)) - installed stock table. Original/default payload behind RoomBackgroundTilemapAtlas.transfer. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs

- [ ] **RoomCharacterAtlas.planar** ([L11](../csharp/src/SuperMetroid.Core/Assets/RoomCharacterAtlas.cs#L11)) - installed stock table. Original/default payload behind RoomCharacterAtlas.planar. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs

- [ ] **RoomFxAnimatedTileAtlas.transfer** ([L9](../csharp/src/SuperMetroid.Core/Assets/RoomFxAnimatedTileAtlas.cs#L9)) - installed stock table. Original/default payload behind RoomFxAnimatedTileAtlas.transfer. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/RoomMetatileAtlas.cs

- [ ] **RoomMetatileAtlas.blockDefinitions** ([L15](../csharp/src/SuperMetroid.Core/Assets/RoomMetatileAtlas.cs#L15)) - installed stock table. Original/default payload behind RoomMetatileAtlas.blockDefinitions. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/RoomPaletteFxPresentation.cs

- [ ] **RoomPaletteFxPresentation.colors** ([L12](../csharp/src/SuperMetroid.Core/Assets/RoomPaletteFxPresentation.cs#L12)) - installed stock table. Remaining original palette payloads only. Exclude specifically completed heat, suit-loading and ending-logo fade fields; their component dispositions remain authoritative. Other effect inks/endpoints are not exempt.

### csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs

- [ ] **RoomSkyTilemapCatalog.pages** ([L9](../csharp/src/SuperMetroid.Core/Assets/RoomSkyTilemapCatalog.cs#L9)) - installed stock table. Original/default payload behind RoomSkyTilemapCatalog.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/RoomStaticPalette.cs

- [ ] **RoomStaticPalette.nativeBytes** ([L11](../csharp/src/SuperMetroid.Core/Assets/RoomStaticPalette.cs#L11)) - installed stock table. Original/default payload behind RoomStaticPalette.nativeBytes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/RoomEnemyAuxiliaryDefinitionCatalog.cs

- [ ] **RoomEnemyAuxiliaryDefinitionCatalog.Definitions** ([L12](../csharp/src/SuperMetroid.Core/Game/RoomEnemyAuxiliaryDefinitionCatalog.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemyDefinitionCatalog.cs

- [ ] **RoomEnemyDefinitionCatalog.Definitions** ([L15](../csharp/src/SuperMetroid.Core/Game/RoomEnemyDefinitionCatalog.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment0.cs

- [ ] **RoomEnemyGraphicsSetDefinitions.BuildSegment0 / literal at L7** ([L7](../csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment0.cs#L7)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment1.cs

- [ ] **RoomEnemyGraphicsSetDefinitions.BuildSegment1 / literal at L7** ([L7](../csharp/src/SuperMetroid.Core/Game/RoomEnemyGraphicsSetDefinitions.Segment1.cs#L7)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment0.cs

- [ ] **RoomEnemyPopulationDefinitions.BuildSegment0 / literal at L7** ([L7](../csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment0.cs#L7)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment1.cs

- [ ] **RoomEnemyPopulationDefinitions.BuildSegment1 / literal at L7** ([L7](../csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment1.cs#L7)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment2.cs

- [ ] **RoomEnemyPopulationDefinitions.BuildSegment2 / literal at L7** ([L7](../csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment2.cs#L7)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment3.cs

- [ ] **RoomEnemyPopulationDefinitions.BuildSegment3 / literal at L7** ([L7](../csharp/src/SuperMetroid.Core/Game/RoomEnemyPopulationDefinitions.Segment3.cs#L7)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySpawnNameDefinitions.cs

- [x] **RoomEnemySpawnNameDefinitions.Records** - mixed: calculated structure and family names; only precise lexical residual retained ([L12](../csharp/src/SuperMetroid.Core/Game/RoomEnemySpawnNameDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectDefinitions.cs

- [x] **RoomSpriteObjectDefinitions.InstructionPointers** ([L12](../csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectInstructionProgramDefinitions.cs

- [ ] **RoomSpriteObjectInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomSpriteObjectInstructionProgramDefinitions.PresentationWords** ([L165](../csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectInstructionProgramDefinitions.cs#L165)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/RoomBackgroundTilemapSourceDefinitions.cs

- [x] **RoomBackgroundTilemapSources.Sources** ([L13](../csharp/src/SuperMetroid.Core/Rooms/RoomBackgroundTilemapSourceDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/RoomLevelStreamDefinitions.cs

- [ ] **RoomLevelStreamDefinitions.Installed stock level corpus** ([L14](../csharp/src/SuperMetroid.Core/Rooms/RoomLevelStreamDefinitions.cs#L14)) - embedded stock table. RoomLevelStreams.bin contains the canonical native level allocations keyed by source identity. Foreground collision words, BTS bytes and background words require independent field dispositions; RoomLevelData live/streaming copies are aliases, not additional tables.

### csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs

- [ ] **RoomScrollDefinitions.Definitions** ([L25](../csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs

- [x] **RoomTilesetDefinitions.Entries** ([L14](../csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs

- [ ] **RoomVisualLayout.foreground** ([L12](../csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs#L12)) - installed stock table. Original/default payload behind RoomVisualLayout.foreground. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **RoomVisualLayout.background** ([L13](../csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs#L13)) - installed stock table. Original/default payload behind RoomVisualLayout.background. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/SaveRamLayout.cs

- [x] **SaveRamLayout.NativeSlotOffsets** ([L97](../csharp/src/SuperMetroid.Core/Game/SaveRamLayout.cs#L97)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SaveStationElectricityInstructionProgramDefinitions.cs

- [x] **SaveStationElectricityInstructionProgramDefinitions.Words** ([L21](../csharp/src/SuperMetroid.Core/Game/SaveStationElectricityInstructionProgramDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SaveStationElectricityInstructionProgramDefinitions.PresentationWords** ([L39](../csharp/src/SuperMetroid.Core/Game/SaveStationElectricityInstructionProgramDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SkreeMetareeAnimationDefinitions.cs

- [x] **SkreeMetareeAnimationDefinitions.MetareeInstructionLists** ([L23](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeAnimationDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SkreeMetareeAnimationDefinitions.SkreeInstructionLists** ([L35](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeAnimationDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs

- [ ] **SkreeMetareeInstructionProgramDefinitions.MetareeWords** ([L22](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SkreeMetareeInstructionProgramDefinitions.SkreeWords** ([L36](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SkreeMetareeInstructionProgramDefinitions.MetareePresentationWords** ([L50](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SkreeMetareeInstructionProgramDefinitions.SkreePresentationWords** ([L58](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SkreeMetareeParticleInstructionProgramDefinitions.cs

- [x] **SkreeMetareeParticleInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeParticleInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SkreeMetareeParticleInstructionProgramDefinitions.PresentationWords** ([L32](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeParticleInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SkulteraInstructionProgramDefinitions.cs

- [ ] **SkulteraInstructionProgramDefinitions.Words** ([L29](../csharp/src/SuperMetroid.Core/Game/SkulteraInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SkulteraInstructionProgramDefinitions.PresentationWords** ([L52](../csharp/src/SuperMetroid.Core/Game/SkulteraInstructionProgramDefinitions.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs

- [x] **SporeSpawnColorCatalog.spores** ([L23](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L23)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.spores. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SporeSpawnColorCatalog.health** ([L24](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L24)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.health. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SporeSpawnColorCatalog.deathSprite** ([L25](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L25)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.deathSprite. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SporeSpawnColorCatalog.deathLevel** ([L26](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L26)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.deathLevel. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SporeSpawnColorCatalog.deathBackground** ([L27](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L27)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.deathBackground. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/SporeSpawnCollisionDefinitions.cs

- [x] **SporeSpawnCollisionDefinitions.Frames** ([L50](../csharp/src/SuperMetroid.Core/Game/SporeSpawnCollisionDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SporeSpawnCollisionDefinitions.Lists** ([L67](../csharp/src/SuperMetroid.Core/Game/SporeSpawnCollisionDefinitions.cs#L67)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SporeSpawnInstructionProgramDefinitions.cs

- [ ] **SporeSpawnInstructionProgramDefinitions.Words** ([L36](../csharp/src/SuperMetroid.Core/Game/SporeSpawnInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SporeSpawnInstructionProgramDefinitions.PresentationWords** ([L79](../csharp/src/SuperMetroid.Core/Game/SporeSpawnInstructionProgramDefinitions.cs#L79)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs

- [x] **SporeSpawnProjectileDefinitions.StalkYOffsets** ([L10](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SporeSpawnProjectileDefinitions.SpawnerXPositions** ([L13](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SporeSpawnProjectileDefinitions.Movement** ([L17](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileInstructionProgramDefinitions.cs

- [ ] **SporeSpawnProjectileInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SporeSpawnProjectileInstructionProgramDefinitions.PresentationWords** ([L48](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileInstructionProgramDefinitions.cs#L48)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/StokeInstructionProgramDefinitions.cs

- [ ] **StokeInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/StokeInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **StokeInstructionProgramDefinitions.PresentationWords** ([L41](../csharp/src/SuperMetroid.Core/Game/StokeInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/StokeProjectileInstructionProgramDefinitions.cs

- [ ] **StokeProjectileInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/StokeProjectileInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **StokeProjectileInstructionProgramDefinitions.PresentationWords** ([L30](../csharp/src/SuperMetroid.Core/Game/StokeProjectileInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/TitleGradientPresentation.cs

- [ ] **TitleGradientPresentation.variants** ([L13](../csharp/src/SuperMetroid.Core/Assets/TitleGradientPresentation.cs#L13)) - installed stock table. Original/default payload behind TitleGradientPresentation.variants. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/TitleGraphicsPresentation.cs

- [ ] **TitleGraphicsPresentation.babyCharacters** ([L12](../csharp/src/SuperMetroid.Core/Assets/TitleGraphicsPresentation.cs#L12)) - installed stock table. Original/default payload behind TitleGraphicsPresentation.babyCharacters. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TitleGraphicsPresentation.mode7Characters** ([L12](../csharp/src/SuperMetroid.Core/Assets/TitleGraphicsPresentation.cs#L12)) - installed stock table. Original/default payload behind TitleGraphicsPresentation.mode7Characters. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TitleGraphicsPresentation.mode7Map** ([L12](../csharp/src/SuperMetroid.Core/Assets/TitleGraphicsPresentation.cs#L12)) - installed stock table. Original/default payload behind TitleGraphicsPresentation.mode7Map. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TitleGraphicsPresentation.objectCharacters** ([L12](../csharp/src/SuperMetroid.Core/Assets/TitleGraphicsPresentation.cs#L12)) - installed stock table. Original/default payload behind TitleGraphicsPresentation.objectCharacters. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TitleGraphicsPresentation.sprites** ([L13](../csharp/src/SuperMetroid.Core/Assets/TitleGraphicsPresentation.cs#L13)) - installed stock table. Original/default payload behind TitleGraphicsPresentation.sprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/TitlePalettePresentation.cs

- [ ] **TitlePalettePresentation.colors** ([L13](../csharp/src/SuperMetroid.Core/Assets/TitlePalettePresentation.cs#L13)) - installed stock table. Original/default payload behind TitlePalettePresentation.colors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **TitlePalettePresentation.animatedColors** ([L14](../csharp/src/SuperMetroid.Core/Assets/TitlePalettePresentation.cs#L14)) - installed stock table. Original/default payload behind TitlePalettePresentation.animatedColors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/TitleSpriteDefinitions.cs

- [x] **TitleSpriteDefinitions.Pointers** ([L8](../csharp/src/SuperMetroid.Core/Assets/TitleSpriteDefinitions.cs#L8)) - factory-built stock table. Stored ushort[] initialized by CollectPointers(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Frontend/TitleSequenceInstructionDefinitions.cs

- [ ] **TitleSequenceInstructionDefinitions.Program** ([L12](../csharp/src/SuperMetroid.Core/Frontend/TitleSequenceInstructionDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/TitleSequenceRomData.cs

- [x] **TitleSequenceRomData.Vram.BabySourcePages** ([L94](../csharp/src/SuperMetroid.Core/Frontend/TitleSequenceRomData.cs#L94)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.cs

- [ ] **TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.Definitions** ([L38](../csharp/src/SuperMetroid.Core/Game/TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/WallSpacePirateInstructionProgramDefinitions.cs

- [ ] **WallSpacePirateInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/WallSpacePirateInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **WallSpacePirateInstructionProgramDefinitions.PresentationWords** ([L86](../csharp/src/SuperMetroid.Core/Game/WallSpacePirateInstructionProgramDefinitions.cs#L86)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ZebesEscapeExplosionDefinitions.cs

- [ ] **ZebesEscapeExplosionDefinitions.Definitions** ([L16](../csharp/src/SuperMetroid.Core/Game/ZebesEscapeExplosionDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.cs

- [ ] **ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.LavaDurations** ([L36](../csharp/src/SuperMetroid.Core/Game/ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.Definitions** ([L39](../csharp/src/SuperMetroid.Core/Game/ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.cs

- [ ] **ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.Durations** ([L50](../csharp/src/SuperMetroid.Core/Game/ZebesExplosionForegroundPaletteFxProgramMechanicsDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.cs

- [ ] **ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.Definitions** ([L42](../csharp/src/SuperMetroid.Core/Game/ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.cs

- [ ] **ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.All** ([L73](../csharp/src/SuperMetroid.Core/Game/ZebesExplosionWhiteoutPaletteFxProgramMechanicsDefinitions.cs#L73)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

## Agent handoff

- Completed conversions: 32 named definitions (batches 1/2/4/5/6); Tourian statue grey intermediate payload also converted (batch 3, container remains open).
- Justified retained entries: none.
- Confirmation results: isolated builds and --lookup-stream-4 pass; legacy presentation-read verifier limitations recorded in batch 2.
- Cross-stream dependencies and proposed shared-file patches: pending.
- Remaining entries: all unchecked entries above.


## Batch 1: Ridley pogo and breakup arithmetic

Six definitions converted; no retained exception.

- `RidleyPogoDefinitions`: all four tables removed. Horizontal pattern increments are $20; ordinary stages add $08. Two unused lead-in stages preserve their separate lower progression. Upward acceleration is a linear stage progression after the first entry; downward acceleration doubles through stage three then follows the higher linear range. Vertical launch magnitudes combine stage growth and pattern increments, preserving the larger initial pattern step and the stage-three faster-pattern adjustment. No generated table is cached.
- `RidleyExplosionDefinitions.PartRecords`: initializer addresses and part parameters derive from native record layout. Independent lifetime inputs72/8/40/128 remain required; this entry is not complete. `TailTipInstructionLists` derives sixteen six-byte addresses from orientation.
- Original evidence: pinned bank A6 `SetRidleyPogoSpeeds` tables `$B94D..B9D4`, breakup lifetime/initializer tables `$C6CE..C6FD`, and tail-tip orientation pointers `$C7BA..C7D9`. The original disassembly explicitly identifies unused pogo lead-in stages, speed-pattern ordering and acceleration roles.
- Confirmation: isolated Verification build passed (1431 warnings, zero errors). `--lookup-stream-4` passed direct original-ROM comparisons for all 24 pogo records (four fields), all twelve breakup records (parameter/lifetime/callback), all sixteen tip pointers, and rejected pattern/stage/parameter/orientation bounds. Revision SHA is checked by the command.
- Changed files: two production catalogs, `Program.LookupStream4.cs`, local `Program.cs` command wiring, this report. Coordinator owns shared inventories and publication.
- Remaining scope: 219 unchecked named definitions. In the partially converted breakup file, `SpawnOrder` and `DeathExplosionPlacements` remain open independently; this batch grants neither a retention justification.
## Batch 2: Skree/Metaree phases, particle loops and save electricity

Six more definitions converted; 12 total complete and 213 remain. No retained exception.

- `SkreeMetareeAnimationDefinitions`: both four-entry phase selectors now dispatch named idle/preparation/dive/stop phases to their named programs.
- `SkreeMetareeParticleInstructionProgramDefinitions`: derives both eight-byte drawing/goto-self programs, their six mechanics words and two presentation operand addresses. The sixteen-frame duration, exact program order and mechanics/presentation byte ownership remain unchanged.
- `SaveStationElectricityInstructionProgramDefinitions`: derives timer setup, eight one-frame drawing records, decrement/branch and deletion, including all thirteen mechanics words and eight presentation operand addresses.
- Original pinned evidence: bank A3 `$894E..8955` and `$C69C..C6A3`; bank86 `$8ABD..8ACC` and `$E683..E6AC`. Direct native comparisons cover every word, indexed enumeration order, byte ownership and rejected input bounds.
- Added actual consumer confirmation in `--lookup-stream-4`: twenty complete electricity cycles in exact frame order, one-frame durations, correct installed presentation operands and deletion on the next command; both four-particle debris bursts execute repeated sixteen-frame goto-self loops with their original compiled visual identity. Guards reject mechanics cartridge reads.
- Verification build passed (1216 warnings, zero errors); `--lookup-stream-4` passed. Two pre-existing legacy commands (`--skree-metaree-particle-instruction-mechanics`, `--save-station-electricity-instruction-mechanics`) failed obsolete assertions requiring presentation ROM reads after executing their loop checks. Current production already uses compiled/installed presentation identities. The new focused fixture confirms those actual identities; shared legacy verifier files were not changed. Initial new fixture assumptions about a uniform presentation route were corrected: electricity uses `PresentationOperandAddress`, debris uses its compiled `SpritemapPointer`.
- Follow-up review cleanup: Ridley initializer/program address bases now have named XML-documented catalog constants. No production behavior changed by that cleanup.
## Batch 3: installed Tourian statue grey interpolation

Partial payload conversion inside the still-open `RoomPaletteFxPresentation.colors` entry; no additional container checkmark and no retention exception.

- New `Assets/TourianStatueGreyColorDefinitions.cs` computes the six intermediate rows from supplied first/last endpoints: each RGB5 channel is start + sign(delta) * floor((abs(delta)*frame+3)/7); color zero adopts the last endpoint immediately. All 48 intermediate words are computed on demand, without a generated lookup cache.
- `RoomPaletteFxPresentation.Load` removes an intermediate only when it equals calculation from the supplied endpoints. Explicit independent edits stay stored. Endpoint edits preserve every unchanged intermediate that no longer fits. All 64 pointer identities remain enumerable exactly once, and explicit supplied values take read precedence.
- Original evidence: pinned bank8D `$E240+20*frame+2*color`, eight grey-out rows shared by the four statue programs. Existing control catalog XML independently records the channel interpolation; direct raw ROM comparisons verify it here through the installed loader.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-4` passed original64word output, exact color-pointer classification, only16 endpoint words remaining stored, identity enumeration, missing-endpoint rejection, and six real-JSON independent endpoint/intermediate edits including color zero.
- Remaining: the sixteen endpoint words and other palette families in the container still require their own conversion or acceptable evidence. The stream retains 213 unchecked named definitions; this partial field conversion does not lower that count.
## Batch 4: Ridley attack distributions and movement targets

Thirteen definitions converted; 25 complete and 200 unchecked. No retained exception.

- `RidleyAttackChoices`: six stored eight-way distributions become direct semantic situation/choice dispatch. Four/four and six/two thresholds preserve the original random-choice ordering; single-action situations return their named action. The independently exposed reversed health distribution remains available.
- `RidleyMovementTargets`: ascending/descending quarter-arena targets and mirrored release destinations are calculated from facing; ground-attack and carry anchors use named facing cases. Hover divisor indexes follow the initial-four/then-two progression; grab indexes double-plus-one capped at ten. Invalid direct function indices retain bounds rejection.
- `RoomEnemySystem.Ridley` calls the direct functions. Existing facing/health clamps, branch order, zero-health counter mutation, single RNG advance and immediate execution of the chosen action are unchanged. No replacement list is generated or cached.
- Evidence: pinned bank A6 action pointers `$B38C..B3EB`; targets `$B60D/$B63B/$B6C8/$BBEB/$BC62`; divisor indexes `$B439/$BB4E`. Native comments identify facing and fight situations.
- Confirmation: Verification build passed (1431 warnings, zero errors). `--lookup-stream-4` passed all previous scoped checks plus existing Ridley choice/target confirmation, adapted only from span indexing to function calls. All48 original action words and23 target/divisor words match; the existing production checks verify choice/RNG order and same-frame setup, carry/health clamping, side movement and grab acceleration with mechanics reads forbidden. Their original assertions and raw-ROM oracles remain intact.
- Additional ownership granted by coordinator: `Program.RidleyAttackChoices.cs` and `Program.RidleyMovementTargets.cs`; the local stream command now calls these focused checks.
## Batch 5: Draygon burial Evir radial approach

One complete five-column definition converted; 26 definitions complete and 199 remain. No retained exception.

- `DraygonBurialEvirDefinitions.Entries` is removed. The six angles are $68-$10*entry. Subspeeds are floor(65535*abs(cos(angle))) and floor(65535*sin(angle)); spawn coordinates are floor(256+508*cos(angle)) and floor(512-508*sin(angle)), with native signed-X word wrapping. All values are evaluated from angle geometry, without storing the six results.
- Evidence: pinned bankA5 `$A1AF..A1F6`. Native annotations explicitly identify the $FFFF-scaled trigonometric subspeeds and equally spaced angles; the six positions independently match the radius508 circle centered at(256,512). Every one of the30 meaningful original fields matches raw ROM, including the two negative X positions. Angle-padding words are unused and remain outside the record API.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-4` passed the thirty raw-field comparisons and invalid entry bounds, alongside its existing focused checks. No consumer or mutable state ownership changed.
## Batch 6: Spore Spawn geometry, fly and eye-door sweat programs

Six definitions converted; 32 complete and 193 unchecked. No retained exception.

- Spore Spawn stalk offsets derive from -64+8*index, and ceiling emitters from32+64*index. The existing four-entry domain and rejected higher arguments remain unchanged. Native `$86:DCB9..DCC0` and `$86:DCE6..DCED` independently match all eight words. The adjacent fifth native stalk word is outside the pre-existing API domain; this conversion does not extend that contract. The separate wrapped spore movement stream is still pending.
- Fly instruction mechanics and presentation locations derive from four two-frame drawing records and goto-first-frame at `$A2:B013..B026`.
- Eye Door sweat mechanics and presentation locations derive from the six-frame falling loop and impact sequence (clear movement, three six-frame drawings, delete) at `$86:B615..B62C`.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-4` passed all changed native words, enumeration order/count, exact presentation/mechanics byte ownership, unaligned-read rejection and index bounds. Existing unrelated presentation data remain supplied.
- Stream4 worktree now pauses for the coordinator-requested rotation back to stream1. All193 unchecked entries and Tourian endpoint/container scopes remain explicit pending work; no exception was inferred for them.
## Batch 7: installed Power Bomb color calculation

One more complete definition (`preExplosion`);33 complete and192 unchecked. `explosion` also has21 of32 triplets converted, but stays unchecked. No retained exception.

- `PowerBombFixedColorCatalog` calculates all16 stock pre-explosion RGB triplets, the14 yellow explosion triplets and seven grayscale-crest triplets on demand. Stock pre-explosion storage is empty; explosion storage contains only the11 still-unresolved tail colors. No generated color cache is retained.
- JSON validation and public sequence/index domains are preserved. Only supplied values differing from the calculation stay in the override dictionaries; explicit independent channel changes retain read priority. Serialization documents remain complete, so editing one value does not alter any neighboring color.
- Evidence: pinned bank88 `PowerBomb_PreExplosion_Colors` `$9079..90A8`, `PowerBombExplosion_Colors` `$8D85..8DE4`. Source comments identify white/yellow phases; linear channel ramps and the two-frame grayscale ascent/one-frame descent reproduce all111 converted component bytes exactly.
- Removed the obsolete irregular-tail retention rationale from the granted `SamusPaletteRomData.PowerBomb` XML and documented the now-applied crest formulas. Tail indices21..31 still require conversion or concrete impossible/nonsense evidence; irregularity alone grants no exemption.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-4` passed all48 original installed RGB triplets, exact calculated/storage domains, five independent real-JSON edits across pre-explosion/opening/crest/tail, unchanged neighbors and rejected bounds. Existing Power Bomb/Crystal Flash guarded lifecycles and95-frame Ceres explosion checks also pass.
### Related palette documentation audit

Removed the obsolete normal-suit pointer retention language: the current implementation already resolves semantic suit cases. Removed the cinematic intro-grey retention argument based on failed uniform-ramp fits and the adjacent hurt-palette live-cartridge wording. Both independent payload obligations remain explicit. Rechecked death-explosion timing against native `$9B:B75B` countdown/advance, `$B823` nine timer bytes, and `$92:EDBE` fixed-position drawing selection. Its existing exception is specifically the chosen entry/per-drawing/final cinematic dwell times, with no motion or measured-brightness quantity determining them; clarified that authored data in general receives no exemption. No production behavior changed in this documentation cleanup.
## Batch 8: enemy geometry/dispatch and save-slot layout

Seven more definitions converted;40 complete and185 unchecked. No retained exception.

- Draygon cannon selection dispatches the four named roles, preserving every control word and wall muzzle coordinate. The consumer retains its random-bit selection, disabled-cannon check and allocation ordering.
- Eye-door origin words calculate the used left/right block offsets and the unused staggered/row patterns. Sweat velocities derive horizontal direction and shared vertical speed. Word-level access preserves all existing overlapping-pair parameter cases; odd and out-of-range parameters retain the consumer's rejection. Corrected the origin table's source identity from initializer `$B62D` to actual payload `$B65B`.
- Ridley claw X decreases12 pixels per clamped facing. The three Y heights advance10.5 pixels rounded upward. Existing clamps, odd foot-index aliases and the adjacent-code read window remain behaviorally unchanged; `ExistingOutOfRangeWindow` stays unchecked and independently required.
- Oum animation dispatch maps the eight named facing/action states to named programs.
- Save-slot origins calculate16+slot*1628, replacing the stored three-word span. Serializer, replay exporter and existing verifier consumers call the new bounded API; payload bytes and directory layout are unchanged.
- Evidence: pinned bankA5 `$87AA..87F3`, bank86 `$B65B..B682` and `$B6B1..B6B8`, bankA6 `$B9D5..B9DF`, bankA2 `$CB77..CB86`, bank81 `SaveSlotOffsets` `$812B..8130`.
- Confirmation: Verification build passed (1431 warnings, zero errors), DebugRunner build passed (42 warnings, zero errors), and `--lookup-stream-4` passed. Direct original-word checks cover every changed geometry/dispatch/slot value and rejected bounds. Existing Ridley verification preserves all65536 raw facing/foot-index values,1,179,648 actual carry placements and228,150 collision-boundary checks. Existing SRAM schema assertions pass. No gameplay discovery or expanded unrelated acceptance was performed.
## Batch 9: Kzan and Ceres Ridley projectile programs

Three definitions converted; 43 complete and 182 unchecked. No retained exception.

- Kzan's program directly selects its one-tick draw and sleep control words.
- Ceres Ridley's fireball setup and four-frame loop calculate their control and operand locations. Final afterburn calculates five five-tick draws followed by deletion. Horizontal, vertical and directional spawning programs share that draw layout with the appropriate callback inserted after the first frame. Ordered enumeration, native word identities, odd-address vertical layout and exact byte classification are preserved without stored or generated rows.
- Removed obsolete presentation-pointer retention language from the replaced field. This batch changes operand locations only; it grants no exemption to presentation payloads.
- Evidence: pinned bank A6 `InstList_Kzan` `$8B29..8B2E`; bank86 fireball `$9552..9573`, final afterburn `$9574..958B`, horizontal `$95A0..95B9`, vertical `$95D3..95EC`, directional `$9606..961F`.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-4` passed. The new focused check compares all44 control words with the original ROM and all26 interleaved operand addresses in order, checks both bytes, rejects unaligned reads, adjacent code, wrong banks and invalid enumeration indices. Existing stream checks also pass.
## Batch 10: Lower Norfair Rio animation programs

PresentationWords converted; Words remains required for independent preparation, cooldown and flame holds. No retained exception.

- Removed both startup-generated arrays and their list builders. Program ranges identify idle, preparation, descent, ascent, cooldown and flame behavior; frame strides calculate presentation operands. Direct control evaluation preserves flame visibility callbacks, animation-finished signals, sleep and loop targets. Indexed views enumerate the calculated program without storing rows.
- Idle/descent/ascent share uniform holds. Preparation/cooldown endpoint exceptions and flame 6/4/3 timing remain unexplained input choices. Matching them with ordinal expressions does not establish a conversion; Words stays unchecked until those inputs have a valid disposition.
- Evidence: pinned bankA2 `InstList_Holtz_Idle_0` through `InstList_Holtz_Flames`, `$C61A..C6BF`; both turnaround sequences select pose7 at their one-tick center.
- Confirmation: Verification build passed (1431 warnings, zero errors), and `--lookup-stream-4` passed. The existing `VerifyLowerNorfairRioInstructionProgramDefinitions` is now wired into this focused command unchanged: all51 control words,32 presentation selectors, all seven actual interpreter programs, three callback effects, initializer selections and mechanics-read guards pass. No unrelated gameplay investigation.
## Batch 11: Draygon projectile programs

Two definitions converted; 47 complete and 178 unchecked. No retained exception.

- Six constant goop draws and three flight draws calculate their word/operand addresses. Shot goop retains two eight-tick draws, the drop callback, shared delete jump and private trailing delete. Touch and sleep identities remain explicit control cases.
- Turret formation calculates two six-frame decreasing-duration ramps with floors3 then2; the second ramp is uniformly one tick faster. Four ten-tick charging frames follow, then the fired callback. No stored or generated instruction rows remain.
- Evidence: pinned bank86 `$8C38..8C67` and `$8CA4..8CF5`; original sprite names distinguish formation, charging and flight phases.
- Confirmation: full build passed (1431 warnings, zero errors); final incremental verifier build passed (25 warnings, zero errors); `--lookup-stream-4` passed. Existing native38-word, actual goop/turret producer, touch/drop, loop/sleep/delete and fired-callback assertions remain. Updated the obsolete expectation of27 live ROM presentation reads: all27 exact native operand identities are now compared in order and observed through actual installed-frame selections, with zero cartridge presentation reads. This changes the expected transport, not the native selected frame identity.
- Additional verifier ownership was granted before editing `Program.DraygonProjectileInstructionProgramDefinitions.cs`.
## Batch 12: Draygon flash and health interpolation

One full definition converted; 48 complete and 177 unchecked. The healthBands container stays unchecked because its endpoint colors remain required. No retained exception.

- White flash calculates the transparent backdrop and white visible inks, storing only supplied colors that differ. No stock flash words remain stored.
- Health bands calculate all24 intermediate RGB5 colors by nearest-seventh interpolation between the first/last four-color rows. Only8 endpoint words remain stored for stock data; those endpoint colors are explicitly unresolved. Independently edited middle colors override the calculation. Edited endpoints do not implicitly alter other supplied colors: any intermediate mismatch becomes its own override.
- Content identity keeps the exact prior ordered palette/row encoding. CGRAM destinations, white-frame suppression of health application, native even-index rejection and loader validation remain intact.
- Evidence: pinned bankA5 `DraygonHealthBasedPaletteTable` `$96AF..96EE`; `Palette_Draygon_WhiteFlash` `$A297..A2B6`.
- Confirmation: Verification build passed (1433 warnings, zero errors); `--lookup-stream-4` passed. New focused checks cover all48 changed native colors, zero stored stock flash words/eight health endpoint words, unchanged content hashes, actual background/sprite/health CGRAM writes, independent JSON edits to backdrop/visible flash/middle health/endpoint health, and invalid bounds/native odd selectors.
## Batch 13: Botwoon health interpolation

Partial conversion; the container remains unchecked. Totals remain48 complete and177 unchecked. No retained exception.

- Botwoon health colors calculate94 intermediate words through nearest-seventh RGB5 interpolation. Stock storage falls from128 words to32 endpoint words plus the two nonmatching transparent-slot words in bands1/2. Both endpoints and those two words remain explicit unresolved obligations; failing this interpolation does not exempt them.
- Independently supplied middle, endpoint and transparent-slot edits remain exact; differing colors are stored as overrides. The content identity retains the original ordered eight-row encoding and bounds behavior.
- Evidence: pinned bankB3 `BotwoonHealthBasedPalettes`, `$971B..981A`. All colored slots interpolate between first/last rows; slot0 has `$2003` at bands1/2 while its endpoints are zero.
- Confirmation: Verification build passed (1433 warnings, zero errors); `--lookup-stream-4` passed. Focused checks verify all128 native values, exactly34 stored stock words, unchanged content identities, independent JSON edits and index rejection.
- Also normalized the preceding Draygon interpolation source annotation into member XML documentation; no behavior changed there.

## Batch 14: Botwoon hole selection and wall-fill reconciliation

Two definitions resolved;50 complete and175 unchecked. No retained exception.

- The four hole rectangles dispatch by named native Left/Bottom/Top/Right identities. Each case specifies its center and derives the four-pixel inset; existing eight-pixel right/bottom and target calculations remain. Original eight-byte offsets and all invalid-offset rejections remain exact. These semantic names are explicit in pinned bankB3 `BotwoonHoleHitboxes`, `$949B..94BA`.
- `BotwoonWallPlmDrawDefinitions.All` was already converted in this worktree baseline. Static call review confirms gameplay uses `LevelWordAt`'s bounded constant air fill; `All` materializes an export DTO only. Pinned bank84 `$930F..9324` is one vertical run of nine `$00FF` words plus zero terminator. This entry is reconciled as implemented, not exempted; no duplicate production rewrite.
- Confirmation: build passed (1433 warnings on full build;25 on final incremental, zero errors), `--lookup-stream-4` passed. Native hole left/top/right/bottom and centers match; existing actual detection confirms exclusive right/bottom boundaries and rejects all invalid ushort offsets. Existing wall-stock mapping confirms native words, selected content identity and editable override independence.
- Removed an accidentally added Draygon verifier invocation from the standalone Rio command; combined execution remains scoped to `--lookup-stream-4`.
## Batch 15: Draygon empty OAM frame membership

One definition converted;51 complete and174 unchecked. No retained exception.

- Removed the48-address membership set. Each facing has six and four one-component frames (ten-byte stride), seven two-component frames (eighteen-byte stride), and seven frame starts determined by preceding component counts3..8. The final frame repeats eight components; only its start is part of the membership calculation. Every component still resolves to a zero-count native hitbox list.
- Evidence: pinned bankA5 `ExtendedSpritemap_Draygon_4/1B/22/29` and mirrored `34/4B/59/60`; named XML anchors identify the native starts. Record layout is two-byte count plus eight bytes per component.
- Confirmation: build passed (1433 warnings on full build;1218 on final incremental, zero errors); `--lookup-stream-4` passed. Existing native OAM fixture confirms all48 frames and130 original zero-hitbox components, actual shot/touch walkers with ROM reads forbidden, and exclusion of12 Spore Spawn frames.
- The fixture initially treated newly catalogued BG2 roots as OAM, failing on `draygon_bg2_A31B`. With coordinator ownership granted, it now skips only proven `DraygonBg2FrameDefinitions.IsFrame` roots. Existing native component/list and actual callback assertions are preserved.

### Rotation handoff after batch15

Stream4 remains unfinished (174 entries). Partial palettes still require Tourian/Draygon/Botwoon endpoints, Botwoon transparent-slot words, Power Bomb explosion tail, and their unchecked containers. Other unresolved scope includes Draygon BG2 collision/programs, Botwoon movement corpus, Spore movement, remaining enemy programs, room/presentation content and artwork.

Static beam-art review is pending, not an exemption: native first256-byte/eight-tile sheets at `$9A:F200/F400/F600/F800/FA00` selected by `$90:C3B1` have blank final3 tiles in Power/Ice, shared diagonal fragments in Power/Wave and Plasma/Spazer, plus nonuniform highlights. Any computable blank/repeated structure must be separated before a narrower impossible/nonsense disposition. No beam artwork changes were made.

Worker rotates back to stream1; no stream4 edits should be copied into that isolated worktree.
## Batch 16: surface-lightning color calculation

Partial payload conversion inside the still-open `RoomPaletteFxPresentation.colors` entry; totals remain51 complete and174 unchecked. No retention exception.

- All104 surface-lightning words derive from13 brightness phases and eight descending shades. Phase progression rises0..4, falls3..0, then falls4..1; phase4 is white, others calculate `$2D6C + $18C6 * phase - $0421 * color`.
- Integration review leaves the chosen surface base and brightness-step parameters pending alongside the seven dark-lightning inputs. Replacing samples with a formula does not by itself justify those independent choices.
- Matching stock words are removed on load; independent supplied edits retain precedence. Audit enumeration reconstructs all104 identities exactly once even when the color is calculated. No other room palette payload is marked complete.
- Evidence: pinned bank8D definition `$F765`, surface-lightning records `$EB43..EC3D`, colors `$EB45..EC4D`. This resolves the concrete color dependency identified by Stream3 batch26 (`1bf84157a`).
- Confirmation: Verification build passed (1433 warnings, zero errors); `--lookup-stream-4` passed. Focused checks compare all104 native words, require zero stored stock words, verify exact color address ownership across the program and exactly-once identity enumeration, and confirm four independent JSON edits with all neighboring samples unchanged.

## Batch 17: dark-lightning channel fade

Partial payload conversion inside the still-open `RoomPaletteFxPresentation.colors` entry; totals remain51 complete and174 unchecked. No retention exception.

- Dark-lightning subtracts five RGB5 units per phase independently from each channel, clamped at zero. Its phases ascend0..4, descend3..0, hold another neutral0, then descend4..1.
- Seven first-row inputs remain required unresolved payload. The other91 stock words are calculated. Only samples matching the supplied inputs are removed; editing an input preserves each independently supplied neighboring sample through explicit overrides. Audit identities remain complete and unique.
- Evidence: pinned bank8D `UNUSED_InstList_PaletteFXObject_DarkLightning_1/2/3`, definition `$F769`, colors `$EC78..ED78`. The native neutral row and five-unit clamped channel progression confirm the derivation; starting-color irregularity is not treated as an exemption.
- Confirmation: Verification build passed (1433 warnings, zero errors); `--lookup-stream-4` passed. Focused checks compare all98 native colors, prove only seven stock inputs remain stored, cover the exact calculated-pointer domain and unique identity enumeration, and preserve all98 supplied values under four independent input/sample JSON edits.

## Batch 18: Mama/Baby Turtle instruction layouts

PresentationWords converted; Words remains required for independent spin and shell-transition holds. No retained exception.

- Removed both factory-built arrays and all list-building helpers. Eight crawl steps calculate their ten-byte callback/draw layout; spinning programs preserve the baby's extra stoppable callback; named shell programs calculate record layout, directional rise callbacks and terminal sleeps. Unequal spin and shell-transition timing is unresolved.
- Direct control resolution uses these semantic program rules. Native-order enumeration scans the bounded program region without caching; every sub-$8000 control is a draw duration whose following word owns presentation, yielding all75 operand addresses without storing a second mapping.
- Evidence: pinned bankA2 thirteen `InstList_MamaTurtle/BabyTurtle` programs from `$8B80` through `$8D4E`, excluding adjacent code gaps and movement definitions starting `$8D50`.
- Confirmation: Verification build passed (1433 warnings, zero errors); `--lookup-stream-4` passed. Existing unchanged focused turtle verifier confirms117 original control words in strict order, both-byte ownership,75 unique presentation addresses and their mechanics exclusion, adjacent movement rejection and allocation-free direct resolution. This verifier is a definition-boundary check; its legacy console wording about executing all programs is broader than its actual assertions, so no such execution claim is made here.

## Batch 19: Eye Door projectile control layouts

Two more definitions converted;55 complete and170 unchecked. No retained exception.

- Initial4/3/2-tick setup and impact2/3/4-tick reverse cadence calculate draw records. Aiming/pre-instruction installation, the16-tick flight loop, four4-tick shot frames and deletion remain semantic control cases. All19 controls and11 presentation identities derive from these layouts.
- Evidence: pinned bank86 `InstList_EnemyProjectile_EyeDoorProjectile_Normal_0/1`, `Explode`, and `Shot_EyeDoorProjectile`, `$B5D9..B614`. Catalog XML now uses these exact source symbols.
- Confirmation: full Verification build1433 warnings/zero errors, final incremental25 warnings/zero errors; `--lookup-stream-4` passed. Existing native-word, aiming/flight, real opened-door impact handoff, shot/impact deletion, mechanics exclusion, adjacent program rejection and allocation checks pass.
- Reproduced the obsolete verifier expectation of11 live presentation reads (actual0). Granted verifier now checks every actual installed operand against the exact native program step and all11 unique operands, plus zero presentation reads. An attempted raw spritemap assertion was corrected because this presentation path intentionally uses a placeholder pointer and identifies artwork by installed operand; production behavior was unchanged.

## Batch 20: Draygon BG2 collision-component identities

Three more definitions converted;58 complete and167 unchecked. No retained exception.

- Removed first-body, empty-body and second-body singleton component arrays. A calculated view emits the sole zero-offset component for each valid BG2 frame. Facing selects the body list for phases0..7 and16; other phases select the empty hitbox list. Existing OAM identities enumerate no collision components and keep their rejection contract.
- The view preserves length, indexing, foreach and explicit oracle ToArray use; only the BG2 verifier's explicit span binding needed adaptation. Runtime collision callback ordering is unchanged. Eight stored rectangles remain independently unresolved, with no exemption.
- Evidence: pinned bankA5 `ExtendedSpritemap_Draygon_A` `$A31B` and `ExtendedSpritemap_Draygon_3A` `$A643`, seventeen ten-byte frame records per facing; native hitbox lists `$AA95/$AAC7/$ABAB`.
- Confirmation: Verification build passed (1433 warnings, zero errors); `--lookup-stream-4` passed. Existing BG2 fixture retains all34 native component counts/offsets/list identities, all three hitbox lists and rectangle/callback comparisons, guarded actual shot callback dispatch and invalid-frame rejection. Existing48-frame OAM fixture still passes its empty-component and callback assertions.

## Batch 18 checklist reconciliation

The AddBabyCrawl factory was removed, but its independent ten-tick hold remains required. The entry is unchecked; structural conversion alone does not complete its values.

## Coordinator timing re-audit

MamaTurtleInstructionProgramDefinitions.Words and LowerNorfairRioInstructionProgramDefinitions.Words are reopened. Native value agreement confirmed behavior but did not justify independent timing choices. The integrated stream now has57 complete and168 unchecked; uniform AddBabyCrawl and presentation-address conversions remain complete. Source corrections isolate the exact pending timing inputs; integrated Verification1442warnings/zeroerrors and focused stream4 checks pass. No retention exception is granted.

## Batch 46: named room graphics-theme resource selection

One definition converted; 58 integrated complete and 167 unchecked. Installed block, character and palette payloads remain independently required.

- Removed the29-entry definition array. Named graphics themes select named block, character and palette resources through cases; shared Crateria/Wrecked Ship/Norfair/Ceres/station resources and independent palette variants are explicit. The definition identity calculates from the first nine-byte record plus9 times the native selector.
- Each theme and source member documents its native symbol/address. These are mutually exclusive resource identities, not combinable flags. This converts semantic artwork-resource dispatch under the user's case-statement allowance; it makes no claim that artwork contents derive from a theme name.
- Evidence: pinned bank8F Tileset_Table_0_UpperCrateria through Tileset_Table_1C_Draygon at E6A2..E7A6 and selector table E7A7..E7DF. Each native record contains three24-bit resource identities.
- Confirmation: Verification build passed (1433 warnings, zero errors); --lookup-stream-4 passed. Focused checks compare all116 native pointer/resource fields across29 themes, including shared-source and independent-palette choices, and preserve invalid1D/7F/FF rejection. No room execution or room sweep was used.

## Batch 47: derive compressed background source identities from commands

One definition converted; 59 integrated complete and 166 unchecked. LibraryBackgroundProgramDefinitions command payloads remain independently required in their owning stream; this conversion does not modify or dispose of them.

- Removed the separate58-address lookup. Membership walks DecompressToWorkRam operands; the calculated immutable list enumerates successive distinct source addresses in ascending order without caching a second source table. Count and indexing derive from that same view, preserving IReadOnlyList behavior and index rejection.
- Evidence: native bank8F library-background command lists selected by retail states contain the compressed source identities as command0004 operands. The independent import-time LibraryBackgroundSourceInventory scanner reads those original operands and yields exactly the previous58 distinct addresses. Other transfer command operands are excluded.
- Confirmation: final Verification build passed (1193 warnings, zero errors); --lookup-stream-4 passed. Focused checks compare the complete native58-source set, count, ordered enumeration and every indexed value; all source memberships pass, adjacent interior bytes and absent identities reject, and negative/past-end/extreme indexes retain their exception type. Confirmation only walks source metadata, without room execution or gameplay discovery.

## Batch 48: message and controller-button semantic dispatch

Three definitions converted; 62 integrated complete and 163 unchecked. Message text, tile/pixel payloads and independently editable presentation fields remain required.

- The 29 message records now select named setup/draw routines and named presentation identities by GameplayMessageId. Content-boundary records 1B and 1D remain explicit, with native pointer identities preserved so diagnostic content sizing remains exact. XML identifies every native payload symbol/address. This metadata currently has only diagnostic callers in this worktree; native-triple proof is not claimed as evidence of a production dependency.
- Button glyph selection now executes ordered named controller-mask cases. The exact A/B/X/Y/Select/L/R priority survives malformed multibit bindings, and no-match still selects the native blank. Message-specific glyph patch positions now dispatch by named message ID; the full original 1..27 domain, including dummy boundary, and rejection outside it remain exact.
- Granted GameplayMessageBoxState changes are limited to its two selector bindings. The actual patch operation still writes only the selected cell in the supplied tilemap; no installed message content or neighboring edit is replaced.
- Evidence: pinned bank85 MessageDefinitionsPointers at869B..8748; DrawSpecialButton_SetupPPUForLargeMessageBox BIT tests at83D4..8407 and glyphs8426..8435; Special_Button_Tilemap_Offsets at8749..877E.
- Confirmation: final Verification build 1218 warnings/zero errors; --lookup-stream-4 passed. New focused checks compare all87 native definition words and bounds, all65536 binding values against native BIT immediates/glyph words through both catalog and actual message resolver, all27 patch offsets and every other byte-ID rejection. Actual PatchConfiguredButton preserves all192 supplied cells except its one native destination for each accepted identity. No unrelated full message lifecycle fixture was run.
- Integration dependency: ResourceAudit/MessageClosedContractDefinitions.cs line10 hashes GameplayMessageRomData.cs. Coordinator owns its refresh; no audit file was edited here.

## Batch 50: derive notice text rectangles from imported glyph rows

Three definitions converted: 65 integrated conversions, 160 unchecked. Notice text/template/border payloads remain independently required.

- Removed the three arrays of fixed rectangle records. The stock-region view scans the already imported 32-cell rows for supported visible glyph runs. Single interword spaces remain inside a phrase; wider gaps and nontext cells split runs, leaving the independent YES/NO choices and excluding arrow graphics. Row, column and width derive from the supplied tilemap instead of duplicating its layout in another table.
- Granted consumer file is csharp/src/SuperMetroid.AssetExtraction/GameplayMessageNoticeExtractor.cs. Its only change passes the already-read template to StockTextRegions. The document shape, text extraction, palette checks, selections and independent edit behavior are unchanged.
- Evidence: pinned bank85 MessageTilemaps map/energy/missile completion content at917F/923F/92FF and save question at93BF. Original geometry is exactly (0,8,15),(2,10,10) for map/energy; (0,8,14),(2,10,10) for missiles; and (0,8,14),(1,8,8),(3,10,3),(3,19,2) for save. Both save owners share the latter source.
- Confirmation: Verification build includes the AssetExtraction dependency; final build1193 warnings/zero errors. --lookup-stream-4 passed all14 original rectangles across five notices, complete native border/content output and actual installed initial tilemaps, both real YES-to-NO transitions, and a live MAP TEST edit with every cell outside its rectangle unchanged. Phase/window radius and exact stock restoration remain intact with cartridge reads forbidden during actual message use. No stale lifecycle verifier was reused as a native oracle.
- Integration dependency: ResourceAudit/MessageClosedContractDefinitions.cs line29 hashes GameplayMessageNoticeDefinitions.cs. Coordinator owns its refresh; no audit file was changed here.

## Batch 52: Ridley tail terrain traversal and physical probe offsets

Two definitions converted: 67 wholly converted, 158 unchecked.

- Replaced the five joint-index samples with direct reverse traversal from the tail tip through joint two. Replaced the five Y-offset samples with the physical distinction between the tip (16 pixels) and ordinary joints (18 pixels). The seven-segment/null-level gates, unchecked word addition, room bounds and first-hit return remain intact.
- Evidence: pinned CheckForTailCollisionWithFloor at A6:B7E7..B84C explicitly probes segments 6,5,4,3,2 in that order. ADC immediates at B7F2/B806/B81A/B82E/B842 hold16/18/18/18/18. This is an unrolled sequence of collision operations, now represented directly as iteration and geometry rather than local lookup arrays.
- Confirmation: Verification root integration build1444 warnings/zero errors; --lookup-stream-4 passed. New focused actual-method checks isolate each of the seven segments against two solid rows and five boundary/wrapping Y coordinates (70 cases), obtaining expected offsets from the original native ADC operands. Segments0/1 stay excluded; missing terrain and incomplete-tail gates remain false. Static review confirms descending first-hit order. No gameplay discovery was performed, and no source-hash dependency was found for this partial.

## Batch 53: Ridley initial angular spacing and circular tip identities

Two definitions converted: 69 wholly converted, 156 unchecked. Independent tail distances, wing cadence, OAM attributes and image payloads remain required.

- Initial angles now calculate a quarter-turn base plus link index times the native ideal inter-segment separation. New granted Game/RidleyTailDefinitions.cs documents the native base and separation; both encounter initializers use the same named separation. The seven independent distance values remain untouched and unchecked.
- Tail-tip identities now calculate the address of a consecutive seven-byte one-object OAM record. Native direction sector12 points left at the first record; reversing the sixteen-sector circular index preserves every direction and wrap. The frame exporter enumerates this same view instead of a replacement pointer array. Named XML constants identify the native base, record layout and direction orientation.
- Evidence: InitializeTailParts at A6:D2D6 sets ideal separation16 at D2FD; original D38A..D397 angles are4000 through4060 by16. RidleyTailTipSpritemapPointers at DCBA..DCD8 selects sixteen records DCDA..DD49; each contains one two-byte count plus one five-byte OAM object. This derives identities only, not the independently supplied OAM or pixel content.
- Confirmation: Root integration Verification build1444 warnings/zero errors; --lookup-stream-4 passed. Actual initialization matches all seven original angle words and preserves distance/direction/stagger fields; native separation immediate matches. Every sixteen-direction identity and original one-object record count matches, the complete sorted supplemental identity enumeration is unchanged, and invalid/extreme direction selectors retain InvalidDataException. Initial-angle bounds contain exactly seven links. No ResourceAudit source-hash reference was found for the changed existing files.

## Batch 54: Ridley wing stroke and facing identity selection

One definition converted: 70 wholly converted, 155 unchecked. The independent speed-to-wing-timer values and OAM/pixel payloads remain required.

- Replaced twenty stored pointers with the ten-phase wing cycle: six descending elevations and the four interior elevations in reverse. Six named native pose identities express fully/mostly/slightly raised and lowered wings. The opposite-facing records preserve the same sizes and order, so their identities derive by the native block displacement. No completed animation-pointer array is cached.
- Evidence: DrawRidleyWings pointer lists A6:DB02..DB28 describe the exact two mirrored cycles. Left-facing records DD4A/DD6A/DD85/DD96/DDA7/DDC2 have right-facing counterparts DDE2/DE02/DE1D/DE2E/DE3F/DE5A, each displaced98 hexadecimal bytes. XML ties every named pose to its native symbol and address.
- Confirmation: Root integration Verification build1444 warnings/zero errors; --lookup-stream-4 passed all20 original wing identities, complete sorted supplemental frame enumeration, and negative/past-end/extreme selector rejection. Previous sixteen-direction tail identity and initialization checks remain passing. This confirms selected identities and export membership without claiming an independent artwork-payload conversion.

## Batch 55: Ridley grab and release movement policy

One definition converted: 71 wholly converted, 154 unchecked.

- Replaced the28-byte movement flag table with named SamusMovementType cases classifying ordinary, morphed, or immune movements. New granted Game/RidleySamusInteractionDefinitions.cs exposes the actual consumed operations: CanGrab and ReleaseIntangibilityFrames. Unknown byte selectors preserve no-grab and ten-frame fallback.
- Static source audit found exactly two references to the former array: SamusMovementUsesRidleyGrab masks80 and ReleaseNorfairRidleyGrab masks40. No field exposes the table or consumes its lower six bits, so reproducing unused FF low bits is unnecessary. Both consumers now call the named operations directly; null-Samus behavior and all release state writes remain intact.
- Evidence: A6:BCF1..BD1F CheckIfSamusMorphedSpinJumpingDamageBoosting indexes the named movement handler. BIT uses the target byte as the high byte, translating bit7 to carry and preserving bit6 as overflow. ReleaseSamus at BC95..BCA0 chooses immediate6 at BC99 when overflow is set, otherwise10 at BC9E. Native data explicitly classifies Morph/Spring Ball and unused glitch-ball families together.
- Confirmation: Root integration Verification build1444 warnings/zero errors; --lookup-stream-4 passed. All256 underlying byte selectors match the original28 native rows or the previous managed out-of-domain policy. Actual grab and release methods match native classification/duration for all253 real poses with cartridge reads forbidden; release clears GrabState and sets TailWhipRequest/TailFunctionIndex exactly. Null-Samus no-grab/standing-release fallbacks pass. No adjacent-table emulation or new raw flag API was introduced.

## Batch 58: calculated Ridley collision composition and identity domains

Three definitions converted: 74 wholly converted,151 unchecked. FrameKeys/ListKeys/RightBase are complete; LeftBase, Frames and Lists remain required. Exact remaining component basis is four left-facing origin pairs(15,22),(-8,7),(16,0),(-3,-24) and forward Y=-6. All independent rectangle bounds remain required.

- Frame identities calculate from consecutive native record extents; hitbox-list identity enumeration derives sorted distinct keys from the independently required rectangle payload. Right-facing origins reflect left-facing X and preserve Y, while right-facing list identities use the native matching-block displacementCA. No duplicate right-origin array remains.
- Replaced eleven expanded component arrays with a calculated view selecting named facing/mouth/leg states. Leg variants advance across one-rectangle lists; mouth variants advance across two-rectangle lists. Forward facing selects its centered single component. Its independent Y offset and four left-side geometry pairs are explicitly unresolved inputs, so Frames is not counted complete merely because its container disappeared.
- Evidence: A6:E983..EAD6 has ten two-byte-header/four-component records of34 bytes; EAD7 is the one-component forward frame. ExtendedSpritemap_Ridley_* symbols name each facing, mouth and leg state. Left list groups begin EAE1 (head), EB2F (legs), EB59(hand), EB67(torso); their right counterparts are displacedCA bytes. Original component order remains legs/hand/torso/head, with forward alone.
- Granted CeresRidleyComposition consumer change is only the inferred calculated-view binding. OrdinaryCombat already used inferred foreach and required no edit. Program.RidleyCollisionDefinitions binding changes retain all assertions.
- Reproduced fixture prerequisite failures: first, its syntheticFFFF enemy invoked the removed generic ROM walker and threw at OrdinaryCombat.cs3109; after replacing that oracle under coordinator approval, the fixture's instance-only reflection lookup failed because the Ceres helper is now static. The corrected reference independently traverses original ROM components/rectangles/callbacks with pinned A0:9ADF..9B78 touch and9C43..9D20 shot edges; static reflection now reaches the same real Ceres helper. These are fixture repairs, not production collision fixes.
- Confirmation: root integration Verification build1444 warnings/zero errors; --lookup-stream-4 passed all11 frames,41 native components,17 lists,24 original rectangles and11232 actual touch/shot boundary comparisons, plus existing actual Ceres overlap comparisons. No runtime cartridge reads occur. Additional exact identity checks derive expected roots/lists from original record lengths/operands, compare all65536 pointer memberships, preserve sorted/indexed enumeration and out-of-range rejection. No closure source-hash reference was found for the changed production files.

## Batch 59: precise bounded claw instruction-word disposition

One narrowly retained definition:74 wholly converted, one retained,150 unchecked. No real claw geometry is exempted.

- Coordinator independently approved only ExistingOutOfRangeWindow's six native instruction words, after checking the ROM and pinned disassembly: A6:B9E1..B9EC bytes AF28787EF01F8512100449FF, little-endian words28AF/7E78/1FF0/1285/0410/FF49.
- Exact native identities: B9E1 LDA.l $7E7828 occupies AF28787E; B9E5 BEQ +1F occupiesF01F; B9E7 STA $12 occupies8512; B9E9 BPL +04 occupies1004; B9EB EOR #FFFF begins49FF (the final high operand is outside this window). These unrelated opcode/address/branch bytes have no functional relationship to six physical claw positions. Generating them from the ported behavior would require re-encoding the same native machine instructions, which is nonsense for the geometry conversion.
- Managed ReadY explicitly bounds the observation to min(feetDistanceIndex>>1,8): inputs0..5 select the three calculated geometry words; inputs6..FFFF can only observe these six words, with the last clamped. RoomEnemySystem.GetNorfairRidleyClawY is the sole production reader. This preserves an existing managed API boundary; it claims neither a native clamp nor full native memory/CPU emulation.
- Ordinary X and Y geometry remain calculated from facing and foot elevation. No other adjacent code, coordinates, artwork or payload receives an exemption.
- Confirmation: documentation/disposition only; unchanged six values and consumer behavior passed the immediately preceding lookup-stream-4 run. The existing scoped verifier compares all65536 ReadY inputs to the native bounded nine-word window and confirms actual ordinary carry/collision behavior. No new executable test or source change was needed.

## Beam palette partial integration: aliases and highlight relationships

Integrated worker commits54ec3420b,6ed815fd4 and912ab27e1. Removed the obsolete nonuniform-artwork retention rationale. BeamPaletteCatalog.palettes remains unchecked; no new exception or completed definition is claimed. Stream checkpoint remains74 converted,one retained,150 unchecked.

Twelve expanded runtime rows (192 colors) now retain43 independent stock inputs and derive family aliases, common Power colors0/1, non-Ice black slots9..14, Wave/Plasma highlight midpoints and Spazer highlight channels(G,G,R) from Plasma. The latter formulas match pinned90:C42B/C42D/C42F,C44B/C44D/C44F,C46B/C46D/C46F. Installation preserves independently supplied deviations, including unchanged dependent colors when a basis input is edited; runtime calculates relationships per access.

Required inputs: Power0..8 and15; Ice2..15; Wave and Plasma2..5,7,8,15; Spazer2..4,8,15. These43 values remain unresolved. Palette content receives no exemption.

Root Verification build1444 existing warnings/zero errors; --lookup-stream-4 confirms every native output across12 selections, targeted independent/common/alias/black/endpoint/midpoint/cross-family edits, unchanged neighboring CGRAM and stock immutability, and invalid selectors. Refreshed BeamPaletteCatalog source closure hash; ResourceAudit build zero warnings/errors.


## Batch 49: enemy-name family numbering and shared lexical stems

Mixed disposition for RoomEnemySpawnNameDefinitions.Records: 74 wholly converted, one mixed, one retained,149 unchecked. The exact chosen-character residual below is approved; all record metadata and established lexical relationships are calculated.

- Twenty-one of the ninety referenced names now compose their native labels per access. Twelve pirate labels combine BATTA, the wall/ninja/walking family digit, and the exclusive color suffix. Three Kihunters use HACHI plus the family digit. Ripper2/Shutter2/RobotNoPower append their numeric variant to the original stem; H/M Zoomer and S Sidehopper reuse their shared lexical stems. No calculated full-name lookup is cached.
- Native identity enumeration now walks fourteen-byte record positions and emits only the exact referenced name domain, preserving original order and holes. Existing ten-character space padding, little-endian five-word encoding and numeric debug-spritemap ordinal remain calculated.
- Exact remaining chosen lexical strings: ATOMIC, BOTOON, BOYON, DESSGEEGA, DORI, DRAGON, EBI, EYE, NAMI, FISH, GAI, GAMET, GEEGA, GERUDA, HAND, HIBASHI, HIRU, HOLTZ, HOTARY, KAGO, KAME, KAMER, KANI, KOMA, KZAN, LAVAMAN, MELLA, MEMU, MERO, METALEE, METMOD, METROID, MULTI, NDRA, NOMI, NOVA, OUM, OUMU, PIPE, POLYP, PUROMI, PUU, PUYO, REFLEC, RINKA, RIO, RIPPER, ROBO, RSTONE, SABOTEN, SBUG, SCLAYD, SDEATH, SHUTTER, SIDE, SKREE, SPA, SQUEEWPT, STOKE, TOGE, VIOLA, WAVER, YARD, ZEB, ZEBBO, ZEELA, ZOA, ZOOMER, FUNE; plus BATTA/HACHI, color suffixes Br/No/Na/Ma/Tu and prefix letters H/M/S. Only these exact chosen characters have the independently reviewed nonsense disposition: the native consumer copies literal text without interpreting it as behavior, and no managed AI/physics quantity can determine the selected spelling (for example BOTOON rather than Botwoon). No entire name-table or other text/artwork exemption is granted.
- Evidence: bankB4 quoted EnemyName_* records, notably DDB3/DDCF pirate families and E205..E2AD colored variants, DEA1/E2C9/E2D7 Kihunters, and the named stem/alias identities documented in the catalog. Record_EnemySpawnData at A0:8923..893F copies five literal ASCII words,8941 reads the separate numeric index, and8948..8968 publishes the snapshot. It does not parse the chosen lexical spelling into AI or physics.
- Confirmation: Root integration Verification build1444 warnings/zero errors; --lookup-stream-4 passed the existing independent native90-pointer ordered-domain proof, all540 original text/index words and absent/unaligned identity rejection. No exploratory gameplay or new unrelated fixture was added. No ResourceAudit source hash reference was found for this catalog.
- Managed consumer confirmation: RoomEnemySystem.InitializeSlot publishes ReadSpawnNameWords(definition) into RoomEnemySpawnSnapshot.NameWords; the reader returns the compiled words (or fixture data) opaquely. RoomEnemyData defines only Word0..Word4 and Word6. Static production usage has no character parsing into AI or physics. Coordinator independently confirmed both native copy instructions and these managed consumers before approving the exact lexical boundary.

## Integrated Spore Spawn collision components

Frames is complete: dead/closed roots select a single zero-offset head; seven opening roots advance the head layout and oscillate the inner point through B/C/D/C; three fully-open roots hold the final head and cycle B/C/D. Native A5:EE65-EEE5 and EF3D-EF61 establish component count, order, zero offsets and list identities. The intervening unused roots remain rejected. Lists remains unchecked; this does not exempt its hitbox coordinates or callback payloads.

Root Verification build passed with 1445 warnings and zero errors. --lookup-stream4-spore-collision confirms all 12 roots and 12 referenced lists, 2376 actual touch/shot comparisons against an independent native-data walker, source-read denial and invalid root/component boundaries. The obsolete generic runtime cartridge oracle was replaced with a fixture-only native walker; production execution stays guarded. Overall checkpoint: 510 converted, 15 justified retained/mixed, 603 pending.

## Integrated room sprite program dispatch

InstructionPointers is complete as meaningful program dispatch. Native B4:BC65-BC77 consumes the object-kind argument, selects its program at BDA8-BE23, installs that program and loads its initial timer. The switch preserves all 62 mutually exclusive native program identities, with source symbols and addresses documented on catalog members, including unused native programs. This is the permitted giant-case conversion; it does not exempt any program timing, visual selectors or artwork payloads.

Root Verification build passed with 1445 warnings and zero errors. --lookup-stream4-sprite-dispatch confirms all 62 native entries and actual guarded spawns, exact initial instruction/timer/visual, retained kind/position/graphics arguments and invalid selectors. Overall checkpoint: 511 converted, 15 justified retained/mixed, 602 pending.

## Integrated title-card layout (partial)

TitleSequenceInstructionDefinitions calculates progressive text addresses from cumulative two-byte OAM headers and two five-byte objects per letter. Explicit structural groups preserve Metroid's split lists around the unused debug copyright map and its blank-space reveal. Named scene triggers and deletion follow each card. Native 8B:A03D-A0C8 and the corresponding bank-8C lists establish the layout.

Program remains unchecked: initial hold 60, reveal cadence 8, final holds 45/120 and logo hold 32 remain required independent values. The worker's full-conversion claim is not accepted; no timing or artwork exemption is granted.

Root Verification build passed with 1445 warnings and zero errors. --lookup-stream4-title-card confirms all 140 native bytes, 139 aligned/unaligned word windows, outer bounds and trailing partial-word rejection. Refreshed the cinematic source-audit hash; ResourceAudit builds with zero warnings/errors. Counts unchanged: 511 converted, 15 justified retained/mixed, 602 pending.

## Integrated title sprite identities and Baby pages

TitleSpriteDefinitions.Pointers is complete: enumerate the 31 distinct addresses in native sorted order directly from calculated title-card layouts, including copyright and logo, without caching a pointer table. TitleSequenceRomData.Vram.BabySourcePages is complete: min(frame, 4-frame) calculates the four-phase 0/1/2/1 source-page cycle. Pinned 8B:A131-A140 and the referenced DMA descriptors establish the cycle, page size and destination. Artwork and title program timing remain independently required.

Root Verification build passed with 1445 warnings and zero errors. --lookup-stream4-title-identities confirms the independently decoded set and order of all 31 native selectors, all four source pages, native DMA sizes/destinations and invalid phase boundaries. Both cinematic source hashes refreshed; ResourceAudit builds with zero warnings/errors. Overall checkpoint: 513 converted, 15 justified retained/mixed, 600 pending.

## Integrated HUD digit calculation

healthDigits and ammoDigits are complete: both native rows at 80:9DBF/9DD3 use cyclic glyph ordinal (digit + 9) % 10 with palette-three and priority bits. Stock catalogs store no digit words; only independently edited words are retained, separately for health and ammunition. Glyph pixels remain independently required. This integration excludes the queued HUD-anchor conversion.

Root Verification build passed with 1445 warnings and zero errors. --lookup-stream4-hud-digits confirms all 20 native words, all ten numerals through actual two-place health and three-place ammunition drawing, zero stock entries, separate edited palettes surviving serialization with one override each, unchanged neighboring digits and invalid-digit rejection. Presentation source hash refreshed; ResourceAudit builds with zero warnings/errors. Overall checkpoint: 515 converted, 15 justified retained/mixed, 598 pending.

## Integrated HUD anchor geometry

EnergyTankByteOffsets, ItemByteOffsets, autoAnchors and energyTankAnchors are complete. Fourteen tanks occupy two seven-cell rows, bottom first; item offsets follow the three-cell missile icon then two-cell icons with blank gaps; AUTO occupies two columns over three rows. Native 80:9CCE-9CE9, 9D6E-9D77 and destination stores 9B64-9B87 establish those layouts. Installed catalogs retain only independent position edits. Artwork, icons and AUTO cell payloads remain separate required entries.

Root Verification build passed with 1445 warnings and zero errors. --lookup-stream4-hud-anchors confirms 14 tank/five item offsets, six native AUTO destinations, extracted tank positions, actual drawing/clearing, zero stock anchors, four serialized position overrides, unchanged neighbors, overlap rejection and bounds. Removed span APIs have no remaining source consumers. Presentation source hash refreshed; ResourceAudit builds with zero warnings/errors. Overall checkpoint: 519 converted, 15 justified retained/mixed, 594 pending.

## Integrated AUTO cell reflection and palette (partial)

GameplayHudPresentation.autoFull and autoEmpty remain unchecked. Native 80:998B-99A1 derives the bottom row by vertical reflection of the top row and empty-state cells by toggling palette bit 12. Eight cells now calculate from four independent full-state glyph/style words; those four words remain required without exception. Independently supplied edits retain exact values for both states and all neighbors.

Root Verification build passed with 1445 warnings and zero errors. --lookup-stream4-hud-auto-cells confirms all 12 native cells through actual drawing, four stored basis words with zero stock overrides, serialized independent palette/tile/flip/priority edits, all unchanged neighbors and invalid cell bounds. Presentation hash refreshed; ResourceAudit builds with zero warnings/errors. Counts unchanged: 519 converted, 15 justified retained/mixed, 594 pending.

## Integrated fly identities and escape effect cases

FlyVisualDefinitions.Frames / literal at L15 is complete: four consecutive seven-byte single-object OAM records at A2:B1E8-B203 calculate the frame identities and preserve editable names. Independent OAM artwork remains required.

ZebesEscapeExplosionDefinitions.Definitions remains unchecked. Named effect cases preserve the 8F:C1D6/C1DE sprite/sound pairs and group repeated effects, but bucket weights/order and the two sound-bearing choices remain independent required inputs. The worker's claim that both entries were complete is not accepted. No retention exception is granted.

Root Verification build passed with 1445 warnings and zero errors. --lookup-stream4-fly-escape confirms all four fly addresses against native instruction operands, bank/names/one-object counts, all 16 escape bytes and 128 actual random/inherited spawn handoffs with positions, sounds, source-read denial and invalid domains. Overall checkpoint: 520 converted, 15 justified retained/mixed, 593 pending.

## Integrated Skultera mirrored layouts

PresentationWords is complete: paired swimming/turning blocks calculate 32 control positions and 22 visual addresses using native record widths at A3:902A-9095. Layer callbacks, loop targets, turn-completion callback and sleep retain their named behaviors. Words remains unchecked: swimming hold 14 and four turn-half holds 13/10/8/6 remain required independent inputs. Mirroring removes repetition without excusing those inputs; the worker's corrected timing treatment is included, and root review also tracks swimming cadence. No exception is granted.

Root Verification build passed with 1445 warnings and zero errors. --skultera-instruction-mechanics confirms all 32 native words, both swimming loops and turns, layer/completion callbacks, terminal sleeps, 22 compiled visual selectors, source-read guards, bounds and allocations. Overall checkpoint: 521 converted, 15 justified retained/mixed, 592 pending.

## Integrated Spore Spawn projectile layout

PresentationWords is complete: native 86:DC00-DC58 record widths and callback boundaries calculate 28 control positions and 17 visual addresses for ceiling emitter, release, airborne spore, stalk and shot/drop programs. Words remains unchecked. Release holds 1/6/16/6/1, shot holds 1/3/6/5/5/5/6, closed-emitter hold 1 and airborne/stalk holds 5 remain required independent inputs. Includes the worker's timing correction and root's explicit tracking of the fixed holds. No exception is granted.

Root Verification build passed with 1445 warnings and zero errors. --spore-spawn-projectile-instruction-mechanics confirms all 28 native words, all three real producers, release/loop/shot instruction paths, spawn/drop/property callbacks, all 17 actual presentation operand selections, zero source reads, bounds and allocations. The path fixture advances timers explicitly; this confirms instruction selection and native duration words, not a separate elapsed-time simulation. Overall checkpoint: 522 converted, 15 justified retained/mixed, 591 pending.

## Integrated Stoke and Skree/Metaree layouts

Four presentation-address entries are complete: Stoke (12), Stoke projectile (2), Skree (11) and Metaree (11). Direction/species block structure, timed-record widths, named callbacks, loop targets and sleep tails calculate their positions. Native A2:8932-897D, 86:DB0C-DB17, A3:8910-894D and C65E-C69B establish the layouts.

All four mechanics entries remain unchecked. Stoke walking holds 8/16/8/8 and attack holds 16, projectile cadence 16, and shared Skree/Metaree idle 10, preparation 16/8, dive 2 and stop 1 remain required independent inputs. The worker's walking-timing correction is included; root review also keeps the fixed cadences open. Four entries complete, not the worker's original eight. No exception is granted.

Root Verification build passed with 1445 warnings and zero errors. Focused stoke-instruction-mechanics, stoke-projectile-instruction-mechanics and skree-metaree-instruction-mechanics checks pass: 26/4/40 native control words, all directional walks/attacks/spawns and projectile loops/deletion, all eight Skree/Metaree programs and callbacks, 36 compiled presentation selections, guards/bounds/allocations. Projectile path checks explicitly advance timers and do not claim separate elapsed-time simulation. Overall checkpoint: 526 converted, 15 justified retained/mixed, 587 pending.

### Root correction: Baby Turtle crawl timing remains required

Reopened `MamaTurtleInstructionProgramDefinitions.AddBabyCrawl / literal at L202`. Removing its factory and repeating a scalar did not resolve the chosen ten-tick duration: TryCrawl returns10 for fields2/6 in each ten-byte record. Native A2:8B82/8B86 through8BC8/8BCC and8C74/8C78 through8CBA/8CBE retain32 such samples. Structure/presentation conversions remain intact, but this timing entry has no accepted derivation or impossible/nonsense disposition. No gameplay code changed for this correction.

### Root integration: Oum collision-list addresses

List pointers now derive from the first native record and successive record extents: two-byte rectangle count plus twelve bytes per rectangle. No replacement list-pointer or rectangle-count array is introduced. Lists geometry/callback payload remains required. The pre-existing FrameKeys cache and builder are also removed: an immutable view calculates the ten-byte frame stride on access without changing current consumer bindings.

Root build passed (1445 warnings, zero errors). `--lookup-stream-4-oum-layout` confirms all30 native frame operands, all66 selected rectangle coordinates/touch/shot callbacks, enumeration coverage, calculated frame-index boundaries and complete ushort address rejection. This confirms selection/data identity, not gameplay collision behavior. ListPointers is complete; no retention exception added. Inventory:534 converted,15 justified retained/mixed,579 pending.

### Root integration: elevatube export enumeration and Ridley timing correction

AllDraws now yields its single export definition without storing a wrapper lookup. Draw remains required for visual block180. The source closure hash is refreshed. Root final Verification rebuild passed25warnings/zero errors after correcting a verifier method-name typo; ResourceAudit passed0/0. `--lookup-stream-4-elevatube` passes native delay/draw/sound/delete, source guards, independent artwork edits, physical/sound isolation, stock repair and invalid input checks.

Ridley PartRecords is reopened: equal-sized initializer addresses and part parameters derive correctly, but tail lifetime base72, shared step8, body base40 and torso128 are independent choices without an accepted disposition. Prior claims that all columns were complete are removed. This status correction changes no gameplay values. One completion and one reopening leave534 converted,15 justified retained/mixed,579 pending.

### Root integration: procedural Ridley breakup spawning

The native A6:C932-C986 sequence is executable spawning: tail tip inward, then Wings, Legs, Torso, OpenHeadAndNeck and Claw. Runtime allocation remains inside each ordered call, preserving partial failure, RNG assignment and the one-shot guard. No replacement spawn-order array is retained. PartRecords lifetime inputs72/8/40/128 remain required; their earlier reopening is unchanged.

Root build passed (1445 warnings, zero errors). `--lookup-stream-4-breakup-order` passes the native twelve-call order, original32 fragment initializations and ten actual death-effect spawns, plus full/partial/empty pool cases, exact RNG-to-fragment assignment, native lifetimes and no repeated allocation/RNG after success or failure. Expected allocation failures are caught inside the fixture. SpawnOrder is complete with no retention exception. Inventory:535 converted,15 justified retained/mixed,578 pending.

## Batch 63: Baby capsule reflected transfer phase

One definition converted: 109 wholly converted, one mixed, one retained, 114 unchecked. Transfer artwork and other animation timing/content fields remain independent and are not completed by this change.

- Removed the runtime four-pointer array. Granted CeresMode7TransferDefinitions.BabyFrameForPhase calculates the reflected ordinal min(phase, 4-phase) across the three consecutive, equally sized two-transfer records. The caller retains its exact NMI gate and advances/masks the phase before selecting the transfer.
- Native evidence: A6:ACBC..ACD9 gates on NMI&3, increments then masks the stored phase, and selects pointers ACDA..ACE1. These are ACE2, ACF5, AD08, ACF5. Each record contains two nine-byte low-byte VRAM descriptors and its zero terminator. The calculation preserves the 0,1,2,1 reflected phase without a replacement pointer lookup.
- Confirmation: Verification build 1433 warnings/zero errors; --lookup-stream-4-baby-phase passes all four native identity operands and invalid-domain rejection. A focused invocation of the actual getaway handler across one 16-NMI cycle confirms advance-before-select, unchanged phase on other NMIs, exact four Baby tilemap bytes from original transfer descriptors, and preservation of character high bytes. Palette assets are supplied explicitly and the runtime bus denies cartridge reads. No other timing or transfer payload is exempted.
- No ResourceAudit source-hash reference was found for either changed production file. The only newly granted path is csharp/src/SuperMetroid.Core/Game/CeresMode7TransferDefinitions.cs; its existing transfer arrays/content are untouched.

## Batch 66: one shared Ridley tail rest geometry

One duplicate mapping converted: 110 wholly converted, one mixed, one retained, 113 unchecked. CreateInitialRidleyTailSegments/distances remains required for three chosen base/shaft/tip lengths, and maximumDistances remains required separately.

- Initialization and shrink thresholds now consume RidleyTailDefinitions.RestDistance. It selects the physical base, five shaft links or tip and returns their shared rest separation. Removed both repeated seven-element local arrays; the three chosen lengths2/8/5 pixels are explicitly documented required inputs, not claimed as calculated or exempt.
- Native evidence: A6:D37C..D389 installs seven8.8-pixel separations0200/0800/0800/0800/0800/0800/0500. The six shrink instructions load exactly the matching initialized separations at CF7F, CFB5, CFEB, D021, D057 and D08D. The base is excluded from updates. This establishes the neutral mapping as a functional alias of initial rest geometry; it does not explain or exempt the three chosen lengths.
- Confirmation: Verification build 1433 warnings/zero errors; --lookup-stream-4-tail-rest passes all seven native initialized distances and six independently read shrink immediates, plus the existing initial angle/direction/stagger checks. Actual updater checks preserve below/equal/above threshold behavior, subtraction by native half-pixel decrement, unchanged base, and the existing ordering that clears a passed target then still extends/clamps on that frame. Invalid rest indices preserve IndexOutOfRangeException.
- No ResourceAudit source-hash reference was found for the three changed production files. The separate maximum-distance array and all independent angular/movement parameters are unchanged.

## Batch 67: calculated Japanese subtitle placement

One layout definition converted: 111 wholly converted, one mixed, one retained, 112 unchecked. EndingFontAtlas.transfer remains required; this change does not exempt the chosen Japanese phrase or its character pixels.

- New granted EndingTextLayoutDefinitions calculates a centered thirteen-glyph strip across two32-cell screen rows. The first five glyphs occupy trailing atlas columns5B..5F/6B..6F; the continuation starts after the copyright digit pair at80/90. Lower halves are one atlas row beneath the tops. Other screen cells use the existing blank identity.
- EndingTextPresentation no longer stores the64 stock cell words. SubtitleSequence calculates indexing/enumeration and copies cells directly to the consumer's tilemap; only independent supplied differences remain in a sparse override dictionary. The sole existing frontend CopyTo call compiles unchanged. Source document hashing remains identical, and supplied raw words retain their previous unrestricted ushort behavior.
- Native evidence:8C:DF5B..DFDA contains the caption identified by the source as アイテム発見パーセンテージ. Top words atDF6D..DF85 are5B..5F then80..87; bottom words are6B..6F then90..97. These identities follow the16-tile atlas width, the first partial row, and continuation after the copyright digit pair. The phrase occupies columns9..21, exactly floor((32-13)/2), with blank4F elsewhere. Selection of the phrase artwork remains distinct from calculating its placement and atlas traversal.
- Confirmation: Verification build1433 warnings/zero errors; --lookup-stream-4-ending-subtitle passes all64 original words, indexed bounds, enumeration, copy length/untouched suffix and failure-before-write for short output. Targeted edits cover a blank, first glyph, atlas-wrap glyph and lower half while preserving all other cells and the original catalog. The actual installed ending state draws each version to its tilemap and VRAM with cartridge reads denied, then clears it and preserves completion/scroll handoff. No native-constructor replay or unrelated ending behavior was used.
- Closure dependency: ResourceAudit/TextAndMapClosedContractDefinitions.cs references EndingTextPresentation.cs; refresh its source hash and include the new EndingTextLayoutDefinitions.cs calculation dependency centrally. No other ResourceAudit source-hash reference was found.
## Batch 68: direct stock calculation proof and producer-panel decomposition

Partial resultPanel conversion only; counts stay 111 wholly converted, one mixed, one retained, 112 unchecked. This batch also corrects batch67's subtitle atlas traversal before coordinator integration.

- Reproduced an incomplete subtitle calculation: direct comparison at the continuation expected tile80 but got70. The independently supplied override mechanism had preserved visible output by storing sixteen native stock words, so the prior output-only confirmation did not establish a fully calculated layout. The corrected operation consumes five glyphs from the partial5B row, then continues after the copyright digits' two rows60/70 at80. Top/bottom halves retain their16-tile spacing. Source/report wording now describes that actual packed layout.
- Both subtitle and result-panel checks now compare calculated defaults directly with original ROM cells and assert zero stock overrides. The corrected subtitle calculation passes all64 words without sampled fallback values. This is a conversion-proof correction, not a gameplay-output fix; stock output had remained exact.
- resultPanel's288 cached words are replaced by calculated centered credit lines and sparse supplied edits. Glyph height determines line stacking, lower halves use the existing glyph compiler, and arbitrary non-heading template edits remain independent. The supplied PRODUCED BY field retains its fixed original eleven-cell region, padding and precedence over raw template cells.
- Remaining resultPanel inputs are explicit: the supplied chosen heading text (stock PRODUCED BY), chosen labels DEER FORCE, OF and TEAM SHIKAMARU, one blank row between lines, and BG palette identities6/7 for the two large labels. No impossible/nonsense exemption is claimed yet for those lexical/style choices. Font artwork remains separately required.
- Native evidence:8C:DC9B..DEDA places PRODUCED BY on row0, DEER FORCE on rows2/3, OF on row5 and TEAM SHIKAMARU on rows7/8. Small/large heights are1/2, all four lines are horizontally centered, and one blank row separates successive lines. Native8B:E19F..E1B1 copies all240hex bytes opaquely into the cinematic tilemap; managed EndingCreditsState.cs273 copies BuildResultPanel into its post-credit tilemap before upload.
- Confirmation: final incremental Verification build1218 warnings/zero errors; --lookup-stream-4-ending-panel passes all288 directly calculated producer-panel words and64 directly calculated subtitle words, with zero stock overrides in both catalogs. Targeted blank/top/bottom/team edits, supplied short heading with conflicting template content, output snapshot mutation and complete source identity remain correct. Corrected subtitle tests also retain independent edits and actual installed draw/VRAM/clear/scroll behavior with cartridge reads denied.
- Integrate this correction with479318ecb before publishing subtitle completion. TextAndMapClosedContractDefinitions still requires both EndingTextPresentation and the new EndingTextLayoutDefinitions dependency hashes; no other closure source references were found.

## Batch 69: sparse-default proof audit and exact beam basis

Verification-only follow-up; no additional completed entries or production changes. Counts remain 111 wholly converted, one mixed, one retained, 112 unchecked.

- After the subtitle fallback masking was identified, statically reviewed the previous owned sparse-default confirmation methods. Lightning, Tourian grey, title gradient/ambient, Draygon, Botwoon, Power Bomb, HUD anchors/digits/AUTO and Zebes ambient/layer/foreground/whiteout already assert zero stock overrides or the exact independently required stored-basis size/membership. Their existing output comparisons therefore do not silently permit arbitrary extra fallback samples. No unrelated gameplay checks were added or run.
- BeamPaletteCatalog's earlier proof compared selected output and independent edits but lacked its stated basis-storage assertion. Added exact count43 and membership for the documented Power/Ice/Wave/Plasma/Spazer basis: Power0..8/15; Ice2..15; Wave/Plasma2..5/7/8/15; Spazer2..4/8/15. Thus the other149 selected colors cannot remain as stored stock fallback samples.
- The initial new membership oracle accidentally swapped raw Wave/Ice selector numbers; replaced those literals with the existing named SamusBeamFlags identities (Wave1, Ice2). The count43 assertion already passed before that fixture correction; no production values or formula changed.
- Confirmation: final incremental Verification build25 warnings/zero errors; --lookup-stream-4-beam-basis passes exact stored basis plus all192 original selected colors, existing independent input/alias/midpoint/black-slot edits, immutable original catalog, selected CGRAM isolation and bounds. The43 chosen color inputs remain explicitly required; this is proof of the claimed partial conversion, not an artwork exemption.
- Coordinator approved a brief rotation to the isolated Stream1 worktree for the analogous arm-cannon zero-stock-override assertion, preserving this Stream4 checkpoint.

### Integrated producer-credit spacing and lexical review

The nine-row panel distributes remaining rows after two small and two large glyph lines across three gaps. Direct confirmation matches all288 native cells with zero stock overrides. Exact PRODUCED BY / DEER FORCE / OF / TEAM SHIKAMARU wording is narrowly retained as chosen lexical identity: the native consumer copies the glyphs without interpreting them. Palette6/7 and font pixels remain required; resultPanel stays unchecked. Worker commits e1306d8de and4c62bd5fe; root build1445 warnings/zero errors and focused ending-panel confirmation passed. Inventory remains548 converted,16 justified retained/mixed,564 pending.

### Integrated Spore Spawn fade calculations and shared coloring

The sprite fade calculates96 intermediate RGB5 words between its critical-health alias and16 final colors. Background calculates96 words from13 original room-color sites and16 final colors. Level calculates72 words from12 room-color sites, retaining28 unresolved trajectory words and12 other final colors. All25 room sites remain required under RoomStaticPalette.nativeBytes, all64 health words remain required, and all72 stored death words remain required. These are partial conversions without exemptions. The16 spore colors alias healthy row0 with zero stock overrides, completing only SporeSpawnColorCatalog.spores.

Root reviewed native A5:E8CA/E91C layer-copy consumers and confirmed all native values, exact dictionary membership, initial room colors, independent edits, bounds and canonical identities with --lookup-stream-4-spore-fade. Verification build1445 warnings/0errors; ResourceAudit0/0. Worker chain206efcee,48084c6d,d20ada86,c0d031b3,3586276c. Aggregate549 converted/16 retained-mixed/563 pending. Corrected stale JSON aggregate counters from authoritative per-entry states.

## Integrated calculated beam tile artwork

Partial chain a688b01dd,26f45f27d,4c6fcce67,df4bd76d9,a534adf4e,f424d0bee,033e34f77. Selection-aware atlases calculate transpose/quarter-turn/reflection, transparent regions, repeated ribbon/impact rows and exact shared tiles/families. Native transfer is eight tiles/256bytes per selection; the unused second half of each512-byte incbin is outside this upload. Runtime materializes DMA bytes from inputs and relationships without caching expanded stock. Independent supplied pixels remain independent across aliases and beam selections.

Root independently viewed the native five-family tile sheet and checked90:C441/C451 palette and transfer consumers. Only five Plasma tile0 pen assignments are retained: row3 x0/1/3=4/2/15(edge base/highlight/shadow),row4 x0/1=5/3(center highlight/mid). Native plane bytesF806/07/16/17=11/55/BB/11 andF808/09/18/19=FF/55/AA/00 establish those values. They select painted pixel classes, not samples of a quantity determining pen numbers; arithmetic fitting would re-encode the painting. This exception excludes RGB colors,profile widths/periods/phases and every other pixel.

Runtime retains1227 unresolved pixel sites,4912 derived/shared sites and5 narrowly justified pen roles. BeamTileAtlas remains unchecked. Root Verification build1446warnings/zero errors; --lookup-stream-4-beam-geometry passes all3072native transfer bytes,exact basis/seed membership,independent edits and300 actual queued NMI drains with cartridge reads denied and adjacent VRAM preserved. Source hash dependencies refreshed including native beam flags; ResourceAudit0/0. Inventory unchanged558 converted/21 retained-mixed/549 pending.
## Integrated Spazer diagonal exterior

0be87bd7c calculates104transparent exterior pixels from the diagonal band's geometry. Native band lead1,width3 and24interior ink pixels remain required inputs; this is a partial conversion, not an artwork exemption. Runtime beam basis is1123pixels plus the five previously reviewed categorical Plasma pen assignments;5016pixels are calculated/shared. Both aggregate artwork entries remain pending.

Root Verification1446warnings/zero errors; focused beam geometry confirms3072native bytes,exact1123basis and transparent domain,independent edits,348actual queued NMI drains,readguard/bounds/adjacent VRAM. ResourceAudit0warnings/0errors. Inventory unchanged558converted/21retained-mixed/549pending.