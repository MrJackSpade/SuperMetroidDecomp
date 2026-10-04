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

- [ ] **BotwoonNavigationDefinitions.Holes** ([L39](../csharp/src/SuperMetroid.Core/Game/BotwoonNavigationDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs

- [ ] **BotwoonWallPlmDrawDefinitions.All** ([L29](../csharp/src/SuperMetroid.Core/Rooms/BotwoonWallPlmDrawDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs

- [ ] **DraygonColorCatalog.intro** ([L24](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L24)) - installed stock table. Original/default payload behind DraygonColorCatalog.intro. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **DraygonColorCatalog.background** ([L25](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L25)) - installed stock table. Original/default payload behind DraygonColorCatalog.background. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **DraygonColorCatalog.sprite** ([L26](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L26)) - installed stock table. Original/default payload behind DraygonColorCatalog.sprite. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **DraygonColorCatalog.whiteFlash** ([L27](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L27)) - installed stock table. Original/default payload behind DraygonColorCatalog.whiteFlash. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **DraygonColorCatalog.healthBands** ([L28](../csharp/src/SuperMetroid.Core/Assets/DraygonColorCatalog.cs#L28)) - installed stock table. Original/default payload behind DraygonColorCatalog.healthBands. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/DraygonBurialEvirDefinitions.cs

- [ ] **DraygonBurialEvirDefinitions.Entries** ([L14](../csharp/src/SuperMetroid.Core/Game/DraygonBurialEvirDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonCannonData.cs

- [ ] **DraygonCannonData.FiringTargets** ([L24](../csharp/src/SuperMetroid.Core/Game/DraygonCannonData.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs

- [ ] **DraygonCollisionDefinitions.FirstBody** ([L28](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DraygonCollisionDefinitions.Empty** ([L30](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DraygonCollisionDefinitions.SecondBody** ([L32](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DraygonCollisionDefinitions.FirstBodyHitboxes** ([L35](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DraygonCollisionDefinitions.SecondBodyHitboxes** ([L47](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Bg2.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Oam.cs

- [ ] **DraygonCollisionDefinitions.EmptyOamFrames** ([L15](../csharp/src/SuperMetroid.Core/Game/DraygonCollisionDefinitions.Oam.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonInstructionProgramDefinitions.cs

- [ ] **DraygonInstructionProgramDefinitions.Words** ([L89](../csharp/src/SuperMetroid.Core/Game/DraygonInstructionProgramDefinitions.cs#L89)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DraygonInstructionProgramDefinitions.PresentationWords** ([L268](../csharp/src/SuperMetroid.Core/Game/DraygonInstructionProgramDefinitions.cs#L268)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DraygonProjectileInstructionProgramDefinitions.cs

- [ ] **DraygonProjectileInstructionProgramDefinitions.Words** ([L30](../csharp/src/SuperMetroid.Core/Game/DraygonProjectileInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DraygonProjectileInstructionProgramDefinitions.PresentationWords** ([L73](../csharp/src/SuperMetroid.Core/Game/DraygonProjectileInstructionProgramDefinitions.cs#L73)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [ ] **EndingTextPresentation.japaneseSubtitle** ([L11](../csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs#L11)) - installed stock table. Original/default payload behind EndingTextPresentation.japaneseSubtitle. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Frontend/EndingCreditsRomData.cs

- [ ] **EndingCreditsRomData.Motion.PlanetFastPattern** ([L195](../csharp/src/SuperMetroid.Core/Frontend/EndingCreditsRomData.cs#L195)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EndingCreditsRomData.Motion.PlanetSlowPattern** ([L202](../csharp/src/SuperMetroid.Core/Frontend/EndingCreditsRomData.cs#L202)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EtecoonInstructionProgramDefinitions.cs

- [ ] **EtecoonInstructionProgramDefinitions.Words** ([L55](../csharp/src/SuperMetroid.Core/Game/EtecoonInstructionProgramDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EtecoonInstructionProgramDefinitions.PresentationWords** ([L93](../csharp/src/SuperMetroid.Core/Game/EtecoonInstructionProgramDefinitions.cs#L93)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EyeDoorEnemyProjectileRomData.cs

- [ ] **EyeDoorEnemyProjectileRomData.ProjectileOriginOffsets** ([L24](../csharp/src/SuperMetroid.Core/Game/EyeDoorEnemyProjectileRomData.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EyeDoorEnemyProjectileRomData.SweatVelocities** ([L30](../csharp/src/SuperMetroid.Core/Game/EyeDoorEnemyProjectileRomData.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EyeDoorProjectileInstructionProgramDefinitions.cs

- [ ] **EyeDoorProjectileInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/EyeDoorProjectileInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EyeDoorProjectileInstructionProgramDefinitions.PresentationWords** ([L53](../csharp/src/SuperMetroid.Core/Game/EyeDoorProjectileInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EyeDoorSweatInstructionProgramDefinitions.cs

- [ ] **EyeDoorSweatInstructionProgramDefinitions.Words** ([L21](../csharp/src/SuperMetroid.Core/Game/EyeDoorSweatInstructionProgramDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EyeDoorSweatInstructionProgramDefinitions.PresentationWords** ([L34](../csharp/src/SuperMetroid.Core/Game/EyeDoorSweatInstructionProgramDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/FlyVisualDefinitions.cs

- [ ] **FlyVisualDefinitions.Frames / literal at L15** ([L15](../csharp/src/SuperMetroid.Core/Assets/FlyVisualDefinitions.cs#L15)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs

- [ ] **FlyInstructionProgramDefinitions.Words** ([L15](../csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **FlyInstructionProgramDefinitions.PresentationWords** ([L20](../csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs

- [ ] **GameplayBasePaletteCatalog.initial** ([L9](../csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs#L9)) - installed stock table. Original/default payload behind GameplayBasePaletteCatalog.initial. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayBasePaletteCatalog.commonSprites** ([L10](../csharp/src/SuperMetroid.Core/Assets/GameplayBasePaletteCatalog.cs#L10)) - installed stock table. Original/default payload behind GameplayBasePaletteCatalog.commonSprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs

- [ ] **GameplayHudDefinitions.EnergyTankByteOffsets** ([L47](../csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GameplayHudDefinitions.ItemByteOffsets** ([L53](../csharp/src/SuperMetroid.Core/Assets/GameplayHudDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs

- [ ] **GameplayHudPresentation.template** ([L10](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L10)) - installed stock table. Original/default payload behind GameplayHudPresentation.template. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.topRowTransfer** ([L11](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L11)) - installed stock table. Stock document.TopRow cells serialized as DMA bytes. This is distinct from document.Template; conversion must target the stock cells, not the byte-packing loop.
- [ ] **GameplayHudPresentation.healthDigits** ([L12](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L12)) - installed stock table. Original/default payload behind GameplayHudPresentation.healthDigits. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.ammoDigits** ([L13](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L13)) - installed stock table. Original/default payload behind GameplayHudPresentation.ammoDigits. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.autoFull** ([L14](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L14)) - installed stock table. Original/default payload behind GameplayHudPresentation.autoFull. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.autoEmpty** ([L15](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L15)) - installed stock table. Original/default payload behind GameplayHudPresentation.autoEmpty. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.autoAnchors** ([L16](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L16)) - installed stock table. Original/default payload behind GameplayHudPresentation.autoAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.energyTankAnchors** ([L17](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L17)) - installed stock table. Original/default payload behind GameplayHudPresentation.energyTankAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameplayHudPresentation.icons** ([L18](../csharp/src/SuperMetroid.Core/Assets/GameplayHudPresentation.cs#L18)) - installed stock table. Original/default payload behind GameplayHudPresentation.icons. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs

- [ ] **GameplayMessageNoticeDefinitions.MapAndEnergyRegions** ([L23](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GameplayMessageNoticeDefinitions.MissileRegions** ([L29](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GameplayMessageNoticeDefinitions.SaveRegions** ([L35](../csharp/src/SuperMetroid.Core/Assets/GameplayMessageNoticeDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **GameplayMessageDefinitions.Definitions** ([L13](../csharp/src/SuperMetroid.Core/Game/GameplayMessageDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs

- [ ] **GameplayMessageRomData.Buttons.OrderedGlyphs** ([L76](../csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GameplayMessageRomData.Buttons.SpecialGlyphByteOffsets** ([L92](../csharp/src/SuperMetroid.Core/Game/GameplayMessageRomData.cs#L92)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/KzanInstructionProgramDefinitions.cs

- [ ] **KzanInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/KzanInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs

- [ ] **LowerNorfairRioInstructionProgramDefinitions.Words** ([L31](../csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs#L31)) - factory-built stock table. Stored LowerNorfairRioInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **LowerNorfairRioInstructionProgramDefinitions.PresentationWords** ([L33](../csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs#L33)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs

- [ ] **MamaTurtleInstructionProgramDefinitions.Words** ([L43](../csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs#L43)) - factory-built stock table. Stored MamaTurtleInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **MamaTurtleInstructionProgramDefinitions.PresentationWords** ([L45](../csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs#L45)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
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

- [ ] **MaridiaLargeSnailCollisionDefinitions.ListPointers** ([L33](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailCollisionDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MaridiaLargeSnailCollisionDefinitions.Lists** ([L42](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailCollisionDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionDefinitions.cs

- [ ] **MaridiaLargeSnailInstructionDefinitions.InstructionPointers** ([L24](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs

- [ ] **MaridiaLargeSnailInstructionProgramDefinitions.Words** ([L42](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MaridiaLargeSnailInstructionProgramDefinitions.PresentationWords** ([L101](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs#L101)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs

- [ ] **MaridiaElevatubePlmDefinitions.Draw** ([L23](../csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MaridiaElevatubePlmDefinitions.AllDraws** ([L30](../csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PlanetZebesTextPaletteFxProgramMechanicsDefinitions.cs

- [ ] **PlanetZebesTextPaletteFxProgramMechanicsDefinitions.Definitions** ([L45](../csharp/src/SuperMetroid.Core/Game/PlanetZebesTextPaletteFxProgramMechanicsDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs

- [ ] **PowerBombFixedColorCatalog.preExplosion** ([L17](../csharp/src/SuperMetroid.Core/Assets/PowerBombFixedColorCatalog.cs#L17)) - installed stock table. Original/default payload behind PowerBombFixedColorCatalog.preExplosion. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
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

- [ ] **RidleySupplementalVisualDefinitions.WingPointers** ([L18](../csharp/src/SuperMetroid.Core/Assets/RidleySupplementalVisualDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleySupplementalVisualDefinitions.TailTipPointers** ([L30](../csharp/src/SuperMetroid.Core/Assets/RidleySupplementalVisualDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CeresRidleyProjectileInstructionProgramDefinitions.cs

- [ ] **CeresRidleyProjectileInstructionProgramDefinitions.Words** ([L80](../csharp/src/SuperMetroid.Core/Game/CeresRidleyProjectileInstructionProgramDefinitions.cs#L80)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CeresRidleyProjectileInstructionProgramDefinitions.PresentationWords** ([L167](../csharp/src/SuperMetroid.Core/Game/CeresRidleyProjectileInstructionProgramDefinitions.cs#L167)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs

- [ ] **RidleyAttackChoices.BelowHalfHealth** ([L10](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyAttackChoices.AboveHalfHealth** ([L14](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyAttackChoices.DamageBoosting** ([L18](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyAttackChoices.PogoZone** ([L22](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyAttackChoices.SpinJumping** ([L26](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyAttackChoices.ZeroHealth** ([L30](../csharp/src/SuperMetroid.Core/Game/RidleyAttackChoices.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs

- [ ] **RidleyClawOffsets.X** ([L7](../csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyClawOffsets.Y** ([L10](../csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyClawOffsets.ExistingOutOfRangeWindow** ([L19](../csharp/src/SuperMetroid.Core/Game/RidleyClawOffsets.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs

- [ ] **RidleyCollisionDefinitions.FrameKeys** ([L24](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyCollisionDefinitions.ListKeys** ([L29](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyCollisionDefinitions.LeftBase** ([L36](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyCollisionDefinitions.RightBase** ([L39](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyCollisionDefinitions.Frames** ([L43](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyCollisionDefinitions.Lists** ([L58](../csharp/src/SuperMetroid.Core/Game/RidleyCollisionDefinitions.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs

- [ ] **RidleyExplosionDefinitions.SpawnOrder** ([L46](../csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyExplosionDefinitions.PartRecords** ([L66](../csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs#L66)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyExplosionDefinitions.DeathExplosionPlacements** ([L87](../csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs#L87)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyExplosionDefinitions.TailTipInstructionLists** ([L105](../csharp/src/SuperMetroid.Core/Game/RidleyExplosionDefinitions.cs#L105)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyInstructionProgramDefinitions.cs

- [ ] **RidleyInstructionProgramDefinitions.Words** ([L34](../csharp/src/SuperMetroid.Core/Game/RidleyInstructionProgramDefinitions.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyInstructionProgramDefinitions.PresentationWords** ([L112](../csharp/src/SuperMetroid.Core/Game/RidleyInstructionProgramDefinitions.cs#L112)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs

- [ ] **RidleyMovementTargets.DescendingPogoX** ([L7](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyMovementTargets.AscendingPogoX** ([L9](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyMovementTargets.GroundAttackX** ([L11](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyMovementTargets.CarryAnchorX** ([L13](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyMovementTargets.CarryReleaseX** ([L15](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyMovementTargets.HoverDivisorIndexes** ([L17](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleyMovementTargets.GrabDivisorIndexes** ([L19](../csharp/src/SuperMetroid.Core/Game/RidleyMovementTargets.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs

- [x] **RidleyPogoDefinitions.Upward** ([L7](../csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs#L7)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyPogoDefinitions.Downward** ([L9](../csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs#L9)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyPogoDefinitions.Horizontal** ([L12](../csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RidleyPogoDefinitions.Vertical** ([L20](../csharp/src/SuperMetroid.Core/Game/RidleyPogoDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidley.cs

- [ ] **RoomEnemySystem.CreateInitialRidleyTailSegments / distances** ([L916](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidley.cs#L916)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.CreateInitialRidleyTailSegments / angles** ([L917](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidley.cs#L917)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs

- [ ] **RoomEnemySystem.CeresRidleyWingAnimationDeltas** ([L10](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.UpdateRidleyTailDistances / maximumDistances** ([L323](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs#L323)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.UpdateRidleyTailDistances / neutralDistances** ([L324](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyComposition.cs#L324)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyMode7.cs

- [ ] **RoomEnemySystem.TickCeresRidleyMode7Getaway / babyTransferPointers** ([L101](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.CeresRidleyMode7.cs#L101)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs

- [ ] **RoomEnemySystem.RidleySamusMovementFlags** ([L15](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.RidleyTailTouchesTerrain / segmentIndexes** ([L672](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs#L672)) - method-local definition. Five indexed joint selections 6 through 2; calculate the reverse traversal directly.
- [ ] **RoomEnemySystem.RidleyTailTouchesTerrain / yOffsets** ([L673](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Ridley.cs#L673)) - method-local definition. Tip probe offset 16 followed by four joint offsets 18; express the tip/joint distinction directly.

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

- [ ] **RoomEnemySpawnNameDefinitions.Records** ([L12](../csharp/src/SuperMetroid.Core/Game/RoomEnemySpawnNameDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectDefinitions.cs

- [ ] **RoomSpriteObjectDefinitions.InstructionPointers** ([L12](../csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectInstructionProgramDefinitions.cs

- [ ] **RoomSpriteObjectInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomSpriteObjectInstructionProgramDefinitions.PresentationWords** ([L165](../csharp/src/SuperMetroid.Core/Game/RoomSpriteObjectInstructionProgramDefinitions.cs#L165)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/RoomBackgroundTilemapSourceDefinitions.cs

- [ ] **RoomBackgroundTilemapSources.Sources** ([L13](../csharp/src/SuperMetroid.Core/Rooms/RoomBackgroundTilemapSourceDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/RoomLevelStreamDefinitions.cs

- [ ] **RoomLevelStreamDefinitions.Installed stock level corpus** ([L14](../csharp/src/SuperMetroid.Core/Rooms/RoomLevelStreamDefinitions.cs#L14)) - embedded stock table. RoomLevelStreams.bin contains the canonical native level allocations keyed by source identity. Foreground collision words, BTS bytes and background words require independent field dispositions; RoomLevelData live/streaming copies are aliases, not additional tables.

### csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs

- [ ] **RoomScrollDefinitions.Definitions** ([L25](../csharp/src/SuperMetroid.Core/Rooms/RoomScrollDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs

- [ ] **RoomTilesetDefinitions.Entries** ([L14](../csharp/src/SuperMetroid.Core/Rooms/RoomTilesetDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs

- [ ] **RoomVisualLayout.foreground** ([L12](../csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs#L12)) - installed stock table. Original/default payload behind RoomVisualLayout.foreground. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **RoomVisualLayout.background** ([L13](../csharp/src/SuperMetroid.Core/Rooms/RoomVisualLayoutCatalog.cs#L13)) - installed stock table. Original/default payload behind RoomVisualLayout.background. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/SaveRamLayout.cs

- [ ] **SaveRamLayout.NativeSlotOffsets** ([L97](../csharp/src/SuperMetroid.Core/Game/SaveRamLayout.cs#L97)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SaveStationElectricityInstructionProgramDefinitions.cs

- [x] **SaveStationElectricityInstructionProgramDefinitions.Words** ([L21](../csharp/src/SuperMetroid.Core/Game/SaveStationElectricityInstructionProgramDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SaveStationElectricityInstructionProgramDefinitions.PresentationWords** ([L39](../csharp/src/SuperMetroid.Core/Game/SaveStationElectricityInstructionProgramDefinitions.cs#L39)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SkreeMetareeAnimationDefinitions.cs

- [x] **SkreeMetareeAnimationDefinitions.MetareeInstructionLists** ([L23](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeAnimationDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SkreeMetareeAnimationDefinitions.SkreeInstructionLists** ([L35](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeAnimationDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs

- [ ] **SkreeMetareeInstructionProgramDefinitions.MetareeWords** ([L22](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SkreeMetareeInstructionProgramDefinitions.SkreeWords** ([L36](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SkreeMetareeInstructionProgramDefinitions.MetareePresentationWords** ([L50](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SkreeMetareeInstructionProgramDefinitions.SkreePresentationWords** ([L58](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeInstructionProgramDefinitions.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SkreeMetareeParticleInstructionProgramDefinitions.cs

- [x] **SkreeMetareeParticleInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeParticleInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SkreeMetareeParticleInstructionProgramDefinitions.PresentationWords** ([L32](../csharp/src/SuperMetroid.Core/Game/SkreeMetareeParticleInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SkulteraInstructionProgramDefinitions.cs

- [ ] **SkulteraInstructionProgramDefinitions.Words** ([L29](../csharp/src/SuperMetroid.Core/Game/SkulteraInstructionProgramDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SkulteraInstructionProgramDefinitions.PresentationWords** ([L52](../csharp/src/SuperMetroid.Core/Game/SkulteraInstructionProgramDefinitions.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs

- [ ] **SporeSpawnColorCatalog.spores** ([L23](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L23)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.spores. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SporeSpawnColorCatalog.health** ([L24](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L24)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.health. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SporeSpawnColorCatalog.deathSprite** ([L25](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L25)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.deathSprite. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SporeSpawnColorCatalog.deathLevel** ([L26](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L26)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.deathLevel. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SporeSpawnColorCatalog.deathBackground** ([L27](../csharp/src/SuperMetroid.Core/Assets/SporeSpawnColorCatalog.cs#L27)) - installed stock table. Original/default payload behind SporeSpawnColorCatalog.deathBackground. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/SporeSpawnCollisionDefinitions.cs

- [ ] **SporeSpawnCollisionDefinitions.Frames** ([L50](../csharp/src/SuperMetroid.Core/Game/SporeSpawnCollisionDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SporeSpawnCollisionDefinitions.Lists** ([L67](../csharp/src/SuperMetroid.Core/Game/SporeSpawnCollisionDefinitions.cs#L67)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SporeSpawnInstructionProgramDefinitions.cs

- [ ] **SporeSpawnInstructionProgramDefinitions.Words** ([L36](../csharp/src/SuperMetroid.Core/Game/SporeSpawnInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SporeSpawnInstructionProgramDefinitions.PresentationWords** ([L79](../csharp/src/SuperMetroid.Core/Game/SporeSpawnInstructionProgramDefinitions.cs#L79)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs

- [ ] **SporeSpawnProjectileDefinitions.StalkYOffsets** ([L10](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SporeSpawnProjectileDefinitions.SpawnerXPositions** ([L13](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SporeSpawnProjectileDefinitions.Movement** ([L17](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileInstructionProgramDefinitions.cs

- [ ] **SporeSpawnProjectileInstructionProgramDefinitions.Words** ([L25](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SporeSpawnProjectileInstructionProgramDefinitions.PresentationWords** ([L48](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileInstructionProgramDefinitions.cs#L48)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/StokeInstructionProgramDefinitions.cs

- [ ] **StokeInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/StokeInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **StokeInstructionProgramDefinitions.PresentationWords** ([L41](../csharp/src/SuperMetroid.Core/Game/StokeInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/StokeProjectileInstructionProgramDefinitions.cs

- [ ] **StokeProjectileInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/StokeProjectileInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **StokeProjectileInstructionProgramDefinitions.PresentationWords** ([L30](../csharp/src/SuperMetroid.Core/Game/StokeProjectileInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **TitleSpriteDefinitions.Pointers** ([L8](../csharp/src/SuperMetroid.Core/Assets/TitleSpriteDefinitions.cs#L8)) - factory-built stock table. Stored ushort[] initialized by CollectPointers(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Frontend/TitleSequenceInstructionDefinitions.cs

- [ ] **TitleSequenceInstructionDefinitions.Program** ([L12](../csharp/src/SuperMetroid.Core/Frontend/TitleSequenceInstructionDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/TitleSequenceRomData.cs

- [ ] **TitleSequenceRomData.Vram.BabySourcePages** ([L94](../csharp/src/SuperMetroid.Core/Frontend/TitleSequenceRomData.cs#L94)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- Completed conversions: pending.
- Justified retained entries: pending.
- Confirmation results: pending.
- Cross-stream dependencies and proposed shared-file patches: pending.
- Remaining entries: all unchecked entries above.


## Batch 1: Ridley pogo and breakup arithmetic

Six definitions converted; no retained exception.

- `RidleyPogoDefinitions`: all four tables removed. Horizontal pattern increments are $20; ordinary stages add $08. Two unused lead-in stages preserve their separate lower progression. Upward acceleration is a linear stage progression after the first entry; downward acceleration doubles through stage three then follows the higher linear range. Vertical launch magnitudes combine stage growth and pattern increments, preserving the larger initial pattern step and the stage-three faster-pattern adjustment. No generated table is cached.
- `RidleyExplosionDefinitions.PartRecords`: all three record columns derive from the part selector. Tail lifetime advances eight frames and equal-sized initializer addresses advance 24 bytes. Body initializers advance 50 bytes; body lifetimes advance eight frames except the torso expires last. `TailTipInstructionLists` derives sixteen six-byte program addresses from orientation.
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