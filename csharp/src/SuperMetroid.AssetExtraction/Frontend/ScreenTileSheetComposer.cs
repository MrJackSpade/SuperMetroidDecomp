using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Spatial;

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
        AreaMapPresentationCatalog maps = sources.Maps;
        return sheet switch
        {
            SpriteSheetId.IntroSceneTiles => Compose(sources.Intro.BackgroundCharacters.Transfer.Span[..IntroCinematicRomData.Transfers.BackgroundCharacterBytes],
                BgBitDepth.FourBpp, IntroColours(sources.Intro)),
            SpriteSheetId.IntroFontTiles => Compose(maps.IntroFont.Transfer.Span, BgBitDepth.TwoBpp, IntroColours(sources.Intro)),
            SpriteSheetId.CeresFlightObjectTiles => ComposeFlightObjects(sources.Intro.CeresFlight),
            SpriteSheetId.CeresIntroObjectTiles => Compose(sources.Intro.IntroObjectCharacters.Transfer.Span[..CeresDestructionRomData.Transfers.SharedObjectCharacterBytes],
                BgBitDepth.FourBpp, FlightColours(sources.Intro.CeresFlight)),
            SpriteSheetId.ZebesRevealTiles => Compose(sources.Intro.CeresDestruction.ZebesCharacters.Transfer.Span[..CeresDestructionRomData.Transfers.FloorCharacterBytes],
                BgBitDepth.FourBpp, FlightColours(sources.Intro.CeresFlight)),
            SpriteSheetId.EndingCreditsFontTiles => Compose(maps.EndingFont.Transfer.Span[..EndingCreditsRomData.Rendering.FontCharacterBytes],
                BgBitDepth.FourBpp, EndingColours(sources, EndingPaletteId.Credits)),
            SpriteSheetId.EndingTextFontTiles => Compose(maps.EndingFont.Transfer.Span[..EndingCreditsRomData.Rendering.FontCharacterBytes],
                BgBitDepth.FourBpp, EndingColours(sources, EndingPaletteId.PostCredits)),
            SpriteSheetId.EndingSubtitleTiles => Compose(maps.EndingFont.Transfer.Span.Slice(EndingPostShotDefinitions.SubtitleFontOffset,
                EndingPostShotUploadDefinitions.SubtitleLength), BgBitDepth.FourBpp, EndingColours(sources, EndingPaletteId.PostCredits)),
            SpriteSheetId.EndingLogoTiles => Compose(sources.EndingObjects.PostShotLogoTiles.Transfer.Span, BgBitDepth.FourBpp,
                EndingColours(sources, EndingPaletteId.PostCredits)),
            SpriteSheetId.EndingWaitingTiles => Compose(sources.EndingObjects.WaitingSamus.Transfer.Span[..EndingCreditsRomData.Rendering.WaitingCharacterBytes],
                BgBitDepth.FourBpp, EndingColours(sources, EndingPaletteId.PostCredits)),
            SpriteSheetId.EndingWavySamusTiles => Compose(sources.EndingObjects.PostCreditsFragmentA.Transfer.Span[..EndingCreditsRomData.Rendering.PostCreditsFragmentABytes],
                BgBitDepth.TwoBpp, EndingColours(sources, EndingPaletteId.PostCredits)),
            SpriteSheetId.EndingExplosionTiles => Compose(sources.EndingObjects.Explosion.Transfer.Span[..EndingCreditsRomData.Rendering.ExplosionObjectBytes],
                BgBitDepth.FourBpp, EndingColours(sources, EndingPaletteId.Explosion)),
            SpriteSheetId.MenuForegroundTiles => Compose(maps.WorldArtwork.ForegroundCharacters(), BgBitDepth.FourBpp, maps.Palettes.FileSelect),
            SpriteSheetId.MenuMapTiles => Compose(maps.Tiles.Characters, BgBitDepth.FourBpp, maps.Palettes.FileSelect),
            SpriteSheetId.PauseMapTiles => Compose(maps.Tiles.Characters, BgBitDepth.FourBpp, maps.Palettes.Pause),
            SpriteSheetId.PauseInterfaceTiles => Compose(maps.PauseTiles.Characters, BgBitDepth.FourBpp, maps.Palettes.Pause),
            SpriteSheetId.PauseHudTiles => Compose(maps.HudTiles.Transfer.Span[..HudTileAtlasFormat.CharacterByteCount], BgBitDepth.TwoBpp,
                maps.Palettes.Pause),
            _ when AreaOf(ScreenTileSheetDefinitions.AreaMapForegrounds, sheet) is { } area =>
                Compose(maps.WorldArtwork.ForegroundCharacters(), BgBitDepth.FourBpp, maps.Palettes.World(area)),
            _ when AreaOf(ScreenTileSheetDefinitions.AreaMapBackgrounds, sheet) is { } area =>
                Compose(maps.WorldArtwork.BackgroundCharacters(), BgBitDepth.TwoBpp, maps.Palettes.World(area)),
            _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The sheet holds no screen tiles."),
        };
    }

    /// <summary>
    /// The sheet of <paramref name="characters"/>, planar <paramref name="depth"/> characters, each under each palette row
    /// coloured from <paramref name="palette"/>'s background half.
    /// </summary>
    /// <exception cref="InvalidDataException">The characters are not whole characters of the depth.</exception>
    internal static ComposedSpriteSheet Compose(ReadOnlySpan<byte> characters, BgBitDepth depth, ReadOnlySpan<Bgr555> palette)
    {
        int bytes = ScreenTileSheet.CharacterWordsOf(depth) * sizeof(ushort);
        if (characters.IsEmpty || characters.Length % bytes != 0)
            throw new InvalidDataException($"{characters.Length} bytes are not whole {depth} characters.");
        int count = characters.Length / bytes;
        int blockRows = ScreenTileSheet.BlockRowsOf(count);
        var canvas = new RoomTileCanvas(ScreenTileSheet.Columns * ScreenTileSheet.TilePixels,
            ScreenTileSheet.PaletteRows * blockRows * ScreenTileSheet.TilePixels);
        Bgr555[] colours = palette[..SpriteImage.SpriteColourCount].ToArray();
        for (int row = 0; row < ScreenTileSheet.PaletteRows; row++)
        {
            for (int character = 0; character < count; character++)
            {
                Position16<ImagePixels> corner = ScreenTileSheet.CornerOf(character, row, blockRows);
                SnesBgTilemapWord word = SnesBgTilemapWord.Create(0, row, priority: false);
                ReadOnlySpan<byte> pixels = characters.Slice(character * bytes, bytes);
                if (depth == BgBitDepth.FourBpp)
                    canvas.PaintTile(corner.X, corner.Y, word, pixels, colours);
                else
                    canvas.PaintTwoBppTile(corner.X, corner.Y, word, pixels, colours);
            }
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [ScreenTileSheet.FrameName] = TileWordSheetComposer.Frame(canvas, 0, ScreenTileSheet.PaletteRows * blockRows,
                depth == BgBitDepth.FourBpp ? TileWordLayer.Bg12 : TileWordLayer.Bg3),
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
            SpriteSheetId.TitleMode7Tiles => ComposeTitleMode7(sources.Maps.TitleGraphics, sources.Maps.TitlePalette),
            SpriteSheetId.TitleBabyTiles => ComposeTitleBaby(sources.Maps.TitleGraphics, sources.Maps.TitlePalette),
            SpriteSheetId.CeresFlightMode7Tiles => ComposeFlightMode7(sources.Intro.CeresFlight),
            SpriteSheetId.EndingEscapeAMode7Tiles => ComposeMode7Pixels(sources.EndingMode7[EndingFloorSceneId.EscapeA].Characters.Span,
                EndingColours(sources.EndingPalettes, EndingPaletteId.Escape)),
            SpriteSheetId.EndingEscapeBMode7Tiles => ComposeMode7Pixels(sources.EndingMode7[EndingFloorSceneId.EscapeB].Characters.Span,
                EndingColours(sources.EndingPalettes, EndingPaletteId.Escape)),
            SpriteSheetId.EndingPlanetExplosionMode7Tiles => ComposeMode7Pixels(sources.EndingMode7[EndingFloorSceneId.PlanetExplosion].Characters.Span,
                EndingColours(sources.EndingPalettes, EndingPaletteId.Explosion)),
            SpriteSheetId.EndingFlyawayMode7Tiles => ComposeMode7Pixels(sources.Intro.CeresFlight.FloorCharacters.Span[..EndingCreditsRomData.Rendering.FloorBytes],
                EndingColours(sources.EndingPalettes, EndingPaletteId.Escape)),
            SpriteSheetId.EndingRewardIconMode7Tiles => ComposeRewardIcon(sources.EndingMode7, sources.EndingPalettes),
            _ => throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The sheet holds no Mode 7 characters."),
        };
    }

    /// <summary>The title's Mode 7 characters under its palette.</summary>
    internal static ComposedSpriteSheet ComposeTitleMode7(TitleGraphicsPresentation graphics, TitlePalettePresentation palette) =>
        ComposeMode7Pixels(Ensure.NotNull(graphics).FloorCharacters[..TitleSequenceRomData.Transfers.FloorCharacterByteCount], TitleColours(palette));

    /// <summary>The title's Baby Metroid animation pages under its palette.</summary>
    internal static ComposedSpriteSheet ComposeTitleBaby(TitleGraphicsPresentation graphics, TitlePalettePresentation palette) =>
        ComposeMode7Pixels(Ensure.NotNull(graphics).BabyCharacters, TitleColours(palette));

    /// <summary>The Ceres flight's Mode 7 characters under its palette.</summary>
    internal static ComposedSpriteSheet ComposeFlightMode7(CeresFlightArtworkCatalog flight) =>
        ComposeMode7Pixels(Ensure.NotNull(flight).FloorCharacters.Span[..CeresFlightRomData.Transfers.FloorCharacterByteCount], FlightColours(flight));

    /// <summary>The ending's reward icon's Mode 7 characters under the post-credits palette.</summary>
    private static ComposedSpriteSheet ComposeRewardIcon(EndingFloorArtworkCatalog mode7, EndingPaletteCatalog palettes) =>
        ComposeMode7Pixels(HighBytes(mode7.RewardIcon.Transfer.Span), EndingColours(palettes, EndingPaletteId.PostCredits));

    /// <summary>The sheet of Mode 7 characters whose pixels <paramref name="pixels"/> holds, one colour index each, coloured from <paramref name="palette"/>.</summary>
    /// <exception cref="InvalidDataException">The pixels are not whole characters.</exception>
    internal static ComposedSpriteSheet ComposeMode7Pixels(ReadOnlySpan<byte> pixels, ReadOnlySpan<Bgr555> palette)
    {
        if (pixels.IsEmpty || pixels.Length % Mode7CharacterSheet.TileBytes != 0)
            throw new InvalidDataException($"{pixels.Length} pixels are not whole Mode 7 characters.");
        int tiles = pixels.Length / Mode7CharacterSheet.TileBytes;
        var canvas = new RoomTileCanvas(Mode7CharacterSheet.TilesPerRow * Mode7CharacterSheet.TilePixels,
            Mode7CharacterSheet.RowsOf(tiles) * Mode7CharacterSheet.TilePixels);
        Bgr555[] colours = palette[..SpriteImage.ColourCountOf(SpriteImageColours.FullPalette)].ToArray();
        SpritePaletteRows coloured = SpritePaletteRows.None;
        for (int tile = 0; tile < tiles; tile++)
        {
            Position16<ImagePixels> corner = Mode7CharacterSheet.Corner(tile);
            for (int pixel = 0; pixel < Mode7CharacterSheet.TileBytes; pixel++)
            {
                byte index = pixels[tile * Mode7CharacterSheet.TileBytes + pixel];
                if (index % 16 != 0 && index < SpriteImage.SpriteColourCount)
                    coloured |= SpritePaletteRowSet.Rows[index / 16];
                canvas.PaintIndex(corner.X + pixel % Mode7CharacterSheet.TilePixels, corner.Y + pixel / Mode7CharacterSheet.TilePixels, index, colours);
            }
        }
        var frames = new Dictionary<string, SpriteSheetFrameDocument>(StringComparer.Ordinal)
        {
            [Mode7CharacterSheet.FrameName] = new SpriteSheetFrameDocument
            {
                Corner = default,
                Size = new Extent16<ImagePixels>((ushort)canvas.Width, (ushort)canvas.Height),
                Origin = default,
                Rows = coloured,
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
    private static ReadOnlySpan<Bgr555> TitleColours(TitlePalettePresentation palette)
    {
        var colours = new ScreenPalette();
        Ensure.NotNull(palette).Apply(colours);
        return colours.Colors;
    }

    /// <summary>The Ceres flight's object characters under its palette, which its caption shows.</summary>
    internal static ComposedSpriteSheet ComposeFlightObjects(CeresFlightArtworkCatalog flight) =>
        Compose(Ensure.NotNull(flight).ObjectCharacters.Span[..CeresFlightRomData.Transfers.ObjectCharacterByteCount], BgBitDepth.FourBpp, FlightColours(flight));

    /// <summary>The opening cinematic's palette.</summary>
    private static ReadOnlySpan<Bgr555> IntroColours(IntroCinematicArtworkCatalog intro)
    {
        var colours = new ScreenPalette();
        intro.Palette.LoadTo(colours);
        return colours.Colors;
    }

    /// <summary>The ending's palette <paramref name="palette"/>, whose colours its scenes load where they lie.</summary>
    private static ReadOnlySpan<Bgr555> EndingColours(ScreenTileSources sources, EndingPaletteId palette) => EndingColours(sources.EndingPalettes, palette);

    /// <summary>The ending's palette <paramref name="palette"/> from <paramref name="palettes"/>.</summary>
    private static ReadOnlySpan<Bgr555> EndingColours(EndingPaletteCatalog palettes, EndingPaletteId palette)
    {
        var colours = new ScreenPalette();
        palettes[palette].LoadTo(colours, 0, ScreenPalette.ColorCount, 0);
        return colours.Colors;
    }

    /// <summary>The Ceres flight's palette.</summary>
    private static ReadOnlySpan<Bgr555> FlightColours(CeresFlightArtworkCatalog flight)
    {
        var colours = new ScreenPalette();
        flight.Palette.LoadTo(colours);
        return colours.Colors;
    }

    /// <summary>The area whose sheet <paramref name="sheet"/> is among <paramref name="sheets"/>, or null.</summary>
    private static AreaId? AreaOf(IReadOnlyDictionary<AreaId, SpriteSheetId> sheets, SpriteSheetId sheet) =>
        sheets.Where(entry => entry.Value == sheet).Select(entry => (AreaId?)entry.Key).FirstOrDefault();
}
