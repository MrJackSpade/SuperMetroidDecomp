using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Composes the ending's sprite sheets (#1392): each holds the families a page of the ending shows, composed over the
/// characters its scene's transfers leave on that page and coloured as the scene loads its palette; the post-credits
/// pages also hold the shooting stars.
/// </summary>
internal static class EndingSpriteSheetComposer
{
    /// <summary>CGRAM's first sprite colour.</summary>
    private const int SpriteColours = 128;

    /// <summary>What ending sheet <paramref name="sheet"/> composes.</summary>
    internal static SpriteSheetSources Sources(SpriteSheetId sheet, EndingObjectArtworkCatalog objects, EndingPaletteCatalog palettes,
        EndingFontAtlas font)
    {
        Ensure.NotNull(objects);
        Ensure.NotNull(palettes);
        EndingPage page = Page(sheet);
        byte[] image = EndingScreenImage.Compose(page.Transfers, new EndingScreenArtwork(null, null, objects, font, page.Reward));
        byte objectSelection = EndingCreditsPresentation.ObjectSelection(page.ObjectSheet);
        SpriteArtwork characters = ObjCharacterImage.Page(image, objectSelection);
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(objectSelection);
        List<SheetFrameSource> frames = [.. IntroSpriteSheetComposer.Frames(page.Families(objects), characters, sizes)];
        if (page.Stars)
            frames.AddRange(EndingShootingStarDefinitions.SheetFrames.Select(frame =>
            {
                string name = EndingShootingStarDefinitions.SheetFrameName(frame);
                return new SheetFrameSource(name, new SpriteFrame(name, [ObjPartFrames.Part(SnesSpritemapXWord.Create(0, large: false),
                    SnesSpritemapYByte.Create(0), EndingShootingStarDefinitions.Attribute(frame), 0, sizes)]), characters,
                    FrontendSheetPalette.FramePalette);
            }));
        var colours = new ScreenPalette();
        page.LoadColours(palettes, colours);
        return new SpriteSheetSources(frames, colours.Colors[SpriteColours..].ToArray(), sizes, PartsKeepOwnRows: true);
    }

    /// <summary>The frames ending sheet <paramref name="sheet"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(SpriteSheetId sheet, EndingObjectArtworkCatalog objects)
    {
        Ensure.NotNull(objects);
        EndingPage page = Page(sheet);
        IEnumerable<string> families = IntroSpriteSheetComposer.FrameNames(page.Families(objects));
        return page.Stars ? families.Concat(EndingShootingStarDefinitions.SheetFrames.Select(EndingShootingStarDefinitions.SheetFrameName)) : families;
    }

    /// <summary>The page ending sheet <paramref name="sheet"/> shows.</summary>
    private static EndingPage Page(SpriteSheetId sheet)
    {
        int logoQuarters = EndingSpriteSheets.ShootingSheets.IndexOf(sheet);
        if (logoQuarters >= 0)
            return Shooting(logoQuarters);
        return sheet switch
        {
            SpriteSheetId.EndingCloudSprites => new(EndingObjectSheet.Clouds, EndingReward.Armored, [new(EndingUploadKind.EscapeClouds, 0, null)],
                objects => [objects.CloudSprites], Stars: false, EscapeColours),
            SpriteSheetId.EndingExplosionSprites => new(EndingObjectSheet.Explosion, EndingReward.Armored,
                [
                    new(EndingUploadKind.ExplosionObjects, 0, null),
                    .. Enum.GetValues<EndingObjectFragmentId>().Select(fragment => new EndingUpload(EndingUploadKind.ObjectFragment, 0, null, fragment)),
                    new(EndingUploadKind.EndingFont, 0, null),
                ],
                objects => [objects.ExplosionSprites, objects.CompletionTextSprites, EndingAnimalEscapeDefinitions.Sprites], Stars: false,
                (palettes, cgram) =>
                {
                    EscapeColours(palettes, cgram);
                    palettes[EndingPaletteId.Explosion].LoadTo(cgram.Sprites, palettes[EndingPaletteId.Explosion].FirstColorFor(cgram.Sprites));
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
                SpriteSheetId.TitleMode7Tiles or SpriteSheetId.TitleBabyTiles or
                SpriteSheetId.CeresFlightMode7Tiles or SpriteSheetId.EndingEscapeAMode7Tiles or
                SpriteSheetId.EndingEscapeBMode7Tiles or SpriteSheetId.EndingPlanetExplosionMode7Tiles or
                SpriteSheetId.EndingFlyawayMode7Tiles or SpriteSheetId.EndingRewardIconMode7Tiles =>
                throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Not an ending sheet."),
            _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
        };
    }

    /// <summary>The post-credits reward page of an ending of <paramref name="reward"/>.</summary>
    private static EndingPage Reward(EndingReward reward) => new(EndingObjectSheet.Reward, reward,
        [new(EndingUploadKind.CreditsCharacterArt, 0, null)], objects => [objects.RewardSprites], Stars: true, PostCreditsColours);

    /// <summary>
    /// The shooting scene's page once <paramref name="logoQuarters"/> of the logo's character transfers have replaced its
    /// own; the logo assembles once all have. The reward's characters lie outside this page, so any reward shows it.
    /// </summary>
    private static EndingPage Shooting(int logoQuarters)
    {
        List<EndingUpload> transfers = [new(EndingUploadKind.CreditsCharacterArt, 0, null)];
        int logo = 0;
        for (int index = 0; index < EndingPostShotUploadDefinitions.Count; index++)
        {
            if (EndingPostShotUploadDefinitions.Get(index).Source == EndingPostShotUploadSource.LogoTiles && logo++ == logoQuarters)
                break;
            transfers.Add(new(EndingUploadKind.PostShotTransfer, index, null));
        }
        bool logoIn = logoQuarters == EndingSpriteSheets.ShootingSheets.Length - 1;
        return new(EndingObjectSheet.RewardFalling, EndingReward.Armored, [.. transfers],
            objects => logoIn ? [objects.RewardSprites, objects.LogoSprites] : [objects.RewardSprites], Stars: true, PostCreditsColours);
    }

    /// <summary>The escape scenes' colours.</summary>
    private static void EscapeColours(EndingPaletteCatalog palettes, ScreenPalette colours) =>
        palettes[EndingPaletteId.Escape].LoadTo(colours.All, 0);

    /// <summary>The post-credits colours: the credits' background half under the post-credits scenes' own.</summary>
    private static void PostCreditsColours(EndingPaletteCatalog palettes, ScreenPalette colours)
    {
        palettes[EndingPaletteId.Credits].LoadTo(colours.Background, 0);
        ColourRun restored = colours.All.Slice(EndingColourRowDefinitions.PostCreditsKeptColours,
            ScreenPalette.ColorCount - EndingColourRowDefinitions.PostCreditsKeptColours);
        palettes[EndingPaletteId.PostCredits].LoadTo(restored, palettes[EndingPaletteId.PostCredits].FirstColorFor(restored));
    }

    /// <summary>
    /// A page of the ending: the object sheet that selects it, the reward the scene's transfers dress, those transfers in
    /// order, the families it shows, whether the stars show on it, and how the scene loads its colours.
    /// </summary>
    private sealed record EndingPage(EndingObjectSheet ObjectSheet, EndingReward Reward, IReadOnlyList<EndingUpload> Transfers,
        Func<EndingObjectArtworkCatalog, IEnumerable<ICinematicSpriteFrames>> Families, bool Stars,
        Action<EndingPaletteCatalog, ScreenPalette> LoadColours);
}
