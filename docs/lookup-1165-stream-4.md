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
- [ ] **EndingTextPresentation.japaneseSubtitle** ([L11](../csharp/src/SuperMetroid.Core/Assets/EndingTextPresentation.cs#L11)) - installed stock table. Original/default payload behind EndingTextPresentation.japaneseSubtitle. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

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

- [ ] **FlyVisualDefinitions.Frames / literal at L15** ([L15](../csharp/src/SuperMetroid.Core/Assets/FlyVisualDefinitions.cs#L15)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs

- [x] **FlyInstructionProgramDefinitions.Words** ([L15](../csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **FlyInstructionProgramDefinitions.PresentationWords** ([L20](../csharp/src/SuperMetroid.Core/Game/FlyInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **KzanInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/KzanInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs

- [ ] **LowerNorfairRioInstructionProgramDefinitions.Words** ([L31](../csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs#L31)) - factory-built stock table. Stored LowerNorfairRioInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **LowerNorfairRioInstructionProgramDefinitions.PresentationWords** ([L33](../csharp/src/SuperMetroid.Core/Game/LowerNorfairRioInstructionProgramDefinitions.cs#L33)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs

- [ ] **MamaTurtleInstructionProgramDefinitions.Words** ([L43](../csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs#L43)) - factory-built stock table. Stored MamaTurtleInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **MamaTurtleInstructionProgramDefinitions.PresentationWords** ([L45](../csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs#L45)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **MamaTurtleInstructionProgramDefinitions.AddBabyCrawl / literal at L202** ([L202](../csharp/src/SuperMetroid.Core/Game/MamaTurtleInstructionProgramDefinitions.cs#L202)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

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

- [x] **MaridiaLargeSnailInstructionDefinitions.InstructionPointers** ([L24](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs

- [ ] **MaridiaLargeSnailInstructionProgramDefinitions.Words** ([L42](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MaridiaLargeSnailInstructionProgramDefinitions.PresentationWords** ([L101](../csharp/src/SuperMetroid.Core/Game/MaridiaLargeSnailInstructionProgramDefinitions.cs#L101)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs

- [ ] **MaridiaElevatubePlmDefinitions.Draw** ([L23](../csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MaridiaElevatubePlmDefinitions.AllDraws** ([L30](../csharp/src/SuperMetroid.Core/Rooms/MaridiaElevatubePlmDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [ ] **RidleySupplementalVisualDefinitions.WingPointers** ([L18](../csharp/src/SuperMetroid.Core/Assets/RidleySupplementalVisualDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RidleySupplementalVisualDefinitions.TailTipPointers** ([L30](../csharp/src/SuperMetroid.Core/Assets/RidleySupplementalVisualDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **SporeSpawnProjectileDefinitions.StalkYOffsets** ([L10](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SporeSpawnProjectileDefinitions.SpawnerXPositions** ([L13](../csharp/src/SuperMetroid.Core/Game/SporeSpawnProjectileDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
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

- Completed conversions: 32 named definitions (batches 1/2/4/5/6); Tourian statue grey intermediate payload also converted (batch 3, container remains open).
- Justified retained entries: none.
- Confirmation results: isolated builds and --lookup-stream-4 pass; legacy presentation-read verifier limitations recorded in batch 2.
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

The separately inventoried AddBabyCrawl two-duration literal was removed with its entire factory helper in592e59b21. Its two ten-tick poses are calculated in TryCrawl and covered by the same117-native-word confirmation. Marking that already-completed entry brings totals to59 complete and166 unchecked; no production change or additional exemption.

## Coordinator timing re-audit

MamaTurtleInstructionProgramDefinitions.Words and LowerNorfairRioInstructionProgramDefinitions.Words are reopened. Native value agreement confirmed behavior but did not justify independent timing choices. The integrated stream now has57 complete and168 unchecked; uniform AddBabyCrawl and presentation-address conversions remain complete. Source corrections isolate the exact pending timing inputs; integrated Verification1442warnings/zeroerrors and focused stream4 checks pass. No retention exception is granted.
