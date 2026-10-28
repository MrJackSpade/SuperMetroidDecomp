using SuperMetroid.Core.Assets;
using System.Text;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Names a pre-coloured block picture by what the block shows (#1392): its four tile words, as its definition gives them
/// before the level word mirrors the block, and the artwork bound over each tile's character with how far into that
/// artwork the character lies. A block definition a collectible writes, or one whose characters an animation rebinds,
/// therefore names another picture, and extraction and the room resolve the same name.
/// </summary>
public static class RoomBlockPictureNames
{
    /// <summary>VRAM words in one four-bpp background character.</summary>
    public const int CharacterWords = 16;

    /// <summary>Tiles in a block.</summary>
    public const int Tiles = 4;

    /// <summary>The name of the picture of a block whose tiles are <paramref name="tiles"/> over <paramref name="shown"/>.</summary>
    /// <param name="tiles">The block's four tile words: top left, top right, bottom left, bottom right.</param>
    /// <param name="shown">What shows over each tile's character, in the same order.</param>
    /// <exception cref="ArgumentException">Either span does not hold four entries.</exception>
    public static string Of(ReadOnlySpan<ushort> tiles, ReadOnlySpan<BgArtworkShown> shown)
    {
        Ensure.LengthEqual(tiles, Tiles);
        Ensure.LengthEqual(shown, Tiles);
        var name = new StringBuilder();
        for (int tile = 0; tile < Tiles; tile++)
            name.Append(tile == 0 ? "" : ".").Append(tiles[tile].ToString("X4", System.Globalization.CultureInfo.InvariantCulture));
        for (int tile = 0; tile < Tiles; tile++)
            name.Append('|').Append(shown[tile].Artwork.Name).Append('+')
                .Append(shown[tile].WordOffset.ToString("X4", System.Globalization.CultureInfo.InvariantCulture));
        return name.ToString();
    }
}
