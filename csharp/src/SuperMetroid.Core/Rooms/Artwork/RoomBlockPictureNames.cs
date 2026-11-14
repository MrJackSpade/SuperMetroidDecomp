using System.Collections.Immutable;
using System.Globalization;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Names a pre-coloured block picture (#1392, #1431, #1466, #1712, #1791): the block, by its number among those its room
/// loads, then the values of the appearance inputs it depends on. Kraid's backdrop, an animation's frame or Ceres Ridley's
/// look therefore names another picture of the same block, and extraction and the room resolve the same name. A collectible showing its own picture names its kind and frame (#1538). A background image's 16x16 cell whose
/// picture changes is named by the image and the cell's place in it (#1717); its pictures add the values of the inputs it
/// depends on, and the image's manifest records the name, which the room reads.
/// </summary>
public static class RoomBlockPictureNames
{
    /// <summary>The name of the picture of block <paramref name="block"/> while <paramref name="inputs"/> take <paramref name="values"/>.</summary>
    /// <param name="block">The block's index among those its room loads.</param>
    /// <param name="inputs">The inputs the block depends on.</param>
    /// <param name="values">The value of each input, in the same order.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block index is negative.</exception>
    /// <exception cref="ArgumentException">There is not one value for each input.</exception>
    public static string Of(int block, ImmutableArray<AppearanceInput> inputs, ReadOnlySpan<string> values) =>
        AppearancePictureNames.Of(string.Create(CultureInfo.InvariantCulture, $"block {Ensure.AtLeastZero(block)}"), inputs, values);

    /// <summary>The name of the picture of <paramref name="collectible"/>'s frame (#1538), which depends on no input.</summary>
    public static string OfCollectible(CollectiblePicture collectible) =>
        string.Create(CultureInfo.InvariantCulture, $"collectible {collectible.Kind} frame {collectible.Frame}");

    /// <summary>
    /// The name the pictures of cell <paramref name="cell"/> of background image <paramref name="background"/> begin with
    /// (#1717), before the values of the inputs the cell depends on.
    /// </summary>
    /// <param name="background">The background image's installed name.</param>
    /// <param name="cell">The cell's index in the image, row by row.</param>
    /// <exception cref="ArgumentException">The image's name is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The cell index is negative.</exception>
    public static string OfBackgroundCell(string background, int cell)
    {
        ArgumentException.ThrowIfNullOrEmpty(background);
        return string.Create(CultureInfo.InvariantCulture, $"background {background} cell {Ensure.AtLeastZero(cell)}");
    }
}
