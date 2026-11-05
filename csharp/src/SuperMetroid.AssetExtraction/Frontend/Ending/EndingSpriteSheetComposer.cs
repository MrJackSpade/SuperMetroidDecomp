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
/// Composes the ending's sprite sheets (#1392, #1513): each holds the families a scene of the ending shows, each family
/// composed over the characters the scene's transfers give it and coloured as the scene loads its palette; the
/// post-credits sheets also hold the shooting stars. The shooting scene's sheet holds the logo the scene ends on, composed
/// from the logo's own characters: no sheet shows a page partly overwritten by the next scene's transfers.
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
        var transferArtwork = new EndingTransferArtwork(objects, font, page.Reward);
        int objectSelection = EndingSheetObjectSelectionDefinitions.Of(page.ObjectSheet);
        ScreenObjectSizes sizes = ObjectSelectionCodec.Sizes(objectSelection);
        EndingObjectSourceArtwork sourceArtwork = EndingObjectSourceArtwork.Of(objects);
        SpriteArtwork characters = Characters(page.Transfers, transferArtwork, objectSelection);
        List<SheetFrameSource> frames = [.. IntroSpriteSheetComposer.Frames(page.Families(sourceArtwork), characters, sizes)];
        if (page.Logo)
            frames.AddRange(IntroSpriteSheetComposer.Frames([sourceArtwork.LogoSprites],
                Characters(LogoTransfers, transferArtwork, objectSelection), sizes));
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
        EndingObjectSourceArtwork sourceArtwork = EndingObjectSourceArtwork.Of(objects);
        IEnumerable<string> families = IntroSpriteSheetComposer.FrameNames(page.Families(sourceArtwork));
        if (page.Logo)
            families = families.Concat(IntroSpriteSheetComposer.FrameNames([sourceArtwork.LogoSprites]));
        return page.Stars ? families.Concat(StarSheetFrames.Select(EndingShootingStarFrames.SheetFrameName)) : families;
    }

    /// <summary>The characters <paramref name="transfers"/> leave on the sprite page <paramref name="objectSelection"/> selects.</summary>
    private static SpriteArtwork Characters(IReadOnlyList<EndingTransfer> transfers, EndingTransferArtwork artwork, int objectSelection) =>
        ObjCharacterImage.Page(EndingScreenImage.Compose(transfers, artwork), objectSelection);

    /// <summary>The transfers that give the logo its characters: the post-credits art, then every post-shot part.</summary>
    private static IReadOnlyList<EndingTransfer> LogoTransfers =>
    [
        new EndingTransfer(EndingTransferKind.CreditsCharacterArt),
        .. Enumerable.Range(0, EndingPostShotUploadDefinitions.Count).Select(index => new EndingTransfer(EndingTransferKind.PostShotPart, index)),
    ];

    /// <summary>The page ending sheet <paramref name="sheet"/> shows.</summary>
    private static EndingPage Page(SpriteSheetId sheet)
    {
        return sheet switch
        {
            SpriteSheetId.EndingCloudSprites => new(EndingObjectSheet.Clouds, EndingReward.Armored, [new EndingTransfer(EndingTransferKind.EscapeClouds)],
                objects => [objects.CloudSprites], Stars: false, Logo: false, EscapeColours),
            SpriteSheetId.EndingExplosionSprites => new(EndingObjectSheet.Explosion, EndingReward.Armored,
                [
                    new EndingTransfer(EndingTransferKind.ExplosionObjects),
                    .. Enum.GetValues<EndingObjectFragmentId>().Select(fragment => new EndingTransfer(EndingTransferKind.ExplosionMap, Fragment: fragment)),
                    new EndingTransfer(EndingTransferKind.EndingFont),
                ],
                objects => [objects.ExplosionSprites, objects.CompletionTextSprites, EndingAnimalEscapeParts.Sprites], Stars: false, Logo: false,
                palettes => SpritesOf(palettes, EndingPaletteId.Explosion)),
            SpriteSheetId.EndingSuitedRewardSprites => Reward(EndingReward.Armored),
            SpriteSheetId.EndingSuitlessRewardSprites => Reward(EndingReward.Suitless),
            SpriteSheetId.EndingShootingSprites => Shooting,
            SpriteSheetId.EscapeTimer or SpriteSheetId.Grapple or SpriteSheetId.SamusBody or
                SpriteSheetId.SamusOverlays or SpriteSheetId.SamusAtmosphere or
                SpriteSheetId.SamusDeath or SpriteSheetId.GameOver or SpriteSheetId.FileSelect or
                SpriteSheetId.GameOptions or SpriteSheetId.MapSprites or SpriteSheetId.PauseSprites or
                SpriteSheetId.Title or SpriteSheetId.IntroSprites or SpriteSheetId.CeresFlightSprites or
                SpriteSheetId.CeresDestructionSprites or SpriteSheetId.ZebesRevealSprites or
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
        [new EndingTransfer(EndingTransferKind.CreditsCharacterArt)], objects => [objects.RewardSprites], Stars: true, Logo: false,
        PostCreditsColours);

    /// <summary>
    /// The shooting scene: the falling, landing and shooting Samus and the stars over the post-credits art, and the logo
    /// the scene ends on over its own characters. The reward's characters lie outside this page, so any reward shows it.
    /// </summary>
    private static EndingPage Shooting => new(EndingObjectSheet.RewardFalling, EndingReward.Armored,
        [new EndingTransfer(EndingTransferKind.CreditsCharacterArt)], objects => [objects.RewardSprites], Stars: true, Logo: true,
        PostCreditsColours);

    /// <summary>The escape scenes' sprite colours.</summary>
    private static RgbColor[] EscapeColours(EndingPaletteSet palettes) => SpritesOf(palettes, EndingPaletteId.Escape);

    /// <summary>The post-credits scenes' sprite colours, which the credits' end restores.</summary>
    private static RgbColor[] PostCreditsColours(EndingPaletteSet palettes) => SpritesOf(palettes, EndingPaletteId.PostCredits);

    /// <summary>The sprite colours of the whole screen <paramref name="palette"/>.</summary>
    private static RgbColor[] SpritesOf(EndingPaletteSet palettes, EndingPaletteId palette) => palettes[palette].Sprites.ToArray();

    /// <summary>
    /// A scene of the ending: the object sheet that selects it, the reward the scene's transfers dress, those transfers in
    /// order, the families it shows over them, whether the stars show on it, whether it also holds the logo over the
    /// logo's own characters, and the sprite colours its scene loads.
    /// </summary>
    private sealed record EndingPage(EndingObjectSheet ObjectSheet, EndingReward Reward, IReadOnlyList<EndingTransfer> Transfers,
        Func<EndingObjectSourceArtwork, IEnumerable<ICinematicSpriteFrames>> Families, bool Stars, bool Logo,
        Func<EndingPaletteSet, RgbColor[]> SpriteColours);
}
