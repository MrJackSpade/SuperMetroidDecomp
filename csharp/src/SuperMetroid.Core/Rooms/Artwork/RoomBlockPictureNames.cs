using System.Collections.Immutable;
using System.Globalization;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Names a pre-coloured block picture (#1392, #1431, #1466): the block's index among those its room loads, marked when
/// it shows the sprites' artwork, then the values of the appearance inputs it depends on. A collectible shown in a slot,
/// Kraid's backdrop or an animation's frame therefore names another picture of the same block, and extraction and the
/// room resolve the same name.
/// </summary>
public static class RoomBlockPictureNames
{
    /// <summary>The mark of a block picture over the sprites' artwork.</summary>
    private const string OverSprites = "@sprites";

    /// <summary>The name of the picture of block <paramref name="block"/> over <paramref name="artwork"/> while <paramref name="inputs"/> take <paramref name="values"/>.</summary>
    /// <param name="block">The block's index among those its room loads.</param>
    /// <param name="artwork">The artwork the room's blocks show.</param>
    /// <param name="inputs">The inputs the block depends on over that artwork.</param>
    /// <param name="values">The value of each input, in the same order.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block index is negative.</exception>
    /// <exception cref="ArgumentException">There is not one value for each input.</exception>
    public static string Of(int block, RoomBlockArtwork artwork, ImmutableArray<AppearanceInput> inputs, ReadOnlySpan<string> values) =>
        AppearancePictureNames.Of(Ensure.AtLeastZero(block).ToString("X3", CultureInfo.InvariantCulture)
            + (artwork == RoomBlockArtwork.Sprites ? OverSprites : ""), inputs, values);
}
