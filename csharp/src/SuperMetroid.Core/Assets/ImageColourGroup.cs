using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// One of an installed image's own colour groups (#1464): sixteen of the standard colours the import pipeline coloured
/// the image with, which a pixel's ink names. A sprite image's group shows, unless a draw recolours it, the shared sprite
/// colour store the pipeline coloured it from; a gameplay background image's group draws in the background set the
/// pipeline named for it.
/// </summary>
/// <remarks>
/// A group is known by its place among the image's standard colours, which is how the installed image stores its inks
/// and how a backend keys a draw's colours. The place says nothing about where gameplay holds the colours a group shows:
/// <see cref="Store"/> and the image's background sets say that.
/// </remarks>
public readonly record struct ImageColourGroup
{
    /// <summary>The colour groups an installed image is coloured in.</summary>
    public const int Count = 8;

    /// <summary>The colours of one group.</summary>
    public const int Colours = 16;

    /// <summary>Names the group at <paramref name="number"/> among the image's standard colours.</summary>
    private ImageColourGroup(int number) => Number = number;

    /// <summary>Every group, in the order the image keeps their colours.</summary>
    public static ImmutableArray<ImageColourGroup> Every { get; } =
        [.. Enumerable.Range(0, Count).Select(number => new ImageColourGroup(number))];

    /// <summary>The group whose colours the image keeps at place <paramref name="number"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The place is not one of the image's groups.</exception>
    internal static ImageColourGroup Numbered(int number) => Every[Ensure.BetweenInclusive(0, Count - 1, number)];

    /// <summary>The group a pixel inked <paramref name="paletteIndex"/> among a sixteen-colour image's colours was coloured from.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The index lies past the image's groups.</exception>
    internal static ImageColourGroup OfInk(int paletteIndex) => Numbered(paletteIndex / Colours);

    /// <summary>The group's place among the image's standard colours, which the installed image's inks store.</summary>
    internal int Number { get; }

    /// <summary>Where the group's colours start among the image's standard colours.</summary>
    internal int FirstColour => Number * Colours;

    /// <summary>
    /// The shared sprite colour store a sprite image's group shows while no draw recolours it: the import pipeline colours
    /// each group of a sprite image from the store of the same place.
    /// </summary>
    internal SpriteColourStore Store => SpriteColourStore.Every[Number];
}
