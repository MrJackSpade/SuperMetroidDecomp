using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What each character a frontend background layer's tilemap words can name shows (#1392): the pre-coloured tile of the
/// artwork loaded there, by the character's index from the layer's character base; none where no artwork the screen
/// loads shows that character.
/// </summary>
public sealed class ScreenLayerPictures
{
    /// <summary>The character indices a tilemap word can name.</summary>
    public const int CharacterIndices = 1024;

    /// <summary>Holds each character index's tile.</summary>
    /// <exception cref="ArgumentException">The tiles do not cover every character index.</exception>
    internal ScreenLayerPictures(ImmutableArray<ScreenTilePicture?> tiles, BgBitDepth depth)
    {
        Ensure.LengthEqual(tiles.AsSpan(), CharacterIndices);
        Tiles = tiles;
        Depth = depth;
        Sheets = [.. tiles.OfType<ScreenTilePicture>().Select(tile => tile.Sheet).Distinct()];
    }

    /// <summary>Each character index's tile, or null where no artwork shows that character.</summary>
    public ImmutableArray<ScreenTilePicture?> Tiles { get; }

    /// <summary>The bit depth of the layer's characters.</summary>
    public BgBitDepth Depth { get; }

    /// <summary>The sheets the tiles come from, each once.</summary>
    public ImmutableArray<ScreenTileSheet> Sheets { get; }

    /// <summary>
    /// What tile word <paramref name="word"/> shows at <paramref name="pixel"/> of its character: its tile's pixel under the
    /// word's palette row, flipped as the word flips it, with the word's priority.
    /// </summary>
    /// <exception cref="InvalidDataException">The word names a character no artwork the screen loads shows.</exception>
    public BgSample Sample(SnesBgTilemapWord word, Position16<CharacterPixels> pixel)
    {
        ScreenTilePicture tile = Tiles[word.CharacterIndex]
            ?? throw new InvalidDataException($"Tile word ${word.Raw:X4} names character {word.CharacterIndex}, which no artwork the screen loads shows.");
        Position16<ImagePixels> corner = tile.Sheet.Corner(tile.Character, word.PaletteIndex);
        int x = word.FlipHorizontally ? ScreenTileSheet.TilePixels - 1 - pixel.X : pixel.X;
        int y = word.FlipVertically ? ScreenTileSheet.TilePixels - 1 - pixel.Y : pixel.Y;
        return BgSample.OfTilePicture(new BgTileSample(word, pixel), tile.Sheet.Picture,
            new Position16<ImagePixels>((ushort)(corner.X + x), (ushort)(corner.Y + y)));
    }
}
