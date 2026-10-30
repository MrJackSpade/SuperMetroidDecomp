using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the frontend's screen tile sheets (#1392): each background artwork's characters under each palette row, laid
/// out as <see cref="ScreenTileSheet"/> reads them, coloured with the standard palette of the screen that shows them.
/// </summary>
internal static class ScreenTileSheetComposer
{
    /// <summary>Composes screen tile sheet <paramref name="sheet"/> from the stock artwork <paramref name="sources"/> read.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds no screen tiles.</exception>
    internal static ComposedSpriteSheet Compose(SpriteSheetId sheet, ScreenTileSources sources)
    {
        Ensure.NotNull(sources);
        AreaMapPresentation maps = sources.Maps;
        return sheet switch
        {
            SpriteSheetId.IntroFontTiles => Compose(IntroFontCharacters.Of(maps.IntroFont).Characters.Span, TileInkDepth.FourColours, IntroColours(sources.Intro)),
            SpriteSheetId.MenuForegroundTiles => Compose(WorldMapCharacters.Of(maps.WorldArtwork).ForegroundCharacters(), TileInkDepth.SixteenColours, ScreenColourImage.Encode(maps.Palettes.FileSelect)),
            SpriteSheetId.MenuMapTiles => Compose(MapCharacterArtwork.Of(maps).Map.Characters, TileInkDepth.SixteenColours, ScreenColourImage.Encode(maps.Palettes.FileSelect)),
            SpriteSheetId.PauseMapTiles => Compose(MapCharacterArtwork.Of(maps).Map.Characters, TileInkDepth.SixteenColours, ScreenColourImage.Encode(maps.Palettes.Pause)),
            SpriteSheetId.PauseInterfaceTiles => Compose(MapCharacterArtwork.Of(maps).Pause.Characters, TileInkDepth.SixteenColours, ScreenColourImage.Encode(maps.Palettes.Pause)),
            SpriteSheetId.AreaMapForegroundTilesCrateria or SpriteSheetId.AreaMapForegroundTilesBrinstar or
                SpriteSheetId.AreaMapForegroundTilesNorfair or SpriteSheetId.AreaMapForegroundTilesWreckedShip or
                SpriteSheetId.AreaMapForegroundTilesMaridia or SpriteSheetId.AreaMapForegroundTilesTourian =>
                Compose(WorldMapCharacters.Of(maps.WorldArtwork).ForegroundCharacters(), TileInkDepth.SixteenColours, ScreenColourImage.Encode(maps.Palettes.World(AreaFor(ScreenTileSheetDefinitions.AreaMapForegrounds, sheet)))),
            SpriteSheetId.AreaMapBackgroundTilesCrateria or SpriteSheetId.AreaMapBackgroundTilesBrinstar or
                SpriteSheetId.AreaMapBackgroundTilesNorfair or SpriteSheetId.AreaMapBackgroundTilesWreckedShip or
                SpriteSheetId.AreaMapBackgroundTilesMaridia or SpriteSheetId.AreaMapBackgroundTilesTourian =>
                Compose(WorldMapCharacters.Of(maps.WorldArtwork).BackgroundCharacters(), TileInkDepth.FourColours, ScreenColourImage.Encode(maps.Palettes.World(AreaFor(ScreenTileSheetDefinitions.AreaMapBackgrounds, sheet)))),
            SpriteSheetId.PauseHud or SpriteSheetId.EscapeTimer or SpriteSheetId.Grapple or SpriteSheetId.SamusBodyTop or
                SpriteSheetId.SamusBodyBottom or SpriteSheetId.SamusOverlays or SpriteSheetId.SamusAtmosphere or
                SpriteSheetId.SamusDeath or SpriteSheetId.GameOver or SpriteSheetId.FileSelect or
                SpriteSheetId.GameOptions or SpriteSheetId.MapSprites or SpriteSheetId.PauseSprites or
                SpriteSheetId.Title or SpriteSheetId.IntroSprites or SpriteSheetId.CeresFlightSprites or
                SpriteSheetId.CeresDestructionSprites or SpriteSheetId.ZebesRevealSprites or
                SpriteSheetId.EndingCloudSprites or SpriteSheetId.EndingExplosionSprites or
                SpriteSheetId.EndingSuitedRewardSprites or SpriteSheetId.EndingSuitlessRewardSprites or
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
                SpriteSheetId.TitleFloor or SpriteSheetId.CeresFloor or SpriteSheetId.EndingEscapeAFloor or
                SpriteSheetId.EndingEscapeBFloor or SpriteSheetId.EndingPlanetExplosionFloor or
                SpriteSheetId.EndingFlyawayFloor or SpriteSheetId.EndingRewardIconFloor or SpriteSheetId.EndingCreditsRows or
                SpriteSheetId.EndingPostCreditsPictures or SpriteSheetId.EndingWavySamusPicture or
                SpriteSheetId.EndingExplosionPictures or SpriteSheetId.CeresCinematicPictures or SpriteSheetId.IntroScenePictures =>
                throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The sheet holds no screen tiles."),
            _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
        };
    }

    /// <summary>
    /// The sheet of <paramref name="characters"/>, planar <paramref name="depth"/> characters, each under each palette row
    /// coloured from <paramref name="palette"/>'s background half.
    /// </summary>
    /// <exception cref="InvalidDataException">The characters are not whole characters of the depth.</exception>
    internal static ComposedSpriteSheet Compose(ReadOnlySpan<byte> characters, TileInkDepth depth, ReadOnlySpan<RgbColor> palette)
    {
        int bytes = ScreenTileSheet.CharacterWordsOf(depth) * sizeof(ushort);
        if (characters.IsEmpty || characters.Length % bytes != 0)
            throw new InvalidDataException($"{characters.Length} bytes are not whole {depth} characters.");
        int count = characters.Length / bytes;
        int blockRows = ScreenTileSheet.BlockRowsOf(count);
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(ScreenTileSheet.Columns * ScreenTileSheet.TilePixels, checked((ushort)(ScreenTileSheet.ColourVariants * blockRows * ScreenTileSheet.TilePixels))));
        RgbColor[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
        for (int row = 0; row < ScreenTileSheet.ColourVariants; row++)
        {
            for (int character = 0; character < count; character++)
            {
                Position16<ImagePixels> corner = ScreenTileSheet.CornerOf(character, row, blockRows);
                SnesBgTilemapWord word = SnesBgTilemapWord.Create(0, row, priority: false);
                ReadOnlySpan<byte> pixels = characters.Slice(character * bytes, bytes);
                if (depth == TileInkDepth.SixteenColours)
                    canvas.PaintTile(corner.X, corner.Y, word, pixels, colours);
                else
                    canvas.PaintTwoBppTile(corner.X, corner.Y, word, pixels, colours);
            }
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [ScreenTileSheet.FrameName] = WrittenPictureSheetComposer.Frame(canvas, 0, ScreenTileSheet.ColourVariants * blockRows,
                depth == TileInkDepth.SixteenColours ? WrittenPictureLayer.Bg12 : WrittenPictureLayer.Bg3),
        };
        return canvas.ToSheet(colours, frames, ScreenTileSheet.ColoursOf(depth));
    }

    /// <summary>The title's palette.</summary>
    internal static ReadOnlySpan<RgbColor> TitleColours(TitlePalettePresentation palette)
    {
        var colours = new ScreenPalette();
        Ensure.NotNull(palette).Apply(colours);
        return ScreenColourImage.Encode(colours);
    }

    /// <summary>The opening cinematic's palette.</summary>
    private static ReadOnlySpan<RgbColor> IntroColours(IntroCinematicArtwork intro)
    {
        return ScreenColourImage.Encode(intro.Palette.Colours);
    }

    /// <summary>The ending's palette <paramref name="palette"/> from <paramref name="palettes"/>.</summary>
    internal static ReadOnlySpan<RgbColor> EndingColours(EndingPaletteSet palettes, EndingPaletteId palette)
    {
        return ScreenColourImage.Encode(palettes[palette].Screen);
    }

    /// <summary>The Ceres flight's palette.</summary>
    internal static ReadOnlySpan<RgbColor> FlightColours(CeresFlightArtwork flight)
    {
        return ScreenColourImage.Encode(flight.Palette.Colours);
    }

    /// <summary>The area whose sheet <paramref name="sheet"/> is among <paramref name="sheets"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No area's sheet is <paramref name="sheet"/>.</exception>
    private static AreaId AreaFor(IReadOnlyDictionary<AreaId, SpriteSheetId> sheets, SpriteSheetId sheet) =>
        AreaOf(sheets, sheet) ?? throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The sheet is no area's.");

    /// <summary>The area whose sheet <paramref name="sheet"/> is among <paramref name="sheets"/>, or null.</summary>
    private static AreaId? AreaOf(IReadOnlyDictionary<AreaId, SpriteSheetId> sheets, SpriteSheetId sheet) =>
        sheets.Where(entry => entry.Value == sheet).Select(entry => (AreaId?)entry.Key).FirstOrDefault();
}
