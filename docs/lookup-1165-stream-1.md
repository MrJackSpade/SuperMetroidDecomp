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

- [x] **BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.Definitions** ([L27](../csharp/src/SuperMetroid.Core/Game/BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BrinstarPipeBugInstructionProgramDefinitions.cs

- [x] **BrinstarPipeBugInstructionProgramDefinitions.Words** ([L21](../csharp/src/SuperMetroid.Core/Game/BrinstarPipeBugInstructionProgramDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **BrinstarPipeBugInstructionProgramDefinitions.PresentationWords** ([L46](../csharp/src/SuperMetroid.Core/Game/BrinstarPipeBugInstructionProgramDefinitions.cs#L46)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BullInstructionProgramDefinitions.cs

- [x] **BullInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/BullInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **BullInstructionProgramDefinitions.PresentationWords** ([L31](../csharp/src/SuperMetroid.Core/Game/BullInstructionProgramDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/BullMovementDefinitions.cs

- [x] **BullMovementDefinitions.Intervals / deceleration** ([L54](../csharp/src/SuperMetroid.Core/Game/BullMovementDefinitions.cs#L54)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Bull.cs

- [x] **RoomEnemySystem.BullShotAngles** ([L152](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Bull.cs#L152)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CacatacInstructionProgramDefinitions.cs

- [x] **CacatacInstructionProgramDefinitions.Words** ([L56](../csharp/src/SuperMetroid.Core/Game/CacatacInstructionProgramDefinitions.cs#L56)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **CacatacInstructionProgramDefinitions.PresentationWords** ([L116](../csharp/src/SuperMetroid.Core/Game/CacatacInstructionProgramDefinitions.cs#L116)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **CommonEnemyEmptyExtendedFrameDefinitions.Banks** ([L18](../csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CommonEnemyProjectileInstructionProgramDefinitions.cs

- [x] **CommonEnemyProjectileInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/CommonEnemyProjectileInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/DragonVisualDefinitions.cs

- [ ] **DragonVisualDefinitions.Frames / literal at L16** ([L16](../csharp/src/SuperMetroid.Core/Assets/DragonVisualDefinitions.cs#L16)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/DragonAnimationDefinitions.cs

- [x] **DragonAnimationDefinitions.InstructionLists** ([L28](../csharp/src/SuperMetroid.Core/Game/DragonAnimationDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DragonFireballInstructionProgramDefinitions.cs

- [x] **DragonFireballInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/DragonFireballInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DragonFireballInstructionProgramDefinitions.PresentationWords** ([L50](../csharp/src/SuperMetroid.Core/Game/DragonFireballInstructionProgramDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/DragonInstructionProgramDefinitions.cs

- [x] **DragonInstructionProgramDefinitions.Words** ([L38](../csharp/src/SuperMetroid.Core/Game/DragonInstructionProgramDefinitions.cs#L38)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **DragonInstructionProgramDefinitions.PresentationWords** ([L61](../csharp/src/SuperMetroid.Core/Game/DragonInstructionProgramDefinitions.cs#L61)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **EscapeEtecoonDefinitions.Initializations** ([L22](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs

- [x] **EscapeEtecoonInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EscapeEtecoonInstructionProgramDefinitions.PresentationWords** ([L85](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs#L85)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EscapeTimer.cs

- [ ] **EscapeTimer.CentisecondDecrements** ([L21](../csharp/src/SuperMetroid.Core/Game/EscapeTimer.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/FallingSparkInstructionProgramDefinitions.cs

- [x] **FallingSparkInstructionProgramDefinitions.Words** ([L27](../csharp/src/SuperMetroid.Core/Game/FallingSparkInstructionProgramDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **FallingSparkInstructionProgramDefinitions.PresentationWords** ([L49](../csharp/src/SuperMetroid.Core/Game/FallingSparkInstructionProgramDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HibashiDefinitions.cs

- [x] **HibashiDefinitions.ActivityFrames** ([L29](../csharp/src/SuperMetroid.Core/Game/HibashiDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/HibashiInstructionProgramDefinitions.cs

- [x] **HibashiInstructionProgramDefinitions.Words** ([L22](../csharp/src/SuperMetroid.Core/Game/HibashiInstructionProgramDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **HibashiInstructionProgramDefinitions.PresentationWords** ([L52](../csharp/src/SuperMetroid.Core/Game/HibashiInstructionProgramDefinitions.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/MetroidVisualDefinitions.cs

- [ ] **MetroidVisualDefinitions.Frames / literal at L16** ([L16](../csharp/src/SuperMetroid.Core/Assets/MetroidVisualDefinitions.cs#L16)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs

- [x] **MetroidBehaviorDefinitions.EscapeDisplacements** ([L14](../csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MetroidBehaviorDefinitions.RandomCrySoundEffects** ([L26](../csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **PowampSpikeInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/PowampSpikeInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PowampSpikeInstructionProgramDefinitions.PresentationWords** ([L35](../csharp/src/SuperMetroid.Core/Game/PowampSpikeInstructionProgramDefinitions.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs

- [x] **RoomEnemySystem.PowampWiggleOffsets** ([L76](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs#L76)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.PowampRisingBalloonYOffsets** ([L77](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs#L77)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.PowampSinkingBalloonYOffsets** ([L78](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.Powamp.cs#L78)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PowampSpikes.cs

- [x] **RoomEnemySystem.PowampSpikeXAccelerations** ([L15](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PowampSpikes.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **RoomEnemySystem.PowampSpikeYAccelerations** ([L18](../csharp/src/SuperMetroid.Core/Game/RoomEnemySystem.PowampSpikes.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **SamusBeamCallbackDefinitions.UnchargedDefinitions** ([L10](../csharp/src/SuperMetroid.Core/Game/SamusBeamCallbackDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusBeamCallbackDefinitions.ChargedDefinitions** ([L32](../csharp/src/SuperMetroid.Core/Game/SamusBeamCallbackDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusBombSpreadLaunchDefinitions.cs

- [x] **SamusBombSpreadLaunchDefinitions.Launches** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusBombSpreadLaunchDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs

- [x] **SamusHorizontalMotionDefinitions.AirMaximums** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusHorizontalMotionDefinitions.WaterMaximums** ([L16](../csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusHorizontalMotionDefinitions.LavaMaximums** ([L25](../csharp/src/SuperMetroid.Core/Game/SamusHorizontalMotionDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **SamusPoseInputDefinitions.ListByPose** ([L13](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs

- [x] **SamusPoseInputRulesEarly.ListA0DE** ([L13](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA0EC** ([L20](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA172** ([L47](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L47)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA1F8** ([L74](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L74)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA242** ([L91](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L91)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA28C** ([L108](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L108)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA2BE** ([L121](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L121)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA2F6** ([L135](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L135)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA376** ([L161](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L161)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA3F6** ([L187](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L187)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA40A** ([L195](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L195)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA41E** ([L203](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L203)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA46E** ([L221](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L221)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA4BE** ([L239](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L239)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA50E** ([L257](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L257)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA55E** ([L275](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L275)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA5AE** ([L293](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L293)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA5FE** ([L311](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L311)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA618** ([L320](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L320)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA632** ([L329](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L329)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA64C** ([L338](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L338)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA66C** ([L362](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L362)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA6BC** ([L380](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L380)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA70C** ([L398](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L398)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA750** ([L414](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L414)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA794** ([L430](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L430)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA7AE** ([L439](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L439)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA7CC** ([L458](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L458)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA7E0** ([L466](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L466)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA874** ([L484](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L484)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA8AC** ([L498](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L498)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA8E4** ([L512](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L512)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA8EC** ([L518](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L518)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA8FC** ([L524](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L524)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesEarly.ListA904** ([L530](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesEarly.cs#L530)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs

- [x] **SamusPoseInputRulesLate.ListA90C** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA926** ([L17](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA940** ([L26](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA954** ([L34](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L34)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA968** ([L42](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA97C** ([L50](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA990** ([L58](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L58)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA998** ([L64](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L64)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA9A0** ([L70](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L70)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA9C6** ([L81](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L81)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListA9EC** ([L92](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L92)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAA12** ([L103](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L103)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAA38** ([L114](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L114)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAA7C** ([L130](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L130)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAAC0** ([L146](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L146)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAB3A** ([L171](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L171)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListABB4** ([L196](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L196)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAC40** ([L224](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L224)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListACCC** ([L252](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L252)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListACE0** ([L260](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L260)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListACF4** ([L268](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L268)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAD08** ([L276](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L276)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAD1C** ([L284](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L284)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAD30** ([L292](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L292)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAD44** ([L300](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L300)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAD58** ([L308](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L308)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAD6C** ([L316](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L316)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAD80** ([L324](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L324)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAD94** ([L332](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L332)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListADD2** ([L347](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L347)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAE10** ([L362](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L362)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAE18** ([L368](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L368)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAE56** ([L383](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L383)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAE94** ([L398](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L398)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAEDE** ([L415](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L415)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAF28** ([L432](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L432)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAF60** ([L446](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L446)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAF98** ([L460](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L460)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAFAC** ([L468](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L468)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAFC0** ([L476](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L476)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAFD4** ([L484](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L484)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAFE8** ([L492](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L492)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusPoseInputRulesLate.ListAFFC** ([L500](../csharp/src/SuperMetroid.Core/Game/SamusPoseInputRulesLate.cs#L500)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs

- [ ] **SamusPoseProjectileOriginDefinitions.YCorrections** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileCooldownDefinitions.cs

- [x] **SamusProjectileCooldownDefinitions.Delays** ([L19](../csharp/src/SuperMetroid.Core/Game/SamusProjectileCooldownDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileDamageDefinitions.cs

- [x] **SamusProjectileDamageDefinitions.BeamDamage** ([L12](../csharp/src/SuperMetroid.Core/Game/SamusProjectileDamageDefinitions.cs#L12)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileInstructionDefinitions.cs

- [ ] **SamusProjectileInstructionDefinitions.Words** ([L12](../csharp/src/SuperMetroid.Core/Game/SamusProjectileInstructionDefinitions.cs#L12)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs

- [x] **SamusProjectileMotionDefinitions.Missile** ([L14](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusProjectileMotionDefinitions.SuperMissile** ([L17](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusProjectileMotionDefinitions.Beam** ([L20](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileOriginDefinitions.cs

- [ ] **SamusProjectileOriginDefinitions.Offsets** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusProjectileOriginDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs

- [ ] **SamusProjectileRadiusDefinitions.Radii** ([L18](../csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs#L18)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileSelectionDefinitions.cs

- [x] **SamusProjectileSelectionDefinitions.Pointers** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSelectionDefinitions.cs#L15)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs

- [x] **SamusProjectileSoundRoutingDefinitions.UnchargedSounds** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusProjectileSoundRoutingDefinitions.ChargedSounds** ([L23](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusShinesparkState.cs

- [ ] **SamusShinesparkState.FinishCrash / departureAngles** ([L651](../csharp/src/SuperMetroid.Core/Game/SamusShinesparkState.cs#L651)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs

- [x] **SamusSpecialSequenceRomData.Death.DeathTileSegments** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusSpecialSequenceRomData.Death.MovementTypeInitialFrames** ([L24](../csharp/src/SuperMetroid.Core/Game/SamusSpecialSequenceRomData.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs

- [x] **SamusVerticalMotionDefinitions.Jump** ([L13](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L13)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.HighJump** ([L16](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L16)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.WallJump** ([L19](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.HighWallJump** ([L22](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.GravityFractions** ([L25](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.KnockbackWords** ([L28](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusVerticalMotionDefinitions.BombJumpWords** ([L31](../csharp/src/SuperMetroid.Core/Game/SamusVerticalMotionDefinitions.cs#L31)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/SciserVisualDefinitions.cs

- [x] **SciserVisualDefinitions.Frames / literal at L16** ([L16](../csharp/src/SuperMetroid.Core/Assets/SciserVisualDefinitions.cs#L16)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/SciserInstructionProgramDefinitions.cs

- [x] **SciserInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/SciserInstructionProgramDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SciserInstructionProgramDefinitions.PresentationWords** ([L49](../csharp/src/SuperMetroid.Core/Game/SciserInstructionProgramDefinitions.cs#L49)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Audio/SpcMusicTables.cs

- [ ] **SpcMusicTables.panVolume** ([L50](../csharp/src/SuperMetroid.Core/Audio/SpcMusicTables.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs

- [ ] **SpeedBoosterBlockPlmDrawDefinitions.BombReveal** ([L19](../csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SpeedBoosterBlockPlmDrawDefinitions.All** ([L24](../csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs

- [ ] **WorldMapArtwork.background** ([L8](../csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs#L8)) - installed stock table. Original/default payload behind WorldMapArtwork.background. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **WorldMapArtwork.foreground** ([L8](../csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs#L8)) - installed stock table. Original/default payload behind WorldMapArtwork.foreground. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

## Agent handoff

- Completed conversions: 117 definitions in batches 1 through 10 below.
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
## Batch 4: horizontal motion, projectile acceleration and sound dispatch

Eight more definitions converted; 26 complete and 200 remain. No retained exception.

- `SamusHorizontalMotionDefinitions`: maximum speed dispatches by named movement type and medium. The existing contiguous native record addressing remains intact, including the two air selectors that read the beginning of the water records; acceleration/deceleration fields and mutable aliases remain unchanged.
- `SamusProjectileMotionDefinitions`: signed X/Y acceleration derives from eight compass octants, collapsing the duplicated facing directions and selecting the native cardinal/diagonal magnitude. Preserves the interleaved missile/super layout, separated beam axes, and bounded adjacent speed-row observations.
- `SamusProjectileSoundRoutingDefinitions`: derives sound requests from charge state, beam family and Ice/Wave variants. Uncharged invalid combinations reach the charged family; charged invalid combinations select the adjacent named non-beam requests. No stored array or generated cache remains.
- Evidence: pinned bank90 horizontal motion records consumed by the indexed speed routes, `$C28F..C2CE` sound requests and `$C2D1..C37A` projectile speed/acceleration words. Semantic movement and weapon comments agree with the converted branches.
- Confirmation: Verification incremental build passed (25 warnings, zero errors); `--lookup-stream-1` passed. Checks cover all 492 indexed motion words, all 85 projectile motion words, the actual power-beam initializer across all 16 combinations and ten directions with mechanics ROM access forbidden, rejection bounds, and all 32 sound/adjacent observations.
- The older `VerifyBeamSpeedRows` whole-frame fixture reaches unsupported unrelated animation mechanics `$9386DD` before completing. This batch uses a focused direct production-initializer fixture with the same original ROM oracle and ROM-read guard; the unrelated animation implementation and existing verifier remain unchanged.
## Batch 5: beam callback dispatch

Two more definitions converted; 28 complete and 198 remain. No retained exception.

- `SamusBeamCallbackDefinitions`: Wave selects terrain-passing movement. Uncharged Wave/Ice-Wave select the three-frame trail; charge, Spazer or Plasma select four frames. Non-Wave beams select ordinary motion. The bounded invalid-combination adjacent-code observations dispatch explicitly to their existing named identities, preserving unsupported-null and translated behavior separately.
- Original evidence: pinned bank90 `$B96E..B98D` and `$BA3E..BA5D`, including twelve native callback words and four adjacent instruction words per producer. No callback array or generated cache remains.
- Confirmation: Verification build passed (25 warnings, zero errors); `--lookup-stream-1` passed all32 original callback identities and actual `TryFireBeam` initialization for all24 legal charge/combination cases with source ranges forbidden. The verifier's original full-frame path remains available; the scoped command uses the new initialization-only option because the older full-frame fixture reaches unrelated unsupported animation mechanics `$9386F5`.
## Batch 6: beam damage by weapon identity

One more definition converted; 29 complete and 197 remain. No retained exception.

- `SamusProjectileDamageDefinitions.BeamDamage`: resolves physical header rows to named equipment combinations, selects uncharged weapon damage through cases, then multiplies by three for charge. Preserves the charged half's swapped Wave/Plasma and Plasma-Ice/Plasma-Wave header order, exact header alignment, all non-beam cases and rejection boundaries.
- Evidence: pinned bank93 `$8431..8640`, `ProjectileDataTable_Uncharged_*` and `ProjectileDataTable_Charged_*`. All24 original damage words agree; the charged multiplier is exact across all twelve weapon combinations.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-1` passed including existing `VerifyProjectileDamage` original40-header/357-selector checks and seven actual projectile initializer paths with selection/damage ROM reads forbidden. Existing assertions and oracles are unchanged.
## Batch 7: projectile selector and directional program layout

One more definition converted; 30 complete and 196 remain. No retained exception.

- Removed the complete357-entry `SamusProjectileSelectionDefinitions.Pointers` FrozenDictionary and its literal producer. The56 leading selectors dispatch by beam equipment, charge, projectile kind and special attack. Beam rows share the damage catalog's physical-header identity; direction selects eight compass programs, four opposite-direction programs, or the common Ice program as appropriate. Wave preserves its separate upward prefix and downward entry; uncharged Plasma-Wave preserves the skipped first frame on three axes. Missile programs use their twelve-byte stride. Final non-beam rows select named impact, bomb, special, trail and echo programs.
- All address bases and program identities have named XML catalog members. Original damage-header fallthrough, exact odd-address domain, null selector entries and adjacent header observations remain intact. No replacement cache or stock dictionary remains.
- Evidence: pinned bank93 `$83C1..86DA` selectors/header rows and their named instruction programs `$86DB..A16D`; the original row comments name each direction and shared program. Each formula follows the actual program spacing and direction sharing.
- Confirmation: Verification build passed (1216 warnings, zero errors); `--lookup-stream-1` passed every397 original selector/header words, every intervening unaligned rejection, outer/bank/extreme bounds, and the existing seven initializer paths with selection/damage ROM reads forbidden. An initial parenthesization error in the new non-beam/trail switch expression was caught by the focused command and corrected before this verified commit.
## Batch 8: pose input programs

Seventy-nine more definitions converted; 109 complete and 117 remain. No retained exception.

- `SamusPoseInputDefinitions.ListByPose` now dispatches named pose cases to named native input-program identities. All253 authored poses preserve their exact identity, including the eight distinct empty programs. Unnamed native pose identities have dedicated catalog constants with original pointer locations.
- All78 nonempty stored lists in `SamusPoseInputRulesEarly` and `SamusPoseInputRulesLate` are removed. Each program executes direct ordered predicates over named canonical Jump/Shoot/aim/direction bits and returns its named target pose. This is the native first-matching-case behavior expressed as code; no array, row generator, indexed tuple switch or replacement cache remains.
- `SamusPoseTransitionTable` invokes the selected program and preserves raw-zero-input early fallback, ignored Start/Select bits, allowed extra buttons, first-match priority, self-match direct return, empty-program direct return, exhausted-program fallback, and the exact winning native six-byte entry address. The selected rule record remains diagnostic output, not stored lookup data.
- Evidence: pinned bank91 `TransitionTable` and `DetermineProspectivePoseFromTransitionTable`, `$9EE2..B00F` and `$81A9..82D8`; source documents all eight canonical bits and ordered subset matching explicitly. Native list symbols and addresses are recorded beside the program/catalog identities.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-1` passed. The existing independent ROM graph oracle covers253 mappings,86 program identities,598 original conditions and16,580,608 complete decision comparisons, including impossible held/edge combinations, ignored-bit noise, first/self matches, fallback distinction and exact winning metadata, with production ROM reads forbidden. Invalid253..255 poses and existing zero-allocation assertions also pass. Row-span assertions were replaced with direct program emptiness checks; original decision oracle and behavioral assertions are retained.
## Batch 9: Bomb Spread and enemy role/phase definitions

Four more definitions converted; 113 complete and 113 remain. No retained exception.

- Bomb Spread's five launches calculate fuse and all three velocity fields from distance to the central bomb. Preserves the native X direction bit rather than signed-short interpretation, center2.5-pixel upward launch, and exact slot-domain exception.
- Hibashi's22 collision frames rise five pixels per frame. Half-height remains24 through frame17 and shrinks by four on each remaining frame. Both fields and rejected bounds are preserved.
- Dragon animation dispatch selects the named idle/wing/attack program and facing directly. The same six-selector bounds remain in facing/phase helpers.
- Escape Etecoon initialization dispatches by the three named roles; all five fields are included. Paired odd parameter values still select the same role after clearing bit zero; other values still fail.
- Evidence: pinned bank90 `$D8CF..D8F6`, bankA6 `$8DBB..8E12`, bankA2 `$E5EF..E5FA`, bankB3 `$E718..E735` and their named launch/hitbox/phase/role consumers.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-1` passed. New focused check compares all20 Bomb Spread words,44 Hibashi words,six Dragon pointers and all15 Etecoon fields (also through odd-role aliases) directly with the ROM, plus invalid bounds for each API.
## Batch 10: Falling Spark and Powamp spike control layouts

Four more definitions converted; 117 complete and 109 remain. No retained exception.

- Falling Spark calculates three three-frame falling drawings plus loop, followed by eleven one-frame impact drawings and terminal delete. Mechanics enumeration and all14 presentation operand identities derive from that layout.
- Powamp spike calculates three six-frame drawings plus loop and its separate delete program. All six mechanics words and three presentation operands derive from those sections.
- Both direct readers and mechanics-byte classifiers now use the control layout rather than searching stored records. Enumeration bounds, unaligned rejection, presentation exclusion and native address domains remain intact.
- Evidence: pinned bank86 `InstList_EnemyProjectile_FallingSpark_Falling/HitFloor`, `$F353..F390`, and `InstList_EnemyProjectile_PowampSpike/Delete`, `$D208..D219`.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-1` passed every23 original mechanics words,17 presentation addresses, original enumeration order, every low/high-byte ownership decision, all unaligned word rejections, enumeration bounds and outer/bank/extreme byte rejection. This confirms the changed definition APIs directly; runtime consumers are unchanged.
### Rotation handoff after batch 10

Stream1 pauses for the scheduled stream4 rotation at117 completed/109 unchecked definitions. Every unchecked entry remains an implementation obligation; no unexamined entry is retained by implication. Prepared next source review: Brinstar Pipe Bug normal rise/shoot loops and strong alternating rise cadence (`$B3:87AB..882A`, `$8A1D..8A6C`); no edits made there. Larger pending scopes include SPC pan mapping, remaining Samus animation/pose and projectile instruction/radius definitions, enemy instruction layouts and stock artwork payloads. No performance, complexity, size or provenance retention justification applies.

## Batch 11: Brinstar Pipe Bug program layout and cadence

Two more definitions converted;119 complete and107 unchecked. No retained exception.

- Normal rising programs calculate eight two-tick draws; normal shooting calculates six one-tick draws. Strong rising alternates two/one ticks across four draws, while strong shooting uses four three-tick draws. Left/right programs share the same layout. All programs end with their own goto loop.
- Mechanics and presentation addresses enumerate this layout without stored or generated arrays; readers and byte classification calculate exact native domains. All eight entry constants now carry original symbol/address XML documentation.
- Evidence: pinned bankB3 `InstList_Zeb_FacingLeft/Right_Rising/Shooting`, `$87AB..882A`, and `InstList_Zebbo_FacingLeft/Right_Rising/Shooting`, `$8A1D..8A6C`.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-1` passed. Existing Brinstar Pipe Bug verification is wired unchanged and confirms60 original control words,44 visual selectors, all eight actual animation programs and complete loops with mechanics source words forbidden, plus rejected pointers and zero-allocation assertions.
## Batch 12: Metroid escape motion and cry dispatch

Two more definitions converted;121 complete and105 unchecked. No retained exception.

- Escape displacement calculates a two-pixel cardinal rotation from the countdown's low two bits. X/Y signs and zero components preserve all restored ushort selectors.
- Random cries directly dispatch the three named library-two sound variants with the exact native slot order and2/3/3 distribution. Source identities live in XML catalog constants; RNG advancement and sound queuing remain unchanged.
- Evidence: pinned bankA3 `BombedOffVelocities` `$EA3F..EA4E` and `Instruction_Metroid_PlayRandomMetroidSFX.SFX` `$EAD6..EAE5`.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-1` passed. Existing verifier is wired unchanged: all16 original words,65536 actual escape selectors and eight production cry instruction selections agree with source reads forbidden.
## Batch 13: common empty-frame bank domain and projectile delete

Two more definitions converted;123 complete and103 unchecked. No retained exception.

- Supported common empty-frame banks are expressed as native bank ranges, with ordered enumeration generated directly. Exact bank-local frame and OAM-pointer requirements remain unchanged.
- The shared projectile delete program returns its sole mechanics word directly, with the same rejected enumeration indices. Existing direct reader and byte ownership logic are unchanged.
- Evidence: pinned enemy-bank common `$804D/$804F` empty OAM/one-component records in bankA0,A2..AA,B2..B3, and bank86 `InstList_EnemyProjectile_Delete` `$84FC`.
- Confirmation: Verification build passed (1431 warnings, zero errors); `--lookup-stream-1` passed. New focused check covers all256 bank values, exact twelve-bank enumeration order, original OAM/extended-frame component counts, neighboring-pointer rejection, native delete word and invalid enumeration indices.
## Batch 14: blue-spore room-owner program definitions

One more definition converted;124 complete and102 unchecked. No retained exception.

- Removed the two stored six-field setup records and read-only array wrapper. Named standard/Spore Spawn program objects store only their owner; definition identity, setup entry, first record, terminal loop and death-callback policy are derived from that owner and the14x10-byte layout.
- The ordered IReadOnlyList view preserves Count/index/enumeration contracts without storing rows. Direct mechanics resolution selects the two semantic programs explicitly. Program setup, waits and loop control remain unchanged.
- Evidence: pinned bank8D blue-spore standard `$ED99..EE2C`, Spore Spawn `$EE2D..EEC4`, and definition records `$F775/$F779`. Only Spore Spawn installs the area-mini-boss-death pre-instruction.
- Confirmation: Verification build passed (1432 warnings, zero errors); `--lookup-stream-1` passed. Existing focused blue-spore verifier is wired unchanged: all66 original control words, color exclusion, both140-frame cycles and conditional boss-death deletion run with mechanics source reads forbidden. Presentation payloads remain their separate obligation.
## Batch 15: Hibashi and Dragon-fireball control layouts

Four more definitions converted;128 complete and98 unchecked. No retained exception.

- Dragon fireballs calculate four identical two-draw five-tick loops with independent rising/falling and left/right entry identities. All sixteen control and eight presentation addresses follow the twelve-byte program stride.
- Hibashi calculates23 duration/draw/activity-callback records and its invisible hitbox program. Cadence phases retain2/1/2/4 ticks. The first activity callback has its distinct address; following callback starts advance20 bytes. Initial sound and terminal sleeps remain explicit control cases, with source catalog XML.
- Evidence: pinned bank86 `$B4BF..B4EE`; bankA6 `$8D1B..8DAE`, `Instruction_Hibashi_PlaySFX` `$8DAF`, `ActivityFrame0` `$8E13`, and following callbacks starting `$8E2D`.
- Confirmation: full build passed (1432 warnings, zero errors), final incremental verifier build passed (25 warnings, zero errors); `--lookup-stream-1` passed. New checks compare all66 original control words,32 presentation addresses, original order, both-byte ownership, unaligned/presentation/outer/bank rejection and enumeration bounds. Existing Dragon-fireball production loops, both velocity-zero-crossing handoffs and shared deletion pass with mechanics reads forbidden.
- The old Dragon verifier expected8 live ROM presentation reads. With coordinator-granted ownership, it now asserts every actual loop step's exact installed operand identity and zero presentation reads; existing mechanics and behavior assertions remain.
## Batch 16: Cacatac idle and attack layouts

Two more definitions converted;130 complete and96 unchecked. No retained exception.

- Upright/inverted idle records calculate eight eight-tick draws and their distinct loop targets: upright reexecutes movement setup, inverted skips that setup on subsequent loops.
- Attack records calculate alternating21/5-tick poses, sound dispatch and five left-to-right spike commands interleaving cardinal/diagonal selectors. Named direction identities anchor the arithmetic; attack completion returns to the orientation's initial idle entry.
- All56 mechanics words and24 presentation operands derive from shared80-byte orientation layouts; no stored or generated row arrays remain.
- Evidence: pinned bankA2 `InstList_Cacatac_UpsideUp/Down_Idling/Attacking`, `$9E8A..9F29`.
- Confirmation: Verification build passed (1432 warnings on full build,1192 on final incremental, zero errors); `--lookup-stream-1` passed. Existing Cacatac verifier is wired unchanged and confirms all56 native words,24 compiled visual selectors, four actual complete programs and ten real spike spawns with source bytes forbidden.
## Batch 17: Powamp balloon and spike motion

Five more definitions converted; 135 complete and 91 unchecked. No retained exception.

- Twelve wiggle phases calculate a signed three-pixel triangle wave. Rising/sinking balloon poses calculate four-pixel steps. Existing instruction-cursor selection, underflow fallback, centered transition boundaries and position wrapping remain unchanged.
- Eight clockwise spike directions calculate signed 8.8 X acceleration; Y uses a quarter-turn rotation. Horizontal collision still returns before Y acceleration or movement. Native selector bounds remain enforced.
- Evidence: pinned bank A8 `PowampWiggleTable` `$C1A1..C1B8`, `HandlePowampBalloonYOffset` offsets `$C277..C282`; bank86 `PowampSpike_VelocityTable_X/Y` `$D21A..D239` and pre-instruction `$D263`.
- Confirmation: Verification build passed (1432 warnings, zero errors); `--lookup-stream-1` passed. Focused native comparison confirms all34 motion words and invalid selector boundaries. Source diff confirms unchanged cursor fallback, collision ordering and signed velocity wrapping.

## Batch 18: Dragon body, wing and attack layouts

Two more definitions converted; 137 complete and 89 unchecked. No retained exception.

- Idle sleep, two-pose five-tick wing loops and five-pose attack programs derive their26 control words and16 presentation addresses from facing and program layout. Attack durations use semantic initial hold, extension/retraction, fully extended and return phases; completion callback and sleep remain explicit.
- Removed the two stored arrays and binary search over rows. Calculated enumeration preserves native address ordering, rejection and byte ownership; presentation remains owned by compiled visual definitions.
- Evidence: pinned bankA2 `InstList_Dragon_Idle/Wings/Attacking_FacingLeft/Right`, `$E59B..E5EE`, and attack-completion callback `$E5FB`.
- Confirmation: Verification build passed (1432 warnings, zero errors); `--lookup-stream-1` passed. Existing unchanged Dragon verifier compares all26 original control words,16 compiled visual operands and six actual programs, including loop/sleep cursors and attack-completion state, with source mechanics reads forbidden.

## Batch 19: escape Etecoon program layouts

Two more definitions converted; 139 complete and 87 unchecked. No retained exception.

- Four-pose walking loops calculate low/high-tide and escape cadence. Waiting poses alternate64/8 ticks. Gratitude calculates its four repeated eight-tick pose/left-step records, with explicit timer setup, repeat branch and departure handoff. All63 control words and30 presentation addresses derive from these layouts.
- Evidence: pinned bankB3 escape Etecoon programs `$E556..E60F`; gratitude loop `$E5E0..E603` repeats four displacement callbacks with operand-3, then presents the final two poses and enters the escape program.
- Confirmation: full Verification build passed (1432 warnings, zero errors), final incremental build25 warnings/zero errors; `--lookup-stream-1` passed. Existing63 native-word comparisons and seven real programs retain gratitude displacement, escape transition, source guard, bounds and allocation assertions.
- Reproduced obsolete verifier failure: it expected30 live ROM presentation reads and observed0. With coordinator-granted ownership, it now checks all30 actual installed operand identities against native visual values and requires zero presentation reads. Existing mechanics/behavior assertions remain.
- Rotation checkpoint: remaining87 entries stay required. Pending source work includes charge-flare placement/selectors, escape timer cadence, SPC pan and the other unchecked definitions; no exemption has been inferred from an unproven formula. Stream4 resumes next per coordinator scheduling.

## Batch 20: Sciser surface layouts and visual catalog

Three more definitions converted; 142 complete and 84 unchecked. No retained exception.

- Four Sciser surface programs calculate their shared 24-byte layouts: vertical/horizontal movement callback, four eight-frame records, then a goto back to the first record. All 32 control words and 16 presentation identities keep native address order, bounds and byte-guard behavior.
- Twelve visual catalog rows are generated from the four semantic orientations and three animation phases. Each original OAM record occupies 22 bytes (two-byte count and four five-byte objects); stable editable names and orientation order remain unchanged. The actual OAM artwork remains independently required.
- Evidence: pinned bankA3 Sciser instruction lists `$967B..96DA`; twelve four-object spritemaps start `$9703`, ordered up/right/down/left in native storage. Programs use right/left/down/up order and a 0/1/2/1 animation cadence.
- Confirmation: Verification build 1432 warnings, zero errors; `--lookup-stream-1` passed. Unchanged Sciser fixture executes all four real initialized loops and movement callbacks with both instruction/presentation ROM reads forbidden, plus allocation and invalid-pointer assertions. New checks compare all native control words/order, all sixteen visual identities, twelve native spritemap addresses/object counts/banks/stable names, exact mechanics/presentation byte domains and invalid indices.

## Batch 21: death initialization cases and cyclic graphics transfers

Two more definitions converted; 144 complete and 82 unchecked. No retained exception.

- Death initialization now selects frame one for Morph/Spring Ball movement, zero for the two native unused glitch-ball types, and five for other retail movement. A calculated readonly sequence preserves all 28 indices and out-of-domain rejection.
- Five graphics transfer rows now calculate cyclic page `(index + 1) % 5`, source `$9B8000 + page * $400`, and VRAM word `$6000 + page * $200`. A calculated readonly view preserves ordering, length, indexing and enumeration. Runtime, installed atlas, extractor and existing structural verifier bindings adapt to that view; queued DMA behavior is unchanged.
- Evidence: pinned bank9B `SetSamusDeathSequencePose.animationFrames` `$B420..B43B`; `SamusDeathSequencePointers_Source` `$B7BF..B7C8` and `Destination` `$B7C9..B7D2`.
- Reproduced verifier setup failure: existing VerifySamusDeathSequence supplied synthetic palette bytes but omitted the installed artwork catalog required by production, failing at its first flashing palette update. The fixture now constructs a catalog from those same synthetic colors/shades/indices and passes it to Step. All original assertions remain; no production fallback was added.
- Confirmation: Verification and its AssetExtraction dependency build passed (240 warnings/zero errors after the additional extractor binding; final fixture-only rebuild 25 warnings/zero errors). `--lookup-stream-1` passed all 28 native frame bytes, ten native source/destination words, sequence enumeration and bounds. Existing actual death fixture preserves D7/D8 and standing/Morph/spin initialization, pose history, sound consumption, five DMA sizes/sources/destinations, preflash/flashing durations, 135 remaining explosion calls, exact palette colors and terminal state with timer reads forbidden.
