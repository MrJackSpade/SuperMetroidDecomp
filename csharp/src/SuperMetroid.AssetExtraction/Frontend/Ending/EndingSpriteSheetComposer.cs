using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Composes the ending's sprite sheets (#1392): each holds the families a page of the ending shows, composed over the
/// characters its scene's transfers leave on that page and coloured as the scene loads its palette; the post-credits
/// pages also hold the shooting stars.
/// </summary>
internal static class EndingSpriteSheetComposer
{

    /// <summary>What ending sheet <paramref name="sheet"/> composes.</summary>
    internal static SpriteSheetSources Sources(SpriteSheetId sheet, EndingObjectArtwork objects, EndingPaletteSet palettes,
        EndingFontAtlas font)
    {
        Ensure.NotNull(objects);
        Ensure.NotNull(palettes);
        EndingPage page = Page(sheet);
        byte[] image = EndingScreenImage.Compose(page.Transfers, new EndingScreenArtwork(null, null, objects, font, page.Reward));
        int objectSelection = EndingSheetObjectSelectionDefinitions.Of(page.ObjectSheet);
        SpriteArtwork characters = ObjCharacterImage.Page(image, objectSelection);
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(objectSelection);
        List<SheetFrameSource> frames = [.. IntroSpriteSheetComposer.Frames(page.Families(EndingObjectSourceArtwork.Of(objects)), characters, sizes)];
        if (page.Stars)
            frames.AddRange(EndingShootingStarDefinitions.SheetFrames.Select(frame =>
            {
                string name = EndingShootingStarFrames.SheetFrameName(frame);
                return new SheetFrameSource(name, new SpriteFrame(name, [EndingShootingStarParts.Appearance(frame).ToFramePart(0, sizes)]), characters,
                    FrontendSheetPalette.FramePalette);
            }));
        var colours = new ScreenPalette();
        page.LoadColours(palettes, colours);
        return new SpriteSheetSources(frames, colours.SpriteSets.All.Colors.ToArray(), sizes, PartsKeepOwnRows: true);
    }

    /// <summary>The frames ending sheet <paramref name="sheet"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(SpriteSheetId sheet, EndingObjectArtwork objects)
    {
        Ensure.NotNull(objects);
        EndingPage page = Page(sheet);
        IEnumerable<string> families = IntroSpriteSheetComposer.FrameNames(page.Families(EndingObjectSourceArtwork.Of(objects)));
        return page.Stars ? families.Concat(EndingShootingStarDefinitions.SheetFrames.Select(EndingShootingStarFrames.SheetFrameName)) : families;
    }

    /// <summary>The page ending sheet <paramref name="sheet"/> shows.</summary>
    private static EndingPage Page(SpriteSheetId sheet)
    {
        int logoQuarters = EndingSpriteSheets.ShootingSheets.IndexOf(sheet);
        if (logoQuarters >= 0)
            return Shooting(logoQuarters);
        return sheet switch
        {
            SpriteSheetId.EndingCloudSprites => new(EndingObjectSheet.Clouds, EndingReward.Armored, [new EndingSheetUpload(EndingUploadKind.EscapeClouds)],
                objects => [objects.CloudSprites], Stars: false, EscapeColours),
            SpriteSheetId.EndingExplosionSprites => new(EndingObjectSheet.Explosion, EndingReward.Armored,
                [
                    new EndingSheetUpload(EndingUploadKind.ExplosionObjects),
                    .. Enum.GetValues<EndingObjectFragmentId>().Select(fragment => new EndingObjectFragmentUpload(fragment)),
                    new EndingSheetUpload(EndingUploadKind.EndingFont),
                ],
                objects => [objects.ExplosionSprites, objects.CompletionTextSprites, EndingAnimalEscapeParts.Sprites], Stars: false,
                (palettes, cgram) =>
                {
                    EscapeColours(palettes, cgram);
                    palettes[EndingPaletteId.Explosion].Screen.LoadSpritesTo(cgram);
                }),
            SpriteSheetId.EndingSuitedRewardSprites => Reward(EndingReward.Armored),
            SpriteSheetId.EndingSuitlessRewardSprites => Reward(EndingReward.Suitless),
            SpriteSheetId.EscapeTimer or SpriteSheetId.Grapple or SpriteSheetId.SamusBodyTop or
                SpriteSheetId.SamusBodyBottom or SpriteSheetId.SamusOverlays or SpriteSheetId.SamusAtmosphere or
                SpriteSheetId.SamusDeath or SpriteSheetId.GameOver or SpriteSheetId.FileSelect or
                SpriteSheetId.GameOptions or SpriteSheetId.MapSprites or SpriteSheetId.PauseSprites or
                SpriteSheetId.Title or SpriteSheetId.IntroSprites or SpriteSheetId.CeresFlightSprites or
                SpriteSheetId.CeresDestructionSprites or SpriteSheetId.ZebesRevealSprites or
                SpriteSheetId.EndingShootingSprites or SpriteSheetId.EndingShootingLogoQuarterSprites or
                SpriteSheetId.EndingShootingLogoHalfSprites or
                SpriteSheetId.EndingShootingLogoThreeQuartersSprites or SpriteSheetId.EndingLogoSprites or
                SpriteSheetId.IntroProjectiles or SpriteSheetId.ProjectilesPower or SpriteSheetId.ProjectilesWave or
                SpriteSheetId.ProjectilesIce or SpriteSheetId.ProjectilesIceWave or
                SpriteSheetId.ProjectilesSpazer or SpriteSheetId.ProjectilesSpazerWave or
                SpriteSheetId.ProjectilesSpazerIce or SpriteSheetId.ProjectilesSpazerIceWave or
                SpriteSheetId.ProjectilesPlasma or SpriteSheetId.ProjectilesPlasmaWave or
                SpriteSheetId.ProjectilesPlasmaIce or SpriteSheetId.ProjectilesPlasmaIceWave or
                SpriteSheetId.ProjectilesSpazerPlasmaWave or SpriteSheetId.ProjectilesSpazerPlasmaIce or
                SpriteSheetId.MenuForegroundTiles or SpriteSheetId.MenuMapTiles or
                SpriteSheetId.AreaMapForegroundTilesCrateria or SpriteSheetId.AreaMapForegroundTilesBrinstar or
                SpriteSheetId.AreaMapForegroundTilesNorfair or SpriteSheetId.AreaMapForegroundTilesWreckedShip or
                SpriteSheetId.AreaMapForegroundTilesMaridia or SpriteSheetId.AreaMapForegroundTilesTourian or
                SpriteSheetId.AreaMapBackgroundTilesCrateria or SpriteSheetId.AreaMapBackgroundTilesBrinstar or
                SpriteSheetId.AreaMapBackgroundTilesNorfair or SpriteSheetId.AreaMapBackgroundTilesWreckedShip or
                SpriteSheetId.AreaMapBackgroundTilesMaridia or SpriteSheetId.AreaMapBackgroundTilesTourian or
                SpriteSheetId.PauseMapTiles or SpriteSheetId.PauseInterfaceTiles or SpriteSheetId.PauseHudTiles or
                SpriteSheetId.IntroSceneTiles or SpriteSheetId.IntroFontTiles or
                SpriteSheetId.CeresFlightObjectTiles or SpriteSheetId.CeresIntroObjectTiles or
                SpriteSheetId.ZebesRevealTiles or SpriteSheetId.EndingCreditsFontTiles or
                SpriteSheetId.EndingTextFontTiles or SpriteSheetId.EndingSubtitleTiles or
                SpriteSheetId.EndingLogoTiles or SpriteSheetId.EndingWaitingTiles or
                SpriteSheetId.EndingWavySamusTiles or SpriteSheetId.EndingExplosionTiles or
                SpriteSheetId.TitleFloorTiles or SpriteSheetId.TitleBabyTiles or
                SpriteSheetId.CeresFlightFloorTiles or SpriteSheetId.EndingEscapeAFloorTiles or
                SpriteSheetId.EndingEscapeBFloorTiles or SpriteSheetId.EndingPlanetExplosionFloorTiles or
                SpriteSheetId.EndingFlyawayFloorTiles or SpriteSheetId.EndingRewardIconFloorTiles =>
                throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Not an ending sheet."),
            _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
        };
    }

    /// <summary>The post-credits reward page of an ending of <paramref name="reward"/>.</summary>
    private static EndingPage Reward(EndingReward reward) => new(EndingObjectSheet.Reward, reward,
        [new EndingSheetUpload(EndingUploadKind.CreditsCharacterArt)], objects => [objects.RewardSprites], Stars: true, PostCreditsColours);

    /// <summary>
    /// The shooting scene's page once <paramref name="logoQuarters"/> of the logo's character transfers have replaced its
    /// own; the logo assembles once all have. The reward's characters lie outside this page, so any reward shows it.
    /// </summary>
    private static EndingPage Shooting(int logoQuarters)
    {
        List<EndingUpload> transfers = [new EndingSheetUpload(EndingUploadKind.CreditsCharacterArt)];
        int logo = 0;
        for (int index = 0; index < EndingPostShotUploadDefinitions.Count; index++)
        {
            if (EndingPostShotUploads.Get(index).Source == EndingPostShotUploadSource.LogoTiles && logo++ == logoQuarters)
                break;
            transfers.Add(new EndingChunkUpload(EndingUploadKind.PostShotTransfer, index));
        }
        bool logoIn = logoQuarters == EndingSpriteSheets.ShootingSheets.Length - 1;
        return new(EndingObjectSheet.RewardFalling, EndingReward.Armored, [.. transfers],
            objects => logoIn ? [objects.RewardSprites, objects.LogoSprites] : [objects.RewardSprites], Stars: true, PostCreditsColours);
    }

    /// <summary>The escape scenes' colours.</summary>
    private static void EscapeColours(EndingPaletteSet palettes, ScreenPalette colours) =>
        palettes[EndingPaletteId.Escape].Screen.LoadTo(colours);

    /// <summary>The post-credits colours: the credits' background half under the post-credits scenes' own.</summary>
    private static void PostCreditsColours(EndingPaletteSet palettes, ScreenPalette colours)
    {
        palettes[EndingPaletteId.Credits].Screen.LoadBackgroundsTo(colours);
        ScreenColours postCredits = palettes[EndingPaletteId.PostCredits].Screen;
        postCredits.LoadTo(colours, ScreenColourGroup.EndingPostCreditsRestoredBackground);
        postCredits.LoadTo(colours, ScreenColourGroup.EndingPostCreditsRestoredSprites);
    }

    /// <summary>
    /// A page of the ending: the object sheet that selects it, the reward the scene's transfers dress, those transfers in
    /// order, the families it shows, whether the stars show on it, and how the scene loads its colours.
    /// </summary>
    private sealed record EndingPage(EndingObjectSheet ObjectSheet, EndingReward Reward, IReadOnlyList<EndingUpload> Transfers,
        Func<EndingObjectSourceArtwork, IEnumerable<ICinematicSpriteFrames>> Families, bool Stars,
        Action<EndingPaletteSet, ScreenPalette> LoadColours);
}
