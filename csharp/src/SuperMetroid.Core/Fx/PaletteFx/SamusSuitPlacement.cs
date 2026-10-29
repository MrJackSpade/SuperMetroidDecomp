using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>A palette-FX colour group of Samus's suit colours, from her suit's colour <paramref name="Colour"/> (#1464).</summary>
/// <param name="Colour">The group's first written colour among the suit's.</param>
internal sealed record SamusSuitPlacement(int Colour) : PaletteFxColourPlacement
{
    /// <inheritdoc />
    internal override ColourRun From(IPaletteFxColourOwners owners)
    {
        ColourRun suit = Ensure.NotNull(owners).SamusSuit;
        return suit.Slice(Colour, suit.Count - Colour);
    }
}
