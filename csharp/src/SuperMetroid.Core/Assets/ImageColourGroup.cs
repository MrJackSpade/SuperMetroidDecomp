namespace SuperMetroid.Core.Assets;

/// <summary>
/// One of an installed image's own colour groups (#1464): sixteen of the standard colours the import pipeline coloured
/// the image with, which a pixel's ink names. What a group shows is the image's standard colours under the shading its
/// draw names (#1512).
/// </summary>
/// <remarks>
/// A group is known by its place among the image's standard colours, which is how the installed image stores its inks
/// and how a backend keys a draw's colours.
/// </remarks>
public readonly record struct ImageColourGroup
{
    /// <summary>The colour groups an installed image is coloured in.</summary>
    public const int Count = 8;

    /// <summary>The colours of one group.</summary>
    public const int Colours = 16;

    /// <summary>Names the group at <paramref name="number"/> among the image's standard colours.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place is not one of the image's groups.</exception>
    internal ImageColourGroup(int number) => Number = Ensure.BetweenInclusive(0, Count - 1, number);

    /// <summary>The group's place among the image's standard colours, which the installed image's inks store.</summary>
    internal int Number { get; }
}
