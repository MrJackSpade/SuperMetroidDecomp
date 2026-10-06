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

- [x] **ChargeFlarePlacementCatalog.offsets** ([L9](../csharp/src/SuperMetroid.Core/Assets/ChargeFlarePlacementCatalog.cs#L9)) - installed stock table. Original/default payload behind ChargeFlarePlacementCatalog.offsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

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

- [x] **EscapeTimerPresentation.frames** ([L10](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentation.cs#L10)) - installed stock table. Original/default payload behind EscapeTimerPresentation.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **EscapeTimerPresentation.anchors** ([L11](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentation.cs#L11)) - installed stock table. Original/default payload behind EscapeTimerPresentation.anchors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentationDefinitions.cs

- [x] **EscapeTimerPresentationDefinitions.DigitSpritemaps** ([L18](../csharp/src/SuperMetroid.Core/Assets/EscapeTimerPresentationDefinitions.cs#L18)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/EscapeTimerTileAtlas.cs

- [x] **EscapeTimerTileAtlas.transfer** - MIXED COMPLETE: calculated font relationships plus narrowly retained original typeface contours/metrics; see final timer disposition below.

### csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterPresentation.cs

- [x] **EscapeTypewriterPresentation.programs** ([L9](../csharp/src/SuperMetroid.Core/Assets/EscapeTypewriterPresentation.cs#L9)) - installed stock table. Original/default payload behind EscapeTypewriterPresentation.programs. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/EscapeDachoraInstructionProgramDefinitions.cs

- [x] **EscapeDachoraInstructionProgramDefinitions.Words** ([L40](../csharp/src/SuperMetroid.Core/Game/EscapeDachoraInstructionProgramDefinitions.cs#L40)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EscapeDachoraInstructionProgramDefinitions.PresentationWords** ([L163](../csharp/src/SuperMetroid.Core/Game/EscapeDachoraInstructionProgramDefinitions.cs#L163)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EscapeEtecoonDefinitions.cs

- [x] **EscapeEtecoonDefinitions.Initializations** ([L22](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonDefinitions.cs#L22)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs

- [x] **EscapeEtecoonInstructionProgramDefinitions.Words** ([L32](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs#L32)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **EscapeEtecoonInstructionProgramDefinitions.PresentationWords** ([L85](../csharp/src/SuperMetroid.Core/Game/EscapeEtecoonInstructionProgramDefinitions.cs#L85)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/EscapeTimer.cs

- [x] **EscapeTimer.CentisecondDecrements** ([L21](../csharp/src/SuperMetroid.Core/Game/EscapeTimer.cs#L21)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **MetroidInstructionProgramDefinitions.FrameDurations** ([L25](../csharp/src/SuperMetroid.Core/Game/MetroidInstructionProgramDefinitions.cs#L25)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
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

- [x] **OwtchInstructionProgramDefinitions.Words** ([L20](../csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **OwtchInstructionProgramDefinitions.PresentationWords** ([L36](../csharp/src/SuperMetroid.Core/Game/OwtchInstructionProgramDefinitions.cs#L36)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/PowampInstructionProgramDefinitions.cs

- [x] **PowampInstructionProgramDefinitions.Words** ([L41](../csharp/src/SuperMetroid.Core/Game/PowampInstructionProgramDefinitions.cs#L41)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **PowampInstructionProgramDefinitions.PresentationWords** ([L53](../csharp/src/SuperMetroid.Core/Game/PowampInstructionProgramDefinitions.cs#L53)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **SamusArmCannonArtworkCatalog.posePointers** ([L11](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L11)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.posePointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusArmCannonArtworkCatalog.drawingData** ([L12](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L12)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.drawingData. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusArmCannonArtworkCatalog.attributes** ([L13](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L13)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.attributes. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusArmCannonArtworkCatalog.tileSources** ([L14](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L14)) - installed stock table. Original/default payload behind SamusArmCannonArtworkCatalog.tileSources. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusArmCannonArtworkFormat.TileSourcePointers** ([L195](../csharp/src/SuperMetroid.Core/Assets/SamusArmCannonArtworkCatalog.cs#L195)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Assets/SamusAtmosphericArtworkCatalog.cs

- [x] **SamusAtmosphericArtworkCatalog.typeOne** ([L12](../csharp/src/SuperMetroid.Core/Assets/SamusAtmosphericArtworkCatalog.cs#L12)) - installed stock table. Original/default payload behind SamusAtmosphericArtworkCatalog.typeOne. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusAtmosphericArtworkCatalog.sharedTypeFour** ([L13](../csharp/src/SuperMetroid.Core/Assets/SamusAtmosphericArtworkCatalog.cs#L13)) - installed stock table. Original/default payload behind SamusAtmosphericArtworkCatalog.sharedTypeFour. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs

- [x] **SamusBodyArtworkCatalog.topPointers** ([L25](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L25)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.topPointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusBodyArtworkCatalog.bottomPointers** ([L26](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L26)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.bottomPointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusBodyArtworkCatalog.posePointers** ([L27](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L27)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.posePointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusBodyArtworkCatalog.graphicsYOffsets** ([L28](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L28)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.graphicsYOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusBodyArtworkCatalog.landingYOffsets** ([L29](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L29)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.landingYOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusBodyArtworkCatalog.postureYOffsets** ([L30](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L30)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.postureYOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusBodyArtworkCatalog.drainedYOffsets** ([L31](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L31)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.drainedYOffsets. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.frames** ([L32](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L32)) - installed stock table. Original/default payload behind SamusBodyArtworkCatalog.frames. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusBodyArtworkCatalog.top** ([L33](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L33)) - installed stock table. Top set/position to tile-definition mapping and metadata. The actual planar pixels are owned by SamusBodyTileDefinition.planar; the reverse address dictionary is an alias.
- [ ] **SamusBodyArtworkCatalog.bottom** ([L34](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L34)) - installed stock table. Bottom set/position to tile-definition mapping and metadata. The actual planar pixels are owned by SamusBodyTileDefinition.planar; the reverse address dictionary is an alias.
- [ ] **SamusBodyTileDefinition.planar** ([L250](../csharp/src/SuperMetroid.Core/Assets/SamusBodyArtworkCatalog.cs#L250)) - installed stock table. Original/default payload behind SamusBodyTileDefinition.planar. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs

- [x] **SamusDeathTileAtlas.planar** ([L9](../csharp/src/SuperMetroid.Core/Assets/SamusDeathTileAtlas.cs#L9)) - installed stock table. Original/default payload behind SamusDeathTileAtlas.planar. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs

- [x] **SamusHurtColorCatalog.hurt** ([L17](../csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs#L17)) - installed stock table. Original/default payload behind SamusHurtColorCatalog.hurt. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusHurtColorCatalog.intro** ([L18](../csharp/src/SuperMetroid.Core/Assets/SamusHurtColorCatalog.cs#L18)) - installed stock table. Original/default payload behind SamusHurtColorCatalog.intro. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs

- [x] **SamusSpritemapArtworkCatalog.topBases** ([L22](../csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs#L22)) - installed stock table. Original/default payload behind SamusSpritemapArtworkCatalog.topBases. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **SamusSpritemapArtworkCatalog.bottomBases** ([L23](../csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs#L23)) - installed stock table. Original/default payload behind SamusSpritemapArtworkCatalog.bottomBases. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusSpritemapArtworkCatalog.pointers** ([L24](../csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs#L24)) - installed stock table. Original/default payload behind SamusSpritemapArtworkCatalog.pointers. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [ ] **SamusSpritemapArtworkCatalog.definitions** ([L25](../csharp/src/SuperMetroid.Core/Assets/SamusSpritemapArtworkCatalog.cs#L25)) - installed stock table. Original/default payload behind SamusSpritemapArtworkCatalog.definitions. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs

- [x] **SamusVisorColorCatalog.colors** ([L10](../csharp/src/SuperMetroid.Core/Assets/SamusVisorColorCatalog.cs#L10)) - installed stock table. Original/default payload behind SamusVisorColorCatalog.colors. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

### csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs

- [x] **SamusAnimationDelayDefinitions.PosePointers** ([L20](../csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [ ] **SamusAnimationDelayDefinitions.DelayStreams** ([L88](../csharp/src/SuperMetroid.Core/Game/SamusAnimationDelayDefinitions.cs#L88)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusArmCannonDefinitions.cs

- [x] **SamusArmCannonDefinitions.OpenFlags** ([L15](../csharp/src/SuperMetroid.Core/Game/SamusArmCannonDefinitions.cs#L15)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusAtmosphericAnimationDefinitions.cs

- [x] **SamusAtmosphericAnimationDefinitions.FrameTimers** ([L24](../csharp/src/SuperMetroid.Core/Game/SamusAtmosphericAnimationDefinitions.cs#L24)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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
- [x] **SamusHudDefinitions.PostureObservationsByPose** ([L27](../csharp/src/SuperMetroid.Core/Game/SamusHudDefinitions.cs#L27)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

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

- [x] **SamusProjectileInstructionDefinitions.Words** ([L12](../csharp/src/SuperMetroid.Core/Game/SamusProjectileInstructionDefinitions.cs#L12)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs

- [x] **SamusProjectileMotionDefinitions.Missile** ([L14](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs#L14)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusProjectileMotionDefinitions.SuperMissile** ([L17](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs#L17)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.
- [x] **SamusProjectileMotionDefinitions.Beam** ([L20](../csharp/src/SuperMetroid.Core/Game/SamusProjectileMotionDefinitions.cs#L20)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileOriginDefinitions.cs

- [x] **SamusProjectileOriginDefinitions.Offsets** ([L8](../csharp/src/SuperMetroid.Core/Game/SamusProjectileOriginDefinitions.cs#L8)) - stored definition. Fixed stored mapping. Remove the lookup through calculation or meaningful cases, or establish the permitted impossible/nonsense exception. Cover all value columns; nested rows are part of this table definition.

### csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs

- [x] **SamusProjectileRadiusDefinitions.Radii** ([L18](../csharp/src/SuperMetroid.Core/Game/SamusProjectileRadiusDefinitions.cs#L18)) - factory-built stock table. Stored FrozenDictionary<int, ushort> initialized by Create(). Inspect that producer and all value fields; calculating then caching a replacement lookup does not complete conversion.

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

- [x] **WorldMapArtwork.background** ([L8](../csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs#L8)) - installed stock table. Original/default payload behind WorldMapArtwork.background. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.
- [x] **WorldMapArtwork.foreground** ([L8](../csharp/src/SuperMetroid.Core/Assets/WorldMapArtwork.cs#L8)) - installed stock table. Original/default payload behind WorldMapArtwork.foreground. Convert its stock mapping or record an impossible/nonsense justification; preserve independently editable replacement content. Serialization buffers, document properties and loader copies are not extra entries.

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


## Integrated beam and Grapple flare placement

ChargeFlarePlacementCatalog.offsets is complete (8a9a850a5 and c5da84b64). Named direction/turn cases replace stock beam origins; Grapple shares ordinary origins and resolves all low-nibble overreads through the exact adjacent physical-origin or calculated swing-selector owner. Explicit LoadGrapple import preserves resource identity and independently editable coordinates; both stock catalogs store zero overrides. No retention exception is used.

Root confirmation: Verification build1445 warnings/zero errors;128 direct native words,128 independent axis edits,bounds and four actual beam OAM fixtures with reads unavailable pass. ResourceAudit builds with zero warnings/errors; reviewed source dependencies include bounded physical-origin owners. Inventory550 converted/17 retained-mixed/561 pending. Separate sprite and tile artwork remains required.


## Integrated escape timer composition and anchors

EscapeTimerPresentation.frames and anchors are converted (24e5e2eaa). Decimal glyphs use two eight-pixel parts and arithmetic font tile selection; the TIME label consumes the three remaining label tiles plus separators. Three two-digit groups and two separator cells determine centered anchors. The stock dictionaries are empty; immutable calculated parts preserve native drawing order. Independently edited parts, anchors, spacing and palette remain exact overrides.

Root confirmation: Verification build 1445 warnings/zero errors; 25 native parts, four native anchors, ten native-vs-installed decimal OAM fixtures, eleven frame edits, four anchor edits with spacing/palette changes, and bounds pass. ResourceAudit source closure includes the immutable factory and font extent. Font ink remains independently required in EscapeTimerTileAtlas.transfer; no artwork or timing exemption. Inventory 552 converted/17 retained-mixed/559 pending.


## Integrated Powamp program structure

PowampInstructionProgramDefinitions.PresentationWords is converted (c6eae9881): two three-frame body loops and two three-frame sleeping transitions determine all twelve visual operand locations. Control addresses/opcodes/targets now calculate from the same native program structure. Words remains pending: exact holds 5, 9, 1, 6 and 160 are independent required inputs, with no accepted exemption.

Root Verification build 1445 warnings/zero errors. After terminal build completion, --lookup-stream-1-powamp-programs passed independent native traversal of 18 controls and 12 visual operands, native values/order, exact byte-domain classification, wrong-bank and bounds rejection. An earlier premature no-build invocation used the old binary and reached an unrelated legacy missing-palette fixture; it is not passing evidence and no changes were made for that fixture. No gameplay or lifecycle equivalence claim. Inventory 554 converted/17 retained-mixed/557 pending.


## Integrated escape Dachora program structure

EscapeDachoraInstructionProgramDefinitions.PresentationWords is converted (ecb3b4450). Six-frame directional runs at two tide states and the initial/departure frames determine all 43 visual operand addresses. Calculated controls preserve timer setup, movement callbacks, acid/escape branches, loop targets and departure progression. Words remains pending nine independent repeat/hold/acceleration inputs; no retention exception.

Root Verification build 1445 warnings/zero errors; --lookup-stream-1-dachora-programs passes independent native traversal of 119 controls and 43 visual operands, every native callback/target/hold, exact order/read domains/full byte classification and bank/index/address rejection. No gameplay or lifecycle equivalence claim. Inventory 555 converted/17 retained-mixed/556 pending.


## Integrated HUD posture policy and bounded instruction observations

SamusHudDefinitions.PostureObservationsByPose is mixed complete (e68a6acc9). Twelve authored flags at90:DDAA-DDB5 become named pose cases: crouch/stand0; morph/unmorph and unused transitions1. Only207 bounded adjacent-code observations remain: DD75-DDA9(53bytes),DDB6-DE4F(154bytes). Root inspected native DD8C bounds/subtract35/index/mask and managed flag==0||grappleActive. First byte is the low operand of DD74 LDA; the final byte is DE4E BMI displacement, not the following instruction.

These retained bytes encode unrelated native instructions,addresses and branch displacements rather than posture policy. Reconstructing native machine code to synthesize accidental table observations would be nonsense; it would merely re-encode this bounded compatibility content. The exception excludes all12 real flags and every byte outside those exact windows; it does not authorize unbounded memory emulation.

Root Verification build1445 warnings/zero errors; --lookup-stream-1-hud-posture passes all219 native observations,512 actual admission-helper results and DB-FF observation rejection. Inventory556 converted/20 retained-mixed/552 pending.


## Integrated named Samus animation selection

SamusAnimationDelayDefinitions.PosePointers is converted (8887f6556). All253 real poses select127 named native animation identities through semantic cases. FD-FF pointer aliases derive from the actual first six delay-stream bytes, preserving the existing low-bank mutable WRAM behavior. There is no cached pointer array or unrelated instruction-byte exception. DelayStreams remains required for every independent timing/command payload.

Root Verification build1445 warnings/zero errors; --lookup-stream-1-animation-pointers confirms pinned ROM identity,256native pointers,every bounded definition byte,actual animation initialization,immutable-domain rejection and all three aliases under six successive WRAM mutations. Inventory557 converted/20 retained-mixed/551 pending.

## Integrated cannon pose selection

SamusArmCannonArtworkCatalog.posePointers is converted (1b62d9f1a). Named cases map253 real poses to57 native drawing descriptors. Only independent supplied pointer differences are stored; canonical content hashing and FD-FF rejection remain intact. Drawing coordinates and pixel artwork remain pending.

Root Verification build1445 warnings/zero errors; --lookup-stream-1-cannon-poses passes253 native mappings, zero stock overrides,253 independent pointer edits and canonical hashes, unaffected poses and FD-FF bounds. ResourceAudit build zero warnings/errors. Inventory558 converted/20 retained-mixed/550 pending.
## Integrated cannon drawing controls and repeated origins

Partial conversion from2d31d7a,5bea8d416 and ab9d94a8c:130 descriptor direction/mode bytes use named cases;24 adjacent bytes reuse the existing SBA power-bomb cost calculation. Constant-origin descriptors and fixed horizontal running/moonwalking origins derive216 repeated coordinate bytes from their own earlier basis. Independent edits preserve all608 selected bytes without coupling frames.

Exactly238 independent coordinate bytes remain required; drawingData stays unchecked. This is no artwork or timing exemption. Root Verification build1230 warnings/zero errors; --lookup-stream-1-cannon-drawing passes608 native outputs, exact basis/override membership,608 isolated edits, bounds and the253-pose canonical-hash checks. ResourceAudit build0/0. Inventory remains558 converted/20 retained-mixed/550 pending.
## Integrated atmospheric OBJ identity progression

Partial conversion4328cc579: both four-frame small-OBJ lists calculate consecutive tile identities using the native nine-bit OBJ format. Read-only views and independent full-word overrides preserve supplied artwork and canonical hashes. At that checkpoint palette routing and priority remained unresolved. The original entries contain only eight attribute words; pixel artwork is outside their schema. Batch91 below completes both entries.

Root Verification build1445 warnings/zero errors; --lookup-stream-1-atmospheric-attributes passes eight native words, zero stock overrides, eight arbitrary full-word edits, hashes and input isolation, plus16 actual authored OAM frames and the type2 live-WRAM boundary. ResourceAudit0/0. Inventory unchanged558 converted/20 retained-mixed/550 pending.
## Integrated escape warning program selection and layout

Partial conversion c962486be: named Ceres/Zebes selection replaces the program dictionary; calculated line views preserve independent text/destination differences and added/removed lines. Retention covers only the five exact English warning strings at A6:C458,C472,C488,C4A4,C4B6. Their wording is narrative content; generating different text from arithmetic would not preserve it. Glyph identities and geometry are separate.

The exact layout/grouping review is completed in Batch84 below. Character delay2 is separate runtime behavior outside the original programs table schema; it is not a retention exemption or a pending field of this entry. Root Verification build1445 warnings/zero errors; --lookup-stream-1-escape-text passes five native lines, zero stock overrides, independent edits/counts/bounds and271 actual production calls compared to native command parsing for timing,destinations,clicks,glyphs and VRAM with cartridge reads denied. ResourceAudit0/0. Inventory unchanged558 converted/20 retained-mixed/550 pending.
## Integrated narrow Owtch visual cadence disposition

OwtchInstructionProgramDefinitions.Words is mixed complete (dbe23c0e0): structure and selectors calculate; only CyclicVisualHold8 remains as chosen visual playback content. Root inspected the three native rendered poses, A2:A3AD/A3BF loops, A579 shot gate, A0:D03F header and managed movement/collision consumers. The same spiked shell has shifted lower pink/purple pixels, with opposite direction playback orders. Each map is one16x16 part at(-8,-8). Direction callbacks precede the cycles; the cycles contain only timed visuals and Goto.

Radii8/8 are fixed, every map is nonempty, ordinary touch/projectiles use radius boxes, and damage gating uses behavior state. Movement uses independently selected velocity. The exact eight-tick visual tempo is an authored performance, not a physical quantity derivable from these consumers; an arithmetic encoding of that choice would merely disguise it. This exemption excludes reset1,movement,radii,selectors and pixel artwork.

Root Verification build1445 warnings/zero errors; focused --lookup-stream-1-owtch-cadence passes12 native words,6 selectors,both actual25-call loops,direction callbacks,Goto cursors,all three frames,bounds and forbidden source reads. No whole-game claim. Inventory558 converted/21 retained-mixed/549 pending.
## Integrated calculated visor color phases

Partial dcb13a1a2 with dbe23c0e0's narrow ink documentation: the first three RGB5 colors interpolate from3BE0 toward white with nearest half-up rounding; the final three subtract5 per channel from43FF. Stock stores no overrides; supplied independent words remain exact. The only accepted ink is3BE0 at9B:A3C0, identical to the already-reviewed normal-suit categorical visor ink at9408/9528/9808 (OBJ palette4/color4). Root checked those native words and fixed-slot consumers against the earlier SamusSuitColorCatalog ink disposition. Independent catalogs are not coupled.

FullBeamStart43FF and DarkeningStep5 remain required; colors stays unchecked. Existing native-comparison fixtures lacked installed visor content after the earlier resource boundary change; their two baseline states now receive independently imported native colors, with no assertion relaxation. Root Verification build1445 warnings/zero errors; --lookup-stream-1-visor-colors passes six direct native colors,exact basis,zero stock overrides,six independent edits,bounds and actual guarded room/X-ray cycles. ResourceAudit0/0. Inventory unchanged558 converted/21 retained-mixed/549 pending.
## Integrated repeated and reflected cannon coordinates

Partial ad106f3be,16e721d47,8a95f8158: repeated running/moonwalk Y cycles, matching paired-facing Y profiles and29 exact mirrored X origins reduce required coordinate basis238→135. Reflection derives x'=-x-8 from the native single small8px OBJ footprint. Every dependency points to an earlier byte; import compares against supplied source values so independent source/derived edits remain independent. Native downward-vertical00/F7 origins and unmatched Y profiles are preserved as required asymmetries.

DrawingData remains unchecked; no coordinate/artwork exemption. Root Verification build1446 warnings/zero errors; --lookup-stream-1-cannon-drawing passes130control bytes,24cost aliases,290shared coordinates,29reflections,exact135basis/override membership,all608 native outputs and608 isolated byte edits.253pose mappings/edits/canonical hashes and all bounds also pass. ResourceAudit0/0. Inventory unchanged558 converted/21 retained-mixed/549 pending.
## Integrated projectile instruction mechanics

Partial conversion from 3b7c13cde through 7fbc681bd: calculated timed-record geometry, trail phases, semantic one-shot/cycle selection and loop targets replace all1,816 stored mechanics words across805 timed projectile records. The original dictionary and factory are removed. Independent hold durations, phase counts and loop-entry choices remain REQUIRED; Words stays unchecked. This grants no timing or artwork exemption.

Root Verification build1,450 warnings/zero errors. --lookup-stream-1-power-programs confirms all1,816 direct native words, exact byte domains, terminal targets, rejection boundaries and absence of stored fallbacks. --lookup-stream2-projectile-identity-geometry also passes805 native selectors,417 extracted OAM draws,48 startup records,independent edits and bounds. Inventory remains570 converted/21 retained-mixed/537 pending.

## Integrated Samus intro channels and hurt whitening

Partial2685cb494 calculates intro red=green/blue=max(4,red-2),then hurt RGB=floor((2*intro+5*31)/7) for slots1..15. Native9B:A380-A3BE confirms all30relationships. Fifteen source red levels,their slot roles,two zero-slot words and all four tint/blend parameters remain REQUIRED; both aggregates stay unchecked. Independent RGB edits across either palette preserve supplied values through exact overrides.

Root build1451warnings/0errors;32native colors,30relations,exact15level basis,zero stock overrides,96independent RGB edits/exception membership and guarded ordinary/cinematic CGRAM/action/timer cycles pass. ResourceAudit0/0 with helper dependency and catalog hash refreshed. Inventory587converted/21retained-mixed/520pending unchanged.

## Integrated named Samus OAM pose-base dispatch

Conversionca882de7b completes topBases/bottomBases through253 named SamusPoseId cases per half selecting native frame-sequence identities. Stock stores zero base overrides; independently supplied bases remain exact. FD-FF retain their prior installed-domain rejection. Public base spans are uncached snapshots for transport/hash,not replacement lookup caches. All2096pointer payloads and OBJ parts remain REQUIRED.

Root build1451warnings/0errors;506native cases,506independent base edits,actual supplied marker-payload selection,zero-pointer routing,exact old canonical hashes and bounds pass. ResourceAudit0/0; source helper and SamusPoseId dependencies pinned. Inventory589converted/21retained-mixed/518pending.

## Integrated Samus body pointer,offset and component relationships

Combined reviewed chain9ec9cd7cb,520c810c4,7d4a315d5,d08b77115,45f14dcc1,1379438e9,44043f193,44b30f26c completes four entries:topPointers,bottomPointers,posePointers,graphicsYOffsets. Named allocation/pose cases preserve24set identities and253real poses; graphics origins share the existing real-pose operation while independent visual edits remain isolated from physics. FD-FF is excluded.

Landing/posture derive20facing aliases but retain9/12inputs,including adjacentPLB byte. Frame relationships calculate268upper/lower components across X-ray,moving/aiming,crouch,jump/fall lists;4304component bytes and selected counts/order/artwork remain REQUIRED. These aggregates stay unchecked. Canonical hashing uses uncached selected snapshots and preserves independent source/derived edits.

Root build1452warnings/0errors;41native offset bytes/41edits/unaligned reads;253graphics values/edits/physics isolation;24allocation pointers/valid edits/runtime addresses;253pose pointers/edits;268native component aliases/exact basis/688component edits/actual26pose selections all pass with original stock/edited hashes and bounds. ResourceAudit0/0; all changed dependencies pinned. Inventory593converted/21retained-mixed/514pending.


### Independent timer pen identity review

Approved only digit/separator fill pen1, TIME fill pen2 and outline pen14 as selected categorical paint-role identities. Root viewed the native glyph-role export and independently decoded B0:C000 pixel(2,1)=1, pixel(1,0)=14 and B0:C2C0 pixel(0,1)=2. Native80:9FCA chooses OBJ palette5; the production renderer treats0 as transparent and selects CGRAM128+palette*16+pen (209/210/222). Geometry cannot determine these particular chosen slot numbers; a numerical generator would merely restate the art choice. No RGB, footprint, deviation, width, spacing, glyph-one metric/policy or aggregate exemption follows. Latest worker23bdd65e9 leaves416footprint positions/four deviations plus those parameters required; its production integration is still queued. Master595/21/512 unchanged.


## Batch 81: timer font mixed disposition complete

EscapeTimerTileAtlas.transfer is now MIXED COMPLETE. Totals189 wholly converted / nine mixed /28 unchecked. Scope is only the800 planar bytes B0:C000..C31F, not RGB palettes, countdown timing or other artwork.

- Calculated output remains836 outline pixels,312 repeated/reflected pixels and173 geometric basis fill sites. Retained original typeface membership is275 fill sites and their specified blank complement: digit3=47 (B0:C060/C1A0), digit4=45 (C080/C1C0), digit5=51 (C0A0/C1E0), digit6 upper=31 (C0C0), digit7 middle rows3..9=14 (C0E0/C220), digit9=55 (C120/C260), single separator=6 (C280), TIME M=26 (label columns10..16,C2E0/C300).
- Four retained contour-edge decisions: PNG index1161(tile20,x1,y5)=0 and1163(tile20,x3,y5)=0 carve separator outline corners;189(tile23,x5,y0)=14 bridges the label top at M center;999(tile24,x7,y4)=0 trims E's right margin. These are precise paint choices, not an exemption inferred from a failed dilation.
- Eighteen reviewed typography parameters: outline width1; separator advance3; digit stroke2; upper-eight fill end8; two cap end5/stem end7/slope2; seven terminal start10/X2; label stem2/T width6/I origin7/E origin18/E width5/top1/height5/middle inset1; bottom padding1. Reviewed policies are the zero/eight rounded corners and rectangular counters, one's centered stem/flag/foot, two's selected cap/stem/band/foot joins, seven's selected cap/terminal domains, T's centered stem and E's three-arm design.
- Root independently reviewed the native-role image csharp/test-temp/timer-native-glyph-roles.png, original80:9F95..9FB0 BCD nibble selection,80:9FE8..A07A tile composition and80:9FCA palette5 routing. Managed EscapeTimer.cs247..274 computes countdown independently; EscapeTimerPresentation.cs42..44 selects digit identities; EscapeTimerPresentationDefinitions.cs75 derives the selected glyph tiles. The atlas only emits planar artwork. The extractor supplies a diagnostic PNG palette; game RGB is not this entry's payload.
- Narrow nonsense rationale: a numeric digit or glyph transport cell cannot determine this selected bitmap typeface. Its open four, asymmetrical bowls, seven contour and M joins are specific font-design choices; another contour can depict the same digit. Further geometric encoding can describe those choices but still requires the same selected typography/outline inputs. This approval covers only the exact remaining font design listed above, not a general artwork exemption. Existing useful constructions remain calculated.
- Root explicitly approved this complete bounded mixed disposition. Helper names/comments now identify Chosen parameters instead of falsely pending REQUIRED inputs. Independent PNG edits remain supported at every pixel, including calculated/aliased pixels.
- Final confirmation: Verification build1,434 warnings/zero errors; --lookup-stream-1-timer-glyphs passes624 direct native default-mask positions, zero stock fill overrides, exact275-site/four-edge retained basis, all1,600 independent PNG edits, all800 original planar bytes and actual typed/native two-page queue/VRAM upload. One final XML-only correction clarifies the approved outline width versus excluded RGBs; executable tokens unchanged after build. Root refreshed AreaMap, Cinematic, VramDmaPresentation and VramDmaSource contracts.
Root integration confirmation:1452warnings/0errors;Audit0/0;all listed timer checks pass. Master596converted/22retained-mixed/510pending.

## Batch 82: exact timer cadence transcription correction

EscapeTimer.CentisecondDecrements remains REQUIRED pending complete cadence disposition; totals unchanged189 converted/nine mixed/28 unchecked. This is a reproduced correction within the owned table, not a lookup completion.

- Pinned80:9F68/9F69 and retail-ROM offsets1F68/1F69 are02/01; managed indices124/125 were01/02. Corrected only these two transposed entries.
- Exact pre-fix reproduction --lookup-stream-1-timer-cadence starts00:01.50 in RunningInPlace and invokes actual Process at NMI124 and125 independently. It failed:124 expects48 actual49;125 expects49 actual48. The fixture reads expected corrections directly from the SHA-pinned ROM. After correction both pass; whole seconds/minutes and non-expiration are unchanged.
- Effect: the pair's total decrement remains3 centiseconds, but the earlier frame now removes2 rather than1; at an expiration boundary this restores which exact NMI reaches zero. No gameplay or unrelated test search.
- Native80:9EAB..9EB4 selects global NMI low byte masked7F, then decimal SBC. The explicit128-frame period totals213 centiseconds (43 one-unit and85 two-unit corrections). A finite source-only comparison against a uniform213/128 accumulator with initial phase0,64,127 differs at60,68,62 positions respectively; this excludes those ordinary floor/nearest/ceil choices only, not all possible constructions. Nonuniformity is not a retention justification. Full native sequence and consumer packet sent to coordinator for independent functional phase review.
- Verification build1,434 warnings/zero errors; exact two-index focused reproduction now passes. No ResourceAudit source-hash references to EscapeTimer.cs were found. Local Program.cs adds only the focused command; coordinator merges shared wiring.

## Batch 83: complete native countdown quantizer

EscapeTimer.CentisecondDecrements is CONVERTED, with no retained sample/phase exception. Totals190 wholly converted / nine mixed /27 unchecked.

- New domain catalog EscapeTimerCadenceDefinitions replaces all128 stored corrections with hierarchical integer scheduling. Period128 follows native AND7F. Period centiseconds derive by nearest-integer128*100/60=213, with explicit centisecond and nominal-frame-rate units.
- Cumulative floor distributes213 over four32-frame quarters as53,53,53,54. Splitting each quarter into two16-frame halves, odd unit first, yields budgets27,26,27,26,27,26,27,27. Each half differences consecutive rational-accumulator totals; budget27 uses preload1 and budget26 uses preload4 in the16-unit denominator.
- These are bounded quantizer parameters with independent meanings: time rate, subdivision, odd-unit ordering and initial fractional accumulator state. There are no frame-index exceptions, sampled positions, polynomial interpolation or replacement lookup arrays. Root independently reviewed and approved conversion, not retention. This proves an exact functional replacement, not the identity of an unknown historical generation tool.
- Preserves NMI low-seven-bit selection and actual BCD countdown. Incorporates Batch82's independently reproduced correction at indices124/125. At00:00.02, index124 expires and125 leaves00:00.01, matching native2/1.
- Focused --lookup-stream-1-timer-cadence compares all128 independent native bytes,384 phase/mask observations,128 actual Process updates and the two exact expiration cases. New helper exclusively granted by coordinator; no other field or timer behavior changed.
- Final confirmation: build1,434 warnings/zero errors; focused cadence command passes all stated native/Process/mask/expiration assertions. No remaining cadence payload or phase obligation; coordinator records the new catalog grant.

Root integration:1452warnings/zero errors; full focused cadence/native/Process/expiry confirmation passed. Master597converted/24retained-mixed/507pending.

## Batch 84: complete escape-warning text composition

EscapeTypewriterPresentation.programs is MIXED COMPLETE. Totals190 wholly converted / ten mixed /26 unchecked.

- Original scope proven from pre-conversion c962486be^: the programs dictionary stores Program(Id,SourceAddress,Lines), with each line(Destination,Text). There is no delay field. Corrected the prior report's mistaken CharacterDelayFrames2 completion obligation; the separate runtime delay is unchanged and receives no exemption. Real completion still gates Ceres self-destruct handoff and Mother Brain's start-timer phase, so timing is not described as irrelevant.
- Existing calculation selects Ceres/Zebes semantically, derives native source/BG identities, computes destination=tilemap+(firstRow+line*leading)*32+column and exposes an immutable calculated sequence. Stock has zero stored line overrides; independent text/destination/line-count edits remain intact.
- Root independently reviewed original schema, semantic resolver and native A6:C454..C4C9. Prior approved exact lexical content is extended only to its exact five-line grouping/breaks and shared warning-block column5,row8,two-row leading. Native destinations5105/5145/5185 and4905/4945 establish those choices. A different alignment or leading changes the composed warning; no game-state/content quantity selects this specific typography. This is a narrow nonsense disposition for chosen text composition, not a generic UI/art exception.
- Ceres groups SELF DESTRUCT SEQUENCE / ACTIVATED EVACUATE / COLONY IMMEDIATELY; Zebes groups TIME BOMB SET! / ESCAPE IMMEDIATELY!. Glyph pixels and other artwork remain separate. Runtime CharacterDelayFrames2 remains unchanged outside this inventory entry.
- Helper XML now records reviewed layout/grouping choices and correct delay scope. The existing focused native-ROM control/character interpreter compares all271 actual installed Step calls, exact completion/destination/delay/countdown/glyph count/click and every VRAM byte; actual side denies cartridge reads. It also checks five extracted native lines, zero stock overrides, independent edits, added/shortened documents and bounds.
- Source hash dependency: EscapeTypewriterDefinitions.cs in TextAndMapClosedContractDefinitions.cs (coordinator refresh). No production executable behavior changes in this disposition checkpoint.
- Final confirmation: Verification build1,434 warnings/zero errors; --lookup-stream-1-escape-text passes all five native lines, zero stock overrides, independent content/line-count edits and271 actual native-oracle calls.

Root five-line/zero-override/edit/271-call confirmation passed. This checkpoint changes only XML documentation and accounting, with unchanged executable code. Master597converted/26retained-mixed/505pending.

## Batch 85: complete atmospheric exposure choreography

SamusAtmosphericAnimationDefinitions.FrameTimers is MIXED COMPLETE. Totals190 wholly converted / eleven mixed /25 unchecked.

- Preserve semantic uniform holds for footstep types1/2(3),lava-spray4(2),bubbles5(5), and dust6/7 progression3+frame. Narrow retained scalar choices are those three rates plus dust start3/increment1. Exact diving holds at90:8BB5..8BC5 are2,2,3,3,3,5,5,6,7. No failed numeric fit is a retention rationale.
- Root independently reviewed native90:8A65..8AAC reload-before-advance,90:8B93..8BEF domains, managed movement/admission consumers and the source-only nine-frame strip csharp/test-temp/atmospheric-diving-native-nine.png. Source render uses92:83AB..83BB OAM identities, standard OBJ tiles9A:D200+tile*32/native82:8305..831E transfer, original part size/flip/order and initial sprite palette (not captured room CGRAM).
- Diving OAM identities92:D858,D869,D87A,D895,D8B0,D8D5,D8F0,D906,D912 draw ripple, rising/breaking jets and falling remnants. Geometry is discrete animation art; no fluid or damage equation generates its selected exposures. Narrow nonsense disposition covers only these exact five exposure parameters and nine-frame timing choreography. Uniform/progressive calculations remain; frame counts,geometry,damage,other timers/art are excluded.
- SamusAtmosphericEffectsState.UpdateAndDraw94..163 decrements/reloads OLD frame, advances/deletes, pins divingY to live water surface and moves spray/dust slots. Final diving7 remains reloaded into the deleted slot's stale timer; delayed-start reads remain exact. DrawSamusTableSpritemap211..239 emits the selected artwork.
- Consequences are explicitly preserved: SamusLiquidPhysicsState.TrySpawnAirBubbles658..681 gates slot2 plus NMI128-boundary and consumes sound RNG on admission; TrySpawnLavaSurfaceSpray683..710 gates occupied spray and replenishes it. SpawnWaterSplash636..656 initializes splash slots. Independent damage uses SamusLiquidDamageDefinitions at346..347 and AccumulatePeriodicDamage627. No claim that animation timing is consequence-free.
- Existing direct-native comparison covers all37 holds/seven domains and rejection bounds;74 actual UpdateAndDraw expiry/delayed-start cases verify old-frame reload, terminal deletion and delay behavior. New local command --lookup-stream-1-atmospheric-cadence runs only this confirmation, not broader damage/gameplay checks.
- Final confirmation: build1,434 warnings/zero errors; focused atmospheric command passes all37 native holds, seven domains,74 actual expiry/delayed-start cases and rejection contracts. No ResourceAudit source-hash references to SamusAtmosphericAnimationDefinitions.cs were found. Local Program.cs wiring is one focused command for coordinator merge.

Root1452warnings/zero errors;focused37native holds/seven domains/74actual expiry-delayed-start cases passed. Master598converted/28retained-mixed/502pending.

## Batch 86: complete ordinary-Metroid pulse exposure

MetroidInstructionProgramDefinitions.FrameDurations is MIXED COMPLETE. Totals190 wholly converted / twelve mixed /24 unchecked.

- Replace the five-element duration lookup with named contracted-rest,expanded-rest,brightening,peak,return-to-expanded stages. Three rest stages each take one beat; brightening and peak share another beat. Retain only selected beat16 and brightening split6; calculate peak10=16-6. No duplicate magnitude or per-index correction table remains.
- Native A3:E9CF..EA1B repeats the pulse four times while chasing, EA25..EA35 once while draining. Spritemaps F10D,F137,F157,F181,F137 show internal contraction/red-to-white pulsing; source-only csharp/test-temp/metroid-insides-native-five.png uses exact AE:9000 tiles andA3:E9AF palette. Diagnostic labels were regenerated clearly from ROM as16,16,6,10,16.
- Root independently viewed the strip and original records/callback/drain consumers, approving only beat16/split6 as selected drawn-pulse and synchronized-cry choreography. No physical/contact/drain quantity determines that selected exposure split. Native A0:DD7F radii remain10/10; outer shell/electricity are separate objects initializedA3:EA6F/EA93. No damage,artwork or other sound-choice exemption.
- Managed RoomEnemySystem.Metroid.ResolveMetroidTouch/DrainSamusWithMetroid326..384 drains via independent touch/state/suit-dependent fixed-point accumulation. RoomEnemySystem.cs3006..3018 preserves the loop sound: draining sound50 after64ticks; chasing RNG advances once for its cry after four cycles256ticks. All these exact timing consequences remain.
- Focused --lookup-stream-1-metroid-pulse compares25 independent native duration/visual records and runs320 actual scheduler exposure ticks without resetting timers between records. It asserts every decrement, held sprite, pointer order, no premature sound/RNG, both callback boundaries and first-record reloads; compiled mechanics read guard stays zero.
- Verification build1,434 warnings/zero errors and focused command pass. Final changes after build are XML disposition wording only; executable tokens unchanged. Local Program.cs adds one focused command; coordinator merges wiring.
- No ResourceAudit source-hash references to MetroidInstructionProgramDefinitions.cs were found.

Root integrated confirmation:1452warnings/zero errors;25native records,320actual exposure ticks,two callback boundaries and exact sound/RNG ownership pass. Master598converted/30retained-mixed/500pending.

## Batch 87: complete Powamp scripted exposure

PowampInstructionProgramDefinitions.Words is MIXED COMPLETE. Totals190 wholly converted / thirteen mixed /23 unchecked.

- Existing semantic program construction calculates all18 mechanics words: two three-pose body loops with Goto and two three-pose balloon transitions ending Sleep. Exact five remaining inputs are fast cheek5 atA8:C163/C167/C16B, slow cheek9 atC173/C177/C17B, initial balloon1 atC183/C191, intermediate6 atC187/C195 and terminal160 atC18B/C199.
- Root viewed source-only csharp/test-temp/powamp-native-six-poses.png and reviewed native records plus actual alignment consumer. Strip uses exactA8:C675/C67C/C683 cheek poses andC68A/C691/C698 small/medium/large balloon, nativeB1:CC00 tiles/A8:C143 palette. Approved narrow nonsense retention covers only these selected scripted cheek/inflation/deflation exposures and terminal interpreter-delay content: geometry/state does not specify the selected performance tempo. This is not a visual-only exemption or failed-fit argument.
- RoomEnemySystem.Powamp.AlignPowampBalloonY uses the current timed pose to choose exact nativeA8:C277/C27D Y offsets. Therefore the intermediate exposure changes balloon collision-center timing; death-stage reversal also reads the cursor. These consequences remain exact. Independent AI transition10, rise velocity, motion, geometry and artwork receive no exemption.
- Terminal160 leaves the current cursor already at Sleep; nativeA8:812F..8139 and managed ProcessInstructions leave it there when Sleep executes. The sprite/alignment stay terminal before/after, while exact timer0 thenFFFF bookkeeping remains. No arbitrary removal of this observable native state.
- Focused --lookup-stream-1-powamp-cadence compares all12 native duration/visual records, runs376 actual exposure ticks without forcing timer reloads, confirms15/27-tick body loops and167-tick transitions, both actual Goto/Sleep boundaries, every balloon Y position against independent native offset words, terminal alignment and timer underflow. Compiled mechanics read guard remains zero.
- Verification build25 warnings/zero errors (incremental); focused command passes. Subsequent production edits are XML/comments only, executable tokens unchanged. Local Program.cs adds only the focused command. No wider gameplay checks or AI changes.

Root1452 warnings/0 errors;12 native holds,376 uninterrupted scheduler ticks,Goto/Sleep boundaries and native balloon alignment pass. Master598 converted/33 retained-mixed/497 pending.

## Batch 88 checkpoint: exact escape-Dachora choreography confirmation

EscapeDachoraInstructionProgramDefinitions.Words remains unchecked pending coordinator review of its complete nine-parameter choreography packet. No production disposition has changed; totals remain190 converted / thirteen mixed /23 unchecked.

- Exact nativeB3:E964..EA32 uses five six-pose laps in each direction, low-tide hold3/high-tide hold2. Each completed pose callsB3:EAC9/EAD7 to changeX by6, so a direction run travels180pixels. Native mainEB1A returns; there is no separate velocity integrator that supplies this timing.
- DepartureEA34 holds30 beforeEA38 launch pause90, then5,5,4,4,4,3,3,3,2,2,2 and six1-tick poses loopingEA80. Existing hold calculation is max(1,5-floor(frame/3)), with a semantic launch-pause case. Selected initial cadence, decrement, half-gait subdivision, terminal every-update policy, pauses, pacing tempos and lap count are all explicit review inputs; none is silently exempt or described as visual-only.
- Complete review packet sent to coordinator, proposing calculated half-gait countRunFrames/2 and minimum scheduler cadence1 while retaining their selected script policies separately. No approval assumed. Native event/acid branch times and actual displacement are consequences that must stay exact.
- New focused --lookup-stream-1-escape-dachora-cadence independently compares all119 native mechanics words and runs139 timed-record visits/463 actual uninterrupted exposure ticks:180low/120high complete round trips and163departure. Every native sprite/cursor/decrement/X coordinate, both reversal boundaries and maximum-speed reload agree. No forced timer progression. Compiled-mechanics read guard stays zero.
- Final incremental build25 warnings/zero errors; focused command passes. No ResourceAudit source-hash references found. Only verifier/local command/report change at this checkpoint; production untouched.
## Batch 89: complete escape-Dachora movement choreography

EscapeDachoraInstructionProgramDefinitions.Words is MIXED COMPLETE. Totals190 wholly converted / fourteen mixed /22 unchecked. This final disposition supersedes the pending review in Batch88.

- Root independently reviewed pinnedB3:E964..EAA6 complete instruction programs andEAA8..EB1A event/acid branches, movement callbacks, initializer and RTL main. Narrow retained choices are five laps; low/high holds3/2; turn30; launch pause90; initial moving hold5; decrement1; the selected half-gait acceleration policy; and every-update terminal cadence. These compose this specific NPC's movement performance; a physics/state relationship does not select the pacing/departure script. No generic animation exemption.
- Derive AccelerationFramesPerStep=RunFrames/2 instead of storing a duplicate three. The two policies remain explicitly selected choreography, not claimed to be forced by physics. All control ordering, native allocation identities, repeated records, clamped cadence reduction and Goto targets remain calculated.
- Exact displacement and observation timing remain material: every completed running pose invokes X±6; five laps travel180pixels before reversal. Event/acid tests occur after full gait loops. Departure waits and acceleration control when each displacement occurs. Independent movement-step, geometry and artwork are outside this narrow entry disposition.
- Batch88 confirmation independently reads all119 native mechanics words and139 timed-record visits, and executes463 uninterrupted scheduler ticks with exact sprite/cursor/timer/X checks, both full pacing round trips and accelerating departure. The final RunFrames/2 change is rechecked with this focused command; no exploratory cases.
- Final verification: build1,434 warnings/zero errors; the focused119-word/139-record/463-tick confirmation passes after calculating the half-gait subdivision. No ResourceAudit source-hash dependency.


Root1452 warnings/0 errors;119 native words,139 record visits,463 uninterrupted scheduler ticks,two complete pacing round trips and full acceleration pass. Master598 converted/34 retained-mixed/496 pending.

## Batch 90: complete six-color visor light composition

SamusVisorColorCatalog.colors is MIXED COMPLETE. Totals190 wholly converted / fifteen mixed /21 unchecked. This supersedes earlier pending FullBeamStart/DarkeningStep dispositions.

- Root independently reviewed native9B:A3C0 six colors,91:D856..D888 room-cycle handler andDCB4..DD1E X-ray handler. Exact six outputs are3BE0,5FF0,7FFF and43FF,2F5A,1AB5. Widening remains nearest-rounded interpolation from normal-suit ink to white; full-beam cycle remains equal-channel linear darkening.
- Narrow retained painted-light choices are the previously approved normal-suit starting ink3BE0, selected yellow-white full-beam ink43FF RGB5(31,31,16), and five-level per-channel brightness drop. The latter composes the selected visor pulse contrast, not a measured beam quantity. Deriving this specific color appearance from mechanics would require re-embedding the same chosen paint inputs. Only these six original RGB outputs are covered; no generic palette exception.
- Native91:D86F/DCD3/DD09 and managed SamusVisorPaletteState.Update/SamusXrayState.UpdatePalette publish color4 of sprite palette4 (CGRAM196). Phase/timer and beam state select a color; its channels do not govern beam geometry or timing. FrameDelay5 is a separate timing value despite numerical equality and receives no exemption.
- Existing focused --lookup-stream-1-visor-colors directly confirms all6 native calculated defaults, exact two-ink/one-step basis, zero stock overrides,6 independent edits and actual guarded room/X-ray color/state/timer cycles. Catalogs remain independently editable.
- Coordinator source-hash dependency: SamusColorClosedContractDefinitions.cs must refresh SamusVisorColorDefinitions.cs; the catalog itself is unchanged. The isolated older contract lists only the catalog, while root has already included the helper from the original conversion. No shared audit edits here.
- Final confirmation: build1,434 warnings/zero errors; focused native/basis/edits and actual guarded cycles pass. Helper SHA256: DBA9D1F70C6BE1C14DD26F3921A44D0A4A36FEBFD49C079DB458DBC8A1A1E5AC.


Root six-native/basis/zero-overrides/six-independent-edits and guarded room/Xray cycles passed. This checkpoint changes only XML disposition and source hash, not executable behavior; Audit1413existing warnings/0errors. Master600converted/39retained-mixed/489pending.


## Batch 91: complete direct atmospheric OBJ attributes

SamusAtmosphericArtworkCatalog.typeOne and sharedTypeFour are MIXED COMPLETE. Totals190 wholly converted / seventeen mixed /19 unchecked.

- Scope correction: the original catalog/schema accepts two ushort[4] lists of OBJ attributes only. Earlier Batch47 language about pending independent tile artwork within these entries was overbroad. Pixel art, RGB payload and timing remain separate obligations and receive no exemption here.
- Native90:8C0F..8C16 contains2A2C..2A2F;90:8C17..8C1E contains2A48..2A4B. Existing named base+frame calculations retain tile identity, zero flips and hardware nine-bit packing. Palette now derives from the existing enemy-projectile CGRAM destination208, upper-half OBJ base128 and sixteen-color row size. RoomLoadingRomData.InitialEnemyProjectilePaletteCgramIndex shares GameplayBasePaletteFormat.EnemyProjectileInitialColor, so no duplicate palette5 input remains.
- Root reviewed actual catalog scope, native90:8AFC..8B06 raw OAM publication and room palette installation. Narrow retained priority2 composes the exact drawing layer for these eight attribute words. It materially determines visibility relative to backgrounds; this is not consequence-free or an exemption for motion/other artwork. Changing priority would select a different visual composition rather than derive the selected one from effect geometry.
- Existing focused --lookup-stream-1-atmospheric-attributes confirms all8 direct native defaults, zero stock overrides, every independent full-word edit, canonical content identity/bounds and actual OAM publication through VerifySamusAtmosphereArtworkBoundary. Mutable-WRAM type2 remains separate and unchanged.
- Coordinator dependency packet: SamusArtworkClosedContractDefinitions.cs refreshes the helper hash and must include its new transitive numeric owners Assets/GameplayBasePaletteCatalog.cs, Hardware/SnesCgram.cs and Hardware/SnesPpuLayout.cs. Catalog itself is unchanged; Runtime/RoomLoadingRomData.cs documents the identical destination but is not an executable helper dependency. No other current-main helper hash references found. Shared audit files remain coordinator-owned.- Final confirmation: build1,434 warnings/zero errors; eight native defaults/edits/identities and actual16type-frame OAM plus mutable-WRAM boundary checks pass. Helper SHA256: A21FFA403E0D1A22CEA3DF840B8A176C2FA1F117B5D031DAAAB6631F4A7404DF.

Root integration: Verification1452existing warnings/0errors;Audit0/0;eight native defaults/edits/identities,zero stock overrides and16 actual type-frame OAM plus mutable-WRAM boundary pass. Master 600 converted/41 retained-mixed/487 pending.


## Batch 92: complete landing-origin calculation and bounded PLB observation

SamusBodyArtworkCatalog.landingYOffsets is MIXED COMPLETE. Totals190 wholly converted / eighteen mixed /18 unchecked. Only one adjacent native instruction byte is retained; no new coordinate or choreography parameter is exempt.

- Native90:8D28..8D37 rows are normal right/left3,6,0,0 and spin right/left3,3,6,0. Calculate active phases from the existing named pose graphics-origin owner: initial normal/spin landing uses its respective pose origin; recovery uses FacingRightNormalPose. Spin inserts the normal compression phase before recovery. Facing copies and inactive zero padding remain calculated. DefaultGraphicsYOffset delegates to the existing compiled SamusPoseProjectileOriginDefinitions; no new3/6 constants and no mutable cross-catalog coupling.
- Exact animation source91:B22D=05,02,F8,01;B231=05,02,F8,02;B235=03,05,02,F8,01;B23A=03,05,02,F8,02 establishes two normal/three spin active frames and transition to standing01/02. The16-byte backing domain contains padding beyond these active frame counts; all original16 unaligned windows remain supported.
- Bounded source-only raster csharp/test-temp/landing-native-geometry.png decodes native92:D94E pose-frame selectors,92:D91E/D938 seven-byte DMA definitions to VRAM6000/6100 and6080/6180,92:9263/945D OAM bases with92:808D pointers, and9B:9400 palette. It deliberately labels all16 bounded pose/frame indices, including inactive adjacent-frame artwork; no gameplay discovery. Normal active frames0/1 and spin1/2 have opaque foot bottoms23/26, so calculated offsets3/6 anchor both at20=pose radius21−1. Spin initial tucked feet bottom19 minus its native pose origin3 gives16; it is not falsely claimed grounded. Source pose graphics byte91:B62D+8*pose is3 forA4..A7; standing01/02 is6 and native radius byte6 is21.
- This geometry evidence invalidated the initial proposal to exempt3px as arbitrary choreography. The final implementation instead reuses the existing named pose origins and derives the landing/recovery phase mapping. Pixel art and physical origins remain with their existing owners; supplied visual landing and graphics edits remain independent.
- Only90:8D38=$AB, native PLB at Goto_CalculateUsualSamusSpritemapPosition, is narrowly retained instruction content. Native90:8CE7 unaligned16-bitLDA makes it the high byte of the final supported window(index15). Managed TryLandingYOffset admits0..15 over exactly17bytes and rejects all outside. Re-encoding unrelated native PLB to derive a visual coordinate would be nonsense; no wider native memory behavior is modeled. Full wrapped SpritemapYPosition is preserved, not just visible lowY.
- Stock catalog now stores zero landing overrides; independent edited source/facing/adjacent bytes retain exact supplied observations. Focused --lookup-stream-1-landing-placement checks17 direct native defaults,16 overlapping words, all17 independent landing edits (plus unchanged24posture edits), canonical identities/bounds and16 actual Samus.Draw calculations with unchanged physicalY. Existing posture data is not marked complete by this companion proof.
- Final incremental build25 warnings/zero errors; focused command passes. Source-hash dependencies on both catalog/helper occur in SamusArtworkClosedContractDefinitions.cs (three closures) and SamusBodyTransferClosedContractDefinitions.cs. Existing transitive source-origin/pose catalogs remain dependencies. Coordinator refreshes shared contracts.

Root integration: Root Verification25warnings/0errors;Audit0/0;17native bytes,16unaligned windows,41independent landing/posture edits,16actual Draw positions and physical-Y isolation pass. Master 600 converted/42 retained-mixed/486 pending.


## Batch 93: complete posture support geometry

SamusBodyArtworkCatalog.postureYOffsets is MIXED COMPLETE. Totals190 wholly converted / nineteen mixed /17 unchecked. Only two exact one-pixel visual joins are retained; all24 offsets derive from selected source geometry, phase identities, padding and facing aliases.

- Native90:8D80..8D97 right/left pairs: crouch transition[-8,0],morph[-4,-2],unused[0,0],standing[-4,0],unmorph[5,4],unused[0,0]. Native91:B4C2/B4C5 crouch andB4E0/B4E3 stand have one timed pose beforeFD→27/28 or01/02. MorphB4C8/B4D1 andunmorphB4E6/B4EA have two timed poses; remaining zero slots are inactive/unused, not additional art phases.
- Source-only posture-native-geometry.png and posture-destinations-native.png use the same native DMA/OAM decoding as Batch92, with the native no-lower-half rule for morph/unmorph/ball. Crouch-source support23 aligns to crouch27support15 giving−8. Morph supports12/10 align to rolling-ball support8 giving−4/−2. Unmorphfirst support10 aligns to crouch15 giving+5. Standing-source23 andunmorphsecond12 use the two reviewed joins below.
- The initial implementation chose right-ball1D frame0 as its destination support; direct zero-stock-fallback confirmation failed exactlyindices4/5 (derived−5/−3 versus native−4/−2). It was corrected through source geometry, not exceptions. Native91:B378 has eight rolling frames: right1D supports7,6,7,8,7,6,7,8; left41 supports8,7,6,7,8,7,6,7. Their common full rolling support envelope is8. morph-ball-native-eight-phases.png records all16 source frames. The helper calculates this envelope from selected pixels over the native eight-frame domain; no sampled support table or arbitrary facing choice remains.
- Root independently reviewed exact one-pixel joins: standing35-family successor3B has support19 before standing01support20 (join−1 at90:8D8C); second-unmorph3D has support16 before crouch27support15 (join+1 at90:8D91). Their facing copies share the same selected visual composition. No generic art/timing/physics exemption.
- Coordinate origins are identical: native90:8D55..8D6F sign-extends the table byte and adds currentSamusY−camera, bypassing usual graphics-origin subtraction. Completion3B→01 has equal radii21;3D→27 equal16. Native91:FDCA..FDE6 returns before anyY write when the old radius is not smaller. Ordinary91:F4DC/F5EB initializers and91:FB08 animation setup do not shiftY. Managed ApplyAnimationPoseTransition→ApplySimpleGroundedPoseChange likewise changes pose/radius/frame, notY. Entry-time prospective collision movement occurs before both compared frames and cannot explain these exact joins.
- SamusBodyPlacementDefinitions now reconstructs the selected temporary body DMA window, decodes nontransparent support using actual sprite parts/flips/tiles, and subtracts the destination's selected graphics origin. No persistent replacement-offset table exists. Underlying frames/parts/pixels remain independently required in their original inventory entries. Palette channels are irrelevant to nonzero-pen membership and are not exempt.
- Catalog constructor computes posture override membership only after validating/indexing its selected body definitions. Imported offset values remain independent when source PNGs, parts or pointers are edited. Empty/mutable-zero-pointer/unavailable source geometry validly falls back to the supplied offset; schema and later drawing rejection contracts are not narrowed. Stock has ZERO such fallbacks. Existing post-publication exposed part-array mutation is not newly certified; this check covers constructor/import independence.
- Focused --lookup-stream-1-posture-geometry extracts the native body fixture, compares24 direct defaults/zero stock overrides,24 independent offset edits,12 actual active Draw origins with unchanged physicalY, and independent empty parts/zero pointers/blank PNGs/shifted support preservation. The old synthetic facing fixture no longer claims12 mandatory posture basis words; its exact supplied-output checks remain. --lookup-stream-1-landing-placement also passes after this catalog change.
- Final build1,434 warnings/zero errors and both focused commands pass. Final helper summary edit is XML only. Coordinator dependency packet: refresh helper/catalog across SamusArtworkClosedContractDefinitions and SamusBodyTransferClosedContractDefinitions; add transitive SamusSpritemapArtworkCatalog.cs,SamusSpritemapPoseDefinitions.cs andHardware/SnesTileWords.cs wherever missing, alongside existing body/frame/pose/rendering-definition owners. All production changes stay within previously granted owned files; no shared audit edits.

Root integration: Root Verification1452existing warnings/0errors;Audit0/0;24native defaults/zero stock fallbacks,24independent offset edits,12actual Draw cases and empty/pointer/blank/shifted-art independence pass; prior landing check passes. Master 600 converted/43 retained-mixed/485 pending.


## Batch 94: complete drained placement geometry

SamusBodyArtworkCatalog.drainedYOffsets is MIXED COMPLETE. Totals190 wholly converted / twenty mixed /16 unchecked.

- Native90:8DEF..8E0E is exactly32 signed bytes, shared by E8/E9 but aligned to the complete left-facing E9 animation byte program91:B268. Named phases calculate repetitions and ten zero command/operand positions12,13,17,18,24,25,27,28,30,31. The shorter right-facing program is not used to narrow the existing shared32-byte API.
- Source-only drained-native-32-geometry.png decodes the actual native DMA/OAM geometry for every E9 byte index. Native90:8790 and managed ShouldDrawBottomHalf suppress lower art at compact0/1: their supports are10/12. Falling2..6 support28; impact7 support23; kneeling8..11 support15; rise14 support19,15 support23,16 support26. Failed rise19..23 reuses14,15,16,15,14; hit26/Hyper Beam29 reuse kneeling geometry. Null command sprites are blank in the diagnostic, not misread as bank-header OAM lists.
- Falling and intermediate rise align their selected opaque support to ordinary standing-left02 support26 minus existing graphics origin6, yielding20 and offsets-8/-3. Final rise aligns to the existing physical domain owner SamusPoseCollisionDefinitions.ReadVerticalRadius(E9)=21, yielding-5; no duplicate+1 parameter remains. Kneeling/tucked/hit/Hyper Beam stages use negative existing drained graphics origin(-4), yielding+4. Underlying geometry and origins remain with their separately owned entries, not hidden duplicate constants.
- Root independently reviewed the source raster, native controllers and existing radii/supports. Narrow retained scene composition consists of compact common support17, impact support18, and the named phase-alignment policy (including terminal use of radius boundary21). Neither17 nor18 follows the actual relevant crouch support15, rolling envelope8, standing support20 or drained steady support19. Replacing the selected trajectory with one of those would redraw the scene; arithmetic fitting would re-encode its placement. This is no general animation exception and does not exempt timing, pixels or physics.
- Coordinate review:90:8DC1..8DEE sign-extends the selected byte and adds current physicalY-camera, bypassing usual graphics-origin subtraction.91:E4F8..E541 adjusts Y once for radius21 before entering frame2. Falling90:94CB uses ordinary vertical collision then selects7 without another Y change. Controllers91:E571/E59B/E60C and managed SamusDrainedState.PutStanding/Release/PutCrouchingOrFalling change pose/frame/speeds without shifting Y. Consequently the chosen visible excursions are preserved, not mislabeled consequence-free or constant grounded contact.
- SamusBodyPlacementDefinitions shares the existing temporary DMA/nontransparent support decoder from Batch93. Runtime catalog stores only supplied differences; stock stores ZERO drained overrides. Independently edited offsets, selected sprites, blank PNGs, mutable-zero pointers, empty parts and standing/drained graphics origins remain exactly supplied. No new document restriction or coupling of imported edits. As before, post-publication mutation of exposed part arrays is outside this import check.
- Focused --lookup-stream-1-drained-geometry checks all32 direct native defaults/installed byte windows, zero stock fallback,32 independent offset edits and changed identities, exact domain rejection,22 actual nonempty source Draw phases with exact signed Y and unchanged physicalY, plus empty/pointer/blank/shifted-art/origin independence. Command slots are confirmed as byte observations rather than executed as null OAM programs. Final build1,219 warnings/zero errors; focused command passes.
- Granted narrow ContentIdentity.cs change materializes DrainedYOffsets in the identical canonical byte order. Coordinator refreshes helper/catalog/ContentIdentity hashes in SamusArtworkClosedContractDefinitions (three closures) and SamusBodyTransferClosedContractDefinitions. New transitive numeric owner SamusPoseCollisionDefinitions.cs must be included wherever absent; all geometry dependencies from Batch93 remain. No shared audit file edits.

Root integration: Root Verification1452existing warnings/0errors;Audit0/0;32native defaults/zero stock fallbacks,32independent edits,22actual draws and empty/pointer/blank/shifted-art/graphics-origin independence pass. Master 600 converted/48 retained-mixed/480 pending.


## Batch 95: complete cinematic and hurt color composition

SamusHurtColorCatalog.hurt and intro are MIXED COMPLETE. Totals190 wholly converted / twenty-two mixed /14 unchecked. Batch66's required fifteen-level basis is superseded by a calculation, not retained wholesale.

- Native9B:A380..A39F hurt andA3A0..A3BF intro contain32 RGB5 words. The source-only hurt-native-palette-roles.png renders the same standing01 native DMA/OAM geometry under normal Power/intro/hurt palettes and isolates each of the fifteen opaque pens. Pen roles are painted outline, visor and armor/cannon/helmet shade classes; numerical pen order is not a brightness progression.
- Intro's seven magnitudes now calculate: outline4 to hardware full intensity31 spans three equal main intervals4,13,22,31. Half shades are8,18,27; the lowest rounds down and the remaining halves round nearest-up. Named pen roles select main/half shades: shadow1/5/14,highlight2/7,outline3,visor4,bright points6,middle8/9/10/12,deep11/13/15. This is a selected monochrome shade composition, not a per-index numeric fit. Green equals red; blue=max(outline4,red-2) reuses the same outline floor.
- Hurt remains floor((2*introChannel+5*hardwareWhite)/7) for all opaque channels. Native hurt slot0 is calculated black; intro slot0 retains exact3800 compatibility identity already reviewed for normal-suit transparent payloads. Independently editable catalogs remain independent, with no runtime linkage imposed on their supplied replacements.
- Root independently reviewed the image, native fixed palette-copy branch91:D8D9..D8F8 and the revised numeric derivation. Narrow selected paint inputs are outline4, categorical shade assignments, three-main-interval/halfshade composition and darkest-half-down rounding policy, blue drop2, and2:5 source/white contrast. Intermediate gray magnitudes are NOT exempt. These choices determine the selected rendering and cannot be obtained from mechanics without restating its paint design. Only exact32 colors/transparent identity are covered; no timing, sound, radii, pixels or general palette exception.
- Native symbols are consumed only at91:D8E3/D8F0. Managed SamusHurtFlashPalette.Step selects variant by hurt counter parity/cinematic flag and copies sixteen resolved colors to CGRAM192..207. Color channels do not determine its counter/sound/recovery behavior. SnesObjRenderer skips pen0 before palette access, while the original zero-slot words remain exact observable imported data.
- Runtime retains no stock gray-level samples, full-color exceptions or transparent-word fallbacks. All fifteen default levels calculate through named shade roles. Independent input edits store only their own level/channel/blend differences; intro edits never silently change independently supplied hurt RGB. No serialization schema change.
- Focused --lookup-stream-1-hurt-blend confirms32 native outputs,15 direct calculated shade levels,30 channel/blend relations,zero stock shade/color/zero-word fallbacks,96 independent RGB edits with exact stored difference membership, bounds and unchanged stock. Existing guarded ordinary/cinematic seven-call cases preserve palette/action/timer/CGRAM behavior and zero forbidden reads. Final production build1,219 warnings/zero errors; command passes. A verifier-edit compilation mistake was repaired before this successful build; an earlier stale-binary invocation is not counted as evidence.
- Coordinator refreshes SamusHurtColorCatalog.cs and SamusHurtColorDefinitions.cs hashes in SamusColorClosedContractDefinitions.cs. No new transitive production dependency and no shared audit edit. Root SamusPaletteRomData remains untouched.

Root integration: Root Verification1452existing warnings/0errors;Audit0/0;32native colors/15calculated levels/30relations,zero stock shade samples/overrides,96independent RGB edits and guarded ordinary/cinematic cycles pass. Master 600 converted/52 retained-mixed/476 pending.


## Batch 96: complete bounded death-explosion bitmap

SamusDeathTileAtlas.planar is MIXED COMPLETE. Totals190 wholly converted / twenty-three mixed /13 unchecked.

- Pinned bank9B:8000 labels Tiles_SamusDeathSequence as an exact1400-byte binary image. The original entry is only5120 planar bytes/160 tiles, exported as64x160 indexed PNG in upload order. Native9B:B7BF/B7C9's five cyclic pages and destinations were already calculated; this change preserves their exact order and public transfer domain.
- Source-only death-native-nine-phases.png renders every right/left phase from native OAM IDs081C..082D, all five uploaded pages, and actual per-phase palettes selected through9B:B823/B7D3/B80F. It shows selected suited rupture, human anatomy, hair and sparks. Late-phase whiteness substantially comes from palettes; no separate pixel drawing is attributed merely to a color change. Facing pairs already share tile identities with OAM Hflip.
- Six blank allocation tiles at native tile indices72,83,91,105,146,151 (9B:8900/8A60/8B60/8D20/9240/92E0) now calculate zero. Three direct reused patches calculate from their original counterparts:123<-102 ($8F60<-$8CC0),132<-124 ($9080<-$8F80),157<-133 ($93A0<-$90A0). These are semantic same-position anatomy relationships confirmed in late native OAM: hair tip(6,-28) between phases6/7,outer arm(-10,-12) between6/8,and torso(-2,-12) between6/8. Exact source pixel inspection found no further whole nonblank direct/H/V/HV tile reuses; no generic runtime deduplication is introduced.
- Late phases6/7/8 already reuse lower-body16x16tile120,foot tile153 and side tile148. Their different upper blocks101/133,138,141 and hair/arm pieces remain distinct selected pixels. Their existing sharing is not counted as newly removed content.
- Root viewed the complete18phase image, original atlas and death-native-unreferenced-tiles.png. Three nonblank fragments114=$9B8E40,119=$9B8EE0,135=$9B90E0 are not referenced by those18 explosion maps, but remain exact bounded transferred/hash/PNG content. This does not declare them globally dead or invoke a memory-corruption exception.
- Narrow retained content is151 source tiles (4832 planar bytes): exact chosen anatomical/hair/suit-rupture/spark contours and pen membership, including those three copied fragments and transparency within drawn tiles. Pose/timer/transport cannot select this particular human figure, shading and rupture drawing without re-embedding its design; geometric approximation would change the image. Root approved this specific bitmap content after the shared/reflected/late-phase review. No RGB, OAM, timing, physics or other-atlas exemption.
- Runtime stores those source bytes plus independent supplied exceptions to blank/repeated regions. It reconstructs each original page and canonical hash on demand; no full replacement stock atlas table is cached. Independent PNG edits to a source patch preserve independently supplied repeated patches through explicit differences. Original schema/source/size rejection behavior remains.
- Focused --lookup-stream-1-death-pixels confirms all5120 native bytes and direct source relations, exact151tile basis membership with zero stock relation overrides,384 independent edits across every blank/repeated/source byte, exact canonical hashes, all five actual NMI uploads and source/size rejection. Build1,219 warnings/zero errors and focused command pass. Final class comment is XML only.
- Coordinator refreshes SamusDeathTileAtlas.cs in VramDmaSourceContracts.cs and records existing SamusSpecialSequenceRomData.cs page-layout dependency if absent. Search found no other direct source hash reference. Owned atlas/helper-format changes and own focused verifier only; no shared audit edits.

Root integration: Root build1452 warnings/0 errors; ResourceAudit build0/0. Focused5120 native bytes,151 source tiles,zero stock relation overrides,384 independent edits,canonical hashes and five actual uploads passed. Master 600 converted/56 retained-mixed/472 pending.


## Batch 97: complete bounded world-map BG3 characters

WorldMapArtwork.background is MIXED COMPLETE. Totals190 wholly converted / twenty-four mixed /12 unchecked. WorldMapArtwork.foreground remains independently required.

- Exact original scope is $8E:D600..DBFF:1536 planar bytes,96 two-bit characters exported as128x48 pixels. Native bank8E labels Tiles_Beta_Minimap_Area_Select_BG3. Native81:8DDB LoadInitialMenuTiles and pointer81:8E54 copy the whole image. Managed MenuPpuState.BindWorldArtwork calls WorldMapArtwork.LoadTo; FileSelectAreaMapGraphics.BindScreens installs it and RenderBackgrounds renders BG3 with transparent pen0 before additive subscreen composition. Thus all bounded bytes remain observable, including beta characters; there is no dead-data claim. RGB palettes, tilemap placements and688 four-bit foreground characters are outside this entry.
- Source-only world-background-native-labeled.png shows all96 native characters. The helper calculates56 complete primitive cells (3584 pixels): solids0E/0F/1B/53/59..5F,shaft10,arrow11,guides12/1C..1F,room borders20..27,diagonal rooms13..1A/28..2F,inset panels30/31,bevels4B/4C and binary cuts4D/4E/50..52/54..58. Source inspection establishes no additional whole-cell direct/H/V/HV repeats among the remaining30 non-outlined glyph/icon cells.
- Three further shared stroke domains calculate200 pixels: slanted numerals/percent00..0A final blank row (88), label cells0B..0D two bottom separator rows (48), and four paired panel glyphs34/35,36/37,38/39,3A/3B matching top/bottom cap rows (64). They are domain-specific shared strokes, not generic byte deduplication.
- Outlined numerals3C..45 (ordered1..9,0;8E:D9C0..DA5F) retain ten selected pen2 footprint masks,176 occupied bits total. Their one-pixel eight-neighbor pen1 outline/exterior rule derives459 further pixels. Exactly five original edge decisions remain supplied:3D(0,2)=3,40(7,0)/(7,1)=1,45(1,7)/(2,7)=0. These selected missing/extended outline and transparent corner contours were independently reviewed on the native sheet.
- Root approved only remaining1720 selected contour pixels in00..0D,32..3B,46..4A,4F after those200 shared strokes, ten digit masks and the five exact edges. These encode the chosen slanted numeral/EN/icon designs; numeric tile identities and the transfer consumer do not determine that particular typeface or painting. Further arbitrary coordinate cases would re-encode the same design. No other glyph, color or artwork field is exempt.
- Exact selected primitive inputs are outside3/fill1/edge2; shaft10 inset2 lower ledge; arrow11 two-pixel stem and narrowing head;12/1C..1F dotted phases/corner;20..27 named open-side walls;13..1A/28..2F half-cell slope direction/interior/wall/guide flags;30/31 inset rectangle1..6 by1..5 and pen exchange;4B/4C bevel/bottom cutout;4D/4E/50..58 diagonal inclusion/cutout policies. The selected2A/2F join continues left boundary y3 through y4 to dotted guide y5, rather than leaving a gap. 2F additionally has the solid left wall on its filled side. Root reviewed these exact topology/pen choices as selected icon definitions, not a blanket primitive exemption. All raster positions then calculate from those named variants.
- Runtime stores exactly1725 source pixels and ten masks, with zero stock primitive/outline overrides. It reconstructs4419 pixels from geometry/shared strokes/footprint membership; this count includes176 source-mask fill pixels and does not falsely declare those source-free. Independent edited primitive, outline, source or fill pixels preserve the complete supplied PNG. Foreground bytes stay untouched. Original PNG schema and import-time two-bit pen rejection remain.
- Focused --lookup-stream-1-world-background confirms all6144 native pixels, exact direct-default/source-mask membership,96 independent tile edits, a simultaneous full-pixel inversion, complete actual foreground/BG3 VRAM uploads and invalid pen/tile bounds. Final production build1219 warnings/zero errors; command passes. Final follow-up comments only clarify the approved scope.
- Coordinator dependency packet: new Assets/WorldMapTileDefinitions.cs is explicitly granted. No current ResourceAudit contract directly hashes WorldMapArtwork.cs; TextAndMapClosedContractDefinitions.cs hashes its existing MapScreenDefinitions.cs format owner. Add the new helper/atlas to the appropriate reviewed source closure if required centrally. No shared audit edits.

Root integration: Root build1452 warnings/0 errors;6144 native pixels,4419 calculated outputs,1725 residual source pixels/10footprints,96 tile edits,all-pixel inversion,import bounds and actual full uploads passed. Source inspection found no direct ResourceAudit owner hash; MapScreen closure references unchanged format constants only. Master 600 converted/60 retained-mixed/468 pending.


## Batch 98: complete foreground character geometry and selected drawings

WorldMapArtwork.foreground is MIXED COMPLETE. Totals190 wholly converted / twenty-five mixed /11 unchecked. Scope is only the688 four-bit characters at8E:8000..D5FF (22016 planar bytes/128x344 pixels). Palette RGBs, tilemaps, motion and other assets are excluded.

- Native8E:8000 Tiles_Menu_BG1_BG2 includes menu graphics and the first4000bytes used by the anti-piracy screen. The original field contains the complete5600-byte image; native81:8DDB initialization and managed MenuPpuState.BindWorldArtwork/WorldMapArtwork.LoadTo publish the complete bounded payload. Source-only world-foreground-native-labeled.png labels all688 cells. world-zebes-native-composition.png independently reconstructs8E:DC00's32x32 native tilemap with its flips and8E:E400 palettes. No unreferenced-by-this-map byte is discarded.
- Thirty-five blank cells calculate transparent0:009/00F/020/028/029/02A/02E/032/03C/03D/046,1E7..1EF,1F7..1FF,27C/27D,2AC..2AF. Fifteen solid cells calculate from their selected single ink:1A2/1A8/1AE/1BE/1C0/1C1/1C3/1C5/1C6/1C8/1DC/1DD/283/293/294. Named glyph pieces share044<-002,11D<-0DC,12D<-0EC,165<-111,175<-121;097 rotates095 by180degrees. All dependencies normalize to a fixed source pixel rather than retaining chains or a replacement sample array.
- Entire1A0..1E6 material block calculates from diagonals, antidiagonals, one-pixel edge bands, bottom strips and crossing precedence. Its192 selected ink seeds remain explicit. Crossings1AA/1CA stop their secondary fill before the highlighted anti-diagonal. Initial native confirmation caught an overly broad secondary region at1AA(2,4); correcting that geometric boundary and its counterpart, rather than retaining a sampled exception, establishes every native pixel.
- Rounded button halves090..094/0A0..0A4 use center(7.5,7.5), outer radius7/inner6 and the selected narrow soft-edge band. Inner letter content is confined to the three columns nearest the split.095 is a clipped radius1.5 round cap at(7,5.5), inner radius1;096 uses oblique strip7<=x+2y<=14 with two-pixel edge bands. 097's earlier direct alias now normalizes through095's calculated geometry.098/099 are inset beveled squares.09A is the chamfered cap;09B/09C/0AB/0AC calculate frame/fill and retain their internal L/R strokes. Native confirmation caught the initially omitted full horizontal cap on09A row2; final top-cap geometry matches exactly.
- B0..B7/C0..C7 are four15x15 beveled directional-pad crosses with seven-pixel arms, selected vertical/horizontal inner highlight states and center glint. All1024 pixels calculate from three inks. B8..BF/C9..CB/CD..CF are clipped diagonal ramps cycling eight source inks; crossing arms reverse phase. They are NOT retained painted glows. Exactly one native phase decision remains: C9(2,1)=3. Native8E:9920 rows are06543218/00332187/00021876/00008000/00000000/00000000/00000000/00000000. Plane bytes9922=35,9923=39,9932=01,9933=02 independently establish that pixel. world-glow-c9-native.png shows native/ramp/difference; root independently reviewed this exact painted phase discontinuity; no generalized ramp-exception exemption applies.
- Font face/shadow construction uses actual glyph boundaries:000..05F paired in32-cell blocks,060..08F single cells,D0..12F paired fromD0/F0/110,160..19F paired from160/180 EXCLUDING16E/16F. The original broad range mistakenly classified two mechanical rail cells as typography; their two masks, four apparent contours and62 other-ink pixels were removed from the font disposition entirely. Actual original glyph content is286 masks/359 contour decisions. ENERGY09D..09F and TIME0AD..0AF cast straight down; SELECT START0A5..0AA casts down-right across contiguous cells, with one selected extra shadow0AA(0,4)=D. C8 casts down-right andCC straight down. Total300 masks/360 contour decisions. Dilation added unwanted lower-left shadows and was discarded; actual translated silhouette plus specifically drawn bevels is used. world-font-shadow-joins.png and world-font-translated-shadow-joins.png document full zero/one/two/small-zero/Japanese glyphs, masks and exact differences.
- Rails13F/14D/14E/14F/15C/15D/15E/15F/16E/16F calculate uniform horizontal/vertical bands, shared end-terminal bands, empty regions and mirrored elbow silhouettes. 14D's tube profile and matching elbow entry share their selected inks. Remaining176 mechanical shading/detail pixels are separately identified below, not hidden inside font or portrait exemptions.
- Six star cells calculate:200/208/209 single point(3,4);240 half-pixel-centered Manhattan diamond;2AB centered(3,3) radius3 with four ring classes;250 half-pixel-centered Manhattan rings plus four selected rounded shoulders(1,1),(6,1),(1,6),(6,6). All shared star rings derive from2AB's four ink identities;208 retains its separate point inkA. These five source inks remain independent from palette RGBs.
- Final literal drawings are208 cells only:130..13E,140..14C,150..15B (forty detailed emblem/portrait/appendage cells),1F0..1F6 (seven chosen symbols),and161 planet/caption cells within200..2AB excluding200/208/209/240/250/2AB,27C/27D and283/293/294. Native world tilemap selects141 planet identities including calculated solid294; remaining140 texture/limb cells plus21 separately transferred caption/material cells form the161. Planet boundsx74..172,y48..151 have asymmetric poles (115..131 at48 versus117..129 at151) and differing opposing limbs. Their detailed land/cloud/illumination follows selected painted terrain, not radial star bands or a reflected ellipse raster. Original caption/material bytes remain observable in exact full uploads even where this tilemap does not select them.
- Exact675 non-font source-input partition:169 rounded-button letter pixels +96L/R letter pixels;192diagonal-material ink seeds +3flat-fill seeds +6square-panel inks +3rounded-button inks +2small-cap inks +3L/R chamfer inks +8L/R frame inks +3D-pad inks +8glow-ramp inks +5shared-star inks;176mechanical rail/shading pixels;oneC9 phase decision. Thus265 letter pixels +233 ink seeds +176 mechanical pixels +one explicit decision. These are distinct from the208 complete drawing cells and300 font masks/360 contour choices; no unexplained residual category remains.
- Runtime stores exactly14347 source pixels (208*64 drawings +675 non-font inputs +360 font contour decisions) and300 source masks. It calculates10845 blank/geometry/reuse relations plus the mask-derived font pixels. Stock stores no unexpected relation overrides; arbitrary independent PNG edits are preserved by exact per-pixel differences and mask overrides, including changes to source inks and aliased glyphs. Foreground import still validates four-bit indices; BG3 behavior/schema remains exact.
- Focused --lookup-stream-1-world-foreground confirms all44032 native pixels, every direct source relation and fixed dependency, exact14347/300/360 basis membership,1197 independent canonical-source/tile edits, complete pixel inversion, full actual foreground/BG3 VRAM uploads and bounds. Final verification-only build25 warnings/zero errors; command passes. Earlier full production build1194/0. This confirms the requested conversion, not exploratory gameplay.
- Coordinator source dependencies remain Assets/WorldMapArtwork.cs, new Assets/WorldMapTileDefinitions.cs and existing MapScreenDefinitions.cs format owner; no shared audit changes. All code/report/verification changes remain inside the granted Stream1 worktree.
- Final disposition: root independently viewed the complete labeled atlas, composed planet, full-glyph shadow diagnostics and exactC9 difference. Approved only the208 specified drawing cells,300 genuine font masks/360 contours,675 precisely partitioned non-font inputs and named shape/pen/state policies. The irregular terrain, selected portrait/anatomical forms, typography and mechanical shading cannot be generated from tile identities or transfer counts without re-embedding this selected design; substituting another contour changes the artwork. Every proven geometric/shared relationship remains calculated. No RGB, tilemap, timing or other-atlas exception. Final follow-up edits only clarify XML/report disposition; verified implementation is unchanged.

Root integration: Root reviewed native labeled atlas, font shadow joins, composed Zebes map and exact C9 discrepancy. Integrated build1452warnings/zeroerrors; focused proof passes44032 native pixels,10845 geometry/reuse relations,14347source pixels/300masks/360contours,1197 independent source/tile edits, full inversion and actual full VRAM uploads. Master 600 converted/69 retained-mixed/459 pending.


## Batch 99 — complete projectile collision-radius disposition

SamusProjectileRadiusDefinitions.Radii is MIXED COMPLETE. Totals: 190 wholly converted / 26 mixed / 10 unchecked. SamusProjectileInstructionDefinitions.Words remains independently required for its selected timing, phase/loop and trail policies. No completion of projectile artwork is implied.

- Removed the 805-entry FrozenDictionary and the cached sorted pointer array. The existing native program layout now classifies exactly 805 timed records by mutually exclusive family, direction axis and timed phase; an uncached ordered/indexed view preserves identity/order/bounds. Neither executable words nor the intervening A117 empty-spritemap word become radius records.
- Each radius pair now calculates from its family policy. Reflection, repeated poses, linear growth/caps, triangular oscillation, grid rounding and exact shared lobe extents are evaluated on demand. The 32-bit input rejection domain and the existing separate 93:0004/0005 Murder Beam zero observations are unchanged. Those low-bank observations are not 93:8004 instruction bytes and receive no new memory-emulation exception.
- New granted Game/ProjectileWaveEnvelopeDefinitions.cs owns the existing fixed artwork offsets 8/13/15/16 and initial Spazer spread4. Native AE70/AE77/AE7E/AE85 contain centered-OAM coordinates from which the four offsets derive; D64A's first and center lanes establish initial spread4. These five chosen source inputs remain REQUIRED under ProjectileSpriteCatalog.frames. Radius logic uses fixed source geometry, never independently edited imported art.

### Exact physical-policy/source matrix

All ranges are bank93 native program domains; radii occupy timed-record offsets4/5 only. Sizes below are half-extents in pixels. Selected magnitudes and named growth/minimum/cap/rounding decisions were independently reviewed by the coordinator as gameplay hit-reach design, not inferred from a failed universal pixel-bound fit.

| Native domain | Family | Calculation and precisely selected physical policy |
| --- | --- | --- |
| 86DB–873A | Power | Vertical4×4; other compass cases8×4. Direction reflection/sharing calculated. |
| 873B–8952 | Wave/IceWave | Axial thickness4, transverse baseline12 plus clipped triangular bump of step4/height8 on each eight-phase half-cycle. Diagonal radius follows a step2 triangle4..12, with initial/final shoulders8. The invisible upward prelude retains12×4. |
| 8953–8976; 8E77–8F16; 912F–9152 | Ice, charged Power, charged Ice | Shared fixed8×8 envelope. |
| 8977–8A56 | Spazer/IceSpazer | Axial thickness8; initial two transverse phases12 then20. Diagonal linear growth8+4×phase. |
| 8A57–8CF6 | SpazerWave | Axial minimum12 plus exact small-lobe outer edge (shared center distance+half8px OBJ), thickness8. Diagonal clipped trapezoid8..16 in4px steps. Outward/return phases and compass transposes calculate. |
| 8CF7–8D46 | Plasma/IcePlasma | Cross thickness8, axial length16; horizontal startup8 before16, diagonal8. |
| 8D47–8E76 | PlasmaWave | Same shared outer-edge/minimum12 axial envelope, axial length16 except horizontal startup8. Diagonal centered8, otherwise shared small-lobe outer edge rounded down to4px. |
| 8F17–912E; 9153–936A | Charged Wave/IceWave | Paired/reflected phases. Axial envelope12 at center, then8+shared lobe distance; only the middle distance13 shoulder has selected one-pixel collision inset21→20. Diagonal expands from8 by4px for first two outward stages and1px for final two (8/12/16/17/18). Thickness8. Invisible upward prelude12×8. |
| 936B–94BA | Charged Spazer | Paired phases; axial across12 until final spread20, along8 during first four frames then16. Diagonal8 held initially, then4px growth to20. |
| 94BB–9ADA | Charged SpazerWave | First four frames use axial12×8 or diagonal8. Axial spread uses shared lane outer edge rounded down to even pixels, initial transverse minimum12 and subsequent full length16; terminal narrow spread8 remains. Diagonal initial spread envelopes12/12/16, then shared outer edge rounded down to4px. Alternate-frame duplication and reflection calculate. |
| 9ADB–9BEA | Charged Plasma | Paired stages; axial length grows by8 from8 capped28, cross8; diagonal grows by4 from8 to20. |
| 9BEB–9EBA | Charged PlasmaWave | Six-frame startup: along8 for four frames then24; transverse12; diagonal8 then12. Repeating axial across=max12(shared small-lobe edge), along30 vertical/28 horizontal. Repeating diagonal poses use the exact five reviewed semantic bounds below, with alternate pairing and return calculated. |
| 9EBB–9F1A; 9F87–A006; A159–A16C | Missiles, normal/fast bombs and Power Bombs, Wave SBA | Fixed4×4, independently of selected art and cadence. |
| 9F1B–9F86; A039–A06A | Super missiles/link, missile explosion | Fixed8×8. |
| A007–A038; A16D–A1A0 | Beam explosion/unused explosion | Exact zero radii; no claim this alone disables every possible point collision. |
| A06B–A0F2 | Bomb explosion, Plasma SBA, super explosion | Linear8+4×phase capped16, shared across both axes. |
| A0F3–A116; A119–A13C | Unused echo/Shinespark echo | Exact16×32 /32×32 bounds. A117's empty-artwork word is excluded. |
| A13D–A158 | Spazer SBA | X grows4+8×phase, Y8. |

### Five long diagonal PlasmaWave collision poses

- Native labels split six startup records9C9F..9CC7 from the repeating centered/outward/return records9CCF..9D47; opposite slope uses9E37..9EAF. The named centered / first split / middle / outer shoulder / fully spread radii are12/16/17/20/24, observed at radius bytes9CD3/9CE3/9CF3/9D03/9D13 and their alternate/return/direction counterparts.
- Source sprite short-axis half-extents are20/26/29/31/32 (alternates24/31/34/35/37). Their displacements0/6/9/11/12 agree with the existing selected3/4 lobe projection, but an8px physical core plus those displacements yields8/14/17/19/20, not the actual collision bounds. Projecting24 to17 would also fail the actual peak24. There is no common source projection/clamp/step operation selecting the five gameplay boxes; fitting a polynomial or phase-specific corrections would only re-encode the chosen collision design. Retention is limited to these five semantic bounds and the matrix's explicit physical policies.
- Native93:8056/805F and8212/821B mask/store the two bytes directly. Managed SamusProjectileSystem.Animation.RunProjectileInstructionHandler publishes the same pair; SamusProjectileSystem.TrailsAndCollisions.ScanHorizontalWaveShotReactions/ScanVerticalWaveShotReactions use the bounds to enumerate block spans, and RoomEnemySystem.OrdinaryCombat uses them for actual ordinary/extended enemy overlaps (projectile path around907–922; bomb path1809–1821). These values materially control reach, hit timing and reactions; this is not a visual-only exemption. Movement velocity, damage, sound, timing, artwork and other fields are excluded.

### Confirmation and integration dependencies

- Focused command: `--lookup-stream-1-projectile-radii`. Independent native stream traversal confirms all805 record identities/order/indexing, all1610 original radius bytes and direct family calculation with no fallback. Actual interpreters execute1610 projectile and1610 bomb records (native and all-empty independently installed artwork selections), preserving timer, sprite, radii, trail and next pointer with every bus read unavailable. All non-radius bytes in the original bounded domain and outside-address/index rejection remain checked. Native shared OAM sources independently confirm all five helper inputs.
- Static source diagnostic: `csharp/test-temp/projectile-radius-native-geometry.csv` records original ROM spritemap extents and physical radii; it is not an exploratory gameplay probe. Original bank93 DW address comments are occasionally two bytes early inside the repeating program, so exact identities above follow actual supported-ROM traversal and native labels.
- This Stream1 worktree predates the integrated Stream2 artwork implementation. Its old ProjectileSpriteDefinitions.cs is intentionally untouched. The granted root-relative candidate `csharp/test-temp/ProjectileSpriteDefinitions.main-wave-basis-candidate.cs` removes only UnresolvedWaveDistances and SpazerInitialAxialSpread definitions and redirects their eight/two existing consumers to the shared helper. Apply only against original byte-SHA256 `12189DE61842331DA9B79B6D92FE09CA5D84432211805325CE105393DDBD8EFA`; candidate SHA256 `58F32DD9E95DF9A709F5A26CB7589B6D83673FA539D3959019CE57BCE8F333D4`. Root must reconcile if the original hash changes; never overwrite unrelated integrated art changes.
- Root source-closure packet: ProjectileClosedContractDefinitions currently hashes radius definitions and ProjectileSpriteDefinitions (the latter in three closure lists). Refresh those hashes and include new ProjectileWaveEnvelopeDefinitions plus its transitive SamusProjectileInstructionDefinitions layout dependency in applicable closures. No shared audit/manifest file was edited here.
- Final focused build: 1434 warnings / zero errors; the complete native/actual-interpreter/shared-source/domain proof above passed. git diff --check found no whitespace errors.

Root integration: Root reviewed complete family rules, native frame classification, physical collision consumers and exact art-relative bounds.805original records/1610nativebytes and actual projectile/bomb interpreter checks confirm supplied artwork independence, no runtime reads and exact originaldomain; shared art candidate reconciled narrowly. No timing/artwork closure. Master 600 converted/75 retained-mixed/453 pending.


## Batch 100 — complete projectile instruction programs

SamusProjectileInstructionDefinitions.Words is MIXED COMPLETE. Totals: 190 wholly converted / 27 mixed / nine unchecked. This completes the independently required program scope from Batches55–60; the source artwork and five shared lobe inputs remain required under their existing owners.

### Calculated structure

- The existing dictionary/factory had already been removed; this final pass resolves every remaining selected parameter instead of treating container removal as completion. Native timed records remain eight bytes, with duration/trail fields separated from independently installed sprites and the separately completed physical radii. Goto and Delete, target address arithmetic, compass aliases, ordered records and accepted word addresses calculate.
- Four source-owned outward lobe positions give Wave16 = four signed outward/return quarters × four positions; PlasmaWave8 = outward/return × four positions; SpazerWave10 = outward/return × (four positions + initial Spazer lane). These counts are structural consequences of the existing named traversal policy, not new independent magnitude exemptions.
- Charged programs pair the two selected alternating glyph phases: charged Wave16 = 2×8; charged Spazer10 = 2×5 growth poses; charged Plasma8 = 2×4 growth poses. Mature Spazer/Plasma loop entries are total phases minus the final two-pose pair, rather than independent constants8/6.
- Charged SpazerWave24 = two paired startup poses + paired10-phase Spazer cycle; charged PlasmaWave22 = the same six-record Plasma growth prefix + paired8-phase Wave cycle. Their loop targets derive from those exact startup extents. Native first-frame preludes, last-growth-frame loops, complete cycles and one-shot deletion retain their named roles.
- Trail phases calculate as timed ordinals for beam/echo/trail cycles. The separate no-trail bomb/explosion programs publish zero throughout. No source payload, polynomial encoding, generated sample array or per-address replacement word table remains.

### Precisely retained timing/content policy matrix

The coordinator independently reviewed native93:81D8 and81F0..8237 plus the actual managed consumers, and approved only these selected exposure/growth/reach/lifetime choices. Every duration is in interpreter updates. Native program domains are bank93; the fuller range matrix in Batch99 also applies.

| Native source | Remaining selected program choices | Calculated consequence |
| --- | --- | --- |
| 86DB–873A Power | Persistent-frame reload hold15 | Single zero-trail phase loops to itself; exact timer reload remains observable. |
| 873B and8743–8952 Wave/IceWave | Upward invisible prelude4; cycle hold1; signed outward/return traversal | Shared four-position geometry gives16 cyclic records; prelude is skipped by later loops. |
| 8953–8976 Ice | Four selected image/trail phases, hold1 | Ordinals0..3 and full-cycle return. |
| 8977–8A56 Spazer | Three growth phases, hold2, repeat only final growth pose | Final target = start+(count−1)×record size. |
| 8A57–8CF6 SpazerWave | Hold2 and centered/outward/return traversal | Shared lane geometry gives10 phases. |
| 8CF7–8D46 Plasma | Entry hold1; mature persistent hold15; skip entry thereafter | Two records, loop to the second. |
| 8D47–8E76 PlasmaWave | Entry hold1; cyclic hold2; skip entry thereafter | One entry plus shared8-phase cycle. |
| 8E77–8F16 charged Power | Two alternating image/trail phases, hold1 | Complete paired cycle. |
| 8F17–912E; 9153–936A charged Wave/IceWave | Upward invisible prelude3; all cycle holds1; paired outward/return traversal | Shared16 records and exact skipped prelude. |
| 912F–9152 charged Ice | Four selected image/trail phases, hold1 | Ordinals0..3 and complete cycle. |
| 936B–94BA charged Spazer | Five paired growth poses, hold1, repeat mature pair | Ten records, loop index10−2. |
| 94BB–9ADA charged SpazerWave | Two paired startup poses, hold1, then paired shared spread cycle | Four startup+20 cycle records. |
| 9ADB–9BEA charged Plasma | Four paired growth poses, hold1, repeat mature pair | Eight records, loop index8−2. |
| 9BEB–9EBA charged PlasmaWave | Same Plasma growth prefix, hold1, then paired shared Wave cycle | Six startup+16 cycle records. |
| 9EBB–9F86 missile/super/link | Persistent-frame reload hold15 | One zero-trail frame per native direction/link identity. |
| 9F87–9FBE Power Bomb | Three selected image phases, normal hold5/fast hold1, no trail | Same image-domain/control structure; independent fuse chooses the fast list. |
| 9FBF–A006 Bomb | Four selected image phases, normal hold5/fast hold1, no trail | Same image-domain/control structure; independent fuse/explosion timing remains untouched. |
| A007–A06A; A0C1–A0F2; A16D–A1A0 | Six selected explosion phases; beam/missile/unused hold3, super hold5; no trail. Beam/missile/super delete; unused repeats | Exact18- or30-update exposure before terminal action. |
| A06B–A0C0 | Five selected bomb-explosion/Plasma-SBA phases, hold2, no trail; bomb deletes, Plasma SBA repeats | Ten-update exposure; chosen terminal behavior retained. |
| A0F3–A116; A119–A13C | Four empty-sprite echo/trail phases, hold2, full cycle | Trail ordinals0..3; empty spritemapA117 itself is not executable mechanics. |
| A13D–A158 Spazer SBA | Three growth/trail phases, hold2, repeat mature phase | Final target and trail ordinals calculated. |
| A159–A16C Wave SBA | Two image/trail phases, hold8, full cycle | Sixteen-update cycle. |

These choices define the selected projectile sequence, not a physical integration formula. Ordinary/charged beam cardinal speed is independently4px/update, so it does not explain the distinct upward exposure delays4/3 or other holds. Choosing a different time/phase/termination policy changes the native projectile performance rather than deriving missing samples. The exception covers exactly this matrix, with all identified shared structure still calculated; it excludes source pixels/spritemap selections, damage, acceleration, bomb fuse, sounds and all other timers.

### Real consumers and confirmation

- Native93:81F0..8237 and SamusProjectileSystem.Animation.RunProjectileInstructionHandler decrement the timer first, then publish duration/sprite/radii/trail and advance the pointer; terminal Delete clears the projectile. SamusBombProjectileSystem.RunProjectileInstructionHandler uses the corresponding duration/sprite/radius/terminal path. Thus exposure timing controls when collision reach changes and when occupied slots disappear, not merely which pixels appear.
- Native93:81D8 and SamusProjectileSystem.TrailsAndCollisions.GetTrailAnimationFrame read the prior installed record's trail field. Trail spawning precedes motion/animation advancement and uses that ordinal for coordinate selection. The focused confirmation deliberately replaces the cached AnimationFrame with BEEF before this actual private consumer and still requires the previous native record's exact phase.
- SamusBombProjectileSystem.RunBombPreInstruction and SamusPowerBombFuse independently select fast animation/explosion from their fuse state. This batch does not alter or exempt those fuse policies. All original assigned instruction selectors, program ranges and independently imported frame bindings remain intact.
- `--lookup-stream-1-projectile-programs` independently traverses the supported original ROM, identifies all1816 duration/trail/opcode/target words,805 timed records and105 complete program domains, and rejects every nonmechanics byte in the original bounded region. It executes each complete native startup and first loop return or deletion through BOTH actual interpreters, comparing timer, pointer, sprite, physical radii and trail at every update, with runtime bus access unavailable. Native-derived duration sums bound each check, not an arbitrary exploratory run length. Result:1582 exact ticks per owner,101 loop returns/four terminal deletions.
- `--lookup-stream-1-projectile-radii` is rerun because the newly derived layout counts are its direct dependency; all805 native pair identities and actual interpreter/edit checks remain required. No unrelated gameplay tests are added.
- Changed production owners: SamusProjectileInstructionDefinitions.cs and the already granted shared ProjectileWaveEnvelopeDefinitions.cs (four-position domain count only). Root must refresh those transitive projectile-closure hashes after Batch99's shared artwork relocation; no additional artwork mutation or exemption. Local Program.cs only wires the focused command; coordinator owns shared integration.
- Final build: 1219 warnings / zero errors. Both focused program and dependent radius commands passed; no whitespace errors reported by git diff --check.

Root integration: Root reviewed native/managed timer-to-radius/sprite/trail-to-loop/delete ordering and exact family policy matrix. Build1454warnings/zeroerrors,Audit0/0;1816native words/805records/105complete programs,1582ticks per projectile/bomb owner,101loops/4deletes includingactual prior-record trail reads and zero runtime reads. Dependent805radius/1610byte/actualinterpreter proof still passes; only redundant XML wording corrected during integration. Master 600 converted/78 retained-mixed/450 pending.


## Batch 101 — complete cannon cover attachment geometry

Mixed complete: SamusArmCannonArtworkCatalog.drawingData. Local totals are 190 converted / 28 mixed / 8 pending. This resolves the 135 coordinate basis bytes left by Batch 54, without completing the separately owned body OAM, body pixels, animation or other artwork fields.

- Pinned bank90 C663–C790 selects one cover orientation/frame, sign-extends its X/Y bytes, subtracts the pose graphics origin, emits one small OBJ and queues one 32-byte tile transfer. Bank92 body OAM supplies the actual cannon-tip sprite origins. Six ten-frame running profiles and four six-frame diagonal moonwalk profiles match the outwardmost upper-body sprite origin along the gun axis exactly, despite changes in part order. Horizontal selection uses X, vertical/diagonal selection uses Y. Stationary profiles use their first body picture, preserving previously established repeated-coordinate aliases even for later mutable-zero OAM selectors. Downward aim selects the terminal vertical body picture during its preceding diagonal transition. Existing 290 byte aliases and 29 eight-pixel horizontal reflections remain calculated.
- The new SamusArmCannonPlacementDefinitions resolves allocation ownership through existing semantic StockPoseDrawingData dispatch, rather than adding a descriptor-address lookup. Named pose cases choose attachment direction/source phase and the narrow joins below. The body constructor binds its immutable installed OAM geometry; standalone file import retains independent supplied placement until this binding. Import compares supplied cover bytes with their body-derived default, retaining only actual differences. Empty maps, zero pointers and altered body coordinates retain the supplied cover output without narrowing the schema or coupling edits. Runtime never materializes a stock replacement coordinate array.
- Reviewed overlay composition only: horizontal outward one-pixel join; standing diagonal-up inward one-pixel join; standing diagonal-down raised one pixel; airborne/crouched upward aim raised one pixel; downward aim outward one pixel plus a four-pixel raise while falling; left falling-up terminal cover lowered three pixels; reversed horizontal moonwalk cover moved inward three pixels. The named terminal/static/retracted source-phase policies are also selected overlay composition. Ordinary body and cannon paths subtract the same graphics origin (managed SamusState.Rendering 535/596 and SamusArmCannonState.Draw 130–134; native C6F0/C717). Pose03 and05 have identical top0/18, OAM, graphics origin6 and upward-diagonal orientation but cover X14/13; pose16 and2C terminal frames share top0/23 and origin8 but cover Y−32/−28. Thus those exact overlay joins are independently selected visual composition; deriving them from unchanged body quantities would just restate the chosen join. Geometry, reflections and repeated phases remain calculated. No pixel, physics or timer exemption follows.
- Retracted visible attachment derives from the small cannon-hand OBJ origin in spin landing A6 frame0, also used by normal landing A4 frame1. Native bank91 B308 displays one frame for4B before FD/4D; B22D displays two frames forA4 before F8/01; B235 displays three forA6 before F8/01. All displayed cover pairs are that shared attachment. The remaining descriptor tails preserve exactly four independent components: first unused pair(6,2) at90:CAD5/CAD6 and later pair(−19,−2) atCAD7/CAD8. Repetition expands these four values to18 byte observations: CAD5–CAD8, CBFF–CC04 and CC0D–CC14. They cannot be reached as native animation pictures but remain observable through the admitted descriptor byte window and independently edited pose pointers. Root approved only this opaque unused placement content, not an unused-data exception generally. Repetition is calculated; no fabricated physical or visible attachment meaning is assigned to those tails.
- Static source diagnostics: csharp/test-temp/cannon-source-geometry.png shows68 bounded native body/cover windows, original Power palette and landing-specific body-origin adjustment; excess descriptor slots are explicitly marked UNUSED COVER TAIL. cannon-source-attachment.csv records the exact OAM source origins and differences. The guarded script cannon-source-geometry.ps1 reads the supplied ROM/native extraction and renders source content only; it does not run gameplay or hunt defects. Its initial diagnostic palette byte-shift truncation was corrected with explicit integer promotion before the reviewed color image; no production data changed.
- Confirmation: Verification build 1,434 warnings / zero errors; --lookup-stream-1-cannon-placement passes all135 direct native body/tail defaults, all608 native output bytes, zero installed stock coordinate overrides, all608 independently edited bytes with exact unchanged canonical content hashes, empty/zero-pointer/shifted body independence and39 actual cover OAM/DMA draws with exact native X/Y. The existing --lookup-stream-1-cannon-drawing passes controls/cost aliases/repetition/reflection, standalone import independence and253 pose/default/arbitrary-pointer/hash/domain checks. Those standalone135 import inputs are not reported as installed runtime fallbacks.
- Granted source ownership: new Assets/SamusArmCannonPlacementDefinitions.cs and narrow Assets/SamusBodyArtworkCatalog.cs constructor binding, plus existing owned cannon catalog, Program.LookupStream1.cs, local Program.cs and this report. Root must record the helper grant and refresh SamusArmCannonArtworkCatalog/SamusBodyArtworkCatalog source hashes in SamusArtworkClosedContractDefinitions, SamusBodyTransferClosedContractDefinitions and VramDmaSourceContracts, including the new helper and its transitive body-OAM dependency in applicable closures.

Root integration: Root viewed68native body/cover windows and reviewed exact join/domain evidence. Build1456warnings/0errors;135direct defaults,608native bytes/608independent edits,zero stock overrides,canonical hashes,empty/zero/shifted body independence and39actual OAM/DMA draws pass. Standalone import/pose proof also passes. Audit source closures refreshed. Master 601 converted/85 retained-mixed/442 pending.
