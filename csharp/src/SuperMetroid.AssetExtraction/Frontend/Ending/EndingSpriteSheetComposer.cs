using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
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
    /// <summary>The visible shooting-star animation frames with a sheet frame of their own: the growing star's steps and the full star.</summary>
    private static IEnumerable<int> StarSheetFrames => Enumerable.Range(1, EndingShootingStarDefinitions.FullStarFrame);

    /// <summary>
    /// What ending sheet <paramref name="sheet"/> composes: its frames coloured in the sprite colours its scene loads, and
    /// shaded in every ending shade (#1512), coloured from <paramref name="palettes"/> and <paramref name="paletteFx"/>.
    /// </summary>
    internal static SpriteSheetSources Sources(SpriteSheetId sheet, EndingObjectArtwork objects, EndingPaletteSet palettes,
        EndingFontAtlas font, IPaletteFxColorSource paletteFx)
    {
        Ensure.NotNull(objects);
        Ensure.NotNull(palettes);
        EndingPage page = Page(sheet);
        byte[] image = EndingScreenImage.Compose(page.Transfers, new EndingTransferArtwork(objects, font, page.Reward));
        int objectSelection = EndingSheetObjectSelectionDefinitions.Of(page.ObjectSheet);
        SpriteArtwork characters = ObjCharacterImage.Page(image, objectSelection);
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(objectSelection);
        List<SheetFrameSource> frames = [.. IntroSpriteSheetComposer.Frames(page.Families(EndingObjectSourceArtwork.Of(objects)), characters, sizes)];
        if (page.Stars)
            frames.AddRange(StarSheetFrames.Select(frame =>
            {
                string name = EndingShootingStarFrames.SheetFrameName(frame);
                return new SheetFrameSource(name, new SpriteFrame(name, [EndingShootingStarParts.Appearance(frame).ToFramePart(0, sizes)]), characters,
                    FrontendSheetPalette.FrameColourGroup.Number);
            }));
        RgbColor[] sprites = page.SpriteColours(palettes);
        return new SpriteSheetSources(frames, sprites, sizes, PartsKeepOwnRows: true)
        {
            Shades = EndingScreenShades.Sprites(EndingScreenShades.Screen(palettes, paletteFx), sprites,
                FrontendSheetPalette.FrameColourGroup.Number * EndingScreenInks.GroupColours),
        };
    }

    /// <summary>The frames ending sheet <paramref name="sheet"/> holds.</summary>
    internal static IEnumerable<string> FrameNames(SpriteSheetId sheet, EndingObjectArtwork objects)
    {
        Ensure.NotNull(objects);
        EndingPage page = Page(sheet);
        IEnumerable<string> families = IntroSpriteSheetComposer.FrameNames(page.Families(EndingObjectSourceArtwork.Of(objects)));
        return page.Stars ? families.Concat(StarSheetFrames.Select(EndingShootingStarFrames.SheetFrameName)) : families;
    }

    /// <summary>The page ending sheet <paramref name="sheet"/> shows.</summary>
    private static EndingPage Page(SpriteSheetId sheet)
    {
        int logoQuarters = EndingSpriteSheets.ShootingSheets.IndexOf(sheet);
        if (logoQuarters >= 0)
            return Shooting(logoQuarters);
        return sheet switch
        {
            SpriteSheetId.EndingCloudSprites => new(EndingObjectSheet.Clouds, EndingReward.Armored, [new EndingTransfer(EndingTransferKind.EscapeClouds)],
                objects => [objects.CloudSprites], Stars: false, EscapeColours),
            SpriteSheetId.EndingExplosionSprites => new(EndingObjectSheet.Explosion, EndingReward.Armored,
                [
                    new EndingTransfer(EndingTransferKind.ExplosionObjects),
                    .. Enum.GetValues<EndingObjectFragmentId>().Select(fragment => new EndingTransfer(EndingTransferKind.ExplosionMap, Fragment: fragment)),
                    new EndingTransfer(EndingTransferKind.EndingFont),
                ],
                objects => [objects.ExplosionSprites, objects.CompletionTextSprites, EndingAnimalEscapeParts.Sprites], Stars: false,
                palettes => SpritesOf(palettes, EndingPaletteId.Explosion)),
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
                SpriteSheetId.MenuPictures or SpriteSheetId.RoomMapPictures or SpriteSheetId.PausePictures or
                SpriteSheetId.AreaMapForegroundCrateria or SpriteSheetId.AreaMapForegroundBrinstar or
                SpriteSheetId.AreaMapForegroundNorfair or SpriteSheetId.AreaMapForegroundWreckedShip or
                SpriteSheetId.AreaMapForegroundMaridia or SpriteSheetId.AreaMapForegroundTourian or
                SpriteSheetId.AreaMapBackgroundCrateria or SpriteSheetId.AreaMapBackgroundBrinstar or
                SpriteSheetId.AreaMapBackgroundNorfair or SpriteSheetId.AreaMapBackgroundWreckedShip or
                SpriteSheetId.AreaMapBackgroundMaridia or SpriteSheetId.AreaMapBackgroundTourian or
                SpriteSheetId.PauseHud or
                SpriteSheetId.IntroScenePictures or SpriteSheetId.IntroNarrationPictures or
                SpriteSheetId.CeresCinematicPictures or SpriteSheetId.EndingCreditsRows or
                SpriteSheetId.EndingPostCreditsPictures or SpriteSheetId.EndingWavySamusPicture or SpriteSheetId.EndingExplosionPictures or
                SpriteSheetId.TitleFloor or SpriteSheetId.CeresFloor or SpriteSheetId.EndingEscapeAFloor or
                SpriteSheetId.EndingEscapeBFloor or SpriteSheetId.EndingPlanetExplosionFloor or
                SpriteSheetId.EndingFlyawayFloor or SpriteSheetId.EndingRewardIconFloor =>
                throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Not an ending sheet."),
            _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
        };
    }

    /// <summary>The post-credits reward page of an ending of <paramref name="reward"/>.</summary>
    private static EndingPage Reward(EndingReward reward) => new(EndingObjectSheet.Reward, reward,
        [new EndingTransfer(EndingTransferKind.CreditsCharacterArt)], objects => [objects.RewardSprites], Stars: true, PostCreditsColours);

    /// <summary>
    /// The shooting scene's page once <paramref name="logoQuarters"/> of the logo's character transfers have replaced its
    /// own; the logo assembles once all have. The reward's characters lie outside this page, so any reward shows it.
    /// </summary>
    private static EndingPage Shooting(int logoQuarters)
    {
        List<EndingTransfer> transfers = [new EndingTransfer(EndingTransferKind.CreditsCharacterArt)];
        int logo = 0;
        for (int index = 0; index < EndingPostShotUploadDefinitions.Count; index++)
        {
            if (EndingPostShotUploads.Get(index).Source == EndingPostShotUploadSource.LogoTiles && logo++ == logoQuarters)
                break;
            transfers.Add(new EndingTransfer(EndingTransferKind.PostShotPart, index));
        }
        bool logoIn = logoQuarters == EndingSpriteSheets.ShootingSheets.Length - 1;
        return new(EndingObjectSheet.RewardFalling, EndingReward.Armored, [.. transfers],
            objects => logoIn ? [objects.RewardSprites, objects.LogoSprites] : [objects.RewardSprites], Stars: true, PostCreditsColours);
    }

    /// <summary>The escape scenes' sprite colours.</summary>
    private static RgbColor[] EscapeColours(EndingPaletteSet palettes) => SpritesOf(palettes, EndingPaletteId.Escape);

    /// <summary>The post-credits scenes' sprite colours, which the credits' end restores.</summary>
    private static RgbColor[] PostCreditsColours(EndingPaletteSet palettes) => SpritesOf(palettes, EndingPaletteId.PostCredits);

    /// <summary>The sprite colours of the whole screen <paramref name="palette"/>.</summary>
    private static RgbColor[] SpritesOf(EndingPaletteSet palettes, EndingPaletteId palette) => palettes[palette].Sprites.ToArray();

    /// <summary>
    /// A page of the ending: the object sheet that selects it, the reward the scene's transfers dress, those transfers in
    /// order, the families it shows, whether the stars show on it, and the sprite colours its scene loads.
    /// </summary>
    private sealed record EndingPage(EndingObjectSheet ObjectSheet, EndingReward Reward, IReadOnlyList<EndingTransfer> Transfers,
        Func<EndingObjectSourceArtwork, IEnumerable<ICinematicSpriteFrames>> Families, bool Stars,
        Func<EndingPaletteSet, RgbColor[]> SpriteColours);
}
