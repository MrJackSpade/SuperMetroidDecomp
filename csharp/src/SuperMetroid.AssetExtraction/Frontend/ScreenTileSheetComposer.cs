using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the frontend's screen tile sheets (#1392): each background artwork's characters under each palette row, laid
/// out as <see cref="ScreenTileSheet"/> reads them, coloured with the standard palette of the screen that shows them.
/// </summary>
internal static class ScreenTileSheetComposer
{
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
}
