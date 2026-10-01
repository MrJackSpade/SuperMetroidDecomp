using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>Sparse identities whose ownership cannot be expressed as a contiguous index range.</summary>
internal static class ClosedPresentationIdentityDefinitions
{
    internal static int[]? Get(string type, string method, string parameter) => (type, method, parameter) switch
    {
        ("GameplayMessageTitlePresentation", "Build", "messageId") =>
            GameplayMessageTitleDefinitions.MessageIds.ToArray().Select(id => (int)id).ToArray(),
        ("GameplayMessagePanelPresentation", "Build", "messageId") =>
            GameplayMessagePanelDefinitions.MessageIds.ToArray().Select(id => (int)id).ToArray(),
        ("GameplayMessageNoticePresentation", "Build", "messageId") =>
            GameplayMessageNoticeDefinitions.MessageIds.ToArray().Select(id => (int)id).ToArray(),
        ("GameplayMessageNoticePresentation", "ApplySelection", "messageId") =>
            [(int)GameplayMessageId.SaveConfirmation, (int)GameplayMessageId.GunshipSaveConfirmation],
        ("SamusFullBodyCycleColorCatalog", "Apply" or "Resolve", "pointer") => FullBodyPointers(),
        ("SamusSuitColorCatalog", "Apply" or "Resolve", "suitTableOffset") => [0, 2, 4],
        ("EnemyAuxiliaryColorCatalog", "Resolve", "palette") => EnemyAuxiliaryColorDefinitions.All.ToArray()
            .Select(definition => (int)definition.Id).ToArray(),
        ("KraidColorCatalog", "Resolve", "source") => Enum.GetValues<KraidPaletteSource>().Select(id => (int)id).ToArray(),
        ("DachoraColorCatalog", "Resolve", "phase") => Enum.GetValues<DachoraPalettePhase>().Select(id => (int)id).ToArray(),
        ("ShitroidColorCatalog", "TargetColor", "target") => Enum.GetValues<ShitroidColorTarget>().Select(id => (int)id).ToArray(),
        ("EscapeTypewriterPresentation", "Get", "id") => Enum.GetValues<EscapeTypewriterProgramId>()
            .Where(id => id != EscapeTypewriterProgramId.None).Select(id => (int)id).ToArray(),
        ("IntroNarrationPresentation", "GetLines" or "Compile", "page") =>
            Enum.GetValues<IntroNarrationPageId>().Select(id => (int)id).ToArray(),
        ("EndingTextPresentation", "Compile", "sequence") => Enum.GetValues<EndingTextSequence>().Select(id => (int)id).ToArray(),
        ("MapSpriteCatalog", "Draw", "id") => MapSpriteDefinitions.Frames.ToArray().Select(frame => (int)frame.NativeId).ToArray(),
        ("PauseReserveTankPresentation", "Draw", "nativeIdentity") => PauseReserveTankDefinitions.Frames().Select(frame => (int)frame.Id).ToArray(),
        ("TitleGraphicsPresentation", "DrawSprite", "pointer") => TitleSpriteDefinitions.NativePointers.ToArray().Select(pointer => (int)pointer).ToArray(),
        ("IntroCaretSpritePresentation", "Draw", "pointer") => IntroCaretSpriteDefinitions.Frames.ToArray().Select(frame => (int)frame.Pointer).ToArray(),
        ("IntroMotherBrainSpritePresentation", "Draw", "pointer") => IntroMotherBrainSpriteDefinitions.Frames.ToArray().Select(frame => (int)frame.Pointer).ToArray(),
        ("IntroMotherBrainExplosionSpritePresentation", "Draw", "pointer") => IntroMotherBrainExplosionSpriteDefinitions.Frames.ToArray().Select(frame => (int)frame.Pointer).ToArray(),
        ("ProjectileTrailCatalog", "Resolve", "frame") => ProjectileTrailVisualDefinitions.Frames.ToArray().Select(frame => (int)frame).ToArray(),
        ("ProjectileTrailCatalog", "ResolveCurrent", "nextInstruction") => ProjectileTrailVisualDefinitions.Frames.ToArray()
            .Select(frame => (int)unchecked((ushort)(frame + 4))).Concat(new[] { (int)ProjectileTrailDefinitions.Empty,
                ProjectileTrailDefinitions.LeftIce, ProjectileTrailDefinitions.RightIce, ProjectileTrailDefinitions.Wave,
                ProjectileTrailDefinitions.Missile }).Distinct().ToArray(),
        ("ProjectileFrameBindingCatalog", "Resolve", "instructionPointer") => SamusProjectileRadiusDefinitions.TimedRecordPointers
            .Select(pointer => (int)pointer).ToArray(),
        ("ProjectileSpriteCatalog", "Draw", "id") => ProjectileSpriteDefinitions.NativePointers.ToArray().Select(pointer => (int)pointer).ToArray(),
        ("RoomFxLayer3TilemapCatalog", "Resolve", "type") => RoomFxLayer3TilemapFormat.Types.Select(type => (int)type).ToArray(),
        ("RoomFxPaletteBlendCatalog", "Apply", "selection") => RoomFxPaletteBlendDefinitions.Ids.Select(id => (int)id).Append(0).ToArray(),
        ("RoomFxPaletteBlendCatalog", "Resolve", "selection") => RoomFxPaletteBlendDefinitions.Ids.Select(id => (int)id).ToArray(),
        ("RoomCharacterAtlasCatalog", "Get", "sourceAddress") => TilesetSources(definition => definition.CharacterAddress),
        ("RoomMetatileCatalog", "Get", "sourceAddress") => TilesetSources(definition => definition.BlockDefinitionsAddress),
        ("RoomStaticPaletteCatalog", "Get", "sourceAddress") => TilesetSources(definition => definition.PaletteAddress),
        ("AreaMapPresentationCatalog", "Resolve", "asset") =>
            [(int)VramAssetId.StandardHudTiles, (int)VramAssetId.KraidBg3RestoreQuarter0,
             (int)VramAssetId.KraidBg3RestoreQuarter1, (int)VramAssetId.KraidBg3RestoreQuarter2,
             (int)VramAssetId.KraidBg3RestoreQuarter3, (int)VramAssetId.EscapeTimerFirstTiles,
             (int)VramAssetId.EscapeTimerSecondTiles],
        ("RoomBackgroundTilemapCatalog", "Get", "sourceAddress") => RoomBackgroundTilemapSources.All.ToArray(),
        ("RoomVisualLayoutCatalog", "Get", "sourceAddress") => RoomVisualLayoutSourceDefinitions.All.ToArray(),
        // Contains is deliberately absent: querying an unowned ID is valid and
        // returns false rather than looking up missing artwork.
        _ => null,
    };

    private static int[] FullBodyPointers() =>
        new[] { SamusFullBodyCycleFamily.SpeedBooster, SamusFullBodyCycleFamily.ScrewAttack,
            SamusFullBodyCycleFamily.StoredShine, SamusFullBodyCycleFamily.ActiveShinespark }
        .SelectMany(family => Enumerable.Range(0, SamusFullBodyCycleColorFormat.SuitCount)
            .SelectMany(suit => Enumerable.Range(0, SamusFullBodyCycleColorFormat.ShadesPerSuit)
                .Select(shade => (int)SamusFullBodyCycleColorFormat.Pointer(family, suit, shade))))
        .Distinct().ToArray();

    private static int[] TilesetSources(Func<TilesetDefinition, int> select) => Enumerable.Range(0, RoomTilesetDefinitions.Count)
        .Select(index => select(RoomTilesetDefinitions.Get((byte)index))).Distinct().ToArray();
}
