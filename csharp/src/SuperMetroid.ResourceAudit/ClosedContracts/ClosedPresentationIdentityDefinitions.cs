using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Sparse identities whose ownership cannot be expressed as a contiguous index range.</summary>
internal static class ClosedPresentationIdentityDefinitions
{
    /// <summary>Returns audited finite argument identities for a presentation call, or null when no closed set applies.</summary>
    internal static int[]? Get(string type, string method, string parameter) => new PresentationParameter(type, method, parameter) switch
    {
        ("GameplayMessageTitlePresentation", "Build", "messageId") =>
            GameplayMessageTitleDefinitions.MessageIds.SelectToArray(id => (int)id),
        ("GameplayMessagePanelPresentation", "Build", "messageId") =>
            GameplayMessagePanelDefinitions.MessageIds.SelectToArray(id => (int)id),
        ("GameplayMessageNoticePresentation", "Build", "messageId") =>
            GameplayMessageNoticeDefinitions.MessageIds.SelectToArray(id => (int)id),
        ("GameplayMessageNoticePresentation", "ApplySelection", "messageId") =>
            [(int)GameplayMessageId.SaveConfirmation, (int)GameplayMessageId.GunshipSaveConfirmation],
        ("SamusFullBodyCycleColorCatalog", "Apply" or "Resolve", "palette.Family") =>
            Enum.GetValues<SamusFullBodyCycleFamily>().Select(family => (int)family).ToArray(),
        ("SamusSuitColorCatalog", "Apply" or "Resolve", "suitTableOffset") => [0, 2, 4],
        ("EnemyAuxiliaryColorCatalog", "Resolve", "palette") => EnemyAuxiliaryColorDefinitions.All
            .Select(definition => (int)definition.Id).ToArray(),
        ("KraidColorCatalog", "Resolve", "source") => Enum.GetValues<KraidPaletteSource>().Select(id => (int)id).ToArray(),
        ("DachoraColorCatalog", "Resolve", "phase") => Enum.GetValues<DachoraPalettePhase>().Select(id => (int)id).ToArray(),
        ("ShitroidColorCatalog", "TargetColor", "target") => Enum.GetValues<ShitroidColorTarget>().Select(id => (int)id).ToArray(),
        ("EscapeTypewriterPresentation", "Get", "id") => Enum.GetValues<EscapeTypewriterProgramId>()
            .Where(id => id != EscapeTypewriterProgramId.None).Select(id => (int)id).ToArray(),
        ("IntroNarrationPresentation", "GetLines" or "Compile", "page") =>
            Enum.GetValues<IntroNarrationPageId>().Select(id => (int)id).ToArray(),
        ("EndingTextPresentation", "Compile", "sequence") => Enum.GetValues<EndingTextSequence>().Select(id => (int)id).ToArray(),
        ("MapSpriteCatalog", "Draw", "id") => MapSpriteDefinitions.Frames.Select(frame => (int)frame.Id).ToArray(),
        ("PauseReserveTankPresentation", "Draw", "nativeIdentity") => PauseReserveTankDefinitions.Frames().Select(frame => (int)frame.Id).ToArray(),
        ("TitleGraphicsPresentation", "DrawSprite", "sprite") => TitleSpriteDefinitions.All.Select(sprite => (int)sprite).ToArray(),
        ("IntroCaretSpritePresentation", "Draw", "pose.Index") => IntroCaretSpriteDefinitions.Frames.Select(frame => frame.Pose.Index).ToArray(),
        ("IntroMotherBrainSpritePresentation", "Draw", "pose.Index") => IntroMotherBrainSpriteDefinitions.Frames.Select(frame => frame.Pose.Index).ToArray(),
        ("IntroMotherBrainExplosionSpritePresentation", "Draw", "pose.Index") => IntroMotherBrainExplosionSpriteDefinitions.Frames.Select(frame => frame.Pose.Index).ToArray(),
        ("ProjectileTrailCatalog", "Resolve", "frame") => [.. ProjectileTrailVisualDefinitions.Frames.Select(frame => (int)frame)],
        ("RoomFxLayer3TilemapCatalog", "Resolve", "type") => RoomFxLayer3TilemapFormat.Types.Select(type => (int)type).ToArray(),
        ("RoomFxPaletteBlendCatalog", "Apply", "selection") => RoomFxPaletteBlendDefinitions.Ids.Select(id => (int)id).Append(0).ToArray(),
        ("RoomCharacterAtlasCatalog", "Get", "sourceAddress") => TilesetSources(definition => definition.CharacterAddress),
        ("RoomMetatileCatalog", "Get", "sourceAddress") => TilesetSources(definition => definition.BlockDefinitionsAddress),
        ("RoomStaticPaletteCatalog", "Get", "sourceAddress") => TilesetSources(definition => definition.PaletteAddress),
        ("AreaMapPresentationCatalog" or "IGameplayPresentationContent", "Resolve", "asset") =>
            [(int)VramAssetId.StandardHudTiles, (int)VramAssetId.KraidBg3RestoreQuarter0,
             (int)VramAssetId.KraidBg3RestoreQuarter1, (int)VramAssetId.KraidBg3RestoreQuarter2,
             (int)VramAssetId.KraidBg3RestoreQuarter3, (int)VramAssetId.EscapeTimerFirstTiles,
             (int)VramAssetId.EscapeTimerSecondTiles],
        ("RoomBackgroundTilemapCatalog", "Get", "sourceAddress") => RoomBackgroundTilemapSources.All.ToArray(),
        ("RoomVisualLayoutCatalog", "Get", "sourceAddress") => RoomVisualLayoutSourceDefinitions.All.ToArray(),
        ("RoomLevelStreamCatalog", "Get", "sourceAddress") => RoomVisualLayoutSourceDefinitions.All.ToArray(),
        ("MotherBrainSpecialSpriteArtworkCatalog", "Get", "sourceAddress") => MotherBrainSpecialSpriteArtworkDefinitions.All
            .Select(sheet => sheet.SourceAddress).ToArray(),
        ("EnemyTileArtworkCatalog", "LoadTo" or "LoadPaletteTo", "definitionId") => EnemyTileSourceDefinitions.All
            .Select(definition => (int)definition.DefinitionId).ToArray(),
        ("EnemyProjectileSpritemapCatalog", "Get", "frame") => EnemyProjectileSpritemapDefinitions.FrameNames
            .Select(name => (int)EnemyProjectileFrames.Identity(name)).Append((int)EnemyProjectileFrameId.Blank).ToArray(),
        ("EnemyProjectileSpritemapCatalog", "GetProgramFrame", "operandAddress") => EnemyProjectilePresentationFrameSets.All
            .SelectToArray(frame => (int)frame.OperandAddress),
        // Contains is deliberately absent: querying an unowned ID is valid and
        // returns false rather than looking up missing artwork.
        _ => null,
    };

    /// <summary>Collects distinct cartridge source addresses from the closed room tileset definitions.</summary>
    private static int[] TilesetSources(Func<TilesetDefinition, int> select) => Enumerable.Range(0, RoomTilesetDefinitions.Count)
        .Select(index => select(RoomTilesetDefinitions.Get((byte)index))).Distinct().ToArray();
}
