using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Which of a frontend screen's colour owners holds one of its colour groups, and where within that owner (#1464): a
/// run of <paramref name="ColourCount"/> colours from colour <paramref name="Colour"/> of the owner's set
/// <paramref name="Set"/>, which may continue into the owner's following sets. Only
/// <see cref="ScreenColourGroupDefinitions"/> names placements, and only <see cref="ScreenColourGroups"/> reads them.
/// </summary>
/// <param name="Set">The owner's set the group starts in.</param>
/// <param name="Colour">The group's first colour within that set.</param>
/// <param name="ColourCount">The number of colours in the group.</param>
internal abstract record ScreenColourPlacement(int Set, int Colour, int ColourCount)
{
    /// <summary>The group's colours on <paramref name="screen"/>, read and written in place.</summary>
    internal abstract ColourRun In(ScreenPalette screen);

    /// <summary>The group's values among <paramref name="values"/>, kept for each screen colour by owner.</summary>
    internal abstract Span<T> In<T>(PerScreenColour<T> values);

    /// <summary>The sprite set a sprite drawn in the group's colours shows.</summary>
    /// <exception cref="InvalidOperationException">The group is not one whole sprite set.</exception>
    internal abstract int DrawnSpriteSet();
}
