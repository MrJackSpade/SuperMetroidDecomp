using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// A palette-FX colour group of gameplay's background set <paramref name="Set"/>, from its colour
/// <paramref name="Colour"/> (#1464). The group runs to the set's last colour; no effect writes past it.
/// </summary>
/// <param name="Set">The background set that holds the group.</param>
/// <param name="Colour">The group's first written colour within its set.</param>
internal sealed record BackgroundSetPlacement(BackgroundColourSet Set, int Colour) : PaletteFxColourPlacement
{
    /// <inheritdoc />
    internal override ColourRun From(IPaletteFxColourOwners owners)
    {
        ColourRun set = Ensure.NotNull(owners).BackgroundSet(Set);
        return set.Slice(Colour, set.Count - Colour);
    }
}
