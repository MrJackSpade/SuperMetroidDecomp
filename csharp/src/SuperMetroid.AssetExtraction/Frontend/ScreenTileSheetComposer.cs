using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Intro;
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
            SpriteSheetId.IntroSceneTiles => Compose(IntroCinematicSourceArtwork.Of(sources.Intro).BackgroundCharacters.Characters.Span[..IntroCinematicDefinitions.Artwork.BackgroundCharacterBytes],
                TileInkDepth.SixteenColours, IntroColours(sources.Intro)),
            SpriteSheetId.IntroFontTiles => Compose(IntroFontCharacters.Of(maps.IntroFont).Characters.Span, TileInkDepth.FourColours, IntroColours(sources.Intro)),
            SpriteSheetId.CeresFlightObjectTiles => ComposeFlightObjects(sources.Intro.CeresFlight),
            SpriteSheetId.CeresIntroObjectTiles => Compose(IntroCinematicSourceArtwork.Of(sources.Intro).IntroObjectCharacters.Characters.Span[..CeresDestructionSceneDefinitions.SharedObjectCharacterBytes],
                TileInkDepth.SixteenColours, FlightColours(sources.Intro.CeresFlight)),
            SpriteSheetId.ZebesRevealTiles => Compose(sources.Intro.CeresDestruction.ZebesCharacters.Characters.Span[..CeresDestructionArtworkFormat.ZebesCharacterByteCount],
                TileInkDepth.SixteenColours, FlightColours(sources.Intro.CeresFlight)),
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
                SpriteSheetId.TitleFloorTiles or SpriteSheetId.TitleBabyTiles or
                SpriteSheetId.CeresFlightFloorTiles or SpriteSheetId.EndingEscapeAFloorTiles or
                SpriteSheetId.EndingEscapeBFloorTiles or SpriteSheetId.EndingPlanetExplosionFloorTiles or
                SpriteSheetId.EndingFlyawayFloorTiles or SpriteSheetId.EndingRewardIconFloorTiles or SpriteSheetId.EndingCreditsRows or
                SpriteSheetId.EndingPostCreditsPictures or SpriteSheetId.EndingWavySamusPicture or
                SpriteSheetId.EndingExplosionPictures =>
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

    /// <summary>Composes Mode 7 character sheet <paramref name="sheet"/> from the stock artwork <paramref name="sources"/> read.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds no Mode 7 characters.</exception>
    internal static ComposedSpriteSheet ComposeMode7(SpriteSheetId sheet, ScreenTileSources sources)
    {
        Ensure.NotNull(sources);
        return sheet switch
        {
            SpriteSheetId.TitleFloorTiles => ComposeTitleMode7(sources.Maps.TitleGraphics, sources.Maps.TitlePalette),
            SpriteSheetId.TitleBabyTiles => ComposeTitleBaby(sources.Maps.TitleGraphics, sources.Maps.TitlePalette),
            SpriteSheetId.CeresFlightFloorTiles => ComposeFlightMode7(sources.Intro.CeresFlight),
            SpriteSheetId.EndingEscapeAFloorTiles => ComposeFloorPixels(EndingFloorSceneCharacters.Of(sources.EndingMode7[EndingFloorSceneId.EscapeA]).Characters.Span,
                EndingColours(sources.EndingPalettes, EndingPaletteId.Escape)),
            SpriteSheetId.EndingEscapeBFloorTiles => ComposeFloorPixels(EndingFloorSceneCharacters.Of(sources.EndingMode7[EndingFloorSceneId.EscapeB]).Characters.Span,
                EndingColours(sources.EndingPalettes, EndingPaletteId.Escape)),
            SpriteSheetId.EndingPlanetExplosionFloorTiles => ComposeFloorPixels(EndingFloorSceneCharacters.Of(sources.EndingMode7[EndingFloorSceneId.PlanetExplosion]).Characters.Span,
                EndingColours(sources.EndingPalettes, EndingPaletteId.Explosion)),
            SpriteSheetId.EndingFlyawayFloorTiles => ComposeFloorPixels(CeresFlightCharacters.Of(sources.Intro.CeresFlight).FloorCharacters.Span[..EndingArtworkExtentDefinitions.FloorBytes],
                EndingColours(sources.EndingPalettes, EndingPaletteId.Escape)),
            SpriteSheetId.EndingRewardIconFloorTiles => ComposeRewardIcon(sources.EndingMode7, sources.EndingPalettes),
            SpriteSheetId.EscapeTimer or SpriteSheetId.Grapple or SpriteSheetId.SamusBodyTop or
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
                SpriteSheetId.MenuForegroundTiles or SpriteSheetId.MenuMapTiles or
                SpriteSheetId.AreaMapForegroundTilesCrateria or SpriteSheetId.AreaMapForegroundTilesBrinstar or
                SpriteSheetId.AreaMapForegroundTilesNorfair or SpriteSheetId.AreaMapForegroundTilesWreckedShip or
                SpriteSheetId.AreaMapForegroundTilesMaridia or SpriteSheetId.AreaMapForegroundTilesTourian or
                SpriteSheetId.AreaMapBackgroundTilesCrateria or SpriteSheetId.AreaMapBackgroundTilesBrinstar or
                SpriteSheetId.AreaMapBackgroundTilesNorfair or SpriteSheetId.AreaMapBackgroundTilesWreckedShip or
                SpriteSheetId.AreaMapBackgroundTilesMaridia or SpriteSheetId.AreaMapBackgroundTilesTourian or
                SpriteSheetId.PauseMapTiles or SpriteSheetId.PauseInterfaceTiles or SpriteSheetId.PauseHud or
                SpriteSheetId.IntroSceneTiles or SpriteSheetId.IntroFontTiles or
                SpriteSheetId.CeresFlightObjectTiles or SpriteSheetId.CeresIntroObjectTiles or
                SpriteSheetId.ZebesRevealTiles or SpriteSheetId.EndingCreditsRows or
                SpriteSheetId.EndingPostCreditsPictures or SpriteSheetId.EndingWavySamusPicture or SpriteSheetId.EndingExplosionPictures =>
                throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The sheet holds no Mode 7 characters."),
            _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "Undefined sprite sheet."),
        };
    }

    /// <summary>The title's Mode 7 characters under its palette.</summary>
    internal static ComposedSpriteSheet ComposeTitleMode7(TitleGraphicsPresentation graphics, TitlePalettePresentation palette) =>
        ComposeFloorPixels(TitleGraphicsCharacters.Of(Ensure.NotNull(graphics)).FloorCharacters[..TitleSequenceDefinitions.Graphics.FloorCharacterByteCount], TitleColours(palette));

    /// <summary>The title's Baby Metroid animation pages under its palette.</summary>
    internal static ComposedSpriteSheet ComposeTitleBaby(TitleGraphicsPresentation graphics, TitlePalettePresentation palette) =>
        ComposeFloorPixels(TitleGraphicsCharacters.Of(Ensure.NotNull(graphics)).BabyCharacters, TitleColours(palette));

    /// <summary>The Ceres flight's Mode 7 characters under its palette.</summary>
    internal static ComposedSpriteSheet ComposeFlightMode7(CeresFlightArtwork flight) =>
        ComposeFloorPixels(CeresFlightCharacters.Of(Ensure.NotNull(flight)).FloorCharacters.Span[..CeresFlightArtworkFormat.FloorByteCount], FlightColours(flight));

    /// <summary>The ending's reward icon's Mode 7 characters under the post-credits palette.</summary>
    private static ComposedSpriteSheet ComposeRewardIcon(EndingFloorArtwork mode7, EndingPaletteSet palettes) =>
        ComposeFloorPixels(HighBytes(EndingRewardIconCharacters.Of(mode7.RewardIcon).Bytes.Span), EndingColours(palettes, EndingPaletteId.PostCredits));

    /// <summary>The sheet of Mode 7 characters whose pixels <paramref name="pixels"/> holds, one colour index each, coloured from <paramref name="palette"/>.</summary>
    /// <exception cref="InvalidDataException">The pixels are not whole characters.</exception>
    internal static ComposedSpriteSheet ComposeFloorPixels(ReadOnlySpan<byte> pixels, ReadOnlySpan<RgbColor> palette)
    {
        if (pixels.IsEmpty || pixels.Length % FloorCharacterSheet.TileBytes != 0)
            throw new InvalidDataException($"{pixels.Length} pixels are not whole Mode 7 characters.");
        int tiles = pixels.Length / FloorCharacterSheet.TileBytes;
        var canvas = new RoomTileCanvas(new Extent16<ImagePixels>(FloorCharacterSheet.TilesPerRow * FloorCharacterSheet.TilePixels, checked((ushort)(FloorCharacterSheet.RowsOf(tiles) * FloorCharacterSheet.TilePixels))));
        RgbColor[] colours = palette[..SpriteImage.ColourCountOf(SpriteImageColours.FullPalette)].ToArray();
        ImageColourGroups coloured = ImageColourGroups.None;
        for (int tile = 0; tile < tiles; tile++)
        {
            Position16<ImagePixels> corner = FloorCharacterSheet.Corner(tile);
            for (int pixel = 0; pixel < FloorCharacterSheet.TileBytes; pixel++)
            {
                int index = pixels[tile * FloorCharacterSheet.TileBytes + pixel];
                if (index % 16 != 0 && index < SpriteImage.SpriteColourCount)
                    coloured = coloured.With(ImageColourGroup.OfInk(index));
                canvas.PaintIndex(new(checked((ushort)(corner.X + pixel % FloorCharacterSheet.TilePixels)), checked((ushort)(corner.Y + pixel / FloorCharacterSheet.TilePixels))), index, colours);
            }
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [FloorCharacterSheet.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = canvas.Size,
                Origin = default,
                ColourGroups = coloured.ManifestMembers,
                Parts = [],
            },
        };
        return canvas.ToSheet(colours, frames, SpriteImageColours.FullPalette);
    }

    /// <summary>The high byte of each word of <paramref name="words"/>: the Mode 7 pixels of an upload that writes both VRAM ports.</summary>
    private static byte[] HighBytes(ReadOnlySpan<byte> words)
    {
        var pixels = new byte[words.Length / sizeof(ushort)];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
            pixels[pixel] = words[pixel * sizeof(ushort) + 1];
        return pixels;
    }

    /// <summary>The title's palette.</summary>
    private static ReadOnlySpan<RgbColor> TitleColours(TitlePalettePresentation palette)
    {
        var colours = new ScreenPalette();
        Ensure.NotNull(palette).Apply(colours);
        return ScreenColourImage.Encode(colours);
    }

    /// <summary>The Ceres flight's object characters under its palette, which its caption shows.</summary>
    internal static ComposedSpriteSheet ComposeFlightObjects(CeresFlightArtwork flight) =>
        Compose(CeresFlightCharacters.Of(Ensure.NotNull(flight)).ObjectCharacters.Span[..CeresFlightArtworkFormat.ObjectByteCount], TileInkDepth.SixteenColours, FlightColours(flight));

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
    private static ReadOnlySpan<RgbColor> FlightColours(CeresFlightArtwork flight)
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
