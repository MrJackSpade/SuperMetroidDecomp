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

- [x] **ChargeFlareSpriteDefinitions.Selectors** ([L69](../csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteDefinitions.cs#L69)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChargeFlareSpriteDefinitions.NativePointers** ([L127](../csharp/src/SuperMetroid.Core/Assets/ChargeFlareSpriteDefinitions.cs#L127)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/ChargeFlareAnimationDefinitions.cs

- [x] **ChargeFlareAnimationDefinitions.Pointers** ([L20](../csharp/src/SuperMetroid.Core/Game/ChargeFlareAnimationDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **ChargeFlareAnimationDefinitions.Delays** ([L22](../csharp/src/SuperMetroid.Core/Game/ChargeFlareAnimationDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs

- [x] **CommonEnemyEmptyExtendedFrameDefinitions.Banks** ([L18](../csharp/src/SuperMetroid.Core/Game/CommonEnemyEmptyExtendedFrameDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/CommonEnemyProjectileInstructionProgramDefinitions.cs

- [x] **CommonEnemyProjectileInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/CommonEnemyProjectileInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/DragonVisualDefinitions.cs

- [x] **DragonVisualDefinitions.Frames / literal at L16** ([L16](../csharp/src/SuperMetroid.Core/Assets/DragonVisualDefinitions.cs#L16)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

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

- [x] **EscapeTimerPresentationDefinitions.DigitSpritemaps** ([L18](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentationDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **MetroidVisualDefinitions.Frames / literal at L16** ([L16](../csharp/src/SuperMetroid.Core/Assets/MetroidVisualDefinitions.cs#L16)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

### csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs

- [x] **MetroidBehaviorDefinitions.EscapeDisplacements** ([L14](../csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MetroidBehaviorDefinitions.RandomCrySoundEffects** ([L26](../csharp/src/SuperMetroid.Core/Game/MetroidBehaviorDefinitions.cs#L26)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs

- [ ] **MetroidInstructionProgramDefinitions.FrameDurations** ([L25](../csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MetroidInstructionProgramDefinitions.Words** ([L26](../csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs#L26)) - factory-built stock table. Stored MetroidInstructionMechanicsWord[] initialized by BuildMechanicsWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.
- [x] **MetroidInstructionProgramDefinitions.PresentationWords** ([L27](../csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs#L27)) - factory-built stock table. Stored ushort[] initialized by BuildPresentationWords(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Rooms/MetroidsClearedPlmRomData.cs

- [x] **MetroidsClearedPlmRomData.PreInstructionByArgumentWord** ([L35](../csharp/src/SuperMetroid.Core/Rooms/MetroidsClearedPlmRomData.cs#L35)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **MetroidsClearedPlmRomData.EventByArgumentWord** ([L52](../csharp/src/SuperMetroid.Core/Rooms/MetroidsClearedPlmRomData.cs#L52)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/MiscDustProjectileDefinitions.cs

- [x] **MiscDustProjectileDefinitions.SmokePlacements** ([L18](../csharp/src/SuperMetroid.Core/Game/MiscDustProjectileDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NuclearWaffleDefinitions.cs

- [x] **NuclearWaffleDefinitions.Sweeps** ([L30](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NuclearWaffleInstructionProgramDefinitions.cs

- [x] **NuclearWaffleInstructionProgramDefinitions.Words** ([L19](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleInstructionProgramDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **NuclearWaffleInstructionProgramDefinitions.PresentationWords** ([L28](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleInstructionProgramDefinitions.cs#L28)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/NuclearWaffleProjectileInstructionProgramDefinitions.cs

- [x] **NuclearWaffleProjectileInstructionProgramDefinitions.Words** ([L23](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleProjectileInstructionProgramDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWords** ([L41](../csharp/src/SuperMetroid.Core/Game/NuclearWaffleProjectileInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/OwtchStokeVisualDefinitions.cs

- [x] **OwtchStokeVisualDefinitions.Owtch** ([L15](../csharp/src/SuperMetroid.Core/Assets/OwtchStokeVisualDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **OwtchStokeVisualDefinitions.Stoke** ([L21](../csharp/src/SuperMetroid.Core/Assets/OwtchStokeVisualDefinitions.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs

- [ ] **OwtchInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **OwtchInstructionProgramDefinitions.PresentationWords** ([L36](../csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **PuyoInstructionProgramDefinitions.Words** ([L42](../csharp/src/SuperMetroid.Core/Game/PuyoInstructionProgramDefinitions.cs#L42)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PuyoInstructionProgramDefinitions.PresentationWords** ([L57](../csharp/src/SuperMetroid.Core/Game/PuyoInstructionProgramDefinitions.cs#L57)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs

- [ ] **SamusArmCannonArtworkCatalog.posePointers** ([L11](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L11)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.posePointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusArmCannonArtworkCatalog.drawingData** ([L12](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L12)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.drawingData. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusArmCannonArtworkCatalog.attributes** ([L13](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L13)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.attributes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusArmCannonArtworkCatalog.tileSources** ([L14](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L14)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.tileSources. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusArmCannonArtworkFormat.TileSourcePointers** ([L195](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L195)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **SamusArmCannonDefinitions.OpenFlags** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusArmCannonDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusAtmosphericAnimationDefinitions.cs

- [ ] **SamusAtmosphericAnimationDefinitions.FrameTimers** ([L24](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericAnimationDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs

- [x] **SamusAtmosphericEffectDefinitions.WaterSplashKinds** ([L30](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs#L30)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusAtmosphericEffectDefinitions.RunningFootContacts** ([L63](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs#L63)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusAtmosphericEffectDefinitions.CrateriaRoomEffects** ([L73](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericEffectDefinitions.cs#L73)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **SamusHudDefinitions.MovementHandlers** ([L10](../csharp/src/SuperMetroid.Core/Game/SamusHudDefinitions.cs#L10)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusHudDefinitions.PostureObservationsByPose** ([L27](../csharp/src/SuperMetroid.Core/Game/SamusHudDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusPoseAimDefinitions.cs

- [x] **SamusPoseAimDefinitions.Directions** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseAimDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition. **Mixed disposition:** meaningful cases for native poses 00..FC; only the three FD..FF instruction-byte observations retained as nonsense, with exact evidence in Batch 30.

### csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs

- [x] **SamusPoseCollisionDefinitions.VerticalRadii** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseCollisionDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition. **Mixed disposition:** meaningful cases for native poses 00..FC; only the three FD..FF instruction-byte observations retained as nonsense, with exact evidence in Batch 30.

### csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs

- [x] **SamusPoseDispatchDefinitions.Facing** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition. **Mixed disposition:** meaningful cases for native poses 00..FC; only the three FD..FF instruction-byte observations retained as nonsense, with exact evidence in Batch 30.
- [x] **SamusPoseDispatchDefinitions.Movement** ([L29](../csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs#L29)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition. **Mixed disposition:** meaningful cases for native poses 00..FC; only the three FD..FF instruction-byte observations retained as nonsense, with exact evidence in Batch 30.
- [x] **SamusPoseDispatchDefinitions.NoInputPose** ([L50](../csharp/src/SuperMetroid.Core/Game/SamusPoseDispatchDefinitions.cs#L50)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition. **Mixed disposition:** meaningful cases for native poses 00..FC; only the three FD..FF instruction-byte observations retained as nonsense, with exact evidence in Batch 30.

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

- [x] **SamusPoseProjectileOriginDefinitions.YCorrections** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusPoseProjectileOriginDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition. **Mixed disposition:** physical cases for poses 00..FC; exactly three FD..FF instruction-byte observations retained as nonsense, with evidence in Batch 31.

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

- [x] **SamusProjectileOriginDefinitions.Offsets** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusProjectileOriginDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs

- [ ] **SamusProjectileRadiusDefinitions.Radii** ([L18](../csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs#L18)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileSelectionDefinitions.cs

- [x] **SamusProjectileSelectionDefinitions.Pointers** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSelectionDefinitions.cs#L15)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs

- [x] **SamusProjectileSoundRoutingDefinitions.UnchargedSounds** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusProjectileSoundRoutingDefinitions.ChargedSounds** ([L23](../csharp/src/SuperMetroid.Core/Game/SamusProjectileSoundRoutingDefinitions.cs#L23)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusShinesparkState.cs

- [x] **SamusShinesparkState.FinishCrash / departureAngles** ([L651](../csharp/src/SuperMetroid.Core/Game/SamusShinesparkState.cs#L651)) - method-local definition. Fixed values inside a method; not an array parameter/return declaration.

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

- [x] **SpeedBoosterBlockPlmDrawDefinitions.BombReveal** ([L19](../csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs#L19)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SpeedBoosterBlockPlmDrawDefinitions.All** ([L24](../csharp/src/SuperMetroid.Core/Rooms/SpeedBoosterBlockPlmDrawDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

## Batch 22: Nuclear Waffle mirrored sweep and shared loop layouts

Five more definitions converted; 149 complete and 77 unchecked. No retained exception.

- Clockwise/anticlockwise sweep fields calculate by reflection around angle $140: endpoints at plus/minus $50, turn thresholds at plus/minus $40, same-kind link separation 24 angle units and interleaved separation 12 with direction-dependent sign. Both native rows and all six value columns are covered.
- Parent and projectile programs calculate twelve three-tick frames followed by their respective goto opcode and loop target. Four cached mechanics/presentation arrays are removed while preserving all 28 control words, 24 presentation operand identities, byte guards and invalid-domain behavior.
- Evidence: pinned bankA6 parent `$9490..94C3`, sweep endpoints/angle spacing/turn thresholds `$95F6..960D`; bank86 articulated body `$BB5E..BB91`.
- Independently reproduced stale verifier transport expectations: both parent and projectile fixtures expected 12 live presentation reads and observed zero. They now assert zero live reads and capture all actual executed installed operands; parent values match original ROM selectors, projectile operands match each native frame position. These are fixture corrections, not claimed production fixes. Existing initializers, four spawned body links, full loops, shared deletion and source guards remain intact.
- Confirmation: full Verification build 1432 warnings/zero errors; final fixture rebuild 25 warnings/zero errors; `--lookup-stream-1` passed. Existing fixtures verify all twelve geometry words, both complete initializers, actual parent loop and four projectile loops/deletion. New checks confirm native control ordering, all visual identities, exact byte/control domains and index rejection. Existing allocation assertions pass.

## Batch 23: flare, timer digit and Dragon visual identities

Four more definitions converted; 153 complete and 73 unchecked. Actual sprite artwork remains independently required. No retained exception.

- Charge-flare selectors calculate thirty alternating growth-phase selections (0/1, then 2/1, then 2/3), followed by the slow/fast right/left spark runs. The 28 unique identities derive three seven-byte main-flare record steps and two twelve-record, seventeen-byte spark groups. The fourth main flare is a four-object crest; its larger size does not affect the preceding calculated addresses. The initial new count assertion mistakenly expected one object there; pinned `$93:AB81` confirmed four and the assertion/XML were corrected before final confirmation.
- Runtime selectors and unique pointer indexing use calculated readonly views. The extractor and loader materialize only transient inputs required by their existing span-based import APIs; no cached replacement pointer table is introduced.
- Escape-timer decimal glyph pointers calculate twelve-byte record strides. Dragon's catalog calculates four eight-object body records and two one-object wing records per facing direction while preserving the existing idle/wing/attack order and editable names.
- Evidence: pinned bank93 selectors `$A1A1..A20C`, main flare `$AB6C/$AB73/$AB7A/$AB81`, spark groups `$A6FD..A7C8` and `$A8DE..A9A9`; bank80 digit selectors `$9FD4..9FE7` and two-object maps beginning `$9FE8`; bankA2 Dragon program operands `$E59B..E5EE`, maps beginning `$E80C`.
- Confirmation: Verification and AssetExtraction dependency build passed (final 1217 warnings/zero errors); `--lookup-stream-1` passed. Focused checks compare all 54 native flare selectors and actual native-versus-installed OAM at one fixed position, ordered 28-pointer identity set/object counts, all ten native digit pointers, twelve Dragon native pointers/banks/names/object counts and exact index bounds. Existing Dragon behavior fixture remains passing. No broad installation/playthrough check was used.

## Batch 24: Owtch directional loops and Owtch/Stoke visual layouts

Four more definitions converted; 157 complete and 69 unchecked. No retained exception.

- Owtch's two 18-byte programs calculate their directional callback, three eight-tick frames and goto target. Cached control/presentation arrays are removed, preserving twelve controls and six visual identities.
- Owtch visual selection advances through three one-object records for leftward movement and reverses them for rightward movement. Stoke walking/attack phases select calculated positions in each five-record facing group; record object counts are 2/3/2/2/4, giving the native 75-byte facing stride. Both visual row arrays are removed; actual OAM artwork remains independently required.
- Evidence: pinned bankA2 Owtch programs `$A3AB..A3CE`, one-object maps `$A589/$A590/$A597`; Stoke programs `$8932..897D`, maps `$8ACA..8B5F` with two facing groups and shared pre-attack pose.
- Confirmation: full Verification build 1432 warnings/zero errors, final focused-check rebuild 25 warnings/zero errors; `--lookup-stream-1` passed. Unchanged fixtures execute both Owtch directional loops/callbacks and both Stoke walking loops/attacks with actual directional projectile spawns, all eighteen native visual choices and source guards. New checks verify original Owtch control order/values, exact mechanics-byte and both visual domains, owner rejection and index bounds. No fixture corrections were needed.

## Batch 25: Puyo programs, centered smoke boxes and Metroid quota dispatch

Five more definitions converted; 162 complete and 64 unchecked. No retained exception.

- Puyo's three grounded loops derive four equal-duration records and a goto (fast/medium/slow durations 5/8/10); five airborne poses derive one tick and sleep. Both arrays are removed, preserving 28 controls and 17 visual identities.
- Smoke placement calculates a stationary point, centered 8/16-pixel squares, then centered 16x32/32x64 rectangles. Masks are size minus one and offsets are negative half-size, covering all twenty native fields without a record table.
- The Metroid-clear PLM dispatcher calculates nine consecutive one-byte RTS identities and four sixteen-byte quota observer identities. The latter resolve consecutive semantic events beginning FirstMetroidHallCleared; all no-op identities stay distinct and odd/out-of-range arguments still fail.
- Evidence: pinned bankA2 Puyo `$99AD..9A06`; bank86 smoke placement `$E47E..E4A5`; bank84 quota pointer table `$DB28..DB40`, nine RTS handlers `$DAD5..DADD`, four observers `$DADE..DB1D` with immediate event operands.
- Confirmation: full Verification build 1432 warnings/zero errors, final focused-check rebuild 25 warnings/zero errors; `--lookup-stream-1` passed. Unchanged fixtures execute all three grounded/five airborne Puyo programs, all five real smoke placement families with exact X/Y and one RNG advancement, and real PLM population below/at enemy quota with resident sleep lists and invalid-argument rejection. New checks compare thirteen original dispatch pointers/four native event operands, exact Puyo ordered controls/visuals/byte guards, noncontrol rejection and index domains. No fixture corrections were needed.

## Batch 26: Metroid instruction and body-record layouts

Three layout definitions converted; 165 complete and 61 unchecked. The independently inventoried FrameDurations entry remains unchecked and required. No retained exception.

- Removed both factory-created cached Metroid arrays. Chasing calculates twenty timed records plus random-sound/goto control; draining calculates five records plus draining-sound/goto control. Existing five-duration inputs are still consumed, explicitly pending below; no replacement cache is introduced.
- The body catalog calculates four pointers from consecutive OAM records containing 8/6/8/8 objects, preserving all editable identities. Actual object artwork remains independently required.
- Timing obligation: native five-pose pattern 16/16/6/10/16 repeats four times while chasing and once while draining. The 64-tick total and repeated visual cycle do not independently justify its six/ten-tick split. FrameDurations remains required conversion/disposition work, not exempted as authored or small. Layout-cache completion does not complete that separate value input.
- Evidence: pinned bankA3 programs `$E9CF..EA3E`, body maps `Spritemap_Metroid_Insides_0..3` at `$F10D/$F137/$F157/$F181`.
- Confirmation: Verification build 1432 warnings/zero errors; `--lookup-stream-1` passed. Unchanged Metroid fixture executes both complete loops, random/draining sound callbacks, initialization and zero mechanics/presentation reads. New assertions verify all 31 native control values/order, 25 visual identities, four original body pointers/names/banks/object counts and exact byte/control/visual/index domains. No fixture corrections were needed.

## Batch 27: Samus atmospheric/HUD cases and crash departure axes

Five more definitions converted; 170 complete and 56 unchecked. No retained exception. HUD's separately inventoried bounded posture-byte observations remain required.

- Movement-state cases select grounded-pair versus diving splash, and modulo-five running phase selects foot contact. Named native Crateria room identities select landing-site, West Ocean entrance, wet-floor or no-effect policies; all original room bounds remain exact.
- HUD handler selection is an explicit movement-state dispatch: jump, ball, grapple, turning, transition, Draygon-held or standard. All 28 native handler identities and invalid-domain behavior remain unchanged; no table or cached replacement is retained.
- Shinespark crash echoes select horizontal/vertical/diagonal axes from the crash pose, with the second angle a half-turn opposite. Dedicated SamusShinesparkProjectileRomData owns this catalog operation. Runtime calls it before any pose restoration and preserves fixed projectile-slot capacity ordering.
- Evidence: pinned splash `$90:81A4..81BF`, foot contacts `$90:A424..A42D`, duplicated room policy `$90:EDC9..EDD8/$91:F0F3..F102` and corresponding bank8F room-header indices; HUD dispatch `$90:DD05`; crash departure bytes `$90:D4C6..D4D1`.
- Reproduced stale fixture setup: VerifySamusStoredShineAndShinespark failed at its first palette update because synthetic sentinel colors were supplied only as cartridge bytes. It now extracts/loads those same supplied colors into required suit/cycle catalogs for its two UpdatePalette call sites. All existing movement, collision, timing, sound, health, crash and echo assertions remain unchanged; no production fallback was added.
- Confirmation: full Verification build 1432 warnings/zero errors, final fixture rebuild 25 warnings/zero errors; `--lookup-stream-1` passed. Existing atmospheric source/actual-particle checks, HUD native dispatch/actual admission/charge checks and stored-shine/shinespark crash fixture pass. New checks compare all twelve native departure angles, all 256 pose acceptance/rejection cases, named room-header identities and bounds for movement/contact/room policies.

## Batch 28: semantic arm-cannon HUD policy

One more definition converted; 171 complete and 55 unchecked. No retained exception.

- Replaced the six-byte open-cover lookup with named HUD selection cases: Missile, Super Missile and Grapple open the cover; Beam, Power Bomb and X-ray leave it closed. All six accepted identities and the invalid-item rejection contract remain exact. Removed the verification-only span rather than constructing a replacement table.
- Evidence: pinned bank90 `ArmCannonOpenFlags` at `$C7D9..C7DE`, consumed by `UpdateArmCannonIsOpenState` at `$C5EB..C624`; the native HUD item selector is the domain of the cases.
- Reproduced stale fixture prerequisite: the existing native-policy verifier reached `Arm cannon requires installed drawing definitions` on its first Update because it omitted the installed artwork now required by the state. The fixture explicitly extracts/loads its supplied native arm-cannon artwork and binds it to each tested Samus. This is a fixture correction, not a production behavior fix; all original flag and cover-transition assertions remain.
- Confirmation: final Verification build 25 warnings/zero errors; `--lookup-stream-1` passed. All six direct flags match native bytes, the actual update waits for the second stable item sample and starts precisely the expected opening transition, policy ROM reads remain forbidden, and out-of-range HUD selection still throws.
## Batch 29: named-pose facing, movement and no-input dispatch

The native 253-pose dispatch is converted. The three aggregates have a mixed disposition: semantic cases plus the narrow adjacent-code retention approved and documented in Batch 30. They are not counted as wholly converted.

- Removed the three 256-byte pose policy lookups. Cases use the existing exclusive SamusPoseId domain and named movement/facing discriminators; no-input cases select named target poses or the unchanged keep-current sentinel. Turning, moonwalk and unused-record facing values remain exactly native, rather than being inferred from a pose's visible orientation.
- Added exactly 32 previously unnamed native unused pose identities, each with the pinned disassembly symbol and record address in its XML summary. Existing identities remain unchanged. FD/FE/FF are deliberately not declared as poses: their bounded reads still select the exact adjacent `Calc_Xray_HDMADataTable_OffScreen` instruction bytes, explicitly identified separately in each reader.
- Evidence: pinned bank91 `PoseDefinitions` at `$B629..BE10`, eight-byte records with facing/movement/no-input columns zero/one/two; adjacent routine begins `$BE11`. No ResourceAudit closure hash references either changed catalog in this worktree.
- Confirmation: Verification build 1432 warnings/zero errors; `--lookup-stream-1` passed. New assertions independently compare all 768 native column bytes over the full byte domain. Existing pose-dispatch fixture preserves live/prospective facing/movement access, no-input sentinel behavior, actual pose-history publication, invalid adjacent movement errors and warmed allocation checks with all ROM reads forbidden. Existing ordered input graph and native winning-pose checks remain passing.
## Batch 30: named firing directions and physical pose extents; adjacent-code residual audit

The normal pose portion of Directions and VerticalRadii is converted. These and Batch 29's three aggregates have mixed dispositions: converted native pose policies plus only the specified adjacent-code retention. Totals: 171 wholly converted, five mixed, 50 unchecked.

- Direction cases select named ten-way projectile directions, distinct upper/horizontal/lower turning restrictions, or shooting-disabled. Radius cases assign the physical half-height to each named pose, retaining all crouch, ball, spin, aiming-down, standing and special-pose differences. No pose data array remains; all existing current/prospective APIs and full byte-domain results remain exact.
- The complete native pose records are $00..FC at $91:B629..BE10. The byte APIs also preserve precisely three bounded adjacent records, FD/FE/FF, from `Calc_Xray_HDMADataTable_OffScreen`. These are not additional pose identities. The exact residuals are:

| Field | FD | FE | FF |
| --- | --- | --- | --- |
| Facing | BE11=08, PHP | BE19=29, AND immediate opcode | BE21=12, operand of LDA $12 |
| Movement | BE12=8B, PHB | BE1A=00, low operand of AND #FF00 | BE22=38, SEC |
| No-input | BE13=4B, PHK | BE1B=FF, high operand of AND #FF00 | BE23=E5, SBC direct-page opcode |
| Direction | BE14=AB, PLB | BE1C=85, STA direct-page opcode | BE24=14, operand of SBC $14 |
| Radius | BE17=8A, TXA | BE1F=18, operand of STY $18 | BE27=10, BPL opcode |

All addresses are bank $91. Approved narrow nonsense retention after independent coordinator comparison against pinned source and ROM: these fifteen values encode unrelated 65816 instructions and operands, not pose geometry or state policy. Producing them from managed pose logic would invent a relationship; preserving the observed native encoding is the compatibility contract. This retention does not exempt any ordinary pose field or any other code-adjacent payload.

- Bounded reachability: `SamusState.PoseMetadata` accepts only a byte pose, current or prospective. Facing, fallback, aim and radius preserve exactly this domain. Typed movement rejects FD=8B and FF=38 above the retail Special=1B dispatcher limit; FE=00 is Standing. No unbounded address evaluation or native memory emulation is introduced, and this does not claim that FD..FF are normal retail poses.
- Confirmation: Verification build 1432 warnings/zero errors; `--lookup-stream-1` passed. Existing source-guarded pose-dispatch checks confirm all 256 directions; collision fixture confirms all 256 prospective half-heights and actual RefreshCollisionRadii publication with horizontal radius five. Batch 29's 768 native dispatch comparisons and original ordered input/history checks remain passing. No fixture change was necessary.
## Batch 31: physical muzzle cases and pose origin corrections

One wholly converted definition and one mixed disposition added: 172 wholly converted, six mixed, 48 unchecked.

- Physical muzzle offsets now select X/Y together through twenty named standing/running and projectile-direction cases. Address resolution still occurs before row ownership, so directions A..F cross into the next native row, and running-Y overreads use the existing compiled cooldown definition. No replacement array or cache is used; lifecycle bits remain untouched.
- All 253 real pose Y corrections now use named pose cases. Native bytes remain zero-extended, including the $FC correction of drained poses, independent of signed editable graphics offsets.
- Narrow mixed retention approved after independent source/ROM review: only FD at $91:BE15=C2 (REP immediate opcode), FE at BE1D=16 (operand of STA $16), and FF at BE25=85 (STA direct-page opcode). These encode the unrelated X-ray HDMA routine and cannot sensibly be generated by managed pose-origin behavior. The byte-only current/prospective selector contract is unchanged. No other values are exempted.
- Evidence: pinned bank90 `ProjectileOriginOffsetsByDirection` projectile rows `$C204..C253`; bank91 PoseDefinitions Y offset column `$B62D + 8*pose`, with adjacent routine starting `$BE11`.
- Reproduced fixture prerequisites: existing VerifyPoseProjectileOrigin first failed because it supplied its varied graphics offsets only as ROM bytes, and then because its Grapple flare placement was likewise absent from installed resources. It now constructs 256 explicit installed catalogs from the existing small artwork fixture, each carrying the same varied graphics byte, and loads the supplied native Grapple flare placement. Every original physical-versus-visual, launch/cancellation and late-refresh assertion is retained. These are verifier setup corrections, not production behavior fixes.
- Confirmation: final Verification build 25 warnings/zero errors; `--lookup-stream-1` passed. Existing origin fixture verifies 647680 actual beam initializations, native Grapple endpoint versus editable visual-origin separation, all 256 correction values and restricted-pose cancellation. Existing projectile-origin fixture confirms forty native words, cross-row/cooldown nibble behavior and 343872 actual position initializations across native poses, word/lifecycle bits and coordinate wrap boundaries. No unrelated exploratory case was added.
## Batch 32: Speed Booster single reveal definition and inventory reconciliation

Two more definitions resolved: 174 wholly converted, six mixed, 46 unchecked. No additional retention exception.

- Source review established that BombReveal already calculates the physical type-B word with the named SpeedBoosterParent visual identity, then materializes a single count-one run and zero terminator only as an import/export DTO. RoomPlmSystem's actual reveal path directly consumes BombRevealWord and has no stored draw lookup. This closes the stale aggregate inventory entry without claiming a new physical behavior change.
- All now yields that sole calculated record instead of materializing a singleton collection. Its pointer, semantic ID, count, word, terminal offsets and exact rejection behavior remain unchanged.
- Evidence: pinned bank84 `DrawInst_BombReactionSpeedBlock` `$A4F3`: count 0001, level word B0B6, terminator 0000. The original bomb-reaction PLM consumes precisely this one block.
- Confirmation: Verification build 1432 warnings/zero errors; `--lookup-stream-1` passed. Existing native mapping fixture checks all draw fields, complete pointer domain and ordinal visual-ID rejection. Actual bombed Speed Booster PLM draws B0B6, deletes after its single frame and performs zero source reads; existing five boosted-contact/restoration assertions also remain passing. No fixture repair needed.
- Integration dependency: ResourceAudit/PlmProgressionClosedContractDefinitions.cs stores the source hash for SpeedBoosterBlockPlmDrawDefinitions.cs. Coordinator owns that audit and must refresh it with this change; it was not edited in this worktree.

Stream 1 rotation checkpoint: all work through Batch 32 is committed locally. Remaining 46 entries, including SPC pan, uneven animation timing, installed artwork payloads and unresolved program/radius definitions, stay required. Six mixed pose fields retain only their 18 independently reviewed adjacent instruction-byte observations; all 253 native pose policies for those fields are converted.
### Root integration: arm-cannon tile identities and selectors

Twelve native tile sources calculate from 9A00 through B000 in 0200-byte steps, including exact reverse membership. Four physical tile orientations select three successive opening frames, with the closed frame retaining its no-transfer sentinel. Native tile/palette/priority fields combine with direction-dependent horizontal/vertical reflections. Independently edited attributes and tile selectors remain sparse overrides; exported content identity and pixels are preserved.

All three entries (TileSourcePointers, attributes, tileSources) are complete; pose pointers, drawing bytes and tile pixels remain required. No retention exception is added. Root Verification build passed with 1445 warnings/zero errors; ResourceAudit passed with zero warnings/errors. `--lookup-arm-cannon-selectors` confirms twelve native list-derived sources, all 65536 reverse inputs, ten native attributes, forty frame selections, direction-specific edits and canonical hashes, all 384 transferred tile bytes, an independent pixel edit and index/bank/length/interior-address boundaries. Both artwork and VRAM-DMA source hashes refreshed. Inventory: 532 converted, 15 justified retained/mixed, 581 pending.

### Root correction: Owtch cadence remains required

Reopened `OwtchInstructionProgramDefinitions.Words`: the prior451507846 conversion calculates instruction structure but still returns the independently chosen eight-tick hold at three pose positions. That value has no accepted functional derivation or impossible/nonsense disposition. PresentationWords stays complete; naming/repeating the eight-tick cadence does not complete Words. No gameplay behavior changed by this status correction. Overall inventory is531 converted,15 justified retained/mixed,582 pending.

## Batch 36: arm-cannon calculated-default proof

Verification-only correction; totals remain176 wholly converted, six mixed,44 unchecked. No new production conversion or payload exemption is claimed.

- Previous confirmation compared loaded stock and edited outputs with native values but did not explicitly prove that sparse override storage was empty. Added direct comparisons of all ten calculated OBJ attributes and forty direction/frame defaults against original90:C791 and90:C7A5-selected operands, independently of catalog loading. Both stock exception dictionaries must contain zero entries.
- The same focused method retains the complete source membership/index domain, independent edits for every direction, canonical content identities, twelve native DMA mappings and isolated PNG edit checks. Pose pointers, drawing/placement bytes and tile pixels remain required independently.
- Confirmation: incremental Verification build25 warnings/zero errors; --lookup-stream-1-cannon-basis passed. No gameplay exploration was performed. This strengthens the migration proof without changing production behavior or source hashes.
- Integration note: earlier cannon production changes participate in both ResourceAudit/SamusArtworkClosedContractDefinitions.cs and ResourceAudit/VramDmaSourceContracts.cs; coordinator already refreshed those dependencies. This verifier-only batch requires neither hash to change.
