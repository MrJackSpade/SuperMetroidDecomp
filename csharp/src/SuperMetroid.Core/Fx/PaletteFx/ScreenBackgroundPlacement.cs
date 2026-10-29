using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// A palette-FX colour group of a frontend screen's background sets, from colour <paramref name="Colour"/> of the
/// screen's set <paramref name="Set"/> (#1464): a cinematic's or the title's effect, which a screen's images show in
/// the screen's set of the same identity. The group runs to the set's last colour; no effect writes past it.
/// </summary>
/// <param name="Set">The screen's background set the group's first written colour is in.</param>
/// <param name="Colour">The group's first written colour within its set.</param>
internal sealed record ScreenBackgroundPlacement(int Set, int Colour) : PaletteFxColourPlacement
{
    /// <inheritdoc />
    internal override ColourRun From(IPaletteFxColourOwners owners)
    {
        ColourRun set = Ensure.NotNull(owners).ScreenBackgroundSet(Set);
        return set.Slice(Colour, set.Count - Colour);
    }
}
