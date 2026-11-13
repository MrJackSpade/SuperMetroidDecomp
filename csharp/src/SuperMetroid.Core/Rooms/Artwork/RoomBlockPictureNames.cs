using System.Collections.Immutable;
using System.Globalization;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Names a pre-coloured block picture (#1392, #1431, #1466, #1712): the block, by its number among those its room loads,
/// marked when it shows the sprites' artwork, then the values of the appearance inputs it depends on. Kraid's backdrop or an
/// animation's frame therefore names another picture of the same block, and extraction and the room resolve the same
/// name. A collectible showing its own picture names its kind and frame (#1538).
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
        AppearancePictureNames.Of(string.Create(CultureInfo.InvariantCulture, $"block {Ensure.AtLeastZero(block)}")
            + (artwork == RoomBlockArtwork.Sprites ? OverSprites : ""), inputs, values);

    /// <summary>The name of the picture of <paramref name="collectible"/>'s frame (#1538), which depends on no input.</summary>
    public static string OfCollectible(CollectiblePicture collectible) =>
        string.Create(CultureInfo.InvariantCulture, $"collectible {collectible.Kind} frame {collectible.Frame}");
}
