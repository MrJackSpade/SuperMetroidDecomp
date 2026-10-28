using System.Globalization;
using System.Text;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Names a pre-coloured block picture by the block it pictures and what shows over the characters it reads (#1392, #1431):
/// the block's index among those its room loads, then, for each run of characters the block reads, the artwork bound over
/// the run and how far into that artwork the run begins. A collectible bound over a slot's characters, or an animation
/// that paints a block's characters, therefore names another picture of the same block, and extraction and the room resolve
/// the same name.
/// </summary>
public static class RoomBlockPictureNames
{
    /// <summary>VRAM words in one four-bpp background character.</summary>
    public const int CharacterWords = 16;

    /// <summary>The name of the picture of block <paramref name="block"/> over <paramref name="shown"/>.</summary>
    /// <param name="block">The block's index among those its room loads.</param>
    /// <param name="shown">What shows over each run of characters the block reads, in the order its runs are recorded.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block index is negative.</exception>
    /// <exception cref="ArgumentException">Nothing shows over any run.</exception>
    public static string Of(int block, ReadOnlySpan<BgArtworkShown> shown)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(block);
        if (shown.IsEmpty)
            throw new ArgumentException("A block picture shows at least one run of characters.", nameof(shown));
        var name = new StringBuilder(block.ToString("X3", CultureInfo.InvariantCulture));
        foreach (BgArtworkShown run in shown)
            name.Append('|').Append(run.Artwork.Name).Append('+').Append(run.WordOffset.ToString("X4", CultureInfo.InvariantCulture));
        return name.ToString();
    }
}
