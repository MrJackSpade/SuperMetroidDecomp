using System.Collections.Immutable;
using System.Globalization;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Names a pre-coloured block picture (#1392, #1431, #1466, #1712, #1791): the block, by its number among those its room
/// loads, then the values of the appearance inputs it depends on. Kraid's backdrop, an animation's frame or Ceres Ridley's
/// look therefore names another picture of the same block, and extraction and the room resolve the same name. A collectible showing its own picture names its kind and look (#1538, #1994). A background image's block-sized cell whose
/// picture changes is named by the image and the cell's place in it (#1717); its pictures add the values of the inputs it
/// depends on, and the image's manifest records the name, which the room reads.
/// </summary>
public static partial class RoomBlockPictureNames
{
    /// <summary>The name of the picture of block <paramref name="block"/> while <paramref name="inputs"/> take <paramref name="values"/>.</summary>
    /// <param name="block">The block's index among those its room loads.</param>
    /// <param name="inputs">The inputs the block depends on.</param>
    /// <param name="values">The value of each input, in the same order.</param>
    /// <exception cref="ArgumentOutOfRangeException">The block index is negative.</exception>
    /// <exception cref="ArgumentException">There is not one value for each input.</exception>
    public static string Of(int block, ImmutableArray<AppearanceInput> inputs, ReadOnlySpan<string> values) =>
        AppearancePictureNames.Of(OfBlock(block), inputs, values);

    /// <summary>
    /// The name block <paramref name="block"/>'s pictures begin with, before the values of the inputs it depends on: the
    /// block, by its number among those its room loads, which keys the block's installed appearance inputs (#2565).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The block index is negative.</exception>
    public static string OfBlock(int block) => string.Create(CultureInfo.InvariantCulture, $"block {Ensure.AtLeastZero(block)}");

    /// <summary>
    /// The name of the picture of <paramref name="collectible"/>'s look (#1538, #1994), which depends on no input: its kind
    /// and its look spelled by their installed keys (#2544).
    /// </summary>
    public static string OfCollectible(CollectiblePicture collectible) =>
        string.Create(CultureInfo.InvariantCulture,
            $"collectible {InWorldCollectibleKindKeys.Installed.Of(collectible.Kind)} {CollectibleLookKeys.Installed.Of(collectible.Look)}");

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
