using SuperMetroid.Core.Assets;
using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rooms.Backgrounds;
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
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

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
        ("InstalledSamusFullBodyCycleColors", "Apply" or "Resolve", "palette.Family") =>
            Enum.GetValues<SamusFullBodyCycleFamily>().Select(family => (int)family).ToArray(),
        ("InstalledSamusSuitColors", "Apply" or "Resolve", "suitTableOffset") => [0, 2, 4],
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
        ("MapSpriteSheet", "Draw", "id") => MapSpriteNames.Frames.Select(frame => (int)frame.Id).ToArray(),
        ("PauseReserveTankPresentation", "Draw", "visual") => PauseReserveTankParts.Frames().Select(frame => (int)frame.Id).ToArray(),
        ("TitleGraphicsPresentation", "DrawSprite", "sprite") => TitleSpriteDefinitions.All.Select(sprite => (int)sprite).ToArray(),
        // A drawer does not reveal its family: a pose qualifies when some intro family owns it.
        ("CinematicSpriteSheet", "Draw", "pose.Index") => IntroCaretSpriteDefinitions.Frames
            .Concat(IntroMotherBrainSpriteDefinitions.Frames).Concat(IntroMotherBrainExplosionSpriteDefinitions.Frames)
            .Concat(IntroDiscoveryActorSpriteDefinitions.Frames).Concat(IntroEggEffectSpriteDefinitions.Frames)
            .Concat(IntroRinkaSpriteDefinitions.Frames).Concat(IntroScientistSpriteDefinitions.Frames)
            .Select(frame => frame.Pose.Index).Distinct().ToArray(),
        ("InstalledProjectileTrails", "Resolve", "frame") => [.. ProjectileTrailVisualDefinitions.Frames.Select(frame => (int)frame)],
        ("RoomFxLayer3Pages", "Resolve", "type") => RoomFxLayer3TilemapFormat.Types.Select(type => (int)type).ToArray(),
        ("RoomFxBlendInks", "Apply", "selection") => RoomFxPaletteBlendRules.Ids.Select(id => (int)id).Append(0).ToArray(),
        ("AreaMapPresentation" or "IGameplayPresentationContent", "Resolve", "asset") =>
            [(int)InstalledArtworkId.StandardHudTiles, (int)InstalledArtworkId.KraidBg3RestoreQuarter0,
             (int)InstalledArtworkId.KraidBg3RestoreQuarter1, (int)InstalledArtworkId.KraidBg3RestoreQuarter2,
             (int)InstalledArtworkId.KraidBg3RestoreQuarter3, (int)InstalledArtworkId.EscapeTimerFirstTiles,
             (int)InstalledArtworkId.EscapeTimerSecondTiles],
        ("RoomBackgroundTilemaps", "Get", "tilemap") => Enum.GetValues<RoomBackgroundTilemap>().Select(tilemap => (int)tilemap).ToArray(),
        ("MotherBrainSpecialSpriteArtworkCatalog", "Get", "sheet") => MotherBrainSpecialSpriteArtworkDefinitions.All
            .Select(sheet => (int)sheet.Sheet).ToArray(),
        ("EnemyTileArtworkCatalog", "LoadTo" or "LoadPaletteTo", "definitionId") => EnemyTileSourceDefinitions.All
            .Select(definition => (int)definition.DefinitionId).ToArray(),
        ("EnemyProjectileSpritemapCatalog", "Get", "frame") => EnemyProjectileSpritemapDefinitions.FrameNames
            .Select(name => (int)EnemyProjectileFrames.Identity(name)).Append((int)EnemyProjectileFrameId.Blank).ToArray(),
        ("EnemyProjectileSpritemapCatalog", "GetProgramFrame", "frame") => EnemyProjectilePresentationFrameSets.All
            .SelectToArray(frame => (int)frame.Frame),
        // Contains is deliberately absent: querying an unowned ID is valid and
        // returns false rather than looking up missing artwork.
        _ => null,
    };

    /// <summary>Returns audited finite named identities for a presentation call, or null when no closed set applies.</summary>
    internal static string[]? GetNames(string type, string method, string parameter) => new PresentationParameter(type, method, parameter) switch
    {
        ("RoomLevelArtworks" or "InstalledRoomLevelStreams", "Get", "level.Name") =>
            RoomVisualLayoutSourceDefinitions.All.Select(level => level.Name).ToArray(),
        _ => null,
    };
}
