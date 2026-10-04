# Issue 1165 - agent stream 3

GitHub child ticket: [#1240](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1240).

Assignment: **105 files / 226 named table definitions**. [Ownership rules and all streams](lookup-1165-streams.md). Inventory snapshot: 2026-10-04T20:43:25.3086366Z.

## Agent instructions

Work through every entry below under issue 1165. Check current source and review dispositions first. Convert the original mapping into calculation or meaningful cases, or document a concrete impossible/nonsense justification. Preserve behavior and supplied content edits. Independently resolve each value field; checking off a container does not excuse its unresolved payloads.

Edit only source files listed here and this stream checklist/report. Read other files as needed. Ask the coordinator to assign any additional dependency or new source/test file before writing it. Route shared review-ledger, project, verification-entry-point and master-inventory changes through the coordinator. Do not stage, commit or publish other agents' work. Use focused confirmation of identified changes, not exploratory test discovery.

For each completed entry, record the conversion or precise retention evidence, changed files and focused confirmation. Report cross-stream dependencies by path and required contract. Report pending entries honestly; definition counts are not effort estimates.

## Exclusive source files

| File | Definitions |
| --- | ---: |
| [csharp/src/SuperMetroid.Core/Assets/BabyMetroidCutsceneColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/BabyMetroidCutsceneColorCatalog.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/CreditsPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/CreditsPresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/CreditsPresentationDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/CreditsPresentationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EnemyPaletteSheet.cs](../csharp/src/SuperMetroid.Core/Assets/EnemyPaletteSheet.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs) | 8 |
| [csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/GrappleSpriteCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/GrappleSpriteCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/GrappleSpriteDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/GrappleSpriteDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/IntroCaretSpriteDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/IntroCaretSpriteDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/IntroCinematicArtworkCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/IntroCinematicArtworkCatalog.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/IntroCinematicPalette.cs](../csharp/src/SuperMetroid.Core/Assets/IntroCinematicPalette.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/IntroEyeTilemapPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/IntroEyeTilemapPresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/IntroFinalLineTilemap.cs](../csharp/src/SuperMetroid.Core/Assets/IntroFinalLineTilemap.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/IntroFontAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/IntroFontAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/IntroNarrationDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/IntroNarrationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/IntroNarrationPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/IntroNarrationPresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MochtroidVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/MochtroidVisualDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/MotherBrainCorpseArtworkDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/MotherBrainCorpseArtworkDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs](../csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs) | 7 |
| [csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MotherBrainVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/MotherBrainVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/RipperVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/RipperVisualDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/ShaktoolVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/ShaktoolVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/ShitroidColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/ShitroidColorCatalog.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Assets/WorkRobotPaletteCycle.cs](../csharp/src/SuperMetroid.Core/Assets/WorkRobotPaletteCycle.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/WorkRobotVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/WorkRobotVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs](../csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs](../csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Frontend/StockAttractDemoScenes.cs](../csharp/src/SuperMetroid.Core/Frontend/StockAttractDemoScenes.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/BabyMetroidCutsceneState.cs](../csharp/src/SuperMetroid.Core/Game/BabyMetroidCutsceneState.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Game/BabyMetroidRouteDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/BabyMetroidRouteDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/ChootFallingPathDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ChootFallingPathDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/ChootInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ChootInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ChootPatternDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ChootPatternDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CrateriaLightningPaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CrateriaLightningPaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/EnemyDeathExplosionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EnemyDeathExplosionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EnemyDropChanceDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EnemyDropChanceDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/EnemyPickupDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EnemyPickupDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/EnemyPickupInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EnemyPickupInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Game/EnemyVulnerabilityDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EnemyVulnerabilityDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs) | 6 |
| [csharp/src/SuperMetroid.Core/Game/FirefleaInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/FirefleaInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GrappleBodyPlacementDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GrappleBodyPlacementDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Game/HopperAnimationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/HopperAnimationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/HopperInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/HopperInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MochtroidInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MochtroidInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainBabyInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainBabyInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainBodyInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainBodyInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainContactHitboxDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainContactHitboxDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainCorpseRottingState.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainCorpseRottingState.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainDeathExplosionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainDeathExplosionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainDoorFragmentDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainDoorFragmentDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainEnemyProjectileSystem.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainEnemyProjectileSystem.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainFallingTubeInstructionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainFallingTubeInstructionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainFallingTubePopulationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainFallingTubePopulationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamBodyInstructionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamBodyInstructionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamInstructionProgramDefinitions.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainHeadInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainHeadInstructionProgramDefinitions.cs) | 6 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs) | 8 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.StateMachine.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.StateMachine.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainTopTubeInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainTopTubeInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainTurretDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainTurretDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MotherBrainTurretInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MotherBrainTurretInstructionProgramDefinitions.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Game/MultiviolaInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MultiviolaInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/NintendoLogoFadePaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NintendoLogoFadePaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RioInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RioInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RipperInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/RipperInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Mochtroid.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Mochtroid.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainFakeDeath.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainFakeDeath.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwo.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwo.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwoProjectiles.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwoProjectiles.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.ShaktoolProjectiles.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.ShaktoolProjectiles.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ShaktoolProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ShaktoolProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ShitroidInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ShitroidInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SparkInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SparkInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SparkMovementDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SparkMovementDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/WorkRobotInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/WorkRobotInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/WorkRobotLaserDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/WorkRobotLaserDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/YellowPipeBugInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/YellowPipeBugInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Input/StockAttractInputProgramsCatalog.cs](../csharp/src/SuperMetroid.Core/Input/StockAttractInputProgramsCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Rooms/DoorScrollPrograms.cs](../csharp/src/SuperMetroid.Core/Rooms/DoorScrollPrograms.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.cs](../csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.cs) | 1 |

## Work checklist

### csharp/src/SuperMetroid.Core/Game/ChootFallingPathDefinitions.cs

- [ ] **ChootFallingPathDefinitions.Normal** ([L59](../csharp/src/SuperMetroid.Core/Game/ChootFallingPathDefinitions.cs#L59)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ChootFallingPathDefinitions.Wide** ([L92](../csharp/src/SuperMetroid.Core/Game/ChootFallingPathDefinitions.cs#L92)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ChootFallingPathDefinitions.VeryWide** ([L125](../csharp/src/SuperMetroid.Core/Game/ChootFallingPathDefinitions.cs#L125)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChootInstructionProgramDefinitions.cs

- [x] **ChootInstructionProgramDefinitions.Words** ([L45](../csharp/src/SuperMetroid.Core/Game/ChootInstructionProgramDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChootInstructionProgramDefinitions.PresentationWords** ([L60](../csharp/src/SuperMetroid.Core/Game/ChootInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChootPatternDefinitions.cs

- [x] **ChootPatternDefinitions.Patterns** ([L33](../csharp/src/SuperMetroid.Core/Game/ChootPatternDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.cs

- [ ] **CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.Durations** ([L59](../csharp/src/SuperMetroid.Core/Game/CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.cs#L59)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.Definitions** ([L61](../csharp/src/SuperMetroid.Core/Game/CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.cs#L61)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrateriaLightningPaletteFxProgramMechanicsDefinitions.cs

- [ ] **CrateriaLightningPaletteFxProgramMechanicsDefinitions.Definitions** ([L41](../csharp/src/SuperMetroid.Core/Game/CrateriaLightningPaletteFxProgramMechanicsDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CreditsPresentation.cs

- [ ] **CreditsPresentation.rows** ([L9](../csharp/src/SuperMetroid.Core/Assets/CreditsPresentation.cs#L9)) - installed stock table. Original/default payload behind CreditsPresentation.rows. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CreditsPresentationDefinitions.cs

- [ ] **CreditsPresentationDefinitions.Lines** ([L28](../csharp/src/SuperMetroid.Core/Assets/CreditsPresentationDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs

- [ ] **DoorTransitionState.BuildSourceFadeTarget / alwaysPreserved** ([L250](../csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs#L250)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **DoorTransitionState.BuildSourceFadeTarget / commonCreColors** ([L261](../csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs#L261)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **DoorTransitionState.BuildSourceFadeTarget / timerColors** ([L266](../csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs#L266)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Rooms/DoorScrollPrograms.cs

- [ ] **DoorScrollPrograms.Programs** ([L33](../csharp/src/SuperMetroid.Core/Rooms/DoorScrollPrograms.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorCatalog.cs

- [ ] **EnemyAuxiliaryColorCatalog.frames** ([L19](../csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorCatalog.cs#L19)) - installed stock table. Original/default payload behind EnemyAuxiliaryColorCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorDefinitions.cs

- [ ] **EnemyAuxiliaryColorDefinitions.Definitions** ([L29](../csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameCatalog.cs

- [ ] **EnemyBg2FrameCatalog.frames** ([L31](../csharp/src/SuperMetroid.Core/Assets/EnemyBg2FrameCatalog.cs#L31)) - installed stock table. Original/default payload behind EnemyBg2FrameCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs

- [ ] **EnemyExtendedFrameCatalog.frames** ([L39](../csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs#L39)) - installed stock table. Original/default payload behind EnemyExtendedFrameCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EnemyPaletteSheet.cs

- [ ] **EnemyPaletteSheet.colors** ([L16](../csharp/src/SuperMetroid.Core/Assets/EnemyPaletteSheet.cs#L16)) - installed stock table. Original/default payload behind EnemyPaletteSheet.colors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs

- [ ] **EnemyProjectileSpritemapCatalog.frames** ([L26](../csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs#L26)) - installed stock table. Original/default payload behind EnemyProjectileSpritemapCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **EnemyProjectileSpritemapCatalog.programFrames** ([L27](../csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs#L27)) - installed stock table. Original/default payload behind EnemyProjectileSpritemapCatalog.programFrames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **EnemyProjectileSpritemapDefinitions.Frames** ([L289](../csharp/src/SuperMetroid.Core/Assets/EnemyProjectileSpritemapCatalog.cs#L289)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs

- [ ] **EnemySpritemapCatalog.frames** ([L25](../csharp/src/SuperMetroid.Core/Assets/EnemySpritemapCatalog.cs#L25)) - installed stock table. Original/default payload behind EnemySpritemapCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs

- [ ] **EnemySpritemapDefinitions.NamedFrameDefinitions** ([L183](../csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs#L183)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyDeathExplosionDefinitions.cs

- [x] **EnemyDeathExplosionDefinitions.InstructionPointers** ([L21](../csharp/src/SuperMetroid.Core/Game/EnemyDeathExplosionDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs

- [ ] **EnemyDeathInstructionProgramDefinitions.Words** ([L33](../csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EnemyDeathInstructionProgramDefinitions.PresentationWords** ([L127](../csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs#L127)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyDropChanceDefinitions.cs

- [ ] **EnemyDropChanceDefinitions.PackedChances** ([L41](../csharp/src/SuperMetroid.Core/Game/EnemyDropChanceDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyPickupDefinitions.cs

- [x] **EnemyPickupDefinitions.InstructionLists** ([L17](../csharp/src/SuperMetroid.Core/Game/EnemyPickupDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyPickupInstructionProgramDefinitions.cs

- [x] **EnemyPickupInstructionProgramDefinitions.Words** ([L30](../csharp/src/SuperMetroid.Core/Game/EnemyPickupInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EnemyPickupInstructionProgramDefinitions.PresentationWords** ([L64](../csharp/src/SuperMetroid.Core/Game/EnemyPickupInstructionProgramDefinitions.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs

- [ ] **EnemyProjectileInstructionMechanicsDefinitions.BlueRingDurations** ([L76](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EnemyProjectileInstructionMechanicsDefinitions.MiscDustInitialPointers** ([L84](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L84)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EnemyProjectileInstructionMechanicsDefinitions.TimedPrograms** ([L93](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L93)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EnemyProjectileInstructionMechanicsDefinitions.MechanicsWords** ([L162](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L162)) - factory-built stock table. Stored EnemyProjectileMechanicsWordDefinition[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **EnemyProjectileInstructionMechanicsDefinitions.PresentationFrames** ([L165](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L165)) - factory-built stock table. Stored EnemyProjectilePresentationFrameDefinition[] initialized by BuildPresentationFrames(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/EnemyVulnerabilityDefinitions.cs

- [ ] **EnemyVulnerabilityDefinitions.PackedVulnerabilities** ([L74](../csharp/src/SuperMetroid.Core/Game/EnemyVulnerabilityDefinitions.cs#L74)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs

- [ ] **ExploredMapPackingDefinitions.CrateriaIndexes** ([L20](../csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ExploredMapPackingDefinitions.BrinstarIndexes** ([L31](../csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ExploredMapPackingDefinitions.NorfairIndexes** ([L41](../csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ExploredMapPackingDefinitions.WreckedShipIndexes** ([L52](../csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ExploredMapPackingDefinitions.MaridiaIndexes** ([L58](../csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ExploredMapPackingDefinitions.TourianIndexes** ([L68](../csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs#L68)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs

- [ ] **FileSelectPresentation.pages** ([L14](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L14)) - installed stock table. Original/default payload behind FileSelectPresentation.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **FileSelectPresentation.patches** ([L15](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L15)) - installed stock table. Original/default payload behind FileSelectPresentation.patches. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **FileSelectPresentation.digits** ([L16](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L16)) - installed stock table. Original/default payload behind FileSelectPresentation.digits. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **FileSelectPresentation.slotLetters** ([L17](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L17)) - installed stock table. Original/default payload behind FileSelectPresentation.slotLetters. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **FileSelectPresentation.sprites** ([L18](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L18)) - installed stock table. Original/default payload behind FileSelectPresentation.sprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/FirefleaInstructionProgramDefinitions.cs

- [x] **FirefleaInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/FirefleaInstructionProgramDefinitions.cs#L20)) - factory-built stock table. Stored FirefleaInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **FirefleaInstructionProgramDefinitions.PresentationWords** ([L21](../csharp/src/SuperMetroid.Core/Game/FirefleaInstructionProgramDefinitions.cs#L21)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs

- [ ] **GameOptionsPresentation.pages** ([L16](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L16)) - installed stock table. Original/default payload behind GameOptionsPresentation.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOptionsPresentation.controllerLabels** ([L17](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L17)) - installed stock table. Original/default payload behind GameOptionsPresentation.controllerLabels. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOptionsPresentation.controllerLabelAnchors** ([L18](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L18)) - installed stock table. Original/default payload behind GameOptionsPresentation.controllerLabelAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOptionsPresentation.languageRegions** ([L19](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L19)) - installed stock table. Original/default payload behind GameOptionsPresentation.languageRegions. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOptionsPresentation.specialToggles** ([L20](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L20)) - installed stock table. Original/default payload behind GameOptionsPresentation.specialToggles. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOptionsPresentation.sprites** ([L21](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L21)) - installed stock table. Original/default payload behind GameOptionsPresentation.sprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOptionsPresentation.headingAnchors** ([L22](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L22)) - installed stock table. Original/default payload behind GameOptionsPresentation.headingAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOptionsPresentation.cursorAnchors** ([L23](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L23)) - installed stock table. Original/default payload behind GameOptionsPresentation.cursorAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs

- [ ] **GameOverPresentation.tilemap** ([L16](../csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs#L16)) - installed stock table. Original/default payload behind GameOverPresentation.tilemap. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOverPresentation.sprites** ([L17](../csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs#L17)) - installed stock table. Original/default payload behind GameOverPresentation.sprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GrappleSpriteCatalog.cs

- [ ] **GrappleSpriteCatalog.segments** ([L10](../csharp/src/SuperMetroid.Core/Assets/GrappleSpriteCatalog.cs#L10)) - installed stock table. Original/default payload behind GrappleSpriteCatalog.segments. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GrappleSpriteDefinitions.cs

- [ ] **GrappleSpriteDefinitions.SegmentAttributeAddresses** ([L11](../csharp/src/SuperMetroid.Core/Assets/GrappleSpriteDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs

- [ ] **GrappleSwingFrameCatalog.frames** ([L9](../csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs#L9)) - installed stock table. Original/default payload behind GrappleSwingFrameCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs

- [ ] **GrappleTileAtlas.tiles** ([L8](../csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs#L8)) - installed stock table. Original/default payload behind GrappleTileAtlas.tiles. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs

- [x] **GrappleTileDefinitions.transfers** ([L20](../csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GrappleTileDefinitions.segments** ([L33](../csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GrappleBodyPlacementDefinitions.cs

- [ ] **GrappleBodyPlacementDefinitions.LeftOffsets** ([L8](../csharp/src/SuperMetroid.Core/Game/GrappleBodyPlacementDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GrappleBodyPlacementDefinitions.RightOffsets** ([L17](../csharp/src/SuperMetroid.Core/Game/GrappleBodyPlacementDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs

- [ ] **GrappleConnectionDefinitions.Cancellation** ([L11](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GrappleConnectionDefinitions.Handlers** ([L15](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GrappleConnectionDefinitions.SpecialAngleRecords** ([L29](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GrappleConnectionDefinitions.StandingDrops** ([L42](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GrappleConnectionDefinitions.CrouchingDrops** ([L50](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HopperAnimationDefinitions.cs

- [ ] **HopperAnimationDefinitions.Variants** ([L18](../csharp/src/SuperMetroid.Core/Game/HopperAnimationDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HopperInstructionProgramDefinitions.cs

- [ ] **HopperInstructionProgramDefinitions.Words** ([L55](../csharp/src/SuperMetroid.Core/Game/HopperInstructionProgramDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **HopperInstructionProgramDefinitions.PresentationWords** ([L121](../csharp/src/SuperMetroid.Core/Game/HopperInstructionProgramDefinitions.cs#L121)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/IntroCaretSpriteDefinitions.cs

- [ ] **IntroCaretSpriteDefinitions.frameDefinitions** ([L20](../csharp/src/SuperMetroid.Core/Assets/IntroCaretSpriteDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/IntroCinematicArtworkCatalog.cs

- [ ] **IntroCinematicArtworkCatalog.BackgroundPages** ([L76](../csharp/src/SuperMetroid.Core/Assets/IntroCinematicArtworkCatalog.cs#L76)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.
- [ ] **IntroCinematicArtworkCatalog.PortraitTilemap** ([L78](../csharp/src/SuperMetroid.Core/Assets/IntroCinematicArtworkCatalog.cs#L78)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.
- [ ] **IntroCinematicArtworkCatalog.InitialNarrationTilemap** ([L80](../csharp/src/SuperMetroid.Core/Assets/IntroCinematicArtworkCatalog.cs#L80)) - installed stock table. Original stock tile/pixel/word payload stored through an auto-property. Property/constructor/serialized-document copies are one source definition.

### csharp/src/SuperMetroid.Core/Assets/IntroCinematicPalette.cs

- [ ] **IntroCinematicPalette.nativeBytes** ([L10](../csharp/src/SuperMetroid.Core/Assets/IntroCinematicPalette.cs#L10)) - installed stock table. Original/default payload behind IntroCinematicPalette.nativeBytes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/IntroEyeTilemapPresentation.cs

- [ ] **IntroEyeTilemapPresentation.frames** ([L9](../csharp/src/SuperMetroid.Core/Assets/IntroEyeTilemapPresentation.cs#L9)) - installed stock table. Original/default payload behind IntroEyeTilemapPresentation.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/IntroFinalLineTilemap.cs

- [ ] **IntroFinalLineTilemap.words** ([L9](../csharp/src/SuperMetroid.Core/Assets/IntroFinalLineTilemap.cs#L9)) - installed stock table. Original/default payload behind IntroFinalLineTilemap.words. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/IntroFontAtlas.cs

- [ ] **IntroFontAtlas.transfer** ([L6](../csharp/src/SuperMetroid.Core/Assets/IntroFontAtlas.cs#L6)) - installed stock table. Original/default payload behind IntroFontAtlas.transfer. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/IntroNarrationDefinitions.cs

- [ ] **IntroNarrationDefinitions.NativePages** ([L70](../csharp/src/SuperMetroid.Core/Assets/IntroNarrationDefinitions.cs#L70)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/IntroNarrationPresentation.cs

- [ ] **IntroNarrationPresentation.pages** ([L12](../csharp/src/SuperMetroid.Core/Assets/IntroNarrationPresentation.cs#L12)) - installed stock table. Original/default payload behind IntroNarrationPresentation.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs

- [ ] **IntroCinematicRomData.Palette.GameplayRegions** ([L92](../csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs#L92)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **IntroCinematicRomData.Palette.GameplayClearRegions** ([L98](../csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs#L98)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **IntroCinematicRomData.Palette.NarrationRegions** ([L104](../csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs#L104)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **IntroCinematicRomData.Palette.DiscoveryRegions** ([L111](../csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs#L111)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MochtroidVisualDefinitions.cs

- [ ] **MochtroidVisualDefinitions.Selectors** ([L14](../csharp/src/SuperMetroid.Core/Assets/MochtroidVisualDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MochtroidVisualDefinitions.Frames / literal at L22** ([L22](../csharp/src/SuperMetroid.Core/Assets/MochtroidVisualDefinitions.cs#L22)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/MochtroidInstructionProgramDefinitions.cs

- [x] **MochtroidInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/MochtroidInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MochtroidInstructionProgramDefinitions.PresentationWords** ([L28](../csharp/src/SuperMetroid.Core/Game/MochtroidInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Mochtroid.cs

- [ ] **RoomEnemySystem.RunMochtroidShake / xOffsets** ([L212](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Mochtroid.cs#L212)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **RoomEnemySystem.RunMochtroidShake / yOffsets** ([L213](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Mochtroid.cs#L213)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Assets/BabyMetroidCutsceneColorCatalog.cs

- [ ] **BabyMetroidCutsceneColorCatalog.initial** ([L17](../csharp/src/SuperMetroid.Core/Assets/BabyMetroidCutsceneColorCatalog.cs#L17)) - installed stock table. Original/default payload behind BabyMetroidCutsceneColorCatalog.initial. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **BabyMetroidCutsceneColorCatalog.fade** ([L18](../csharp/src/SuperMetroid.Core/Assets/BabyMetroidCutsceneColorCatalog.cs#L18)) - installed stock table. Original/default payload behind BabyMetroidCutsceneColorCatalog.fade. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainCorpseArtworkDefinitions.cs

- [x] **MotherBrainCorpseArtworkDefinitions.VramPageSources** ([L20](../csharp/src/SuperMetroid.Core/Assets/MotherBrainCorpseArtworkDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainCorpseArtworkDefinitions.VramPageDestinations** ([L24](../csharp/src/SuperMetroid.Core/Assets/MotherBrainCorpseArtworkDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs

- [ ] **MotherBrainDeathColorCatalog.bodyFade** ([L19](../csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs#L19)) - installed stock table. Original/default payload behind MotherBrainDeathColorCatalog.bodyFade. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainDeathColorCatalog.legFade** ([L20](../csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs#L20)) - installed stock table. Original/default payload behind MotherBrainDeathColorCatalog.legFade. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainDeathColorCatalog.corpseFade** ([L21](../csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs#L21)) - installed stock table. Original/default payload behind MotherBrainDeathColorCatalog.corpseFade. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainDeathColorCatalog.explodedDoor** ([L22](../csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs#L22)) - installed stock table. Original/default payload behind MotherBrainDeathColorCatalog.explodedDoor. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs

- [x] **MotherBrainEscapeTextArtworkDefinitions.PageSources** ([L17](../csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainEscapeTextArtworkDefinitions.PageByteCounts** ([L21](../csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainEscapeTextArtworkDefinitions.PageDestinations** ([L25](../csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs

- [ ] **MotherBrainHealthPalettePresentation.body** ([L10](../csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs#L10)) - installed stock table. Original/default payload behind MotherBrainHealthPalettePresentation.body. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainHealthPalettePresentation.backLegs** ([L11](../csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs#L11)) - installed stock table. Original/default payload behind MotherBrainHealthPalettePresentation.backLegs. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs

- [ ] **MotherBrainRainbowPalettePresentation.rainbow** ([L10](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L10)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.rainbow. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRainbowPalettePresentation.toGrey** ([L11](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L11)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.toGrey. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRainbowPalettePresentation.fromGrey** ([L12](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L12)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.fromGrey. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRainbowPalettePresentation.fakeDeathToGrey** ([L13](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L13)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.fakeDeathToGrey. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRainbowPalettePresentation.beamCycle** ([L16](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L16)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.beamCycle. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs

- [ ] **MotherBrainRoomColorPresentation.flash** ([L10](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L10)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.flash. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.finalRoom** ([L11](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L11)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.finalRoom. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.phaseTwoAttack** ([L12](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L12)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.phaseTwoAttack. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.phaseTwoRearLeg** ([L13](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L13)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.phaseTwoRearLeg. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.initialGlassShard** ([L14](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L14)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.initialGlassShard. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.initialTubeProjectile** ([L15](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L15)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.initialTubeProjectile. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.recoveryLights** ([L16](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L16)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.recoveryLights. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkDefinitions.cs

- [ ] **MotherBrainSpecialSpriteArtworkDefinitions.All** ([L58](../csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkDefinitions.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainVisualDefinitions.cs

- [ ] **MotherBrainVisualDefinitions.NativePointers** ([L18](../csharp/src/SuperMetroid.Core/Assets/MotherBrainVisualDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/ShitroidColorCatalog.cs

- [ ] **ShitroidColorCatalog.normal** ([L19](../csharp/src/SuperMetroid.Core/Assets/ShitroidColorCatalog.cs#L19)) - installed stock table. Original/default payload behind ShitroidColorCatalog.normal. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **ShitroidColorCatalog.sidehopper** ([L20](../csharp/src/SuperMetroid.Core/Assets/ShitroidColorCatalog.cs#L20)) - installed stock table. Original/default payload behind ShitroidColorCatalog.sidehopper. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **ShitroidColorCatalog.shitroid** ([L21](../csharp/src/SuperMetroid.Core/Assets/ShitroidColorCatalog.cs#L21)) - installed stock table. Original/default payload behind ShitroidColorCatalog.shitroid. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **ShitroidColorCatalog.deadSidehopper** ([L22](../csharp/src/SuperMetroid.Core/Assets/ShitroidColorCatalog.cs#L22)) - installed stock table. Original/default payload behind ShitroidColorCatalog.deadSidehopper. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/BabyMetroidCutsceneState.cs

- [ ] **BabyMetroidCutsceneState.ShakingXOffsets** ([L35](../csharp/src/SuperMetroid.Core/Game/BabyMetroidCutsceneState.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **BabyMetroidCutsceneState.ShakingYOffsets** ([L36](../csharp/src/SuperMetroid.Core/Game/BabyMetroidCutsceneState.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **BabyMetroidCutsceneState.DeathExplosionXOffsets** ([L42](../csharp/src/SuperMetroid.Core/Game/BabyMetroidCutsceneState.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **BabyMetroidCutsceneState.DeathExplosionYOffsets** ([L45](../csharp/src/SuperMetroid.Core/Game/BabyMetroidCutsceneState.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BabyMetroidRouteDefinitions.cs

- [ ] **BabyMetroidRouteDefinitions.Records** ([L53](../csharp/src/SuperMetroid.Core/Game/BabyMetroidRouteDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainBabyInstructionProgramDefinitions.cs

- [ ] **MotherBrainBabyInstructionProgramDefinitions.Words** ([L30](../csharp/src/SuperMetroid.Core/Game/MotherBrainBabyInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainBabyInstructionProgramDefinitions.PresentationWords** ([L45](../csharp/src/SuperMetroid.Core/Game/MotherBrainBabyInstructionProgramDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs

- [ ] **MotherBrainBeamRomData.QuadrantDirections** ([L10](../csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainBodyInstructionProgramDefinitions.cs

- [ ] **MotherBrainBodyInstructionProgramDefinitions.TryWalkFrame / frames** ([L94](../csharp/src/SuperMetroid.Core/Game/MotherBrainBodyInstructionProgramDefinitions.cs#L94)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [ ] **MotherBrainBodyInstructionProgramDefinitions.Words** ([L145](../csharp/src/SuperMetroid.Core/Game/MotherBrainBodyInstructionProgramDefinitions.cs#L145)) - factory-built stock table. Stored MotherBrainBodyInstructionMechanicsWord[] initialized by CreateWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/MotherBrainContactHitboxDefinitions.cs

- [x] **MotherBrainContactHitboxDefinitions.Body** ([L43](../csharp/src/SuperMetroid.Core/Game/MotherBrainContactHitboxDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainContactHitboxDefinitions.Brain** ([L49](../csharp/src/SuperMetroid.Core/Game/MotherBrainContactHitboxDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainContactHitboxDefinitions.Neck** ([L55](../csharp/src/SuperMetroid.Core/Game/MotherBrainContactHitboxDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainCorpseRottingState.cs

- [x] **MotherBrainCorpseRottingState.TileRowOffsets** ([L36](../csharp/src/SuperMetroid.Core/Game/MotherBrainCorpseRottingState.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainCorpseRottingState.ColumnOffsets** ([L41](../csharp/src/SuperMetroid.Core/Game/MotherBrainCorpseRottingState.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainCorpseRottingState.ColumnMinimumY** ([L47](../csharp/src/SuperMetroid.Core/Game/MotherBrainCorpseRottingState.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainCorpseRottingState.InitialGraphicsCopies** ([L52](../csharp/src/SuperMetroid.Core/Game/MotherBrainCorpseRottingState.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainCorpseRottingState.VramTransfers** ([L64](../csharp/src/SuperMetroid.Core/Game/MotherBrainCorpseRottingState.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainDeathExplosionDefinitions.cs

- [x] **MotherBrainDeathExplosionDefinitions.InstructionLists** ([L11](../csharp/src/SuperMetroid.Core/Game/MotherBrainDeathExplosionDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainDoorFragmentDefinitions.cs

- [ ] **MotherBrainDoorFragmentDefinitions.Definitions** ([L18](../csharp/src/SuperMetroid.Core/Game/MotherBrainDoorFragmentDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainEnemyProjectileSystem.cs

- [ ] **MotherBrainEnemyProjectileSystem.BombYAccelerations** ([L42](../csharp/src/SuperMetroid.Core/Game/MotherBrainEnemyProjectileSystem.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainEnemyProjectileSystem.EscapeDoorParticleYOffsets** ([L44](../csharp/src/SuperMetroid.Core/Game/MotherBrainEnemyProjectileSystem.cs#L44)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainEnemyProjectileSystem.EscapeDoorParticleYVelocities** ([L46](../csharp/src/SuperMetroid.Core/Game/MotherBrainEnemyProjectileSystem.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainFallingTubeInstructionDefinitions.cs

- [ ] **MotherBrainFallingTubeInstructionDefinitions.VisualPointers** ([L23](../csharp/src/SuperMetroid.Core/Game/MotherBrainFallingTubeInstructionDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainFallingTubePopulationDefinitions.cs

- [ ] **MotherBrainFallingTubePopulationDefinitions.Pointers** ([L30](../csharp/src/SuperMetroid.Core/Game/MotherBrainFallingTubePopulationDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamBodyInstructionDefinitions.cs

- [ ] **MotherBrainHandBeamBodyInstructionDefinitions.VisualOperands** ([L28](../csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamBodyInstructionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamInstructionProgramDefinitions.cs

- [ ] **MotherBrainHandBeamInstructionProgramDefinitions.StageStarts** ([L25](../csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainHandBeamInstructionProgramDefinitions.Durations** ([L26](../csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainHandBeamInstructionProgramDefinitions.MechanicsWords** ([L27](../csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamInstructionProgramDefinitions.cs#L27)) - factory-built stock table. Stored EnemyProjectileMechanicsWordDefinition[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallInstructions** ([L29](../csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamInstructionProgramDefinitions.cs#L29)) - factory-built stock table. Stored ushort[] initialized by BuildExternalCallInstructions(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **MotherBrainHandBeamInstructionProgramDefinitions.PresentationWords** ([L30](../csharp/src/SuperMetroid.Core/Game/MotherBrainHandBeamInstructionProgramDefinitions.cs#L30)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/MotherBrainHeadInstructionProgramDefinitions.cs

- [ ] **MotherBrainHeadInstructionProgramDefinitions.EarlyWords** ([L63](../csharp/src/SuperMetroid.Core/Game/MotherBrainHeadInstructionProgramDefinitions.cs#L63)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainHeadInstructionProgramDefinitions.RainbowAndNeutralPhaseTwoWords** ([L82](../csharp/src/SuperMetroid.Core/Game/MotherBrainHeadInstructionProgramDefinitions.cs#L82)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainHeadInstructionProgramDefinitions.NeutralWords** ([L90](../csharp/src/SuperMetroid.Core/Game/MotherBrainHeadInstructionProgramDefinitions.cs#L90)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainHeadInstructionProgramDefinitions.CorpseAndRingsWords** ([L100](../csharp/src/SuperMetroid.Core/Game/MotherBrainHeadInstructionProgramDefinitions.cs#L100)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainHeadInstructionProgramDefinitions.BombAndLaserWords** ([L118](../csharp/src/SuperMetroid.Core/Game/MotherBrainHeadInstructionProgramDefinitions.cs#L118)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainHeadInstructionProgramDefinitions.RainbowChargeWords** ([L130](../csharp/src/SuperMetroid.Core/Game/MotherBrainHeadInstructionProgramDefinitions.cs#L130)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs

- [x] **MotherBrainRainbowBeamAttackSequence.PainfulWalkingAnimationDelays** ([L58](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainRainbowBeamAttackSequence.PainfulWalkingNeckAngleDeltas** ([L61](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs#L61)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainRainbowBeamAttackSequence.PainfulWalkingFunctionTimers** ([L64](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainRainbowBeamAttackSequence.EscapeTimerTileTransfers** ([L76](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainRainbowBeamAttackSequence.ExplodedDoorTileTransfers** ([L99](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs#L99)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainRainbowBeamAttackSequence.DeathExplosionOffsets** - justified narrow nonsense retention: 28 decorative body-relative anchors, now isolated in `MotherBrainDeathExplosionDefinitions.DecorativeAnchors`; see batch 18. This is retained visual content, not a converted coordinate generator.
- [ ] **MotherBrainRainbowBeamAttackSequence.ExplosionXOffsets** ([L127](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs#L127)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainRainbowBeamAttackSequence.ExplosionYOffsets** ([L130](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.cs#L130)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.StateMachine.cs

- [x] **MotherBrainRainbowBeamAttackSequence.Step / literal at L995** ([L995](../csharp/src/SuperMetroid.Core/Game/MotherBrainRainbowBeamAttackSequence.StateMachine.cs#L995)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs

- [ ] **MotherBrainRoomPaletteProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainRoomPaletteProgramDefinitions.PresentationWords** ([L33](../csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainTopTubeInstructionProgramDefinitions.cs

- [ ] **MotherBrainTopTubeInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/MotherBrainTopTubeInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MotherBrainTopTubeInstructionProgramDefinitions.PresentationWords** ([L38](../csharp/src/SuperMetroid.Core/Game/MotherBrainTopTubeInstructionProgramDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainTurretDefinitions.cs

- [x] **MotherBrainTurretDefinitions.Turrets** ([L50](../csharp/src/SuperMetroid.Core/Game/MotherBrainTurretDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainTurretDefinitions.Directions** ([L71](../csharp/src/SuperMetroid.Core/Game/MotherBrainTurretDefinitions.cs#L71)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MotherBrainTurretInstructionProgramDefinitions.cs

- [x] **MotherBrainTurretInstructionProgramDefinitions.TurretPrograms** ([L46](../csharp/src/SuperMetroid.Core/Game/MotherBrainTurretInstructionProgramDefinitions.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainTurretInstructionProgramDefinitions.BulletPrograms** ([L52](../csharp/src/SuperMetroid.Core/Game/MotherBrainTurretInstructionProgramDefinitions.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainTurretInstructionProgramDefinitions.Words** ([L54](../csharp/src/SuperMetroid.Core/Game/MotherBrainTurretInstructionProgramDefinitions.cs#L54)) - factory-built stock table. Stored MotherBrainTurretInstructionMechanicsWord[] initialized by BuildWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **MotherBrainTurretInstructionProgramDefinitions.PresentationWords** ([L55](../csharp/src/SuperMetroid.Core/Game/MotherBrainTurretInstructionProgramDefinitions.cs#L55)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainFakeDeath.cs

- [ ] **RoomEnemySystem.MotherBrainFakeDeathExplosionPositions** ([L15](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainFakeDeath.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.MotherBrainFallingTubeXRadius** ([L26](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainFakeDeath.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.MotherBrainFallingTubeYRadius** ([L27](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainFakeDeath.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.MotherBrainFallingTubeFloor** ([L28](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainFakeDeath.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.MotherBrainFallingTubeSmokeXOffsets** ([L29](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainFakeDeath.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwo.cs

- [ ] **RoomEnemySystem.MotherBrainAscentDustXPositions** ([L23](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwo.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.ChooseMotherBrainSecondPhaseAttack / thresholds** ([L558](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwo.cs#L558)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration. Additional source branches/rows at lines 558, 559.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwoProjectiles.cs

- [ ] **RoomEnemySystem.MotherBrainDroolXOffsets** ([L11](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwoProjectiles.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.MotherBrainDroolYOffsets** ([L14](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.MotherBrainPhaseTwoProjectiles.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ShitroidInstructionProgramDefinitions.cs

- [ ] **ShitroidInstructionProgramDefinitions.Words** ([L36](../csharp/src/SuperMetroid.Core/Game/ShitroidInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ShitroidInstructionProgramDefinitions.PresentationWords** ([L65](../csharp/src/SuperMetroid.Core/Game/ShitroidInstructionProgramDefinitions.cs#L65)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MultiviolaInstructionProgramDefinitions.cs

- [ ] **MultiviolaInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/MultiviolaInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MultiviolaInstructionProgramDefinitions.PresentationWords** ([L26](../csharp/src/SuperMetroid.Core/Game/MultiviolaInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NintendoLogoFadePaletteFxProgramMechanicsDefinitions.cs

- [ ] **NintendoLogoFadePaletteFxProgramMechanicsDefinitions.Definitions** ([L72](../csharp/src/SuperMetroid.Core/Game/NintendoLogoFadePaletteFxProgramMechanicsDefinitions.cs#L72)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RioInstructionProgramDefinitions.cs

- [ ] **RioInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/RioInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RioInstructionProgramDefinitions.PresentationWords** ([L57](../csharp/src/SuperMetroid.Core/Game/RioInstructionProgramDefinitions.cs#L57)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/RipperVisualDefinitions.cs

- [x] **RipperVisualDefinitions.Shared** ([L15](../csharp/src/SuperMetroid.Core/Assets/RipperVisualDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RipperVisualDefinitions.Ordinary** ([L27](../csharp/src/SuperMetroid.Core/Assets/RipperVisualDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RipperInstructionProgramDefinitions.cs

- [x] **RipperInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/RipperInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RipperInstructionProgramDefinitions.PresentationWords** ([L48](../csharp/src/SuperMetroid.Core/Game/RipperInstructionProgramDefinitions.cs#L48)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/ShaktoolVisualDefinitions.cs

- [ ] **ShaktoolVisualDefinitions.Frames / literal at L16** ([L16](../csharp/src/SuperMetroid.Core/Assets/ShaktoolVisualDefinitions.cs#L16)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.ShaktoolProjectiles.cs

- [x] **RoomEnemySystem.ShaktoolCircleXOffsets** ([L8](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.ShaktoolProjectiles.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.ShaktoolCircleYOffsets** ([L10](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.ShaktoolProjectiles.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionDefinitions.cs

- [x] **ShaktoolInstructionDefinitions.OrientationInstructions** ([L13](../csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ShaktoolInstructionDefinitions.SegmentInstructions** ([L29](../csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionProgramDefinitions.cs

- [ ] **ShaktoolInstructionProgramDefinitions.Words** ([L62](../csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionProgramDefinitions.cs#L62)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ShaktoolInstructionProgramDefinitions.PresentationWords** ([L146](../csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionProgramDefinitions.cs#L146)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ShaktoolProjectileInstructionProgramDefinitions.cs

- [ ] **ShaktoolProjectileInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/ShaktoolProjectileInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ShaktoolProjectileInstructionProgramDefinitions.PresentationWords** ([L47](../csharp/src/SuperMetroid.Core/Game/ShaktoolProjectileInstructionProgramDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SparkInstructionProgramDefinitions.cs

- [ ] **SparkInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/SparkInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SparkInstructionProgramDefinitions.PresentationWords** ([L43](../csharp/src/SuperMetroid.Core/Game/SparkInstructionProgramDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SparkMovementDefinitions.cs

- [x] **SparkMovementDefinitions.InitialStates** ([L13](../csharp/src/SuperMetroid.Core/Game/SparkMovementDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/StockAttractDemoScenes.cs

- [ ] **StockAttractDemoScenes.Sets** ([L12](../csharp/src/SuperMetroid.Core/Frontend/StockAttractDemoScenes.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Input/StockAttractInputProgramsCatalog.cs

- [ ] **StockAttractInputPrograms.Commands** ([L49](../csharp/src/SuperMetroid.Core/Input/StockAttractInputProgramsCatalog.cs#L49)) - factory-built stock table. Stored Dictionary<ushort, Command> initialized by CreateCommands(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.cs

- [ ] **SuperMetroidRuntime.RunCeresFallingDebrisRoomMain / xPositions** ([L4319](../csharp/src/SuperMetroid.Core/Runtime/SuperMetroidRuntime.cs#L4319)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Assets/WorkRobotPaletteCycle.cs

- [x] **WorkRobotPaletteCycle.frames** ([L20](../csharp/src/SuperMetroid.Core/Assets/WorkRobotPaletteCycle.cs#L20)) - installed stock table. Original/default payload behind WorkRobotPaletteCycle.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/WorkRobotVisualDefinitions.cs

- [ ] **WorkRobotVisualDefinitions.Frames / literal at L17** ([L17](../csharp/src/SuperMetroid.Core/Assets/WorkRobotVisualDefinitions.cs#L17)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/WorkRobotInstructionProgramDefinitions.cs

- [ ] **WorkRobotInstructionProgramDefinitions.Words** ([L62](../csharp/src/SuperMetroid.Core/Game/WorkRobotInstructionProgramDefinitions.cs#L62)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/WorkRobotLaserDefinitions.cs

- [x] **WorkRobotLaserInstructionProgramDefinitions.Words** ([L49](../csharp/src/SuperMetroid.Core/Game/WorkRobotLaserDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **WorkRobotLaserInstructionProgramDefinitions.PresentationWords** ([L62](../csharp/src/SuperMetroid.Core/Game/WorkRobotLaserDefinitions.cs#L62)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/YellowPipeBugInstructionProgramDefinitions.cs

- [x] **YellowPipeBugInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/YellowPipeBugInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **YellowPipeBugInstructionProgramDefinitions.PresentationWords** ([L32](../csharp/src/SuperMetroid.Core/Game/YellowPipeBugInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

## Agent handoff

### Confirmed batch 1: Grapple, Spark and Shaktool

- Converted seven definitions: Grapple transfer records and angle sectors, Spark initial states, Shaktool orientation/segment selectors and circle X/Y offsets. No retention exceptions.
- Grapple sectors fold the 64 buckets into a half turn and reflect it, preserving the native 3 vertical / 8 diagonal / 5 horizontal widths. Transfers calculate endpoint source strides ($200), orientation source strides ($800), contiguous atlas offsets and byte counts on demand. Enumeration order and semantic asset dispatch are preserved; no generated record array remains.
- Spark dispatches steady, intermittent and emitter behavior directly. Selector three still returns the native adjacent pointer/opcode observations, with named compatibility constants; the opcode annotation now correctly identifies LDX $0E54.
- Shaktool orientation programs are consecutive eight-byte records rotated from left-first storage to up-first buckets. Segment dispatch follows primary saw / back arm / front arm / head / front arm / back arm / final saw. Circle placement computes cardinal radius 16 and outward-rounded diagonal radius / sqrt(2); Y is the quarter-turn component.
- Evidence: pinned bank_9B.asm $C346-$C3C4, bank_A8.asm $E682/$E688/$E68E, bank_AA selectors $DD15/$DF13/$DF21, bank_86.asm $BDE3/$BDF3. Focused verification independently reads the pinned NTSC J/U v1.0 ROM (SHA-256 asserted by entry point).
- Changed production: `GrappleTileDefinitions.cs`, `SparkMovementDefinitions.cs`, `ShaktoolInstructionDefinitions.cs`, `RoomEnemySystem.ShaktoolProjectiles.cs`, new assigned `ShaktoolProjectilePlacementDefinitions.cs`. Confirmation: `Program.LookupStream3.cs`, wired through `--lookup-stream-3`.
- Confirmation passed: Verification build (1431 warnings, zero errors) and `dotnet run --project csharp/src/SuperMetroid.Verification --no-build -- --lookup-stream-3`. Covers all ushort Grapple angles and Spark population parameters, all byte Shaktool angles, every transfer field/order, valid orientation/segment records and rejected domains.
- Integration dependency: coordinator must refresh the reviewed GrappleTileDefinitions source hashes in `VramDmaPresentationContractDefinitions.cs` and `VramDmaSourceContracts.cs` after review. No other stream production dependency.

### Confirmed batch 2: Work Robot colors

- Converted the installed six-by-four stock color payload in `WorkRobotPaletteCycle.cs`; no retained exception. Phase advances three positions then reverses. With p = (color + min(frame, 6-frame)) & 3, RGB5 red is 31 - 16*(p/2) - 7*(p%2), green and blue zero.
- The loader discards a fully matching stock payload and resolves colors on demand. Independently supplied changes retain their exact compiled values. ContentIdentity keeps the existing domain and row framing, so identical visible colors retain their identity.
- Evidence: all 24 original words at $A8:CCC1 + 10*frame + 2*color, cross-checked against the pinned bank_A8 assembly palette records.
- Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed original colors, ApplyFrame CGRAM writes, identical stock ContentIdentity, all 72 independent RGB channel edits, their selected identities, and frame/color bounds. Tests reside in the assigned `Program.LookupStream3.cs` partial.
- No new dependencies or shared-file edits.
### Confirmed batch 3: Pickup and Fireflea instruction layouts

- Converted six definitions without retention: enemy pickup/death selectors now dispatch named variants; pickup mechanics and visual operand addresses calculate five timed loops; Fireflea mechanics and visual operand addresses calculate its 52 timed records and loop tail.
- Pickup energy loops have four eight-tick records, missile loops two five-tick records, Power Bombs four five-tick records. Every loop branches to its start; the four earlier programs preserve their unreachable trailing sleep instruction. Metadata selects these semantic programs; no generated word arrays remain.
- Fireflea durations alternate two/one ticks with four-byte timed records; the final Goto and target follow the 52 records. Read, enumeration and ownership APIs calculate directly and retain their original accepted/rejected domains.
- Evidence and confirmation: pinned ROM pickup selectors $86:EF04, death selectors $86:EFD5, all 30 pickup mechanics/16 visual operands in $ED8D-$EDFD, and 54 Fireflea mechanics/52 visual operands in $A3:8C2F-$8D02. Verification build passed (1431 warnings, zero errors), and `--lookup-stream-3` passed native words, enumeration order, exact word/byte ownership, presentation exclusion, invalid indices and invalid kind/variant boundaries.
- Changed production: `EnemyPickupDefinitions.cs`, `EnemyDeathExplosionDefinitions.cs`, `EnemyPickupInstructionProgramDefinitions.cs`, `FirefleaInstructionProgramDefinitions.cs`. Confirmation extends the assigned stream partial; no additional dependencies.
### Confirmed batch 4: Choot control and motion selection

- Converted three definitions: normal/wide/very-wide/slow/very-slow motion selection is direct semantic dispatch, and the idle/jump/fall instruction mechanics plus visual operand addresses calculate on demand.
- Idle disables off-screen processing, displays one tick and sleeps. Jump/fall enable off-screen processing, display eight ticks then one tick and sleep; their final visual operands remain separate. The physical program layouts supply record addresses without cached word arrays.
- Shared path identities are named once in `ChootFallingPathDefinitions.cs`; selection retains the five original loop advances. The three original motion arrays are still required. Their former retention claims based on correction complexity, readability or scaling were deleted, as were obsolete retention claims in the converted selector/control files. No retained exception is asserted.
- Confirmation: Verification build passed (1431 warnings, zero errors), `--lookup-stream-3` passed all five pointers and native indirect loop advances ($A2:DF5E/$DF6A), all eleven native control words/five visual operand addresses ($D82C-$D84A), complete word/byte ownership, alias rejection and index boundaries.
- Changed production: `ChootPatternDefinitions.cs`, `ChootInstructionProgramDefinitions.cs`, `ChootFallingPathDefinitions.cs` (identity sharing and comment cleanup only; motion data unchanged). Tests extend the owned stream partial. No dependencies.
### Confirmed batch 5: Ripper-family animation and visuals

- Converted four definitions. Six direction programs share four timed records (8,7,8,7 ticks), then Goto and the start address. Mechanics and visual operand addresses calculate directly; no program record arrays remain.
- Visual selection computes neutral/first/neutral/second wing phase and direction. Shared GRipper/Ripper II compositions occupy four/three/four OAM-part records per direction; ordinary Ripper has three two-part records per direction. Frame pointers derive from those composition sizes. Both shared enemy owners retain their original union of accepted operand addresses.
- Evidence: all 36 native mechanics and 24 visual operands in the six $A2:E19B/E1AF/E2E0/E2F4/E477/E48B loops, and their $E3C5/$E527 sprite composition sequences.
- Verification build passed (1431 warnings, zero errors). `--lookup-stream-3` passed direct native records/order/ownership/bounds plus the existing six production animation fixtures with source bytes forbidden, including all twelve distinct selected frames.
- Changed production: `RipperInstructionProgramDefinitions.cs`, `RipperVisualDefinitions.cs`; tests extend the owned partial. No retained exception or cross-stream dependency.
### Confirmed batch 6: Mochtroid and Yellow Pipe Bug loops

- Converted four definitions: both families calculate their four-frame loops, Goto commands, branch targets, visual addresses and byte ownership directly. Mochtroid uses fourteen ticks in free flight and five when attached; Yellow Pipe Bug uses four ticks in straight flight and one when arcing, for each facing direction.
- Evidence: original $A3:A745/$A759 and $B3:8EFC/$8F10/$8F24/$8F38 programs. All 36 mechanics and 24 visual operand addresses match the pinned ROM.
- Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed direct native records and ownership/bounds, plus existing real-initializer/state-switch fixtures, two Mochtroid loops and four Yellow Pipe Bug loops with source reads forbidden.
- Changed production: `MochtroidInstructionProgramDefinitions.cs`, `YellowPipeBugInstructionProgramDefinitions.cs`; tests extend the owned partial. No retained exceptions or dependencies.
### Confirmed batch 7: Mother Brain death-fade calculations (partial payload disposition)

- Body/leg fade channels at $AD:EA0A calculate as (initial*(15-frame)+1)/15 using integer division. All 448 source words match. Corpse fade channels at $AD:F119 interpolate between the first/last palettes with nearest-integer rounding over seven intervals; all 120 words match.
- `MotherBrainDeathColorCatalog.cs` now discards matching frame tables and calculates these colors on demand, retaining only 58 independent endpoint words across the three sequences. Thus 510 non-endpoint words no longer remain stored lookups. Any supplied edit that does not match the rule keeps its exact frame values. ContentIdentity retains the original selected row framing.
- The three checklist entries remain unchecked: their endpoint colors still require their own conversion or specific impossible/nonsense justification. The exploded-door palette also remains required. No art/palette exemption is asserted.
- Confirmation: full Verification build passed (1431 warnings, zero errors), then verifier-only rebuild passed (25 warnings, zero errors). `--lookup-stream-3` passed all 568 native fade values, explicit stock calculated-path selection, all 1704 independent RGB channel edits and selected identities, and existing frame/color bounds. Work Robot confirmation additionally asserts that its original frame table is discarded.
- Integration dependency: coordinator must refresh `SequenceColorClosedContractDefinitions.cs` reviewed source hash for `MotherBrainDeathColorCatalog.cs` (old ECF27902773206B741220DB9BD0B94F7E6CB8F69AF591DFF606370EE0E76AC99) after review.
### Confirmed batch 8: Cutscene Baby fade (partial payload disposition)

- The six displayed palettes at $AD:E90C evaluate a linear RGB8-to-black fade, then quantize to RGB5: channel = endpointByte * (6-paletteIndex) / 48. Quantizing the first displayed row before interpolating loses precision and does not reproduce the source.
- The loader solves the bounded per-channel endpoint interval implied by every supplied row, chooses its midpoint, and discards the frame table only when that interval exists (the terminal frame must be black). This is a linear fade with one endpoint triplet per color, not a per-row correction map. Nonmatching edits remain exact supplied frames. Original RGB8 endpoints are not uniquely recoverable and no historical-tool claim is made.
- All 84 original displayed words match; the compatible midpoint endpoints also quantize to all fourteen original undisplayed RGB5 colors at $AD:E8F0. The fourteen independent endpoint colors and fifteen initial-palette colors still require their own disposition. Both checklist entries therefore remain unchecked, with no retained exception.
- Verification build passed (1431 warnings, zero errors). `--lookup-stream-3` passed all original displayed/initial colors, explicit calculated-path selection, the undisplayed endpoint cross-check, all 252 independent RGB channel edits and identities, and original bounds.
- Changed production: `BabyMetroidCutsceneColorCatalog.cs`; tests extend the owned partial. Coordinator hash dependency: `RemainingEnemyColorClosedContractDefinitions.cs` entry for this file (old 3FB29FB3B3F4FD142FC7212F76F4EFF34D44CAC22AAA6958AB223A595F916EB4).
### Confirmed batch 9: Mother Brain drain and fake-death fades (partial payload disposition)

- The eight drain rows selected by $AD:EF87 and eight fake-death rows selected by $AD:ED8A are exact nearest-integer RGB5 endpoint interpolation: `(first * (7-frame) + last * frame + 3) / 7` per channel. Matching assets discard their intermediate rows; runtime writes calculate each color directly. This removes 144 intermediate words across the two sequences while retaining 48 endpoint words for separate disposition.
- All 168 drain words (body, back legs and trailing WRAM word) and 24 fake-death words match the pinned ROM. Version 2 asset fallback reuses the selected stock fake-death fade. Independently edited rows remain exact.
- Verification build passed (1433 warnings, zero errors); `--lookup-stream-3` passed original CGRAM/WRAM destinations, explicit calculated-path selection, all 576 independent channel edits, legacy loading and frame bounds.
- Both entries remain unchecked because their independent endpoint colors are still required. Revival/from-grey also remains required: direct endpoint interpolation differs in two channels of one intermediate body color; no approximation or retention exception was introduced.
- Changed production: `MotherBrainRainbowPalettePresentation.cs`; confirmation extends the assigned stream partial.

### Confirmed batch 10: Rainbow back-leg shading (partial payload disposition)

- All 150 back-leg colors in the ten rainbow frames selected by $AD:E434 are the corresponding body RGB5 channel divided by two, rounded upward. Matching per-frame leg arrays are discarded; each consumer calculates the shadow color directly. An independently edited body or leg color that breaks this relationship keeps its exact supplied leg image.
- Body palettes remain required, and the rainbow checklist entry stays unchecked. Their frame differences show shared color-channel tint offsets with prequantization variation; this is further calculation work, not an authored-data exception. Normal-restoration colors likewise retain their required disposition.
- Verification build passed (1433 warnings, zero errors); `--lookup-stream-3` passed all 300 rainbow words, 30 normal-restoration words, calculated-shadow path selection in all ten frames, all 990 independent body/leg RGB channel edits, original bounds, and the preceding drain/legacy checks.
- Changed production: `MotherBrainRainbowPalettePresentation.cs`; confirmation extends the owned partial. The coordinator-owned source hash dependency remains the same file in `SequenceColorClosedContractDefinitions.cs`.

### Confirmed batch 11: Shitroid brightness pulse (partial payload disposition)

- The eight four-color rows at $A9:F6D1 subtract five from each channel at four brightness levels, then retrace: `phase = min(frame, 7-frame)`. Channels clamp between a shared minimum (native RGB5 5,0,0) and 31. The first two red highlight origins are 36 before clipping, explaining their repeated saturated red values.
- Matching assets discard all eight stored rows and retain only four unclipped origin triplets plus the shared minimum; no frame-indexed cache remains. Nonmatching edits retain exact supplied rows, and content identity keeps its original row framing.
- The normal-cycle entry stays unchecked: the independent origin colors and minimum remain required, as do all three target palettes. No retained exception is asserted.
- Verification build passed (1193 warnings, zero errors on final incremental build); `--lookup-stream-3` passed all 32 native colors, calculated-path selection, all 96 independent RGB channel edits and identities, and original frame/color bounds.
- Changed production: `ShitroidColorCatalog.cs`; confirmation extends the assigned partial. Coordinator dependency: refresh `RemainingEnemyColorClosedContractDefinitions.cs` reviewed source hash (old 34CF411095FFF305D4C51537FDCCAD3CA2BD6FA4C81B307130C5ECFEE363328E).

### Confirmed batch 12: Mother Brain corpse geometry and transfers

- Converted seven inventoried definitions (five corpse-rotting tables and two corpse page mappings), plus both sequence aliases. Tile rows advance seven 32-byte tiles; columns advance one tile. The minimum-Y cases preserve the native missing-column branches. Initial copies select the right-hand frame in six source pages, omitting the last tile in the first four rows.
- Rot transfers derive their visible starting column, width, WRAM source and VRAM destination from each row's geometry. All six records remain available in the original order on every active step; no generated record cache remains. Full corpse page sources/destinations are calculated, and all room/sequence callers use those operations.
- Evidence: all eight native row offsets at $A9:E262, six four-word rot transfers at $A9:E1F4, six seven-byte page records at $A9:9003, and explicit CMP/BCC outline gates in $A9:EA40/$EB0B.
- Verification build passed (1433 warnings, zero errors); `--lookup-stream-3` passed the native mappings/enumeration/bounds, all 48 pixel rows in both real copy/move modes, and the existing installed-artwork fixture (native staging oracle, room/sequence initialization, six cartridge-free VRAM pages, edited PNG/reload behavior).
- Production edits: owned corpse artwork catalog, rotting state, sequence and state machine; additionally assigned sequence Helpers and RoomEnemySystem.MotherBrainRainbowBeam caller files. Verification includes the assigned stream partial and additionally assigned Program.MotherBrainCorpseArtwork.cs. No retained exception.

### Confirmed batch 13: Escape timer/text and exploded-door transfers

- Converted five definitions. Escape text sources, byte counts and destinations calculate five pages from the sheet extent (four full pages and a half page). The combined escape list selects two contiguous timer-number pages followed by those five text pages. The exploded-door list calculates its two full pages from the existing sheet definition. Sequence cursors and same-call handoff conditions retain their original counts and ordering.
- Native evidence: all seven $A6:C4CB-$C4FB timer/text records and both $A9:902F-$903C door records. Domain catalogs now own every source, size and destination used by these calculated operations.
- Verification build passed (25 warnings, zero errors on final verifier rebuild); `--lookup-stream-3` passed all nine native records, actual sequence transfer helper outputs/cursor increments/completion errors, page boundaries, and the installed five-page artwork fixture including visible edit/reload behavior.
- The existing escape-artwork fixture's old uninstalled cartridge fallback no longer exists in production. Its oracle now reads native records/bytes directly, while production continues using installed artwork. The initially failing fixture was rerun and passed after that correction; no gameplay behavior was changed to accommodate it.
- Production: owned escape-text/special-sheet catalogs, sequence, state machine, additionally assigned Helpers and room transfer callers. Verification: owned partial and additionally assigned Program.MotherBrainEscapeTextArtwork.cs. Coordinator hash dependency: `MotherBrainSheetsClosedContractDefinitions.cs` source entry for `MotherBrainSpecialSpriteArtworkDefinitions.cs` (old 4CF6AF09EF81015FDAD738612261276031AFEDC5308C93C2D148E1D3A842D5AF). Independent pixel artwork remains outside these completed metadata definitions; no retained exception is asserted.

### Confirmed batch 14: Drained stagger curves

- Converted the three eight-stage painful-walking definitions into the assigned `MotherBrainPainfulWalkingDefinitions.cs` catalog. Each forward/backward pair shares its settings. Pause timers increase sixteen ticks per pair; animation delay is two ticks for the first pair and increases by two from six thereafter. Neck motion halves angular scale per pair while reducing gain five/four/three/two: `((5-pair) << 8) >> pair`.
- Removed all three stored spans; the state machine and assigned Helpers timer call calculate directly. The outer stage guard and the timer caller's existing clamp to stage seven remain intact.
- Native evidence: all 24 words at $A9:BEEE/$BEFE/$C049. Verification build passed (1433 warnings, zero errors); `--lookup-stream-3` passed every native value, invalid index domains and the actual timer helper for all eight stages plus terminal stages eight and 65535.
- No retained exception or shared inventory/hash edit. The new catalog and expanded Helpers scope were assigned by the coordinator.

### Confirmed batch 15: Mother Brain contact regions

- Converted three component rectangle arrays to region dispatch: lower then upper body, upper then lower brain, and the symmetric neck region. A lightweight component value enumerates those cases without any stored rectangle list. The order remains explicit because collision resolution stops at the first matching region.
- All five signed rectangles retain their exact asymmetry and the gap boundary between brain local Y=0 and Y=1. Unsupported components and out-of-range region indices keep their previous exception types.
- Verification build passed (1433 warnings, zero errors); `--lookup-stream-3` passed the existing native fixture's twenty extent words and 49,155 production collision comparisons, plus region bounds. This reuses the existing confirmation matrix for the changed region-enumeration contract; no new gameplay search was added.
- Production: `MotherBrainContactHitboxDefinitions.cs`; the runtime foreach consumer requires no modification. Verification: owned partial and additionally assigned Program.MotherBrainContactHitboxes.cs span-to-region-list adaptation. No retention or external source-hash dependency found.

### Confirmed batch 16: Death variants and escape palette registrations

- Converted the three death-explosion instruction selectors to small-explosion, smoke and big-explosion cases using their existing named program identities. The parameter domain and exception details remain unchanged.
- Replaced the state-machine palette-pointer literal with four explicit named registrations: shutter, background, general level, then Arkanoid/red-orb flashing. These correspond directly to the four native LDY/JSL spawn pairs at $A9:B295-$B2AD; the second door transfer still triggers them once.
- Verification build passed (1433 warnings, zero errors); `--lookup-stream-3` passed all three native selector words at $86:C929, selector boundaries, and a real three-call escape handoff: no palette registration after door page one, all four native registrations in order after page two, and no repeat on the following text frame.
- Changed production: owned death-explosion catalog and state machine; verification extends the owned partial. Two inventory entries completed, with no retained exception.

### Confirmed batch 17: Room turret placement and launch geometry

- Converted both turret definitions. Twelve placements calculate four repeated three-turret bays with 192-pixel spacing and the original initial-direction cases. Rotation masks are contiguous direction sectors, widened for alternating bays; policy identities use an eight-byte stride.
- Pose identities use six-byte spacing. Bullet velocities calculate sine/cosine octants at speed 704 with integer rounding; muzzle X uses radius 17 and vertical barrel anchors use named direction cases. Neither definition retains a cached array.
- Verification build passed (1433 warnings, zero errors); `--lookup-stream-3` passed the existing native fixture's twelve placements, 96 rotation bytes, sixteen instruction selectors, 32 bullet words, 384 rotations and all twenty real spawns with source tables forbidden.
- Changed production: owned turret catalog; verification reuses the existing turret fixture through the owned partial. Two inventory entries completed, with no retained exception or external source-hash dependency found.
### Confirmed batch 18: Narrow decorative death-scatter exception

- Justified retention applies only to the 28 signed X/Y anchors at $A9:B099-$B108 (seven groups of four). $A9:B03E-$B098 decrements and wraps the group, reads a pair, then uses RNG thresholds only to select explosion type. $86:C8F5 stores the offsets in the nominal velocity fields; $86:C914 adds them unchanged to the body's position each frame, with no velocity integration. The $86:CB13 descriptor has zero hitbox radii, zero damage and damage-disabled properties. These coordinates are the chosen decorative scatter layout itself; generating a different distribution would replace that visual content with invented content. This is the approved nonsense exception, not a performance, complexity, provenance or generic authored-data exemption.
- Moved only the anchor payload out of the functional sequence into the owned death-explosion domain catalog. Group count and width are named; reverse wrapping and flat indexing remain arithmetic, cadence remains executable behavior, and type selection remains semantic cases. The neighboring Baby explosion X/Y offsets and all unrelated geometry retain their separate unresolved obligations.
- Verification build passed (1433 warnings, zero errors); `--lookup-stream-3` passed all 56 native coordinate words and sixteen actual production bursts covering both smoke/mixed counts, every group and wrap, one RNG call per projectile, type independence and body-relative positions.
- One original inventory definition is justified retained, not converted. The converted count remains 47. No external source-hash dependency found.
### Confirmed batch 19: Turret and bullet instruction programs

- Converted all four stored catalogs: turret poses, bullet poses, 49 mechanics words and 21 presentation operand addresses. Six-byte pose strides, two-byte direction selectors and four-byte smoke frames calculate addresses directly. Sleep, direction dispatch, palette reset, movement clearing and deletion use semantic instruction cases; smoke holds its final frame for 32 ticks after four eight-tick frames. No factory-built or cached replacement arrays remain.
- Preserved index exceptions, direction domain, mechanics-word rejection and exact byte ownership. Verification compares all 49 native words and all 65,536 bank-byte ownership decisions to an independent address-set oracle.
- Existing fixture initially failed its obsolete requirement for 21 runtime cartridge spritemap reads. The additionally assigned fixture now asserts all 21 exact native operand identities on actual production pose/smoke frames, installed presentation membership, placeholder behavior and zero runtime presentation reads. Existing real turret producers, all eight bullet selectors, smoke timing, deletion and mechanics source guards remain intact.
- Full build passed (1433 warnings, zero errors), followed by the final verifier rebuild (25 warnings, zero errors); `--lookup-stream-3` passed after the fixture correction. Four inventory entries completed, bringing converted entries to 51 plus one separately justified decorative retention. No external source-hash dependency found.
### Confirmed batch 20: Work Robot laser prefix and loop

- Converted the nine-word mechanics catalog and seven-word presentation address catalog. Seven four-tick frames calculate a four-byte stride; the closing Goto and its fourth-frame target are named control cases. Address reads and byte ownership calculate the mechanics gaps directly.
- The additionally assigned existing fixture now follows native ROM instruction control flow as its oracle, comparing actual installed presentation operand identities and frame durations while requiring zero runtime presentation reads. All five real laser definitions, the complete prefix and loop, and shared shot deletion remain covered.
- Verification build passed (1433 warnings, zero errors); `--lookup-stream-3` passed all nine native words and the actual producer/animation/deletion checks. Two inventory entries completed, bringing converted entries to 53 plus the separate decorative-anchor retention. No external source-hash dependency found.
### Confirmed batch 21: Revival interpolation before quantization (partial)

- The reverse $AD:ED9C palette sequence has 56 of 57 color-channel trajectories consistent with straight RGB8 interpolation before RGB5 quantization. Import solves the finite endpoint intervals implied by the supplied first/last RGB5 values and accepts a calculated channel only if every supplied row agrees. Gameplay evaluates the selected endpoint equation directly; it does not cache generated frame colors.
- Removed the full revival frame array from runtime presentation storage. One channel remains supplied: body color ten green, `[17,16,16,14,14,13,12,11]`, confirmed against pinned $AD:EDAE-$EECA. That unmatched channel and the independent endpoint color basis remain required; failure to fit this fade model is not a retention justification. The containing inventory entry stays unchecked. Custom channel edits remain exact whether they admit an endpoint model or require supplied values.
- Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed all 152 native revival words, body/brain tail preservation, fake-death reuse, WRAM trailing output, all 456 independent channel edits, and a storage assertion proving exactly one of 57 channel curves remains supplied for stock data. Existing drain/rainbow/fake-death fixtures also passed.
- Production: owned `MotherBrainRainbowPalettePresentation.cs`; verification: owned stream partial. Coordinator must refresh its existing `SequenceColorClosedContractDefinitions.cs` source hash. This is partial mathematical removal, with no new completed entry or retention exception.
### Remaining scope

All unchecked entries remain required. The three Choot motion payloads still require conversion or concrete impossible/nonsense evidence; their rejected retention rationale has been removed. Mother Brain fade endpoints remain required after the calculation conversion above. Choot quadratic/cubic phase fits do not establish a complete generator or a retention exception; its motion payloads remain required.
