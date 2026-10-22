using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
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
    /// <summary>Composes screen tile sheet <paramref name="sheet"/> from the menus' presentation.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds no screen tiles.</exception>
    internal static ComposedSpriteSheet Compose(SpriteSheetId sheet, AreaMapPresentationCatalog maps)
    {
        Ensure.NotNull(maps);
        return sheet switch
        {
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

    /// <summary>The area whose sheet <paramref name="sheet"/> is among <paramref name="sheets"/>, or null.</summary>
    private static AreaId? AreaOf(IReadOnlyDictionary<AreaId, SpriteSheetId> sheets, SpriteSheetId sheet) =>
        sheets.Where(entry => entry.Value == sheet).Select(entry => (AreaId?)entry.Key).FirstOrDefault();
}
