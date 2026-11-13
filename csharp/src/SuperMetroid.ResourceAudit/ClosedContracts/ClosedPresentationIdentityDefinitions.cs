using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.World.RoomStates;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Kraid;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.World.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Sparse identities whose ownership cannot be expressed as a contiguous index range.</summary>
internal static class ClosedPresentationIdentityDefinitions
{
    /// <summary>
    /// Returns audited finite argument identities for a presentation call, or null when no closed set applies. An enum
    /// identity is listed by its own value, not a cartridge number: the audit compares it with the constant value the
    /// compiler binds for an enum-member argument.
    /// </summary>
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
        ("SamusFullBodyCycleColours", "Apply" or "Resolve", "palette.Family") =>
            Enum.GetValues<SamusFullBodyCycleFamily>().Select(family => (int)family).ToArray(),
        ("InstalledSamusSuitColors", "Apply" or "Resolve", "shown" or "suit") =>
            Enum.GetValues<SamusSuit>().Select(suit => (int)suit).ToArray(),
        ("EnemyAuxiliaryColorCatalog", "Resolve", "palette") => EnemyAuxiliaryColorDefinitions.All
            .Select(definition => (int)definition.Id).ToArray(),
        ("KraidColorCatalog", "Resolve", "source") => Enum.GetValues<KraidPaletteSource>().Select(id => (int)id).ToArray(),
        ("DachoraColorCatalog", "Resolve", "phase") => Enum.GetValues<DachoraColourPhase>().Select(id => (int)id).ToArray(),
        ("ShitroidColorCatalog", "TargetColor", "target") => Enum.GetValues<ShitroidColorTarget>().Select(id => (int)id).ToArray(),
        ("EscapeTypewriterPresentation", "Get", "id") => Enum.GetValues<EscapeTypewriterMessageId>()
            .Where(id => id != EscapeTypewriterMessageId.None).Select(id => (int)id).ToArray(),
        ("IntroNarrationPresentation", "GetLines" or "Compile", "page") =>
            Enum.GetValues<IntroNarrationPageId>().Select(id => (int)id).ToArray(),
        ("EndingTextPresentation", "Compile", "sequence") => Enum.GetValues<EndingTextSequence>().Select(id => (int)id).ToArray(),
        ("MapSpriteSheet", "Draw", "id") => MapSpriteArtwork.FrameNames.Select(frame => (int)frame.Id).ToArray(),
        ("PauseReserveTankPresentation", "Draw", "visual") => PauseReserveTankParts.Frames().Select(frame => (int)frame.Id).ToArray(),
        ("TitleSpriteDrawing", "DrawSprite", "sprite") => TitleGraphicsFormat.Sprites.Select(sprite => (int)sprite).ToArray(),
        // A drawer does not reveal its family: a pose qualifies when some intro family owns it.
        ("CinematicSpriteSheet", "Draw", "pose.Index") => IntroCaretSpriteDefinitions.Frames
            .Concat(IntroNextPageArrowSpriteDefinitions.Frames)
            .Concat(IntroMotherBrainSpriteDefinitions.Frames).Concat(IntroMotherBrainExplosionSpriteDefinitions.Frames)
            .Concat(IntroDiscoveryActorSpriteDefinitions.Frames).Concat(IntroEggEffectSpriteDefinitions.Frames)
            .Concat(IntroRinkaSpriteDefinitions.Frames).Concat(IntroScientistSpriteDefinitions.Frames)
            .Select(frame => frame.Pose.Index).Distinct().ToArray(),
        ("InstalledProjectileTrails", "Resolve", "frame") => [.. ProjectileTrailVisualDefinitions.Frames.Select(frame => (int)frame)],
        ("InstalledXrayOverlays", "TankBlock", "tank") =>
            [(int)SuperMetroid.Core.Rooms.RoomObjects.InWorldCollectibleKind.EnergyTank, (int)SuperMetroid.Core.Rooms.RoomObjects.InWorldCollectibleKind.MissileTank,
                (int)SuperMetroid.Core.Rooms.RoomObjects.InWorldCollectibleKind.SuperMissileTank, (int)SuperMetroid.Core.Rooms.RoomObjects.InWorldCollectibleKind.PowerBombTank],
        ("RoomFxLayer3Pages", "Resolve", "type") => RoomFxLayer3TilemapFormat.Types.Select(type => (int)type).ToArray(),
        ("RoomFxBlendInks", "Inks", "selection") => RoomFxPaletteBlendDocumentRules.Ids.Select(id => (int)id).Append(0).ToArray(),
        ("MotherBrainSpecialSpriteArtworkCatalog", "Get", "sheet") => MotherBrainSpecialSpriteSheetDefinitions.All
            .Select(sheet => (int)sheet.Sheet).ToArray(),
        ("EnemyTileSourceArtwork", "LoadTo", "definitionId") => EnemyTileSourceDefinitions.All
            .Select(definition => (int)definition.DefinitionId).ToArray(),
        ("EnemyTileSourceArtwork", "PaletteOf" or "TryGetPalette", "enemy") => EnemyTileSourceDefinitions.All
            .Select(definition => (int)definition.DefinitionId).ToArray(),
        ("EnemyProjectileSpritemapCatalog", "Get", "frame") => EnemyProjectileSpritemapDefinitions.FrameNames
            .Select(name => (int)EnemyProjectileFrames.Identity(name)).Append((int)EnemyProjectileFrameId.Blank).ToArray(),
        ("EnemyProjectileSpritemapCatalog", "GetProgramFrame", "frame") => EnemyProjectilePresentationFrameSets.All
            .SelectToArray(frame => (int)frame.Frame),
        // The import pictures every floor animation frame in every Ceres floor (#1536).
        ("RoomFloorSheet", "Picture", "frame") => Enum.GetValues<CeresFloorAnimationFrame>().SelectToArray(frame => (int)frame),
        // A form's message is one the message box shows, which the import pictures in every form it takes (#1536).
        ("MessageBoxSheet", "Picture", "shown.Message") => GameplayMessageArtwork.Forms().Select(form => (int)form.Message).Distinct().ToArray(),
        // Only the effects that show artwork are pictured (#1536, #1706).
        ("RoomEffectPictures", "Artwork", "type") => Enum.GetValues<RoomFxType>().Where(RoomFxOverlayState.ShowsPage)
            .Select(type => (int)type).ToArray(),
        // A sky is installed for each variant some retail room state's scrolling-sky room main reads (#1536).
        ("InstalledRoomArtwork", "Sky", "variant") => RetailRooms.All.SelectMany(RoomStateLists.GetStates)
            .Where(state => ScrollingSkyState.IsScrollingSkyRoomMain(state.MainBehavior))
            .Select(state => (int)ScrollingSkyState.VariantOf(state.MainBehavior)).Distinct().ToArray(),
        // Contains is deliberately absent: querying an unowned ID is valid and
        // returns false rather than looking up missing artwork.
        _ => null,
    };

    /// <summary>Returns audited finite named identities for a presentation call, or null when no closed set applies.</summary>
    internal static string[]? GetNames(string type, string method, string parameter) => new PresentationParameter(type, method, parameter) switch
    {
        ("RoomLevelArtworks" or "InstalledRoomLevelStreams", "Get", "level.Name") =>
            RoomVisualLayoutSourceDefinitions.All.Select(level => level.Name).ToArray(),
        // A spawned object draws a frame some spawner's objects show: on its spawner's own sheet, on a set's shared sheet
        // with every encounter's own frames and every common spawner's objects, or on a sheet of its own (#1536). A picture
        // of an artwork sequence's step is named after its frame; a constant naming one is not owned here.
        ("SpawnedObjectSheet", "Frame", "picture") => SpawnedObjectSheetComposer.FrameNames(SpawnedObjectOwner.Shared,
                SpawnedObjectFrameDefinitions.ByEnemy.Keys.Concat(EncounterFrameDefinitions.ByEnemy.Keys).Distinct(),
                SpawnedObjectFrameDefinitions.SharedByEnemy.Keys, SpawnedObjectFrameDefinitions.Common)
            .Concat(SpawnedObjectFrameDefinitions.BySheet.Keys.SelectMany(owner => SpawnedObjectSheetComposer.FrameNames(owner, [], [], [])))
            .Distinct(StringComparer.Ordinal).ToArray(),
        _ => null,
    };
}
