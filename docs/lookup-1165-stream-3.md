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

- [ ] **ChootInstructionProgramDefinitions.Words** ([L45](../csharp/src/SuperMetroid.Core/Game/ChootInstructionProgramDefinitions.cs#L45)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChootInstructionProgramDefinitions.PresentationWords** ([L60](../csharp/src/SuperMetroid.Core/Game/ChootInstructionProgramDefinitions.cs#L60)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChootPatternDefinitions.cs

- [x] **ChootPatternDefinitions.Patterns** ([L33](../csharp/src/SuperMetroid.Core/Game/ChootPatternDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.cs

- [x] **CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.Durations** ([L59](../csharp/src/SuperMetroid.Core/Game/CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.cs#L59)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.Definitions** ([L61](../csharp/src/SuperMetroid.Core/Game/CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.cs#L61)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CrateriaLightningPaletteFxProgramMechanicsDefinitions.cs

- [x] **CrateriaLightningPaletteFxProgramMechanicsDefinitions.Definitions** ([L41](../csharp/src/SuperMetroid.Core/Game/CrateriaLightningPaletteFxProgramMechanicsDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/CreditsPresentation.cs

- [x] **CreditsPresentation.rows** ([L9](../csharp/src/SuperMetroid.Core/Assets/CreditsPresentation.cs#L9)) - installed stock table. Original/default payload behind CreditsPresentation.rows. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/CreditsPresentationDefinitions.cs

- [x] **CreditsPresentationDefinitions.Lines** ([L28](../csharp/src/SuperMetroid.Core/Assets/CreditsPresentationDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs

- [x] **DoorTransitionState.BuildSourceFadeTarget / alwaysPreserved** ([L250](../csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs#L250)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **DoorTransitionState.BuildSourceFadeTarget / commonCreColors** ([L261](../csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs#L261)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.
- [x] **DoorTransitionState.BuildSourceFadeTarget / timerColors** ([L266](../csharp/src/SuperMetroid.Core/Frontend/DoorTransitionState.cs#L266)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Rooms/DoorScrollPrograms.cs

- [x] **DoorScrollPrograms.Programs** ([L33](../csharp/src/SuperMetroid.Core/Rooms/DoorScrollPrograms.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorCatalog.cs

- [ ] **EnemyAuxiliaryColorCatalog.frames** ([L19](../csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorCatalog.cs#L19)) - installed stock table. Original/default payload behind EnemyAuxiliaryColorCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorDefinitions.cs

- [x] **EnemyAuxiliaryColorDefinitions.Definitions** ([L29](../csharp/src/SuperMetroid.Core/Assets/EnemyAuxiliaryColorDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **EnemySpritemapDefinitions.NamedFrameDefinitions** ([L183](../csharp/src/SuperMetroid.Core/Assets/EnemySpritemapDefinitions.cs#L183)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyDeathExplosionDefinitions.cs

- [x] **EnemyDeathExplosionDefinitions.InstructionPointers** ([L21](../csharp/src/SuperMetroid.Core/Game/EnemyDeathExplosionDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs

- [ ] **EnemyDeathInstructionProgramDefinitions.Words** ([L33](../csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EnemyDeathInstructionProgramDefinitions.PresentationWords** ([L127](../csharp/src/SuperMetroid.Core/Game/EnemyDeathInstructionProgramDefinitions.cs#L127)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyDropChanceDefinitions.cs

- [ ] **EnemyDropChanceDefinitions.PackedChances** ([L41](../csharp/src/SuperMetroid.Core/Game/EnemyDropChanceDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyPickupDefinitions.cs

- [x] **EnemyPickupDefinitions.InstructionLists** ([L17](../csharp/src/SuperMetroid.Core/Game/EnemyPickupDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyPickupInstructionProgramDefinitions.cs

- [ ] **EnemyPickupInstructionProgramDefinitions.Words** ([L30](../csharp/src/SuperMetroid.Core/Game/EnemyPickupInstructionProgramDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EnemyPickupInstructionProgramDefinitions.PresentationWords** ([L64](../csharp/src/SuperMetroid.Core/Game/EnemyPickupInstructionProgramDefinitions.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs

- [ ] **EnemyProjectileInstructionMechanicsDefinitions.BlueRingDurations** ([L76](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EnemyProjectileInstructionMechanicsDefinitions.MiscDustInitialPointers** ([L84](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L84)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EnemyProjectileInstructionMechanicsDefinitions.TimedPrograms** ([L93](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L93)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EnemyProjectileInstructionMechanicsDefinitions.MechanicsWords** ([L162](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L162)) - factory-built stock table. Stored EnemyProjectileMechanicsWordDefinition[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **EnemyProjectileInstructionMechanicsDefinitions.PresentationFrames** ([L165](../csharp/src/SuperMetroid.Core/Game/EnemyProjectileInstructionMechanicsDefinitions.cs#L165)) - factory-built stock table. Stored EnemyProjectilePresentationFrameDefinition[] initialized by BuildPresentationFrames(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/EnemyVulnerabilityDefinitions.cs

- [ ] **EnemyVulnerabilityDefinitions.PackedVulnerabilities** ([L74](../csharp/src/SuperMetroid.Core/Game/EnemyVulnerabilityDefinitions.cs#L74)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ExploredMapPackingDefinitions.cs

- [x] **ExploredMapPackingDefinitions.CrateriaIndexes** - Calculate occupied eight-cell groups in native page order from immutable stock map topology; calculate packed offsets from preceding counts. See batch 56.
- [x] **ExploredMapPackingDefinitions.BrinstarIndexes** - Calculate occupied eight-cell groups in native page order from immutable stock map topology; calculate packed offsets from preceding counts. See batch 56.
- [x] **ExploredMapPackingDefinitions.NorfairIndexes** - Calculate occupied eight-cell groups in native page order from immutable stock map topology; calculate packed offsets from preceding counts. See batch 56.
- [x] **ExploredMapPackingDefinitions.WreckedShipIndexes** - Calculate occupied eight-cell groups in native page order from immutable stock map topology; calculate packed offsets from preceding counts. See batch 56.
- [x] **ExploredMapPackingDefinitions.MaridiaIndexes** - Calculate occupied eight-cell groups in native page order from immutable stock map topology; calculate packed offsets from preceding counts. See batch 56.
- [x] **ExploredMapPackingDefinitions.TourianIndexes** - Calculate occupied eight-cell groups in native page order from immutable stock map topology; calculate packed offsets from preceding counts. See batch 56.

### csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs

- [ ] **FileSelectPresentation.pages** ([L14](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L14)) - installed stock table. Original/default payload behind FileSelectPresentation.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **FileSelectPresentation.patches** ([L15](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L15)) - installed stock table. Original/default payload behind FileSelectPresentation.patches. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **FileSelectPresentation.digits** ([L16](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L16)) - installed stock table. Original/default payload behind FileSelectPresentation.digits. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **FileSelectPresentation.slotLetters** ([L17](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L17)) - installed stock table. Original/default payload behind FileSelectPresentation.slotLetters. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **FileSelectPresentation.sprites** ([L18](../csharp/src/SuperMetroid.Core/Assets/FileSelectPresentation.cs#L18)) - installed stock table. Original/default payload behind FileSelectPresentation.sprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/FirefleaInstructionProgramDefinitions.cs

- [ ] **FirefleaInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/FirefleaInstructionProgramDefinitions.cs#L20)) - factory-built stock table. Stored FirefleaInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **FirefleaInstructionProgramDefinitions.PresentationWords** ([L21](../csharp/src/SuperMetroid.Core/Game/FirefleaInstructionProgramDefinitions.cs#L21)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs

- [ ] **GameOptionsPresentation.pages** ([L16](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L16)) - installed stock table. Original/default payload behind GameOptionsPresentation.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **GameOptionsPresentation.controllerLabels** ([L17](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L17)) - installed stock table. Original/default payload behind GameOptionsPresentation.controllerLabels. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **GameOptionsPresentation.controllerLabelAnchors** ([L18](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L18)) - installed stock table. Original/default payload behind GameOptionsPresentation.controllerLabelAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **GameOptionsPresentation.languageRegions** ([L19](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L19)) - installed stock table. Original/default payload behind GameOptionsPresentation.languageRegions. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **GameOptionsPresentation.specialToggles** ([L20](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L20)) - installed stock table. Original/default payload behind GameOptionsPresentation.specialToggles. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOptionsPresentation.sprites** ([L21](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L21)) - installed stock table. Original/default payload behind GameOptionsPresentation.sprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **GameOptionsPresentation.headingAnchors** ([L22](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L22)) - installed stock table. Original/default payload behind GameOptionsPresentation.headingAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **GameOptionsPresentation.cursorAnchors** ([L23](../csharp/src/SuperMetroid.Core/Assets/GameOptionsPresentation.cs#L23)) - installed stock table. Original/default payload behind GameOptionsPresentation.cursorAnchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs

- [x] **GameOverPresentation.tilemap** ([L16](../csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs#L16)) - installed stock table. Original/default payload behind GameOverPresentation.tilemap. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **GameOverPresentation.sprites** ([L17](../csharp/src/SuperMetroid.Core/Assets/GameOverPresentation.cs#L17)) - installed stock table. Original/default payload behind GameOverPresentation.sprites. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GrappleSpriteCatalog.cs

- [x] **GrappleSpriteCatalog.segments** ([L10](../csharp/src/SuperMetroid.Core/Assets/GrappleSpriteCatalog.cs#L10)) - installed stock table. Original/default payload behind GrappleSpriteCatalog.segments. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GrappleSpriteDefinitions.cs

- [x] **GrappleSpriteDefinitions.SegmentAttributeAddresses** ([L11](../csharp/src/SuperMetroid.Core/Assets/GrappleSpriteDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs

- [x] **GrappleSwingFrameCatalog.frames** ([L9](../csharp/src/SuperMetroid.Core/Assets/GrappleSwingFrameCatalog.cs#L9)) - installed stock table. Original/default payload behind GrappleSwingFrameCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs

- [ ] **GrappleTileAtlas.tiles** ([L8](../csharp/src/SuperMetroid.Core/Assets/GrappleTileAtlas.cs#L8)) - installed stock table. Original/default payload behind GrappleTileAtlas.tiles. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs

- [x] **GrappleTileDefinitions.transfers** ([L20](../csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GrappleTileDefinitions.segments** ([L33](../csharp/src/SuperMetroid.Core/Assets/GrappleTileDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GrappleBodyPlacementDefinitions.cs

- [ ] **GrappleBodyPlacementDefinitions.LeftOffsets** ([L8](../csharp/src/SuperMetroid.Core/Game/GrappleBodyPlacementDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GrappleBodyPlacementDefinitions.RightOffsets** ([L17](../csharp/src/SuperMetroid.Core/Game/GrappleBodyPlacementDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs

- [x] **GrappleConnectionDefinitions.Cancellation** ([L11](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L11)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GrappleConnectionDefinitions.Handlers** ([L15](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **GrappleConnectionDefinitions.SpecialAngleRecords** ([L29](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GrappleConnectionDefinitions.StandingDrops** ([L42](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **GrappleConnectionDefinitions.CrouchingDrops** ([L50](../csharp/src/SuperMetroid.Core/Game/GrappleConnectionDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HopperAnimationDefinitions.cs

- [x] **HopperAnimationDefinitions.Variants** ([L18](../csharp/src/SuperMetroid.Core/Game/HopperAnimationDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HopperInstructionProgramDefinitions.cs

- [ ] **HopperInstructionProgramDefinitions.Words** ([L55](../csharp/src/SuperMetroid.Core/Game/HopperInstructionProgramDefinitions.cs#L55)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **HopperInstructionProgramDefinitions.PresentationWords** ([L121](../csharp/src/SuperMetroid.Core/Game/HopperInstructionProgramDefinitions.cs#L121)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/IntroCaretSpriteDefinitions.cs

- [x] **IntroCaretSpriteDefinitions.frameDefinitions** ([L20](../csharp/src/SuperMetroid.Core/Assets/IntroCaretSpriteDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **IntroNarrationDefinitions.NativePages** ([L70](../csharp/src/SuperMetroid.Core/Assets/IntroNarrationDefinitions.cs#L70)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/IntroNarrationPresentation.cs

- [x] **IntroNarrationPresentation.pages** ([L12](../csharp/src/SuperMetroid.Core/Assets/IntroNarrationPresentation.cs#L12)) - installed stock table. Original/default payload behind IntroNarrationPresentation.pages. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs

- [x] **IntroCinematicRomData.Palette.GameplayRegions** ([L92](../csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs#L92)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **IntroCinematicRomData.Palette.GameplayClearRegions** ([L98](../csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs#L98)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **IntroCinematicRomData.Palette.NarrationRegions** ([L104](../csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs#L104)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **IntroCinematicRomData.Palette.DiscoveryRegions** ([L111](../csharp/src/SuperMetroid.Core/Frontend/IntroCinematicRomData.cs#L111)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MochtroidVisualDefinitions.cs

- [x] **MochtroidVisualDefinitions.Selectors** ([L14](../csharp/src/SuperMetroid.Core/Assets/MochtroidVisualDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MochtroidVisualDefinitions.Frames / literal at L22** ([L22](../csharp/src/SuperMetroid.Core/Assets/MochtroidVisualDefinitions.cs#L22)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

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

- [x] **MotherBrainDeathColorCatalog.bodyFade** ([L19](../csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs#L19)) - installed stock table. Original/default payload behind MotherBrainDeathColorCatalog.bodyFade. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **MotherBrainDeathColorCatalog.legFade** ([L20](../csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs#L20)) - installed stock table. Original/default payload behind MotherBrainDeathColorCatalog.legFade. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **MotherBrainDeathColorCatalog.corpseFade** ([L21](../csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs#L21)) - installed stock table. Original/default payload behind MotherBrainDeathColorCatalog.corpseFade. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainDeathColorCatalog.explodedDoor** ([L22](../csharp/src/SuperMetroid.Core/Assets/MotherBrainDeathColorCatalog.cs#L22)) - installed stock table. Original/default payload behind MotherBrainDeathColorCatalog.explodedDoor. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs

- [x] **MotherBrainEscapeTextArtworkDefinitions.PageSources** ([L17](../csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainEscapeTextArtworkDefinitions.PageByteCounts** ([L21](../csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainEscapeTextArtworkDefinitions.PageDestinations** ([L25](../csharp/src/SuperMetroid.Core/Assets/MotherBrainEscapeTextArtworkDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs

- [x] **MotherBrainHealthPalettePresentation.body** ([L10](../csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs#L10)) - installed stock table. Original/default payload behind MotherBrainHealthPalettePresentation.body. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **MotherBrainHealthPalettePresentation.backLegs** ([L11](../csharp/src/SuperMetroid.Core/Assets/MotherBrainHealthPalettePresentation.cs#L11)) - installed stock table. Original/default payload behind MotherBrainHealthPalettePresentation.backLegs. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs

- [ ] **MotherBrainRainbowPalettePresentation.rainbow** ([L10](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L10)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.rainbow. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRainbowPalettePresentation.toGrey** ([L11](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L11)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.toGrey. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRainbowPalettePresentation.fromGrey** ([L12](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L12)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.fromGrey. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **MotherBrainRainbowPalettePresentation.fakeDeathToGrey** ([L13](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L13)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.fakeDeathToGrey. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **MotherBrainRainbowPalettePresentation.beamCycle** ([L16](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRainbowPalettePresentation.cs#L16)) - installed stock table. Original/default payload behind MotherBrainRainbowPalettePresentation.beamCycle. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs

- [ ] **MotherBrainRoomColorPresentation.flash** ([L10](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L10)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.flash. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.finalRoom** ([L11](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L11)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.finalRoom. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.phaseTwoAttack** ([L12](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L12)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.phaseTwoAttack. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **MotherBrainRoomColorPresentation.phaseTwoRearLeg** ([L13](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L13)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.phaseTwoRearLeg. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.initialGlassShard** ([L14](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L14)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.initialGlassShard. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.initialTubeProjectile** ([L15](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L15)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.initialTubeProjectile. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **MotherBrainRoomColorPresentation.recoveryLights** ([L16](../csharp/src/SuperMetroid.Core/Assets/MotherBrainRoomColorPresentation.cs#L16)) - installed stock table. Original/default payload behind MotherBrainRoomColorPresentation.recoveryLights. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkDefinitions.cs

- [x] **MotherBrainSpecialSpriteArtworkDefinitions.All** ([L58](../csharp/src/SuperMetroid.Core/Assets/MotherBrainSpecialSpriteArtworkDefinitions.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MotherBrainVisualDefinitions.cs

- [x] **MotherBrainVisualDefinitions.NativePointers** ([L18](../csharp/src/SuperMetroid.Core/Assets/MotherBrainVisualDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **MotherBrainBeamRomData.QuadrantDirections** ([L10](../csharp/src/SuperMetroid.Core/Game/MotherBrainBeamRomData.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **MotherBrainFallingTubeInstructionDefinitions.VisualPointers** ([L23](../csharp/src/SuperMetroid.Core/Game/MotherBrainFallingTubeInstructionDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **MotherBrainRoomPaletteProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MotherBrainRoomPaletteProgramDefinitions.PresentationWords** ([L33](../csharp/src/SuperMetroid.Core/Game/MotherBrainRoomPaletteProgramDefinitions.cs#L33)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [x] **ShitroidInstructionProgramDefinitions.PresentationWords** ([L65](../csharp/src/SuperMetroid.Core/Game/ShitroidInstructionProgramDefinitions.cs#L65)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MultiviolaInstructionProgramDefinitions.cs

- [ ] **MultiviolaInstructionProgramDefinitions.Words** ([L17](../csharp/src/SuperMetroid.Core/Game/MultiviolaInstructionProgramDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MultiviolaInstructionProgramDefinitions.PresentationWords** ([L26](../csharp/src/SuperMetroid.Core/Game/MultiviolaInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NintendoLogoFadePaletteFxProgramMechanicsDefinitions.cs

- [ ] **NintendoLogoFadePaletteFxProgramMechanicsDefinitions.Definitions** ([L72](../csharp/src/SuperMetroid.Core/Game/NintendoLogoFadePaletteFxProgramMechanicsDefinitions.cs#L72)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RioInstructionProgramDefinitions.cs

- [ ] **RioInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/RioInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RioInstructionProgramDefinitions.PresentationWords** ([L57](../csharp/src/SuperMetroid.Core/Game/RioInstructionProgramDefinitions.cs#L57)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [x] **ShaktoolInstructionProgramDefinitions.PresentationWords** ([L146](../csharp/src/SuperMetroid.Core/Game/ShaktoolInstructionProgramDefinitions.cs#L146)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ShaktoolProjectileInstructionProgramDefinitions.cs

- [ ] **ShaktoolProjectileInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/ShaktoolProjectileInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ShaktoolProjectileInstructionProgramDefinitions.PresentationWords** ([L47](../csharp/src/SuperMetroid.Core/Game/ShaktoolProjectileInstructionProgramDefinitions.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SparkInstructionProgramDefinitions.cs

- [ ] **SparkInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/SparkInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SparkInstructionProgramDefinitions.PresentationWords** ([L43](../csharp/src/SuperMetroid.Core/Game/SparkInstructionProgramDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SparkMovementDefinitions.cs

- [x] **SparkMovementDefinitions.InitialStates** ([L13](../csharp/src/SuperMetroid.Core/Game/SparkMovementDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Frontend/StockAttractDemoScenes.cs

- [ ] **StockAttractDemoScenes.Sets** ([L12](../csharp/src/SuperMetroid.Core/Frontend/StockAttractDemoScenes.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Input/StockAttractInputProgramsCatalog.cs

- [x] **StockAttractInputPrograms.Commands** - mixed conversion and narrowly justified choreography retention: all823 commands dispatch directly,798 input successors calculate, and only798 duration/held/new-input triples retain the exact native demonstration performance. See batch80; object selection and scene setup receive no exception.

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

- [ ] **YellowPipeBugInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/YellowPipeBugInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
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

- Converted four selector/layout definitions. Pickup and Fireflea control/address structure calculates, but their two timing-bearing Words aggregates remain pending for independent hold magnitudes (pickup8/5; Fireflea2/1). No retention is justified by this structure.
- Pickup energy loops have four eight-tick records, missile loops two five-tick records, Power Bombs four five-tick records. Every loop branches to its start; the four earlier programs preserve their unreachable trailing sleep instruction. Metadata selects these semantic programs; no generated word arrays remain.
- Fireflea durations alternate two/one ticks with four-byte timed records; the final Goto and target follow the 52 records. Read, enumeration and ownership APIs calculate directly and retain their original accepted/rejected domains.
- Evidence and confirmation: pinned ROM pickup selectors $86:EF04, death selectors $86:EFD5, all 30 pickup mechanics/16 visual operands in $ED8D-$EDFD, and 54 Fireflea mechanics/52 visual operands in $A3:8C2F-$8D02. Verification build passed (1431 warnings, zero errors), and `--lookup-stream-3` passed native words, enumeration order, exact word/byte ownership, presentation exclusion, invalid indices and invalid kind/variant boundaries.
- Changed production: `EnemyPickupDefinitions.cs`, `EnemyDeathExplosionDefinitions.cs`, `EnemyPickupInstructionProgramDefinitions.cs`, `FirefleaInstructionProgramDefinitions.cs`. Confirmation extends the assigned stream partial; no additional dependencies.
### Confirmed batch 4: Choot control and motion selection

- Converted two selector/layout definitions. Motion selection is semantic dispatch and visual operand addresses calculate. Idle/jump/fall control structure calculates, but the Words aggregate remains pending for independent idle1 and jump/fall8+1 holds.
- Idle disables off-screen processing, displays one tick and sleeps. Jump/fall enable off-screen processing, display eight ticks then one tick and sleep; their final visual operands remain separate. The physical program layouts supply record addresses without cached word arrays.
- Shared path identities are named once in `ChootFallingPathDefinitions.cs`; selection retains the five original loop advances. The three original motion arrays are still required. Their former retention claims based on correction complexity, readability or scaling were deleted, as were obsolete retention claims in the converted selector/control files. No retained exception is asserted.
- Confirmation: Verification build passed (1431 warnings, zero errors), `--lookup-stream-3` passed all five pointers and native indirect loop advances ($A2:DF5E/$DF6A), all eleven native control words/five visual operand addresses ($D82C-$D84A), complete word/byte ownership, alias rejection and index boundaries.
- Changed production: `ChootPatternDefinitions.cs`, `ChootInstructionProgramDefinitions.cs`, `ChootFallingPathDefinitions.cs` (identity sharing and comment cleanup only; motion data unchanged). Tests extend the owned stream partial. No dependencies.
### Confirmed batch 5: Ripper-family animation and visuals

- Converted three visual/layout definitions. Six direction programs share four timed records (8,7,8,7 ticks), then Goto and the start address. Addresses and control calculate, but Words remains pending: neither the independent cadence8 nor its alternating one-tick reduction has an established disposition.
- Visual selection computes neutral/first/neutral/second wing phase and direction. Shared GRipper/Ripper II compositions occupy four/three/four OAM-part records per direction; ordinary Ripper has three two-part records per direction. Frame pointers derive from those composition sizes. Both shared enemy owners retain their original union of accepted operand addresses.
- Evidence: all 36 native mechanics and 24 visual operands in the six $A2:E19B/E1AF/E2E0/E2F4/E477/E48B loops, and their $E3C5/$E527 sprite composition sequences.
- Verification build passed (1431 warnings, zero errors). `--lookup-stream-3` passed direct native records/order/ownership/bounds plus the existing six production animation fixtures with source bytes forbidden, including all twelve distinct selected frames.
- Changed production: `RipperInstructionProgramDefinitions.cs`, `RipperVisualDefinitions.cs`; tests extend the owned partial. No retained exception or cross-stream dependency.
### Confirmed batch 6: Mochtroid and Yellow Pipe Bug loops

- Converted two visual-address definitions. Both families calculate their four-frame structure, Goto commands, branch targets and byte ownership. Their Words aggregates remain pending: Mochtroid holds14 in free flight and5 when attached; Yellow Pipe Bug holds4 in straight flight and1 when arcing. Naming these states does not derive their independent timing magnitudes.
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
### Confirmed batch 22: Grapple connection and dropped-pose policy

- Converted four arrays to semantic cases. Movement kinds determine cancellation; standing/crouching aim directions determine dropped poses; default/vertical/crouching connection modes select swing or locked directional handlers. Address classification preserves the original contiguous thirty-record domain, including accepted addresses reached from later table bases. Invalid movement/direction indices retain their exception types.
- The separate special-angle physical records remain required. This batch does not claim a physical geometry exception or conversion.
- The existing additionally assigned fixture initially lacked installed swing-frame, flare-placement and Samus body-art definitions required by migrated production paths. Initialized them using existing native extractors, retaining every connection, exact-angle, cancellation/refire and dropped-pose assertion. Each failed path was rerun after setup correction; production gained no fallback.
- Full build passed (1431 warnings, zero errors); final verifier rebuild passed (25 warnings, zero errors). `--lookup-stream-3` passed 100 native words and 48 policy bytes, 3072 address classifications and the existing actual connection/angle/cancel/drop checks with migrated source reads forbidden. Four entries completed, bringing the converted count to 57 plus the separate approved decorative retention. No external source-hash dependency found.
### Confirmed batch 23: Installed Grapple swing-angle frames

- Converted the stock installed 256-entry displayed-frame mapping into nearest-eight-angle rounding and modulo-32 wrap. Import discards stock-compatible mappings; independently edited frame selections remain supplied exactly. The physical body-offset arrays remain separate required work.
- Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed every native selector byte at $9B:C1C2, a reflection check proving stock storage is absent, and every independently changed angle against all 256 output angles. The real connection fixture also passed using the calculated installed mapping.
- One entry completed, bringing converted entries to 58 plus the separately justified decorative retention. No external source-hash dependency found.
### Confirmed batch 24: Rope segment attributes and operand addresses

- Converted the four attribute operand addresses to a calculated four-byte stride, preserving indexing, enumeration and existing caller length semantics. Converted installed stock attributes to consecutive OBJ tiles $21-$24 with palette five, priority three and no flips, matching $94:B18B-$B19A. These fields name the texture animation's contiguous cells and common draw policy; no generated attribute array is retained.
- Independently supplied endpoint appearance stays unchanged. A segment style override remains exact for every field; stock-compatible segment rows are discarded at import.
- Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed all four native attributes and addresses, enumeration/bounds, absent stock attribute storage and 24 independent field edits covering tile column/row, palette, priority and both flips. Existing real Grapple connection checks also passed.
- Two entries completed, bringing the converted count to 60 plus the separate decorative-anchor retention. No external source-hash dependency found.
### Confirmed batch 25: Crateria escape lightning control

- Converted the two-owner catalog into named yellow-lightning and CRE-pixel program cases, preserving definition order and index bounds. Their shared eleven-frame duration schedule now dispatches the three quiet intervals (49, 17, 24 ticks) and one-tick flash records directly; no stored duration list remains. Existing per-program addresses, waits, loop control and frame geometry already calculate directly.
- Removed stale comments that justified retaining color payloads because no simpler numeric rule was known or because they were independently addressed. Color payload conversion remains required; this control-only batch grants no color retention exception.
- Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed the existing two-program fixture: all 52 native mechanics words, excluded color operands, real 98-frame cycles and repeat behavior with mechanics source reads forbidden. Owner ordering and invalid owner/duration bounds also passed.
- Two entries completed, bringing converted entries to 62 plus the separately justified decorative retention. No external source-hash dependency found.
### Confirmed batch 26: Surface and unused-dark lightning control

- Converted the owner catalog and all nested factory-built arrays. Named surface/dark owners now calculate 13/14 frame records, 38/40 word mechanics and two timer bytes each. Frame widths follow color count; setup, two repeated seven-flash groups, one/two neutral intervals, four final flashes and loop commands determine all pointers and durations. Collection order and bounds remain unchanged; no generated record or operand arrays are cached.
- Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed the existing native fixture's 78 words/four bytes, excluded color operands, complete real 503/743-frame cycles, vertical-boundary neutral restarts and migrated-source guards, plus collection bounds.
- Removed the old dark-row retention claim based on irregular values and lack of a supported formula. Both independent color payloads remain pending. The actual installed storage is `Assets/RoomPaletteFxPresentation.cs`, owned outside stream 3; the coordinator will route its conversion and this stream has not edited it.
- Concrete pending surface-color formula recorded by the previous source investigation: for frame indices 0..12, phase sequence is `0,1,2,3,4,3,2,1,0,4,3,2,1`; colors are `0x2D6C + 0x18C6 * phase - 0x0421 * color` for phases zero through three and solid `0x7FFF` for phase four, with color indices zero through seven. The cross-owned installed payload still requires implementation and focused confirmation of this formula. Unused-dark frames use five color phases in order `A,B,C,D,E,D,C,B,A,A,E,D,C,B`, with E black; this phase correspondence is not a justification for retaining its nonzero colors.
- One original inventory entry completed, including all nested control rows, bringing converted entries to 63 plus the separate decorative-anchor retention. No external source-hash dependency found.
### Confirmed batch 27: Credits rows rendered from text

- Removed the stock 520-by-32 compiled tile-word lookup. Requested rows now derive directly from validated editable text, line spacing, column, font glyph calculation, top/bottom half and palette attributes. Initial/interline/section/trailing blanks and blank-glyph attribute behavior remain exact. Source-content hashing is unchanged.
- Loader validation still rejects invalid identities, text, columns, palettes and glyphs, but stores no generated stock rows. The existing explicit `FromCompiledRowsForVerification` constructor keeps copied synthetic rows only for its fixture callers. Ordered line-definition metadata remains a separate required inventory entry.
- Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed the existing fixture's 520 native rows (16,640 words), full real scrolling cadence through completion, editable text/column/palette behavior and strict validation. Additional checks confirmed source identity, row bounds and absent stock row storage.
- One entry completed, bringing converted entries to 64 plus the separate decorative-anchor retention. No external source-hash dependency found.
### Confirmed batch 28: File-select digits and slot letters

- Converted both installed glyph sequences to tile-base arithmetic: decimal digits at $2060 plus digit, and slot letters at $206A plus slot. These named bases already drive native extraction. Import discards matching stock sequences; any independent tile/attribute edit remains exact. Existing tilemap destinations and input domains are preserved.
- Full build passed (1431 warnings, zero errors); final verifier rebuild passed (25 warnings, zero errors). `--lookup-stream-3` passed all thirteen actual glyph writes, stock-storage absence and 78 independent edits covering column, row, palette, priority and both flips through production tilemap-writing methods.
- Two entries completed, bringing converted entries to 66 plus the separate decorative-anchor retention. Other file-select pages, patches and sprite compositions remain required. No external source-hash dependency found.
### Confirmed batch 29: Semantic credits caption roles

- Replaced the 67 stored line records with semantic caption-role cases and numbered contributor groups. Heading suffixes select the small font; contributor names select the large font. Initial, heading, adjoining sound-heading and name spacing derive from each role directly. No generated definition array remains.
- Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed all 67 original role identities and order, all 520 native rendered rows, actual scrolling cadence through completion, editable overrides and strict validation.
- One entry completed, bringing converted entries to 67 plus the separate decorative-anchor retention. Independent contributor text and glyph artwork remain separate payload obligations. No external source-hash dependency found.
### Confirmed batch 30: Door-transition palette preservation

- Removed all three method-local color-index arrays. Named catalog operations now copy the eight permanent HUD colors, five common-CRE colors and four timer colors directly. Each slot carries its native palette symbol and instruction address; the existing CRE-bit condition and its nested active-timer condition remain unchanged.
- Pinned $82:E1F1-$E264 supplies the exact copy operations. The focused fixture independently decodes all seventeen native STA destinations and confirms both copied colors and all untouched black slots for each operation. Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed.
- Three entries completed, bringing converted entries to 70 plus the separate decorative-anchor retention. This converts slot selection only; independent source color payloads remain required. No external source-hash dependency found.
### Confirmed batch 31: Mother Brain health tint before quantization (partial)

- Replaced stored four-state body and rear-leg palettes with a tint calculation. All ninety native RGB channel trajectories fit an RGB8 base in the initial RGB5 quantization interval, blended toward RGB8 `(248,0,0)`. Body blend amount is `state*(state+1)/30`; rear legs add `2/30` for nonzero states. Each output channel is floored after division by eight. Import retains a matching base and discards its intermediate rows; independently edited trajectories that do not fit stay exact.
- Pinned $AD:E6AC-$E723 and $AD:E74C-$E7C3 provide all 120 native words. Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed all native outputs, shared body/brain destinations, absent stock channel-trajectory storage and 360 independent channel edits against all four complete output palettes.
- Both independent base palettes remain required. Batch 34 removes the apparent independent latent precision and calculates their shade ramps. These two inventory entries stay unchecked; this is not a content-retention exception. Converted count remains 70 plus the separately justified decorative-anchor retention.
- Coordinator must refresh `RemainingEnemyColorClosedContractDefinitions.cs` source hash for `MotherBrainHealthPalettePresentation.cs` (previous B33D938BDFF9FAF121A6D5E232B8CD0A00443E5EF104E825843EB78B5142A55B).
### Confirmed batch 32: Mother Brain room-light recovery (partial)

- Replaced all seven stored room-light rows with equal RGB8 intensity steps before RGB5 quantization: `floor(endpoint * (frame + 1) / 56)`. Every one of the 84 native channel trajectories fits an endpoint in the final RGB5 channel's eight-value interval. Stock trajectories are discarded; independent edits remain exact. Older room-color documents still inherit the installed stock recovery object.
- Pinned $AD:F283-$F409 contains seven reverse-selected rows. Verification build passed (1431 warnings, zero errors); `--lookup-stream-3` passed all 196 native words at both CGRAM destinations, absent stock channel trajectories, 588 independent channel edits and legacy-document fallback for all seven steps.
- The independent full-light color basis remains required; batch 37 removes the apparent latent precision and shares matching final-room colors. This inventory entry stays unchecked. Converted count remains 70 plus the separate decorative-anchor retention.
- Coordinator must refresh `ClosedPresentationContractDefinitions.cs` source hash for `MotherBrainRoomColorPresentation.cs` (previous 67630C828581AB534E1F6EBF0E8145762623EEE80F26B534157EB72FEC780AE8).
### Confirmed batch 33: Mother Brain room flash and control

- Converted the sixteen mechanics words and fourteen presentation operands to four-byte frame strides, two-tick durations and the closing loop operation. Flash-strength event selection uses semantic strength cases; no control/address arrays remain. Byte ownership, ordering, index exceptions and mechanics rejection are unchanged.
- Replaced stock fourteen-by-24 flash color rows with a base palette and shared highlight. First thirteen colors interpolate toward that highlight in thirds; remaining eleven level colors darken by `(4-strength)/4`. Both calculations round RGB5 channels to nearest. All four distinct native palettes match exactly; independent edits retain their complete supplied flash content.
- Reproduced the assigned fixture's missing-installed-colors failure before changing it. Initialized existing extraction and replaced obsolete live-ROM expectations with an independent native pointer/timer/full-CGRAM oracle across 48 actual production ticks. All fourteen operands and loop wrap are covered; runtime mechanics and presentation reads remain zero, and existing rejection assertions remain.
- Full build passed (1432 warnings, zero errors); final verifier rebuild passed (25 warnings, zero errors). `--lookup-stream-3` passed the native production oracle, every bank-byte ownership decision, bounds, absent stock flash rows and 1008 independent RGB channel edits across all fourteen palettes and mirrored destinations.
- Two control entries completed, bringing converted entries to 72 plus the separately justified decorative-anchor retention. The independent flash base/highlight, final-room colors and other room-color payloads remain required; the installed flash entry stays unchecked.
- Coordinator must refresh both source hashes in `ClosedPresentationContractDefinitions.cs`: `MotherBrainRoomColorPresentation.cs` and `MotherBrainRoomPaletteProgramDefinitions.cs` (latter previous 68E027B6CA7DCA4C6357729EE8C73276E303D9E94AD934481442F59D4026E1E9).
### Confirmed batch 34: Health palette shade ramps and shared quantization bias (partial)

- Further source analysis establishes a single RGB8 base rule `(RGB5 * 8) + 1` for every health-palette channel. Removed the per-channel precision search and storage. The same triangular tint strengths and red target still reproduce all four native states exactly.
- Base body colors five through eight form a four-step gray ramp; colors nine through thirteen form a five-step brown ramp. Rear-leg colors five through eight form a four-step gray ramp, with its brightest color repeated at color fourteen. Each channel is `floor((endpoint * numerator + denominator/2) / denominator)` with descending numerators. Body color fourteen is white, body color fifteen and the remaining rear-leg slots are black. Stock base-row arrays are discarded; independent edits remain exact.
- Eight independent RGB5 paint choices remain required: body `$269F,$0159,$004C,$0004,$5739,$367F`; rear-leg `$0024,$29AD`. No retention exception is claimed. Static visible-component evidence is recorded below; endpoint retention still awaits independent review.
- Native consumer $AD:E3D5-$E42E identifies BG4 as body, OBJ1 as brain, OBJ3 as rear leg. Body source $AD:E6AC-$E6B2 fills CGRAM $41-$44 and $91-$94; gray endpoint $AD:E6B4 supplies calculated ramps at $45-$48 and $95-$98; brown endpoint $AD:E6BC supplies $49-$4D and $99-$9D. Rear-leg outline $AD:E752 fills $B4; gray endpoint $AD:E754 supplies $B5-$B8 and repeated $BE. These are palette-slot assignments, not a claim that each RGB choice is mathematically irreducible.
- Verification build passed (1432 warnings, zero errors); `--lookup-stream-3` passed all 120 native words, 360 independent channel edits and stock-storage assertions for both shade ramps and intermediate damage rows. Converted count remains 72 plus the separately justified decorative-anchor retention; both health entries remain unchecked.
- Coordinator must refresh the existing health-presentation source hash in `RemainingEnemyColorClosedContractDefinitions.cs`.
### Health endpoint artwork evidence and final narrow disposition

The isolated guarded exporter at `csharp/test-temp/stream3-palette-art` decodes only identified native artwork and produces per-index masks. It does not run gameplay or search for defects. Head source `$B7:8000` is composed with `$A9:A586`; neck uses `$A9:A694`; front/rear standing limb components come from root `$A9:9FA0` and `$B7:9000` leg tiles. The standing root's rear-foot component selects `$A9:A974`, OBJ palette three; its front components select OBJ palette one. Source palette routing agrees with `$AD:E3D5-$E42E`.

| Source word (rear values now calculated) | Source / palette index | Visible role confirmed by masks | Dependent calculation |
| --- | --- | --- | --- |
| `$269F` | `$AD:E6AC`, body/brain index 1 | Bright patches on exposed brain cortex | Damage tint only |
| `$0159` | `$AD:E6AE`, index 2 | Orange middle tones across cortex folds | Damage tint only |
| `$004C` | `$AD:E6B0`, index 3 | Deep red cortex folds/shadows | Damage tint only |
| `$0004` | `$AD:E6B2`, index 4 | Dark silhouette and internal outlines on head; outlines on limb components | Damage tint only |
| `$5739` | `$AD:E6B4`, index 5 | Light gray spikes, skull/teeth and front limb plates | Indices 5..8 use endpoint times 4/4, 3/4, 2/4, 1/4, nearest RGB5 rounding |
| `$367F` | `$AD:E6BC`, index 9 | Brown lower-face/mouth tissue; its darker shades also paint neck joint `$A9:A694` | Indices 9..13 use endpoint times 5/5 through 1/5, nearest RGB5 rounding |
| `$0024` | `$AD:E752`, rear index 4 | Rear-foot and joint outlines | Stronger rear-leg damage tint |
| `$29AD` | `$AD:E754`, rear index 5 | Dim rear-leg/foot plate highlight | Rear indices 5..8 use quarters; index 14 repeats the endpoint |

Artifacts in that tool's `output` directory include `head-a586.png`, its `-index-01` through `-index-15` masks, `neck-a694.png`, front-limb `standing-part-3-a9a7c2.png`, and rear-foot `standing-part-6-a9a974.png` with masks. The export completed successfully. Independent review accepted only the six body paint anchors under the nonsense exception: they supply chosen cortex tones, outlines, bone/plate highlights and tissue color, rather than a health or geometry function. Replacing those six choices would invent different paint content. Rear lighting, all shades, damage tint, neutrals and repeated highlights calculate; the rear source words are not independent retained endpoints. This does not grant a general artwork or palette exception.
### Confirmed batch 35: Final health palette lighting and narrow paint disposition

- Rear gray now derives from the front gray endpoint by halving R/G upward and B downward, followed by the existing quarter-shade ramp. Rear outline preserves front red/blue and adds one RGB5 green step (clamped at the channel bound). This names the specific artistic rear tint; it is not presented as a universal lighting law.
- Final stock representation holds exactly six nonzero body scalar paint anchors and no independent rear anchors. Both base-row and damage-row fallback arrays are null for stock. Rear fields are zero and its palette references the body basis; independent edits retain exact supplied content without coupling edits across palettes.
- Independent coordinator review of pinned source, selected native artwork, index masks and final production representation approved the nonsense exception only for the six body anchors identified in the evidence table. Body shade ramps, all damage states, neutral colors, repeated highlights and both rear-lighting rules remain calculated. No performance, size, complexity, provenance or generic authorship rationale is used.
- Production build passed (1192 warnings, zero errors); final verifier rebuild passed (25 warnings, zero errors). `--lookup-stream-3` passed all 120 native words, 360 independent channel edits, absent stock row storage and explicit six-body/zero-rear anchor assertions.
- Final per-entry disposition: `body` is mixed calculation plus justified retention of six named paint anchors; `backLegs` is converted. Converted entries now total 73, with two separately justified retained entries (these six paint anchors and the earlier decorative death-scatter anchors).
- Coordinator must refresh the health-presentation source hash in `RemainingEnemyColorClosedContractDefinitions.cs`.
### Confirmed batch 36: Mother Brain visual roots and special-sheet selection

- Replaced the eighteen stored OAM roots with record-stride calculations across named head, mouth, damaged-head, high-tile-head and tube runs. Native records occupy two header bytes plus five bytes per character; the neck and component-count boundaries remain explicit. Frame enumeration calculates each published bank/pointer/name without caching a replacement array.
- Replaced the four stored special-sheet registrations with semantic cases selecting legs, Baby, attack restoration or exploded door. Named definition records remain the source identities; collection order, all fields, source-range lookup and original index exception remain unchanged. Independent pixel payloads are not exempted by this metadata conversion.
- Verification build passed (1432 warnings, zero errors); `--lookup-stream-3` passed all eighteen original roots/names/banks, eighteen native character counts, enumeration and bounds, plus all four complete sheet records, source boundaries and collection exceptions. Existing sheet-transfer checks also passed.
- Two entries completed, bringing converted entries to 75 plus the two narrowly justified retained entries.
- Coordinator must refresh `MotherBrainSheetsClosedContractDefinitions.cs` source hash for `MotherBrainSpecialSpriteArtworkDefinitions.cs`. No external source-hash dependency found for `MotherBrainVisualDefinitions.cs`.
### Confirmed batch 37: Shared room-color basis and direct recovery scaling (partial)

- Replaced recovery's per-channel RGB8 precision representation with exact direct RGB5 scaling: `floor(endpoint * (frame + 1) / 7)` for each channel. All 196 native words match without latent precision.
- Recovery reuses 23 colors from the existing final-room basis. Its first slice prepends three background colors to final-room colors 0..10; its second prepends two copies of one neutral highlight to final-room colors 12..23. Only four additional scalar paint inputs remain. The flash calculation also reuses the final-room basis, keeping only its separate highlight input.
- Import accepts these relationships only when every supplied output agrees. Independently editing final-room content leaves flash/recovery content exact, and legacy documents still inherit the stock recovery object. No generated frame arrays remain for stock.
- Verification build passed (1432 warnings, zero errors); `--lookup-stream-3` passed all native recovery words and actual flash loop outputs, 588 independent recovery edits, 1008 independent flash edits, 72 independent final-room channel edits against every recovery/flash output, shared-basis storage assertions and legacy fallback.
- Shared final-room paint content, the flash highlight and four recovery-only inputs remain required. No new completed entry or retention exception; totals remain 75 converted and two narrowly justified retained entries. Coordinator must refresh the existing room-color presentation source hash in `ClosedPresentationContractDefinitions.cs`.
### Confirmed batch 38: Intro scene palette regions

- Replaced four stored region arrays with scene cases selecting named CGRAM regions. Offsets calculate from background/object row and color geometry. Gameplay clear intentionally selects sixteen initial colors while gameplay cross-fades select twenty; native Y operands count colors, including the three- and nine-color regions, despite the existing ByteCount property name.
- Adapted only the three granted runtime helper signatures and the existing fixture parameter type. Fade ordering, calls, timing and independently installed palette content remain unchanged.
- Verification build passed (final rebuild 1217 warnings, zero errors); `--lookup-stream-3` passed all twelve original records in order, each native LDX/LDY operand pair at $8B:B258-B291, B3C8-B3DD and B2F5-B301, and index bounds. No stored replacement arrays or retention exception.
- Four entries completed, bringing the converted count to 79 plus two narrowly justified retained entries. No external source-hash dependency found for the modified intro files.
### Confirmed batch 39: Death-fade starting colors from health state three

- Pinned $AD:EA0A body and $AD:EA26 rear-leg starting rows are exactly the first fourteen colors of health state three; $AD:F119 corpse starts with all fifteen brain colors. Reused the approved six-paint-anchor health generator, its shade/rear-lighting rules and state-three red tint. No independently stored starting rows remain in stock death fades.
- The six approved paint identities now have named native-address XML catalog constants inside the health presentation. Stock health imports share immutable body/rear bases; death import recognizes its own supplied endpoints against that generator. Editing one installed document cannot alter another document's independently supplied content. Unmatched first colors or intermediate frames still retain exact fallback values.
- Verification build passed (1432 warnings, zero errors); `--lookup-stream-3` passed all 568 native death words, all 1704 independent death-channel edits, unchanged content identities, absent starting/frame arrays and the existing 120 native health words plus 360 independent health edits.
- Body and leg fades are converted: every color now derives from the already narrowly approved health paints. Corpse's final fifteen-color endpoint and the fourteen exploded-door colors remain required; this conversion grants no new paint exception. Converted count is 81 plus the same two narrowly justified retained entries.
- Coordinator must refresh health's `RemainingEnemyColorClosedContractDefinitions.cs` source hash and death's `SequenceColorClosedContractDefinitions.cs` source hash.
### Confirmed batch 40: Normal restoration reuses health paint and lighting

- Source $A9:9474/9494 repeats the fifteen body and fifteen rear colors at health state zero. Recognized these exact rows through the shared six-anchor base generator. Normal restoration now calculates both rows without independently storing another palette payload; nonmatching installed body/rear edits retain exact fallback colors.
- Verification build passed (final rebuild 1217 warnings, zero errors); `--lookup-stream-3` passed all thirty original restoration colors and ninety independent normal-channel edits, with explicit absent-row assertions. The existing 900 rainbow channel-edit checks and other drain/revival/health/death checks also passed after the shared frame representation change.
- The source inventory has no separate normal field entry. This removes an additional nested payload without marking the still-pending rainbow table complete; counts remain 81 converted plus two narrowly justified retained entries. Drain/revival/corpse grey endpoints remain required.
- Coordinator must refresh the existing health and rainbow-presentation source hashes in `RemainingEnemyColorClosedContractDefinitions.cs` and `SequenceColorClosedContractDefinitions.cs`.
### Confirmed batch 41: Beam fixed-color hue wheel

- Replaced the 38 stored sampled colors with five semantic hue ramps at full-word position `n = 2 * sample`: red-to-yellow, yellow-to-green, green-to-blue, blue-to-magenta and magenta-to-red. Rising channel `R(t)=(31*t+7)/15`; falling channel `F(t)=31-2*t-(t>=9 ? 1 : 0)`. The source uses R for rising green/blue and decreasing red, F for decreasing green/blue.
- The native blue-to-magenta segment has red `2*t-(t>=7 ? 1 : 0)`, green `t>=8 ? 1 : 0`, blue 31, followed by full magenta at position 60. These preserve the concrete asymmetric source phase boundaries; no claim about the original palette-authoring tool is needed. Five meaningful hue cases replace all rows without a correction table, packed payload or retained exception.
- Native evidence: $88:E833-E8C7 and the four byte-cursor increments at $88:E7FD-E806. The signed terminator stays engine control, and independently edited cycles retain their exact supplied colors.
- Verification build passed (1432 warnings, zero errors); `--lookup-stream-3` passed all 38 native sampled colors, 114 independent channel edits, unchanged initial color, terminator, alignment/bounds and absent stock cycle storage.
- One entry completed: 82 converted plus two narrowly justified retained entries. Coordinator must refresh the rainbow presentation source hash in `SequenceColorClosedContractDefinitions.cs`.
### Confirmed batch 42: Beam dispatch and falling-tube visual selection

- Replaced $AD:DE5F-DE7D with cases for the two edge quadrants: down, right, up, retaining-left and unsupported null targets. Preserved the original index bounds. The separately granted HDMA caller changes only its dispatcher call; scanline geometry and retained/null handling remain unchanged.
- Falling-tube selectors now use the shared final five visual-catalog records, whose roots calculate from native OAM record widths. Removed the duplicate five-pointer array; instruction duration, sleep cadence, visual/mechanics separation and address validation remain unchanged.
- Verification build passed (1432 warnings, zero errors); `--lookup-stream-3` passed all sixteen native dispatcher targets and bounds, plus the existing actual five falling-tube frame/sleep executions with compiled source reads forbidden and all five native visual operands.
- Two entries completed: 84 converted plus two narrowly justified retained entries. Coordinator must refresh `MotherBrainBeamRomData.cs` in `SequenceColorClosedContractDefinitions.cs`; no external source hash found for the other three production files.
### Confirmed batch 45: Dedicated health paint catalog

- Moved the six already approved paint constants from the nested presentation implementation into `Assets/MotherBrainHealthPaintDefinitions.cs`, with their exact values and native-address/art-role XML summaries unchanged. Existing health, normal-restoration and death calculations continue using the same six anchors.
- Organization-only correction requested during integration review; no new retained content or changed disposition. Verification project build passed (1432 warnings, zero errors). Counts remain 84 converted plus two narrowly justified retained entries.
- Coordinator must use the updated health-presentation hash and include the new dedicated paint catalog in its reviewed source closure.
### Confirmed batch 44: Pure door-scroll callback programs

- Replaced the dictionary and all seventy-four stored write arrays with named native callback cases executing Red/Blue/Green stores in original order. Recognition and execution share the same dispatcher; pointer enumeration retains its original order without a registration array. Unknown callbacks remain no-ops; null target arguments remain rejected by TryApply.
- Removed the invalid original retention rationale. The native source consists of straight-line programs, so these are translated executable cases rather than an exempted data mapping. No retention exception is asserted.
- Reused the existing DebugRunner audit's bounded straight-line instruction interpreter inside the owned stream verifier, scoped only to these seventy-four changed callbacks. It compares all fifty scroll bytes, including untouched cells and native word-store effects. Full ushort recognition and original pointer enumeration order also pass.
- Build passed (final rebuild 1217 warnings, zero errors); `--lookup-stream-3` passed. One entry completed: 85 converted plus two narrowly justified retained entries. No external source-hash dependency found for DoorScrollPrograms.
- Included coordinator-requested formatting-only corrections: split nested closing braces in MotherBrainSpecialSpriteArtworkDefinitions and restore MotherBrainVisualDefinitions' final newline. The former's existing MotherBrainSheets source hash must use the formatted source.
### Confirmed batch 43: Shared drained/corpse shade endpoint (partial)

- Source $AD:F0BF (drain final), F1EB (corpse final) and EEB8 (revival start, first thirteen colors) share the same gray body paint. The four cortex/outline colors calculate by nearest RGB5 interpolation from $4F38 to $0CA5 over three intervals. Indices 4..7 reuse the existing health plate shades; tissue index 8 repeats $4F38; indices 13/14 are neutral white/black.
- Tissue indices 8..12 calculate by RGB8 interpolation over four intervals then divide by eight. Import solves the bounded endpoint quantization intervals and accepts the rule only when every output agrees. The stock interval selection is light RGB8 (193,200,155), dark (79,83,55), quantizing to $4F38/$1949; outline is $0CA5. These are three remaining supplied paint inputs, not a newly approved retention exception or a claim about historical authoring tools.
- The shared endpoint class replaces stock stored final-body rows in drain/corpse fades; independent edits fall back exactly. Revival still stores its separately inferred channel endpoints and remains required. No cross-document edit coupling was introduced.
- Guarded static artwork export now also includes `drained-head-a586.png`, `drained-neck-a694.png`, `drained-front-limb-a7c2.png` and index masks in `csharp/test-temp/stream3-palette-art/output`. It decodes the same identified native art with $AD:F1EB: $4F38 supplies cortex highlights and lower-face tissue highlights; $0CA5 supplies silhouette/internal outlines; $1949 supplies the darkest lower-face/neck tissue pixels (index 13). Plate shades remain the original health grays. Export completed successfully; inputs remain pending independent review.
- Production/verifier build passed (1432 warnings then verifier-only 25 warnings, zero errors); `--lookup-stream-3` passed all native drain/corpse outputs, existing independent channel edits and explicit absent final-body-row assertions. No new completed entry; counts remain 84 converted plus two narrowly justified retained entries.
- Coordinator must refresh both rainbow and death presentation hashes in `SequenceColorClosedContractDefinitions.cs`.
### Confirmed batch 51: Approved drained paint catalog and corpse-fade closure

- Independent native/art review approved only three olive-grey paint anchors: $4F38 shared cortex/lower-face highlight, $0CA5 silhouette/internal outline, and $1949 darkest mouth/neck tissue. The native head/neck index masks, $AD:EEB8/F0BF/F1EB rows and EF1F-44/EF5C-81/F0FA-100 copy routing establish those roles. A generic grayscale/lighting transform would invent different paint content; this narrow nonsense exception does not exempt other palettes.
- Added dedicated `MotherBrainDrainedPaintDefinitions` with exact native source/role XML. Its RGB8 representations (193,200,155) and (79,83,55) express the exact quantized tissue gradient; they do not claim a historical palette tool or create a general precision exception. Stock recognition uses these fixed approved anchors; independent installed edits continue through their own exact interval solution or supplied fallback.
- Cortex shades, tissue shades, repeated highlight, original health plate grays and neutral white/black all calculate. The corpse fade now derives its starting row from health state three and its final row from this separately retained three-paint catalog, then interpolates every frame. No corpse endpoint or frame array remains.
- Verification build passed (1432 warnings, zero errors); `--lookup-stream-3` passed the existing native drain/corpse outputs, 1704 independent death-channel edits, independent rainbow/drain edits, identities and absent stock starting/final/frame rows.
- CorpseFade is converted with an explicit dependency on the approved health and drained paint catalogs. Other drain/revival rear/trailing/endpoint obligations remain unchecked. Original-inventory counts are 94 converted plus two narrowly justified retained entries; the new three-anchor catalog is a separately documented retained dependency, not an additional original inventory item.
- Coordinator must refresh the rainbow and death source hashes in `SequenceColorClosedContractDefinitions.cs` and include the new drained paint catalog in the reviewed source closure.
### Confirmed batch 52: Fake-death cortex endpoint closure

- Native $AD:ED8A selects the first three cortex colors of the full transition rows; the initial $AD:EDAE colors are the approved normal-health cortex paints and the final $AD:EEB8 colors are the approved drained cortex ramp. Extended body recognition to these exact prefixes, removing both independent three-color endpoint rows.
- The existing nearest-RGB5 fade calculates all twenty-four outputs. Independent installed prefix edits remain exact supplied content; no new paint exception or coupling to another document was introduced.
- Verification build passed (1432 warnings, zero errors); `--lookup-stream-3` passed all native fake-death words, existing independent channel edits and legacy fallback, with explicit absence of both stored endpoint rows. Full drain/revival checks remain unchanged and pass.
- FakeDeathToGrey is converted using the already approved normal/drained paint dependencies. Counts are 95 converted plus two retained original entries; the separate approved three-anchor drained catalog remains documented in batch 51. Other drain/revival obligations remain unchecked. Coordinator must refresh the rainbow presentation source hash in `SequenceColorClosedContractDefinitions.cs`.
### Batch 62 — controller button glyph composition

- `$82:F659-F6AC` contains seven 3x2 button labels. A/Y reflect the left half horizontally; X reflects the opposite diagonal half in both axes; B uses consecutive columns; Select uses three consecutive columns; L/R share geometrically reflected shoulder-outline corners and use their named central letter. Upper/lower glyph halves follow the 16-character atlas stride. Narrow labels pad their third column with the named blank tile.
- Named atlas identities and their native symbols/roles live in `GameOptionsPresentationDefinitions`. `ControllerLabelWord` calculates all tile indices and PPU flip flags through those semantic glyph cases. Stock retains no controller-label rows; the installed dictionary contains only independently edited labels. Full edited tile, palette, priority and reflection fields remain supported.
- Builds passed (1434 then 25 warnings, zero errors). `--lookup-stream-3` compares all 42 calculated words with the native records, confirms empty stock label storage, all 49 action/button placement combinations, and edited labels with changed reflection/palette through the production drawing path. Existing bounds and geometry confirmations remain intact.
- The controllerLabels entry is converted; pages and sprite artwork remain independently required. Integrated stream accounting is now 99 converted plus two narrowly justified retained entries. Coordinator must refresh the GameOptionsPresentation source hash in `MenuClosedPresentationContractDefinitions`.
### Batch 63 — game-over page from text and glyph identities

- Native `$81:92DC-93E7` fills a blank 32x32 page with five text elements. The existing named `GameOverRomData.Text` cases own their origins; the page now calculates each cell from the title, objective, prompt, YES/N O answers, and small-font parenthesized explanations. Small uppercase glyphs follow the contiguous atlas alphabet; punctuation and large glyph halves use documented semantic identities. Large O intentionally uses the native round zero shape, and V/Y use their native distinct lower halves.
- Stock `GameOverPresentation` stores no tilemap. It creates only the required transient VRAM transfer output. Any independently edited cell preserves the full supplied map, including tile index, palette, priority and reflection. No font pixels, sprites, or Baby palette inputs receive a new exemption.
- Verification build passed (1434 warnings, zero errors); `--lookup-stream-3` confirms all 1024 native cells through the actual VRAM transfer, no stock map storage, modified cells in all five text elements plus a formerly blank cell, exact destination placement, and invalid cell bounds. The extraction oracle still reads the pinned native text streams independently.
- The tilemap entry is converted; other game-over art remains independently required. Original accounting is now 100 converted plus two narrowly justified retained entries. Coordinator must refresh the GameOverPresentation source hash in `MenuClosedPresentationContractDefinitions`.
### Remaining scope

All unchecked entries remain required. The three Choot motion payloads still require conversion or concrete impossible/nonsense evidence; their rejected retention rationale has been removed. Mother Brain fade endpoints remain required after the calculation conversion above. Choot quadratic/cubic phase fits do not establish a complete generator or a retention exception; its motion payloads remain required.

### Batch 56 — calculate explored-map SRAM packing from immutable topology

- Source inspection of pinned bank B5 literal tilemaps established the generator for all six bank $81 lists: partition each area's 2,048 native tilemap words into 256 groups of eight, retain a group iff any character index (`word & $03FF`) differs from blank `$01F`, and enumerate ascending native page order. Counts are Crateria 74, Brinstar 72, Norfair 76, Wrecked Ship 18, Maridia 66, Tourian 21. All 327 indexes agree without extra or missing groups. Destination offsets are cumulative counts; native SaveMap/LoadMap still exclude Ceres.
- Removed all six arrays. `OccupiedByteIndexes` calculates count, indexed access and enumeration from installed `IsDiscoverable` stock rules. There is no replacement cached index table. Native pointer identities remain named catalog constants only for source identity/confirmation.
- `SuperMetroidSaveRam` now requires an explicit map-catalog argument, permits pre-content construction with null, and fails before packing/unpacking when unbound. Its catalog field is nonserialized. Frontend/file-menu presentation rebinding supplies current immutable rules after debugger restoration. Automatic checkpoints use the runtime's bound catalog; JSON/file persistence requires the catalog explicitly. Android and desktop load installed maps before first persistence/migration, and Android import validates against installed maps without a cartridge.
- Granted fixture/tool callers now supply existing installed maps or explicit import-only fixture catalogs. The new shared `SaveMapPresentationFixture` uses existing extraction at a diagnostic boundary and bounded temporary-directory cleanup; it introduces neither a production fallback nor a global catalog. Link-only project additions support those diagnostic consumers. Existing assertions remain intact; no unrelated audit suite was run.
- Focused `--explored-map-packing-definitions` passed all 327 native indexes/counts/offsets, exact save/load behavior with native codec reads forbidden, invalid indexes, and six independently blanked artwork overrides producing byte-identical SRAM. It also confirms unbound saves do not mutate SRAM, unbound valid-slot reads fail, debugger graphs omit the catalog, and actual frontend `BindMapPresentation` restores save decoding after graph restoration. Existing JSON save checks remain available with the new explicit dependency.
- All affected builds passed: Verification (25 warnings), DebugRunner (1451), RenderVerification including Desktop (53), DesktopVerification (20), IntegrationVerification (zero); all zero errors. Six original entries complete: **93 integrated conversions plus two justified-retention original entries**. Immutable source artwork/topology remains an explicit content dependency; this completion removes its derived packing indexes, not an exemption for unrelated map content.

Coordinator integration: the focused native packing and binding proof passed on main. All five affected projects built with zero errors. Added --save-json-persistence to run the existing JSON persistence proof independently; lossless preservation, atomic replacement and legacy migration passed. Six packing definitions complete; the master inventory is435 converted,4 original justified retained,689 pending.

### Batch 57 — options presentation executes shared geometry rules

- Removed duplicate stock storage for the seven controller-label anchors, four language highlight regions, two toggle-box layouts, three heading anchors, and 17 cursor anchors. Loading recognizes exact stock geometry and retains no arrays/dictionaries for those fields; runtime uses the existing calculated native operations in `GameOptionsRomData`, exposed through domain-named presentation helpers. Independently edited geometry still uses the supplied exact coordinates and cells. Input document hashing/serialization and validation are unchanged.
- Native geometry: controller destinations `$82:F639` are three tilemap rows apart; primary/controller/special cursor lists `$82:F307/F31B/F33F` use their calculated row spacing and controller scroll adjustment; language calls `$82:EDF2..EE51` cover two consecutive text rows per choice; special-setting boxes `$82:F149..F158` are two rows of six words; heading setups `$82:F34B/F353/F35B/F369` select the three named page anchors. Existing source-reviewed formulas remain the single definition of stock geometry.
- Verification build passed (25 warnings, zero errors). `--lookup-stream-3` passed the new focused checks: all seven native label destinations and 17 native cursor pairs; 49 action/button label placements using native label words; both language polarities; both states of each toggle; three exact heading OAM compositions with scrolling; invalid action/page rejection; five fields absent from stock storage. Independent edits to every geometry category preserve their exact rendered placements and selected cells.
- Five original entries complete. Pages, controller-label artwork, and sprite payloads remain separately unchecked. The earlier shared-projectile duration boundaries are under an additional coordinator review and are not justified by this geometry batch.
## Integrated Shitroid instruction layout

PresentationWords is complete: four-byte pose records and named callback/branch boundaries calculate all 30 visual addresses and 35 control positions. The twelve post-cry remorse durations use 2 + abs(frame - 4), matching the native contraction/expansion pulse at A9:F95E-F98A. Words remains unchecked: finish-drain hold 128, latched holds 8/8/5/2, normal cadence 16 and pre-branch remorse cadence 10 remain independent required inputs. Root review explicitly includes the latter two holds, which the worker's residual list omitted. No retention exception is granted.

Root Verification build passed with 1445 warnings and zero errors. --shitroid-instruction-mechanics confirms all 35 native words, 30 exact compiled selectors, actual initializer, finish-drain fallthrough, normal/latched loops, both RNG branches and native cry, source-read guards, bounds and allocation checks. The fixture binds the required installed color catalog and expects zero live sprite-selector reads. Overall checkpoint: 506 converted, 15 justified retained/mixed, 607 pending.

## Integrated Spark instruction layout

PresentationWords is complete: timed-record strides, tangible/intangible callbacks, loop back-edges and the terminal sleep calculate 33 control positions and 26 visual addresses from native A8:E5A7-E61B. Words remains unchecked. All ten activation holds 1/2/1/2/1/2/1/1/2/2, loop cadence 3 and deactivation cadence 1 remain required independent choices. Root review explicitly preserves the latter two obligations as well; no retention exception is granted.

Root Verification build passed with 1445 warnings and zero errors. --spark-instruction-mechanics confirms all 33 native words, 26 exact compiled selectors, four actual programs, tangibility callbacks and terminal sleep, initializer selections, source-read guards, boundaries and allocations. Overall checkpoint: 507 converted, 15 justified retained/mixed, 606 pending.

## Integrated Multiviola and Rio layouts

Both PresentationWords entries are complete. Multiviola's fourteen four-byte poses and back-edge calculate 16 control positions and 14 visual addresses. Rio's idle, swoop and recovery block widths calculate 32 controls and 24 visual addresses, including fallthrough and animation-finished/sleep tails. Pinned A2:B2DC-B316 and BB4B-BBB9 establish those structures.

Both Words entries remain unchecked: Multiviola's ten-tick cadence and Rio's four-tick idle and three-tick swoop/recovery cadences remain required independent inputs. This integration resolves two entries, not the four claimed in the worker batch; no retention exception is granted.

Root Verification build passed with 1445 warnings and zero errors. --multiviola-instruction-mechanics confirms all 16 native words and the complete production loop. --rio-instruction-mechanics confirms all 32 native words, actual initializer, idle fallthrough/return, both swoop phases and cooldown completion. Both preserve compiled-selector execution, read guards, boundary and allocation checks. Overall checkpoint: 509 converted, 15 justified retained/mixed, 604 pending.

### Root integration: phase-two rear-leg colors

`phaseTwoRearLeg` now reuses the calculated health-state-zero rear-leg palette: native A9:9494 repeats AD:E74C. The shared health palette already derives its shades from separately reviewed paint anchors; this conversion adds no retained payload or exception. Stock rows are discarded, while independently supplied edits retain their own colors.

Root confirmation: Verification build passed with 1445 warnings and zero errors; ResourceAudit build passed with zero warnings/errors. `--lookup-phase-two-rear-leg` confirms all 15 native colors and CGRAM destinations plus 45 independent RGB edits, preserving every unedited rear-leg color and the attack palette. One inventory definition completed; 527 converted, 15 justified retained/mixed, 586 pending. Other room colors remain required.

### Root integration: auxiliary palette gradients and glow

The four named palette registrations now enumerate explicit domain definitions. Golden Torizo body/belly colors interpolate RGB5 endpoints over seven intervals, holding transparent slot zero until the last band. Face Block mirrors a three-step glow with its separate accent start. Sidehopper drain interpolates actual RGB5 endpoints over five intervals with separate corpse colors; the superseded RGB8 endpoint search was not integrated.

Only `EnemyAuxiliaryColorDefinitions.Definitions` is complete. `EnemyAuxiliaryColorCatalog.frames` remains required: 64 Torizo endpoint words, nine Face Block input colors, and 30 Sidehopper endpoint plus 15 corpse colors still need dispositions. No retention exception is granted.

Root Verification build passed (1445 warnings, zero errors); ResourceAudit build passed (zero warnings/errors). `--lookup-auxiliary-palettes` confirms all 393 native colors, 1179 independent RGB edits, unchanged selected-content hashes, stock calculated-row ownership, and invalid-index boundaries. Inventory: 528 converted, 15 justified retained/mixed, 585 pending.

### Root integration: Work Robot semantic instruction phases

The 594-word dense map is replaced with calculated unpowered, walking, retreat, advance, laser, recoil and ledge phases. Repeated gait exposures, mirrored directions, callback boundaries and split shooting opportunities derive instruction positions directly without a replacement stored program.

`WorkRobotInstructionProgramDefinitions.Words` remains unchecked. Independent inputs still required: initial32, walking10, laser5/2, upward step4, recoil16/96, ledge128, right-retreat contact10, unpowered32767, one-tick entry scheduling and one-tick shooting-callback scheduling. The repeated/twice-speed structure is not an exemption for its magnitude.

Root build passed (1445 warnings, zero errors). `--work-robot-instruction-program-definitions` passes367 native mechanics words,227 installed selectors,22 production entry paths/callback execution, read guards, rejection boundaries and allocation checks. This fixture advances timers; it does not establish elapsed-time behavior. No definition completed or new retention approved by this partial conversion.

### Root integration: synchronized Shaktool instruction structure

Saw loops and eight head-facing record strides now calculate all15 presentation addresses. Semantic attack/bob phases calculate complementary waits for the head and arms without stored word arrays. PresentationWords is complete; Words remains required for independent timing inputs576,128,64,20,4,10,3,119,1908, terminal1 and the one-tick facing increment. No retention exception is granted. Work Robot's already-pending unpowered32767/entry1/callback1 now have explicit source names as well.

Root build passed (1445 warnings, zero errors). Shaktool focused confirmation passes110 native words,15 installed selectors,21 production programs, reset/movement/read guards/bounds/allocation; Work Robot confirmation again passes367 native words,227 selectors and22 production entry paths. Fixtures advance timers, so these checks establish instruction/control behavior rather than elapsed-time simulation. With the separate Baby Crawl reopening, counts stay531 converted,15 justified retained/mixed,582 pending.

### Batch 80 — attract command execution and exact demonstration choreography

- Replaced the factory-built823-entry dictionary with executable Input/Delete/Goto dispatch. All798 Input successors calculate as current identity plus the six-byte native record width;24 deletion sites share the terminating operation, and the one explicit jump returns from `$91:9342` to the named `$91:933C` wait. The four granted partial catalog files now contain domain-specific executable command cases, without any reconstructed command lookup or cached generated rows.
- Independent source review approved a narrow nonsense exception for exactly798 `(duration, held, newlyPressed)` triples. These are the chosen demonstration performance. `$91:83F2-8423` runs the pre-instruction, counts down the specified duration, copies the two controller operands verbatim, and advances six bytes; `$91:83DF-83EC` publishes both words directly. For example, the landing-site script begins289 idle ticks, then later `$91:8C24` holds Left for4 ticks and `$91:8C2A` changes to Right for20 ticks while its new-input word stays zero. Replacing that sequence with a generated gameplay policy, or deriving every edge from held-word differences, would change the demonstration content. This is no exemption for command successors, control operations, object selection, or scene setup.
- Native `$91:8A9B` cancels at game state `$2C`; `$91:8AB0` returns while movement type equals `$1A`, and redirects to `$9346` when it differs. The `$933C` no-input wait loops until that redirect. The continuation is included because the native pre-instruction names it explicitly, irrespective of whether ordinary scene timing displays it. Named script/object headers at `$91:9E52` onward establish the producer/consumer pairing; retained triples live only in the dedicated `StockAttractInputPrograms` catalog partials.
- Reproduced the stale verification reference failure: `DemoInputState.LoadObject` now requires explicit installed-definition delegates, but the old cartridge-reference wrapper supplied none. The granted fixture now supplies native read delegates only to its reference state, preserving every existing timer, held/edge, input-history, cancellation and both `$1A` branch comparison. An unrelated old runtime construction also failed its missing initial-palette prerequisite; coordinator approved removing full room/gameplay initialization from this command-only confirmation rather than adding unrelated presentation setup. Actual23 `AttractDemoInput(scene)`/`StepStock` paths remain exercised. Those production APIs accept no address space, which establishes cartridge-capability exclusion; the unused read-guard counter was removed rather than cited as a zero-read proof.
- Verification explicitly compares every native object header,798 input triples/calculated successors,24 delete opcodes and one jump/destination. `--stock-attract-scenes` passes all23 scene records, four sentinels, all823 command comparisons and the existing isolated script state schedules; no gameplay frames run. Final build passed (25 warnings, zero errors; preceding production build1434 warnings, zero errors). A misplaced fixture count assertion was corrected after a compiler failure before the successful final confirmation.
- `StockAttractInputPrograms.Commands` is mixed converted/justified retention, not wholly calculated. Local accounting becomes117 converted plus three narrowly justified retained/mixed original entries, subject to coordinator reconciliation. `StockAttractDemoScenes.Sets` and independent object-selection/setup obligations remain required. New grants in this batch: the four `Input/StockAttractInputPrograms.Part*Catalog.cs` files and scoped `Verification/Program.AttractDemoScene.cs`.

### Batch 78 — Shaktool linked-circle program structure (partial)

- Removed eighteen mechanics records and eight visual operand addresses. The front circle executes two growth poses immediately; middle/back wait before installing their linked movement pre-instruction, middle adds one growth pose, and each program then loops its final pose. All addresses, callback installation words and loop targets calculate from these three native shapes; no replacement cached table is retained.
- Four exact inputs remain required: growth cadence4 at `$86:BD68/6C/80`, middle launch6 at`BD78`, back launch10 at`BD8C`, and final-pose hold119 at`BD70/84/94`. No launch-spacing/geometry formula or held-pose exception is claimed. `Words` stays unchecked; `PresentationWords` completes, yielding117 converted plus two narrowly justified retained original entries pending coordinator review.
- Reproduced the existing fixture's stale expectation of eight live cartridge spritemap reads (actual zero) before its granted narrow correction. Native instruction decoding now determines each actual producer's expected operand after callback installation or goto; the fixture compares installed operand identity and duration word, confirms all eight definitions were selected, and requires zero live presentation reads. Existing linked-slot, movement callback, same-tick held-pose, rejection and allocation assertions remain intact. Forced timers confirm words/control order, not elapsed animation time.
- Verification build passed (1434 warnings, zero errors); `--shaktool-projectile-instruction-mechanics` passed eighteen native words, all three real linked-circle producers and eight native installed operands. Coordinator should record the granted `Program.ShaktoolProjectileInstructionProgramDefinitions.cs` fixture path and refresh any current program source hash.

### Batch 84 — calculated file-select text patches

- Replaced the stock patch dictionary/cell rows with named Energy, TimeColon and NoData operations. Energy uses three consecutive label tiles plus its final glyph; the colon is a named glyph; ` NO DATA   ` calculates alphabet tiles and spaces, preserving the first unpaletted blank. Each cell advances one column on the same row. Native sources are `$81:B496-B49E`, `$81:B4A8-B4AA` and `$81:B4AC-B4C2`, including terminators.
- The loader discards stock compiled cells after comparison. Independently supplied changes preserve their exact cell count, ordering, positions and tile attributes through the existing content path. The document/serialization representation is not a second runtime mapping or an additional inventory entry.
- Verification build passed (1434 warnings, zero errors), and `--lookup-stream-3` passed. Focused additions directly compare all16 native words and coordinates plus three native terminators, require no stock fallback cells, and confirm all96 independent tile-attribute edits, all16 individual coordinate edits, and each patch's reversed supplied order through actual `ApplyPatch` output. No gameplay search was added.
- `FileSelectPresentation.patches` is converted. Page and sprite artwork obligations remain unchecked. Local accounting is118 converted plus three narrowly justified retained/mixed entries, subject to coordinator reconciliation. Coordinator should refresh the FileSelect presentation dependency hash.

### Batch 85 — Hopper semantic instruction programs (partial timing payload)

- Replaced four variant records,96 mechanics words and40 operand-address rows with species/size dispatch and shared airborne/landed program operations. Native `$A3:AA76-AAC0`, `$A3:AFA5-AFDF`, `$A3:B0C5-B10F` and `$A3:B237-B271` share orientation copies; only Sidehoppers include the sound command/operand prefix. Program extents and visual-operand positions follow those prefix, pose and terminal operation widths. `$A3:AAC2-AAE1` variant selection now selects these executable cases.
- Airborne processing enables off-screen activity, optionally queues the jump sound, presents one pose and sleeps. Landing disables off-screen activity, optionally queues the landing sound, presents four poses, publishes ReadyToHop and sleeps. The four independent timing magnitudes remain explicitly required: airborne1 and landed2,5,2,3 ticks (the repeated2 shares its input). This is not an exception for chosen holds, nor a timing completion claim.
- Build passed (1434 warnings/zero errors; subsequent verifier-only build25 warnings/zero errors). Existing `--hopper-instruction-program-definitions` passes all96 native words,16 floor/ceiling programs, five actual producers including the Tourian alias, sound/off-screen/ready/sleep behavior, rejection/allocation and zero forbidden mechanics/presentation reads. `--lookup-stream-3` additionally passes a native instruction walk identifying all40 exact calculated visual-operand positions. The producer fixture forces instruction timers; no elapsed-time claim is made.
- `HopperAnimationDefinitions.Variants` and `HopperInstructionProgramDefinitions.PresentationWords` are converted. `Words` remains unchecked for its four independent timing inputs. Local accounting becomes120 converted plus three narrowly justified retained/mixed entries, subject to coordinator reconciliation. Refresh hashes for both Hopper definition sources.

### Integrated narrow Spark visual-cadence review

Replaced ten stored activation durations with flash/blank/final-gap/sustained-stage operations and four chosen visual cadences. Root native-source review approves only those four inputs and the shared three-tick callback-free continuous loops as chosen visual choreography: separate function timers control lifetime and emission, and the nonzero blank map retains fixed contact geometry. Deactivation cadence1, the subsequent intangible callback and extra inactive8 remain required. Words stays unchecked; no aggregate count change. Worker commits ee04422db,394df00ed,723df7b36.

Root confirmation: Verification build1445 warnings/zero errors; focused Spark check matches33 native control words and26 visual identities and exercises four production programs with cartridge reads denied. Forced instruction timers confirm transitions, not elapsed timing.

### Integrated semantic enemy frame registration

Replaced472 named cached registrations with semantic identity dispatch;66 native record-stride sequences calculate268 addresses. Root independently read all202 preceding native record headers and confirmed2+5*partCount strides. Terminal record counts do not determine their own address and need not equal preceding counts. Removed cached aggregate array while preserving enumeration, ordinal, range, duplicate exclusion and sorted additional targets. All sprite geometry, tile selection and pixels remain required under their existing entries.

Root Verification build1445warnings/0errors; --lookup-enemy-frame-registration confirms472 exact ordered identities, indexed/range behavior, native additional targets and uniqueness, plus64 historical OAM schemas and54 binding schemas with independent art/remap preservation. ResourceAudit build0/0. Worker09b807962; aggregate550converted/16retained-mixed/562pending. Worker's broader legacy aggregate independently reported unrelated projectile schema11 fixture failure; that unrelated fixture was not run or changed for root's focused confirmation.

### Integrated caret registration

The one visible caret is now a semantic registration property with calculated enumeration, rather than a stored registration row. Loader count binding uses the calculated collection. Root --lookup-intro-caret-registration confirms the native16-bit part count at8C:8D68, exact identity, enumeration and invalid indices. Verification build1445warnings/0errors; ResourceAudit0/0. Worker e944e8857. Artwork and timing remain separately required; inventory551converted/16retained-mixed/561pending.

### Integrated menu composition geometry (partial)

The centered two-tile missile cursor is calculated across GameOver,FileSelect andGameOptions. GameOver Baby frames use centered16x16 geometry; the four8x8 specimen-container caps reflect around that center. Supplied tile identities and cursor ordering remain required, as do pixels; all three sprites entries stay unchecked. Native82:CBCB-CBFA/CFE0-D00A are the identified composition records.

Root --lookup-menu-sprite-geometry passes all15native-imported parts/eight calculated GameOver compositions,135field edits including existing size rejection,reordering/expanded compositions,and all4cursor frames and edits through both other loaders. Verification1445warnings/0errors;ResourceAudit0/0. Both new geometry dependencies are pinned in the three provider contracts. Worker9961db125. Master551converted/16retained-mixed/561pending unchanged.

### Reopened independent hold magnitudes

Mochtroid Words14free-flight/5attached, YellowPipeBug Words4straight/1arc and Ripper Words8/7 remain required: earlier uniform-loop conversion did not derive these independent holds or establish an acceptable exception. Reopened all three Words entries; presentation/control calculations remain valid. Root reconciled master state from the source audit; aggregate now550 converted/16retained-mixed/562 pending after two environmental owner conversions.

### Further early timing audit corrections

Integrated5f00aef40's additional reopenings: Pickup Words energy8/ammunition5 at86:ED8D-EDFD; Fireflea Words2/1 atA3:8C2F-8D02; Choot Words idle1,jump/fall8then1 atA2:D82C-D84A. Earlier Mochtroid/YellowPipeBug/Ripper reopenings already applied in0f38142d4. Original batch3–6 claims now distinguish finished structure from unresolved timing. No runtime code changes; source inspection establishes these are pending magnitudes without a derivation or accepted exception. Master547 converted/16retained-mixed/565 pending.

### Integrated Mochtroid visual selection and registration

Both original visual entries calculate: two0,1,2,1pose cycles select six named frames from native record strides. NativeA3:A9B0 flight records advance2+5*6 through preceding records;AA06 attached records advance2+5*4. Root --lookup-mochtroid-visuals independently walks six native counted records, checks eight operands/rejection, and runs existing real initializer/state-switch/two-loop checks with mechanics reads denied. Build1445warnings/0errors. Worker c5a12a507. Timing14/5,shake amplitude and artwork remain required; master549 converted/16retained-mixed/563 pending.

### Independent Mochtroid visual-cadence review

Root source review approves only the14-tick flight and5-tick attached pulse cadences as chosen animation content. NativeA3:A745-A76B contains duration/map records andGoto without callbacks;A790 dispatches independent movement. A953-A9A7 uses separate80-contact damage and global-frame sound timing. HeaderA0:D8FF fixes10x12collision radii;A0:A08C's nonzero-map gate and fixed-radius contact do not distinguish the six nonzero poses. GenericA0:C276-C2AC only updates animation timer/map/cursor. Managed movement and touch consumers preserve this separation.

Generating a different pulse tempo invents a different visible performance; arithmetic encoding of the exact chosen tempo would simply disguise retained content. This narrow nonsense disposition excludes list-reset1,damage80,steering/shake,artwork and other enemy cadences. Worker follow-up will name/document the two inputs; Words remains pending until integrated review. No count change.

### Integrated reviewed Mochtroid cadence and partial shake

Worker f0dda1607 names14/5pulse tempos with the root-reviewed narrow visual-performance rationale. Words is mixed: layout/control/repetition calculated, two chosen visual tempos retained. List-reset1,damage80,movement and artwork are outside this disposition. Worker5e99435f6 removes both cardinal shake arrays using axis/sign calculation; independent amplitude2 remains explicitly required, both shake entries unchecked.

Root Verification1445warnings/0errors; --lookup-mochtroid-shake matches16native masked-timer cases and existing12word/initializer/state-switch/two-loop/eightselector checks with reads denied. Position wrap,countdown and velocity reset retain their original operations. Master549converted/17retained-mixed/562pending.


## Integrated narration page registry

IntroNarrationDefinitions.NativePages is converted (6d8a8df8f). Named mutually exclusive Page1..Page6 cases preserve source/begin/finish identities and ordered enumeration without a stored registry. Its sole extractor consumer continues to enumerate all pages. This conversion does not dispose of narrative text, placement, delays or glyph artwork.

Root confirmation: Verification build 1445 warnings/zero errors; --lookup-intro-narration-registry passes all six native begin/finish boundaries through extraction, text compilation, ordering and invalid page rejection. ResourceAudit builds with zero warnings/errors. The worker's broader legacy fixture lacked mandatory installed cinematic artwork and was not claimed as passing; this focused check covers the changed registry contract. Inventory 553 converted/17 retained-mixed/558 pending.


## Independent pending integration reviews: narration wording and Ripper cadence

Root approved only the six exact English opening-narration page strings and the single authored break before GALACTIC CIVILIZATION. Native spans are 8C:C383-C796,C797-CB44,CB45-CE32,CE33-D15C,D15D-D510,D511-D5DE. These are selected story prose and emphasis; a formula generating other words changes that authored content, while numerical encoding merely disguises it. Row/column geometry, 29-column wrapping, glyph mapping, callbacks, all timings and artwork are excluded. All 31 rows are regular; greedy wrapping explains 24 of 25 within-page boundaries. Complete layout/edit confirmation is still required before any aggregate closure.

Root separately inspected Ripper-family moving loops A2:E19B/E1AF/E2E0/E2F4/E477/E48B: duration/map pairs and Goto only. Ordinary maps keep body101 and vary wing110/113/103; variant maps keep body105 while changing accessory parts. Headers A0:D3FF/D43F/D47F use fixed8x8/8x4 radii. E221/E353/E4DA movement, terrain reversal and frozen-map selection depend on velocities/physical state rather than animation holds. Only the exact8/7 visual cadence is approved as chosen animation content. Speed, collision, grapple, freeze, reset1 and artwork are excluded. Named cadence representation and confirmation of all six production loops remain required before integration.

Both are narrow reviews, not blanket visual-data exemptions. Inventory remains555 converted/17 retained-mixed/556 pending until integration.


## Integrated narration layout and narrowly retained prose

IntroNarrationPresentation.pages is mixed complete (a665c6e51). Six semantic pages retain only the approved English story strings and the deliberate break before GALACTIC CIVILIZATION. Greedy 29-column wrapping and row4+2*line calculate all31 lines, with no cached stock line-record table. A formula cannot derive the selected story prose without encoding it as content; this exception does not extend to timings, glyph mapping, callbacks or artwork. Exact candidate comparison preserves all independently edited records.

Root Verification build1445 warnings/zero errors; --lookup-narration-layout passes770 direct native glyph/column/row records, six calculated page views, exactly one retained hard break,31 text edits plus three spacing/row edits and bounds. ResourceAudit source contract now describes calculated views and independent fallback records. Inventory555 converted/18 retained-mixed/555 pending.


## Integrated Ripper chosen visual holds

RipperInstructionProgramDefinitions.Words is mixed complete (7f9babb19). Existing program structure is calculated; only reviewed neutral8/alternate7 visual holds remain, now explicitly named rather than encoded as an unexplained subtraction. Native animation-only loops, fixed body/hitbox and independent movement/freeze evidence are recorded above. Speed,collision,grapple,freeze,reset1 and all artwork remain excluded from the exception.

Root Verification build1445 warnings/zero errors; --ripper-instruction-mechanics passes36 native words, all six production-installed loops and24 visual selectors with original bytes forbidden. Inventory555 converted/19 retained-mixed/554 pending.

## Integrated Baby sprite reflection and immutable ordinary-OAM views

Partial6503f14ba: only the three30-part Baby poses A9:F9A8/FA40/FAD8 calculate15 reflected partners from15 supplied half-parts, using x'=-x-size and horizontal flip. Thirty draw-order entries per pose remain supplied. Full matched arrays are discarded; asymmetric edits and changed counts preserve exact owned parts. Ordinary catalog/render/hash APIs now accept immutable indexed views; the span renderer and extended/projectile contracts remain intact.

The selected half-artwork, draw order, pixels and all other compositions remain required; EnemySpritemapCatalog.frames stays unchecked. No broad symmetry/artwork exemption. Compiler integration exposed one previously integrated Dead Torizo fixture's private-constructor/out-view binding; root adapted that binding without changing assertions.

Root terminal Verification rebuild25 warnings/zero errors. --lookup-stream3-baby-sprite-reflection passes90native parts/actual OAM,90 isolated X edits,27 other field edits,reordered/expanded/empty layouts,canonical hashes,bounds and all64 ordinary legacy schemas/54binding schemas. The specifically affected --lookup-stream2-dead-torizo-geometry passes25parts/97tiles/12MVNs/10column limits/192actual row operations and independent edited display. ResourceAudit build0/0. Inventory unchanged558 converted/21 retained-mixed/549 pending.
## Integrated Grapple tile geometry and shared ink encoding

406459523,e8f61e9d7,2788888a6 calculate four endpoint primitives and transpose four horizontal tiles into vertical tiles. Endpoint cases are a filled diamond,hollow-center diamond,tipped square outline,and clipped diamond outline with inner spark. Eight horizontal/diagonal monochrome tiles retain64bytes of independent coverage; four bitplanes calculate from the chosen pen. Exact-match admission preserves independent endpoint,vertical,coverage and colored edits; generated native tiles exist only as transfer output.

This is partial. Ten chosen center/radius/clipping/pen scalars,four frame-to-shape choices and all eight coverage masks remain required. Recognizing a shape does not exempt its parameters or artistic selection. GrappleTileAtlas.tiles remains unchecked; no new retained exception or aggregate count change. Inventory558converted/21retained-mixed/549pending.

Root Verification1447warnings/zero errors; dedicated --lookup-stream3-grapple-tile-patterns confirms512native bytes,64individual bitplane edits,eight binary coverage edits,seven Resolve/TryResolve transfer boundaries,exact64byte coverage storage and no stored stock generated tiles. ResourceAudit0warnings/0errors; both source closures include the new patterns helper and planar encoder. This check confirms transfer content and binding, not player visual validation.
## Integrated corrected death and shared-projectile programs

777a68bdf,a7df0a460,3db73ec60,d82a56c4e replace stored control/presentation arrays with program dispatch,record-layout calculations and shared phase operations. Complete entries: EnemyDeathInstructionProgramDefinitions.PresentationWords; EnemyProjectileInstructionMechanicsDefinitions.MiscDustInitialPointers and PresentationFrames. Their address/identity conversion does not resolve independently selected frame extents,timing or artwork.

Death Words stays pending for respawn64,repetitions5/16,independent blank holds8/8,normal pose5,contact pose2 and other unreviewed choices. Shared TimedPrograms/MechanicsWords and BlueRingDurations remain pending;53selected hold/ramp roles are explicitly named. Ring16/10/expansion-base11 and rainbow3/4 are not approved exceptions. Shared small-explosion4/6/5/6 follows the earlier narrow phase review; this integration grants no broader timing exemption and closes neither timing aggregate. Exact native small sequences86:E138-E150 and ED69-ED85 share six OAM compositions8D:B023-B082/BDFF-BE5E; death adds sound/pickup control. Existing source evidence remains subject to the aggregate review.

Root Verification1450warnings/zero errors. --enemy-death-instruction-mechanics passes five real producers,respawn tail,66native words and31installed selectors with no live instruction-source reads. --enemy-projectile-instruction-mechanics passes307native words,39real programs and25-word recursive hand-beam program,exact frame timers/control/producer assertions and read guards. Verification updates replace stale live-presentation-read expectations with installed-operand/native-duration assertions without relaxing producer behavior. ResourceAudit0warnings/0errors; source closure includes both definitions and shared helper. Inventory561converted/21retained-mixed/546pending.