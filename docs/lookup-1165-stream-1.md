# Issue 1165 - agent stream 1

GitHub child ticket: [#1238](https://github.com/MrJackSpade/SuperMetroidDecomp/issues/1238).

Assignment: **82 files / 226 named table definitions**. [Ownership rules and all streams](lookup-1165-streams.md). Inventory snapshot: 2026-10-04T20:43:25.3086366Z.

## Agent instructions

Work through every entry below under issue 1165. Check current source and review dispositions first. Convert the original mapping into calculation or meaningful cases, or document a concrete impossible/nonsense justification. Preserve behavior and supplied content edits. Independently resolve each value field; checking off a container does not excuse its unresolved payloads.

Edit only source files listed here and this stream checklist/report. Read other files as needed. Ask the coordinator to assign any additional dependency or new source/test file before writing it. Route shared review-ledger, project, verification-entry-point and master-inventory changes through the coordinator. Do not stage, commit or publish other agents' work. Use focused confirmation of identified changes, not exploratory test discovery.

For each completed entry, record the conversion or precise retention evidence, changed files and focused confirmation. Report cross-stream dependencies by path and required contract. Report pending entries honestly; definition counts are not effort estimates.

## Exclusive source files

| File | Definitions |
| --- | ---: |
| [csharp/src/SuperMetroid.Core/Assets/ChargeFlarePlacementCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/ChargeFlarePlacementCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/DragonVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/DragonVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentation.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentationDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterPresentation.cs](../csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterPresentation.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/MetroidVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/MetroidVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/OwtchStokeVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/OwtchStokeVisualDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs) | 5 |
| [csharp/src/SuperMetroid.Core/Assets/SamusAtmosphericArtworkCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/SamusAtmosphericArtworkCatalog.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs) | 11 |
| [csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs](../csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs) | 4 |
| [csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs](../csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/SciserVisualDefinitions.cs](../csharp/src/SuperMetroid.Core/Assets/SciserVisualDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs](../csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Audio/SpcMusicTables.cs](../csharp/src/SuperMetroid.Core/Audio/SpcMusicTables.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/BrinstarPipeBugInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/BrinstarPipeBugInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/BullInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/BullInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/BullMovementDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/BullMovementDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CacatacInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CacatacInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CacatacMovementDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CacatacMovementDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CacatacProjectileDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CacatacProjectileDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CacatacProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CacatacProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/ChargeFlareAnimationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/ChargeFlareAnimationDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/CommonEnemyProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/CommonEnemyProjectileInstructionProgramDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/DragonAnimationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DragonAnimationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/DragonFireballInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DragonFireballInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/DragonInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/DragonInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EscapeDachoraInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EscapeDachoraInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EscapeEtecoonDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/EscapeTimer.cs](../csharp/src/SuperMetroid.Core/Game/EscapeTimer.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/FallingSparkInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/FallingSparkInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/HibashiDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/HibashiDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/HibashiInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/HibashiInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/MiscDustProjectileDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/MiscDustProjectileDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/NuclearWaffleDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/NuclearWaffleInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/NuclearWaffleProjectileInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleProjectileInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/PowampInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PowampInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/PowampSpikeInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PowampSpikeInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/PuyoInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/PuyoInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Bull.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Bull.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PowampSpikes.cs](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PowampSpikes.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SamusArmCannonDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusArmCannonDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusAtmosphericAnimationDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericAnimationDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/SamusBeamCallbackDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusBeamCallbackDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SamusBombSpreadLaunchDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusBombSpreadLaunchDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/SamusHudDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusHudDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SamusPoseAimDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusPoseAimDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/SamusPoseInputDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs) | 35 |
| [csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs) | 43 |
| [csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusProjectileCooldownDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusProjectileCooldownDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusProjectileDamageDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusProjectileDamageDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusProjectileInstructionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusProjectileInstructionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs) | 3 |
| [csharp/src/SuperMetroid.Core/Game/SamusProjectileOriginDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusProjectileOriginDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusProjectileSelectionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSelectionDefinitions.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SamusShinesparkState.cs](../csharp/src/SuperMetroid.Core/Game/SamusShinesparkState.cs) | 1 |
| [csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs](../csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs) | 7 |
| [csharp/src/SuperMetroid.Core/Game/SciserInstructionProgramDefinitions.cs](../csharp/src/SuperMetroid.Core/Game/SciserInstructionProgramDefinitions.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Rooms/MetroidsClearedPlmRomData.cs](../csharp/src/SuperMetroid.Core/Rooms/MetroidsClearedPlmRomData.cs) | 2 |
| [csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs](../csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs) | 2 |

## Work checklist

### csharp/src/SuperMetroid.Core/Game/BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.cs

- [ ] **BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.Definitions** ([L27](../csharp/src/SuperMetroid.Core/Game/BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BrinstarPipeBugInstructionProgramDefinitions.cs

- [ ] **BrinstarPipeBugInstructionProgramDefinitions.Words** ([L21](../csharp/src/SuperMetroid.Core/Game/BrinstarPipeBugInstructionProgramDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **BrinstarPipeBugInstructionProgramDefinitions.PresentationWords** ([L46](../csharp/src/SuperMetroid.Core/Game/BrinstarPipeBugInstructionProgramDefinitions.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BullInstructionProgramDefinitions.cs

- [x] **BullInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/BullInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **BullInstructionProgramDefinitions.PresentationWords** ([L31](../csharp/src/SuperMetroid.Core/Game/BullInstructionProgramDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BullMovementDefinitions.cs

- [x] **BullMovementDefinitions.Intervals / deceleration** ([L54](../csharp/src/SuperMetroid.Core/Game/BullMovementDefinitions.cs#L54)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Bull.cs

- [x] **RoomEnemySystem.BullShotAngles** ([L152](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Bull.cs#L152)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CacatacInstructionProgramDefinitions.cs

- [ ] **CacatacInstructionProgramDefinitions.Words** ([L56](../csharp/src/SuperMetroid.Core/Game/CacatacInstructionProgramDefinitions.cs#L56)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **CacatacInstructionProgramDefinitions.PresentationWords** ([L116](../csharp/src/SuperMetroid.Core/Game/CacatacInstructionProgramDefinitions.cs#L116)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CacatacMovementDefinitions.cs

- [x] **CacatacMovementDefinitions.TravelDistances** ([L22](../csharp/src/SuperMetroid.Core/Game/CacatacMovementDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CacatacProjectileDefinitions.cs

- [x] **CacatacProjectileDefinitions.InstructionLists** ([L17](../csharp/src/SuperMetroid.Core/Game/CacatacProjectileDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CacatacProjectileInstructionProgramDefinitions.cs

- [x] **CacatacProjectileInstructionProgramDefinitions.Words** ([L43](../csharp/src/SuperMetroid.Core/Game/CacatacProjectileInstructionProgramDefinitions.cs#L43)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CacatacProjectileInstructionProgramDefinitions.PresentationWords** ([L73](../csharp/src/SuperMetroid.Core/Game/CacatacProjectileInstructionProgramDefinitions.cs#L73)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/ChargeFlarePlacementCatalog.cs

- [ ] **ChargeFlarePlacementCatalog.offsets** ([L9](../csharp/src/SuperMetroid.Core/Assets/ChargeFlarePlacementCatalog.cs#L9)) - installed stock table. Original/default payload behind ChargeFlarePlacementCatalog.offsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteDefinitions.cs

- [ ] **ChargeFlareSpriteDefinitions.Selectors** ([L69](../csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteDefinitions.cs#L69)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **ChargeFlareSpriteDefinitions.NativePointers** ([L127](../csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteDefinitions.cs#L127)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChargeFlareAnimationDefinitions.cs

- [x] **ChargeFlareAnimationDefinitions.Pointers** ([L20](../csharp/src/SuperMetroid.Core/Game/ChargeFlareAnimationDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChargeFlareAnimationDefinitions.Delays** ([L22](../csharp/src/SuperMetroid.Core/Game/ChargeFlareAnimationDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs

- [ ] **CommonEnemyEmptyExtendedFrameDefinitions.Banks** ([L18](../csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CommonEnemyProjectileInstructionProgramDefinitions.cs

- [ ] **CommonEnemyProjectileInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/CommonEnemyProjectileInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/DragonVisualDefinitions.cs

- [ ] **DragonVisualDefinitions.Frames / literal at L16** ([L16](../csharp/src/SuperMetroid.Core/Assets/DragonVisualDefinitions.cs#L16)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/DragonAnimationDefinitions.cs

- [ ] **DragonAnimationDefinitions.InstructionLists** ([L28](../csharp/src/SuperMetroid.Core/Game/DragonAnimationDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DragonFireballInstructionProgramDefinitions.cs

- [ ] **DragonFireballInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/DragonFireballInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DragonFireballInstructionProgramDefinitions.PresentationWords** ([L50](../csharp/src/SuperMetroid.Core/Game/DragonFireballInstructionProgramDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DragonInstructionProgramDefinitions.cs

- [ ] **DragonInstructionProgramDefinitions.Words** ([L38](../csharp/src/SuperMetroid.Core/Game/DragonInstructionProgramDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **DragonInstructionProgramDefinitions.PresentationWords** ([L61](../csharp/src/SuperMetroid.Core/Game/DragonInstructionProgramDefinitions.cs#L61)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentation.cs

- [ ] **EscapeTimerPresentation.frames** ([L10](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentation.cs#L10)) - installed stock table. Original/default payload behind EscapeTimerPresentation.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **EscapeTimerPresentation.anchors** ([L11](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentation.cs#L11)) - installed stock table. Original/default payload behind EscapeTimerPresentation.anchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentationDefinitions.cs

- [ ] **EscapeTimerPresentationDefinitions.DigitSpritemaps** ([L18](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentationDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs

- [ ] **EscapeTimerTileAtlas.transfer** ([L8](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs#L8)) - installed stock table. Original/default payload behind EscapeTimerTileAtlas.transfer. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterPresentation.cs

- [ ] **EscapeTypewriterPresentation.programs** ([L9](../csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterPresentation.cs#L9)) - installed stock table. Original/default payload behind EscapeTypewriterPresentation.programs. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/EscapeDachoraInstructionProgramDefinitions.cs

- [ ] **EscapeDachoraInstructionProgramDefinitions.Words** ([L40](../csharp/src/SuperMetroid.Core/Game/EscapeDachoraInstructionProgramDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EscapeDachoraInstructionProgramDefinitions.PresentationWords** ([L163](../csharp/src/SuperMetroid.Core/Game/EscapeDachoraInstructionProgramDefinitions.cs#L163)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EscapeEtecoonDefinitions.cs

- [ ] **EscapeEtecoonDefinitions.Initializations** ([L22](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs

- [ ] **EscapeEtecoonInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **EscapeEtecoonInstructionProgramDefinitions.PresentationWords** ([L85](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs#L85)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EscapeTimer.cs

- [ ] **EscapeTimer.CentisecondDecrements** ([L21](../csharp/src/SuperMetroid.Core/Game/EscapeTimer.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/FallingSparkInstructionProgramDefinitions.cs

- [ ] **FallingSparkInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/FallingSparkInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **FallingSparkInstructionProgramDefinitions.PresentationWords** ([L49](../csharp/src/SuperMetroid.Core/Game/FallingSparkInstructionProgramDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HibashiDefinitions.cs

- [ ] **HibashiDefinitions.ActivityFrames** ([L29](../csharp/src/SuperMetroid.Core/Game/HibashiDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HibashiInstructionProgramDefinitions.cs

- [ ] **HibashiInstructionProgramDefinitions.Words** ([L22](../csharp/src/SuperMetroid.Core/Game/HibashiInstructionProgramDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **HibashiInstructionProgramDefinitions.PresentationWords** ([L52](../csharp/src/SuperMetroid.Core/Game/HibashiInstructionProgramDefinitions.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MetroidVisualDefinitions.cs

- [ ] **MetroidVisualDefinitions.Frames / literal at L16** ([L16](../csharp/src/SuperMetroid.Core/Assets/MetroidVisualDefinitions.cs#L16)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs

- [ ] **MetroidBehaviorDefinitions.EscapeDisplacements** ([L14](../csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MetroidBehaviorDefinitions.RandomCrySoundEffects** ([L26](../csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs

- [ ] **MetroidInstructionProgramDefinitions.FrameDurations** ([L25](../csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MetroidInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs#L26)) - factory-built stock table. Stored MetroidInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [ ] **MetroidInstructionProgramDefinitions.PresentationWords** ([L27](../csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs#L27)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Rooms/MetroidsClearedPlmRomData.cs

- [ ] **MetroidsClearedPlmRomData.PreInstructionByArgumentWord** ([L35](../csharp/src/SuperMetroid.Core/Rooms/MetroidsClearedPlmRomData.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **MetroidsClearedPlmRomData.EventByArgumentWord** ([L52](../csharp/src/SuperMetroid.Core/Rooms/MetroidsClearedPlmRomData.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MiscDustProjectileDefinitions.cs

- [ ] **MiscDustProjectileDefinitions.SmokePlacements** ([L18](../csharp/src/SuperMetroid.Core/Game/MiscDustProjectileDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NuclearWaffleDefinitions.cs

- [ ] **NuclearWaffleDefinitions.Sweeps** ([L30](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NuclearWaffleInstructionProgramDefinitions.cs

- [ ] **NuclearWaffleInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NuclearWaffleInstructionProgramDefinitions.PresentationWords** ([L28](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NuclearWaffleProjectileInstructionProgramDefinitions.cs

- [ ] **NuclearWaffleProjectileInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleProjectileInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWords** ([L41](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleProjectileInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/OwtchStokeVisualDefinitions.cs

- [ ] **OwtchStokeVisualDefinitions.Owtch** ([L15](../csharp/src/SuperMetroid.Core/Assets/OwtchStokeVisualDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **OwtchStokeVisualDefinitions.Stoke** ([L21](../csharp/src/SuperMetroid.Core/Assets/OwtchStokeVisualDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs

- [ ] **OwtchInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **OwtchInstructionProgramDefinitions.PresentationWords** ([L36](../csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PowampInstructionProgramDefinitions.cs

- [ ] **PowampInstructionProgramDefinitions.Words** ([L41](../csharp/src/SuperMetroid.Core/Game/PowampInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PowampInstructionProgramDefinitions.PresentationWords** ([L53](../csharp/src/SuperMetroid.Core/Game/PowampInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PowampSpikeInstructionProgramDefinitions.cs

- [ ] **PowampSpikeInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/PowampSpikeInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PowampSpikeInstructionProgramDefinitions.PresentationWords** ([L35](../csharp/src/SuperMetroid.Core/Game/PowampSpikeInstructionProgramDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs

- [ ] **RoomEnemySystem.PowampWiggleOffsets** ([L76](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.PowampRisingBalloonYOffsets** ([L77](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs#L77)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.PowampSinkingBalloonYOffsets** ([L78](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs#L78)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PowampSpikes.cs

- [ ] **RoomEnemySystem.PowampSpikeXAccelerations** ([L15](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PowampSpikes.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **RoomEnemySystem.PowampSpikeYAccelerations** ([L18](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PowampSpikes.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PuyoInstructionProgramDefinitions.cs

- [ ] **PuyoInstructionProgramDefinitions.Words** ([L42](../csharp/src/SuperMetroid.Core/Game/PuyoInstructionProgramDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **PuyoInstructionProgramDefinitions.PresentationWords** ([L57](../csharp/src/SuperMetroid.Core/Game/PuyoInstructionProgramDefinitions.cs#L57)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs

- [ ] **SamusArmCannonArtworkCatalog.posePointers** ([L11](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L11)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.posePointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusArmCannonArtworkCatalog.drawingData** ([L12](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L12)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.drawingData. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusArmCannonArtworkCatalog.attributes** ([L13](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L13)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.attributes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusArmCannonArtworkCatalog.tileSources** ([L14](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L14)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.tileSources. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusArmCannonArtworkFormat.TileSourcePointers** ([L195](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L195)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/SamusAtmosphericArtworkCatalog.cs

- [ ] **SamusAtmosphericArtworkCatalog.typeOne** ([L12](../csharp/src/SuperMetroid.Core/Assets/SamusAtmosphericArtworkCatalog.cs#L12)) - installed stock table. Original/default payload behind SamusAtmosphericArtworkCatalog.typeOne. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusAtmosphericArtworkCatalog.sharedTypeFour** ([L13](../csharp/src/SuperMetroid.Core/Assets/SamusAtmosphericArtworkCatalog.cs#L13)) - installed stock table. Original/default payload behind SamusAtmosphericArtworkCatalog.sharedTypeFour. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs

- [ ] **SamusBodyArtworkCatalog.topPointers** ([L25](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L25)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.topPointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.bottomPointers** ([L26](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L26)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.bottomPointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.posePointers** ([L27](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L27)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.posePointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.graphicsYOffsets** ([L28](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L28)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.graphicsYOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.landingYOffsets** ([L29](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L29)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.landingYOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.postureYOffsets** ([L30](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L30)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.postureYOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.drainedYOffsets** ([L31](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L31)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.drainedYOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.frames** ([L32](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L32)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.top** ([L33](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L33)) - installed stock table. Top set/position to tile-definition mapping and metadata. The actual planar pixels are owned by SamusBodyTileDefinition.planar; the reverse address dictionary is an alias.
- [ ] **SamusBodyArtworkCatalog.bottom** ([L34](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L34)) - installed stock table. Bottom set/position to tile-definition mapping and metadata. The actual planar pixels are owned by SamusBodyTileDefinition.planar; the reverse address dictionary is an alias.
- [ ] **SamusBodyTileDefinition.planar** ([L250](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L250)) - installed stock table. Original/default payload behind SamusBodyTileDefinition.planar. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs

- [ ] **SamusDeathTileAtlas.planar** ([L9](../csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs#L9)) - installed stock table. Original/default payload behind SamusDeathTileAtlas.planar. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs

- [ ] **SamusHurtColorCatalog.hurt** ([L17](../csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs#L17)) - installed stock table. Original/default payload behind SamusHurtColorCatalog.hurt. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusHurtColorCatalog.intro** ([L18](../csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs#L18)) - installed stock table. Original/default payload behind SamusHurtColorCatalog.intro. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs

- [ ] **SamusSpritemapArtworkCatalog.topBases** ([L22](../csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs#L22)) - installed stock table. Original/default payload behind SamusSpritemapArtworkCatalog.topBases. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusSpritemapArtworkCatalog.bottomBases** ([L23](../csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs#L23)) - installed stock table. Original/default payload behind SamusSpritemapArtworkCatalog.bottomBases. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusSpritemapArtworkCatalog.pointers** ([L24](../csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs#L24)) - installed stock table. Original/default payload behind SamusSpritemapArtworkCatalog.pointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusSpritemapArtworkCatalog.definitions** ([L25](../csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs#L25)) - installed stock table. Original/default payload behind SamusSpritemapArtworkCatalog.definitions. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs

- [ ] **SamusVisorColorCatalog.colors** ([L10](../csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs#L10)) - installed stock table. Original/default payload behind SamusVisorColorCatalog.colors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs

- [ ] **SamusAnimationDelayDefinitions.PosePointers** ([L20](../csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusAnimationDelayDefinitions.DelayStreams** ([L88](../csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs#L88)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusArmCannonDefinitions.cs

- [ ] **SamusArmCannonDefinitions.OpenFlags** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusArmCannonDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusAtmosphericAnimationDefinitions.cs

- [ ] **SamusAtmosphericAnimationDefinitions.FrameTimers** ([L24](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericAnimationDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs

- [ ] **SamusAtmosphericEffectDefinitions.WaterSplashKinds** ([L30](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusAtmosphericEffectDefinitions.RunningFootContacts** ([L63](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs#L63)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusAtmosphericEffectDefinitions.CrateriaRoomEffects** ([L73](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs#L73)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusBeamCallbackDefinitions.cs

- [ ] **SamusBeamCallbackDefinitions.UnchargedDefinitions** ([L10](../csharp/src/SuperMetroid.Core/Game/SamusBeamCallbackDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusBeamCallbackDefinitions.ChargedDefinitions** ([L32](../csharp/src/SuperMetroid.Core/Game/SamusBeamCallbackDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusBombSpreadLaunchDefinitions.cs

- [ ] **SamusBombSpreadLaunchDefinitions.Launches** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusBombSpreadLaunchDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs

- [ ] **SamusHorizontalMotionDefinitions.AirMaximums** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusHorizontalMotionDefinitions.WaterMaximums** ([L16](../csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusHorizontalMotionDefinitions.LavaMaximums** ([L25](../csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusHudDefinitions.cs

- [ ] **SamusHudDefinitions.MovementHandlers** ([L10](../csharp/src/SuperMetroid.Core/Game/SamusHudDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusHudDefinitions.PostureObservationsByPose** ([L27](../csharp/src/SuperMetroid.Core/Game/SamusHudDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseAimDefinitions.cs

- [ ] **SamusPoseAimDefinitions.Directions** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseAimDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs

- [ ] **SamusPoseCollisionDefinitions.VerticalRadii** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs

- [ ] **SamusPoseDispatchDefinitions.Facing** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseDispatchDefinitions.Movement** ([L29](../csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseDispatchDefinitions.NoInputPose** ([L50](../csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseInputDefinitions.cs

- [ ] **SamusPoseInputDefinitions.ListByPose** ([L13](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs

- [ ] **SamusPoseInputRulesEarly.ListA0DE** ([L13](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA0EC** ([L20](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA172** ([L47](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA1F8** ([L74](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L74)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA242** ([L91](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L91)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA28C** ([L108](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L108)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA2BE** ([L121](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L121)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA2F6** ([L135](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L135)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA376** ([L161](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L161)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA3F6** ([L187](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L187)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA40A** ([L195](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L195)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA41E** ([L203](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L203)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA46E** ([L221](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L221)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA4BE** ([L239](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L239)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA50E** ([L257](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L257)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA55E** ([L275](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L275)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA5AE** ([L293](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L293)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA5FE** ([L311](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L311)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA618** ([L320](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L320)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA632** ([L329](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L329)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA64C** ([L338](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L338)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA66C** ([L362](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L362)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA6BC** ([L380](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L380)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA70C** ([L398](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L398)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA750** ([L414](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L414)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA794** ([L430](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L430)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA7AE** ([L439](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L439)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA7CC** ([L458](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L458)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA7E0** ([L466](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L466)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA874** ([L484](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L484)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA8AC** ([L498](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L498)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA8E4** ([L512](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L512)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA8EC** ([L518](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L518)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA8FC** ([L524](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L524)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesEarly.ListA904** ([L530](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L530)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs

- [ ] **SamusPoseInputRulesLate.ListA90C** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA926** ([L17](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA940** ([L26](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA954** ([L34](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA968** ([L42](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA97C** ([L50](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA990** ([L58](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA998** ([L64](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA9A0** ([L70](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L70)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA9C6** ([L81](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L81)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListA9EC** ([L92](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L92)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAA12** ([L103](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L103)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAA38** ([L114](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L114)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAA7C** ([L130](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L130)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAAC0** ([L146](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L146)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAB3A** ([L171](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L171)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListABB4** ([L196](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L196)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAC40** ([L224](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L224)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListACCC** ([L252](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L252)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListACE0** ([L260](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L260)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListACF4** ([L268](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L268)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAD08** ([L276](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L276)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAD1C** ([L284](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L284)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAD30** ([L292](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L292)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAD44** ([L300](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L300)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAD58** ([L308](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L308)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAD6C** ([L316](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L316)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAD80** ([L324](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L324)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAD94** ([L332](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L332)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListADD2** ([L347](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L347)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAE10** ([L362](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L362)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAE18** ([L368](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L368)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAE56** ([L383](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L383)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAE94** ([L398](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L398)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAEDE** ([L415](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L415)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAF28** ([L432](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L432)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAF60** ([L446](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L446)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAF98** ([L460](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L460)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAFAC** ([L468](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L468)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAFC0** ([L476](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L476)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAFD4** ([L484](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L484)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAFE8** ([L492](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L492)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusPoseInputRulesLate.ListAFFC** ([L500](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L500)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs

- [ ] **SamusPoseProjectileOriginDefinitions.YCorrections** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileCooldownDefinitions.cs

- [x] **SamusProjectileCooldownDefinitions.Delays** ([L19](../csharp/src/SuperMetroid.Core/Game/SamusProjectileCooldownDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileDamageDefinitions.cs

- [ ] **SamusProjectileDamageDefinitions.BeamDamage** ([L12](../csharp/src/SuperMetroid.Core/Game/SamusProjectileDamageDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileInstructionDefinitions.cs

- [ ] **SamusProjectileInstructionDefinitions.Words** ([L12](../csharp/src/SuperMetroid.Core/Game/SamusProjectileInstructionDefinitions.cs#L12)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs

- [ ] **SamusProjectileMotionDefinitions.Missile** ([L14](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusProjectileMotionDefinitions.SuperMissile** ([L17](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusProjectileMotionDefinitions.Beam** ([L20](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileOriginDefinitions.cs

- [ ] **SamusProjectileOriginDefinitions.Offsets** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusProjectileOriginDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs

- [ ] **SamusProjectileRadiusDefinitions.Radii** ([L18](../csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs#L18)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileSelectionDefinitions.cs

- [ ] **SamusProjectileSelectionDefinitions.Pointers** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSelectionDefinitions.cs#L15)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs

- [ ] **SamusProjectileSoundRoutingDefinitions.UnchargedSounds** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusProjectileSoundRoutingDefinitions.ChargedSounds** ([L23](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusShinesparkState.cs

- [ ] **SamusShinesparkState.FinishCrash / departureAngles** ([L651](../csharp/src/SuperMetroid.Core/Game/SamusShinesparkState.cs#L651)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs

- [ ] **SamusSpecialSequenceRomData.Death.DeathTileSegments** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusSpecialSequenceRomData.Death.MovementTypeInitialFrames** ([L24](../csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs

- [x] **SamusVerticalMotionDefinitions.Jump** ([L13](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.HighJump** ([L16](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.WallJump** ([L19](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.HighWallJump** ([L22](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.GravityFractions** ([L25](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.KnockbackWords** ([L28](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.BombJumpWords** ([L31](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/SciserVisualDefinitions.cs

- [ ] **SciserVisualDefinitions.Frames / literal at L16** ([L16](../csharp/src/SuperMetroid.Core/Assets/SciserVisualDefinitions.cs#L16)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/SciserInstructionProgramDefinitions.cs

- [ ] **SciserInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/SciserInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SciserInstructionProgramDefinitions.PresentationWords** ([L49](../csharp/src/SuperMetroid.Core/Game/SciserInstructionProgramDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Audio/SpcMusicTables.cs

- [ ] **SpcMusicTables.panVolume** ([L50](../csharp/src/SuperMetroid.Core/Audio/SpcMusicTables.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs

- [ ] **SpeedBoosterBlockPlmDrawDefinitions.BombReveal** ([L19](../csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SpeedBoosterBlockPlmDrawDefinitions.All** ([L24](../csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs

- [ ] **WorldMapArtwork.background** ([L8](../csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs#L8)) - installed stock table. Original/default payload behind WorldMapArtwork.background. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **WorldMapArtwork.foreground** ([L8](../csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs#L8)) - installed stock table. Original/default payload behind WorldMapArtwork.foreground. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

## Agent handoff

- Completed conversions: 18 definitions in batches 1 through 3 below.
- Justified retained entries: none.
- Confirmation results: isolated Verification build passed (1431 existing warnings, zero errors); --lookup-stream-1 passed.
- Cross-stream dependencies and proposed shared-file patches: pending.
- Remaining entries: all unchecked entries above.


## Batch 1: vertical launch and firing cadence

Verified by isolated build and --lookup-stream-1.

- `SamusVerticalMotionDefinitions`: removed all seven stored mappings. Air/water/lava and normal/wall/Hi-Jump select semantic launch cases; each 8.8 magnitude splits into native whole/fraction words. Knockback and bomb jumps share their liquid cases; gravity dispatches by medium. Preserves invalid-medium `IndexOutOfRangeException`.
- `SamusProjectileCooldownDefinitions`: removes all 59 delay bytes. Twelve legal beam combinations share 15 uncharged / 30 charged frames except uncharged Plasma+Ice (12); four unused combinations and padding remain zero. Named non-beam kinds select their delays, and all held-fire combinations use 25. Preserves the bounded SpaceTime Beam adjacent byte and invalid-address rejection.
- Original evidence: pinned `upstream-disassembly/src/bank_90.asm`, native `$90:9EA1..9EFB` and `$90:C254..C28E`; these label media, equipment, weapon kinds and charge state explicitly. No retained entry or new exception.
- Focused confirmation: coordinator wires `VerifyLookupStream1(ISnesAddressSpace rom)` from `Program.LookupStream1.cs`. It compares every changed original word/byte directly with the pinned ROM, all four equipment/wall states in all three media, zero padding, the SpaceTime Beam byte, and rejected input boundaries. No gameplay exploration.
- Shared changes requested: verification command entry point only; coordinator owns execution, inventories and publication.
## Batch 2: Bull movement and Cacatac selectors

Verified by isolated build and --lookup-stream-1.

- `BullMovementDefinitions.Intervals` now calculates both fields: acceleration is selector+3; deceleration advances at half rate with the extra hold at selector four. Deleted the earlier invalid simplicity-based retention rationale.
- `RoomEnemySystem.Bull.BullShotAngles` is removed. `BullMovementDefinitions.ShotAngle` calculates 32-unit octants, collapsing the duplicated downward facing slot and wrapping the two upward end slots. The consumer calls it at the same point in the state update and retains invalid-direction rejection.
- `CacatacMovementDefinitions.TravelDistances` is removed: selector zero chooses one block, selectors one through five choose four through eight blocks. Deleted the earlier invalid piecewise-complexity retention rationale.
- `CacatacProjectileDefinitions.InstructionLists` is replaced with named direction/facing to named animation dispatch. Speed-pair behavior remains intact.
- Original source: bank A8 `$D871..D8C8`, bank A2 `$9F36..9F41` and initializer `$9F7E/$9F89`, bank 86 `$D96A..D97C` and direction comments at `$D992`.
- `VerifyLookupStream1` includes all 26 interval words, ten angles, six patrol distances, ten direction pointers and their rejection boundaries. Coordinator runs the command; no new retention exception.

### Batch 1/2 verification completion

`dotnet build csharp/src/SuperMetroid.Verification --no-restore -v quiet` passed with 1431 warnings and zero errors. `dotnet run --project csharp/src/SuperMetroid.Verification --no-build -- --lookup-stream-1` passed. The first boundary check exposed signed subtraction overflow in the new cooldown padding test for `int.MinValue`; the final implementation uses absolute address bounds and the same rejection assertion now passes. Remaining 214 named definitions stay unchecked; no blanket disposition has been applied. Local stream work pauses for the scheduled stream-4 rotation after committing this verified batch.

Next prepared batch (not implemented): `BullInstructionProgramDefinitions`, `CacatacProjectileInstructionProgramDefinitions` and `ChargeFlareAnimationDefinitions` have regular duration/control layout and bounded restart/rewind cadence. Original bank A8 D841..D86F, bank86 D92E..D969, bank90 C481..C4B4 inspected; existing focused consumer verifiers can confirm loops/sleep/presentation boundaries after conversion.
## Batch 3: charge flare and Bull/Cacatac control layouts

Six more definitions converted; 18 complete and 208 remain. No retained exception.

- `ChargeFlareAnimationDefinitions`: three component pointers dispatch to named main/slow/fast programs. Main uses thirty three-frame delays then rewinds fourteen; sparks decrease their delay for two frames, hold their minimum for four, then restart, with fast sparks one frame quicker. No delay array or generated cache remains.
- `BullInstructionProgramDefinitions`: calculates four ten-frame normal drawings plus loop and four three-frame shot drawings repeated five times before returning to normal. Mechanics addresses/values and eight visual operand addresses derive from those control sections.
- `CacatacProjectileInstructionProgramDefinitions`: ten six-byte programs calculate one-frame drawing duration, terminal sleep and the intervening presentation operand address. Direction dispatch from batch2 remains separate from physical program order.
- Evidence: pinned bank90 `$C481..C4B4`, bankA8 `$D841..D86F`, bank86 `$D92E..D969`. The original frame/control layouts were inspected before implementation.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-1` passed all52 flare bytes/51 overlapping words, every Bull/Cacatac program word and presentation identity, byte classifiers, enumeration and invalid bounds. Existing production Bull loop/shot-cycle checks and charge-flare cadence/rewind/restart checks also pass with mechanics reads forbidden. No gameplay discovery or retention exception.