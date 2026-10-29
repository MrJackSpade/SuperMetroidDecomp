using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>A palette-FX colour group of Samus's beam colours, from her beams' colour <paramref name="Colour"/> (#1464).</summary>
/// <param name="Colour">The group's first written colour among the beams'.</param>
internal sealed record SamusBeamsPlacement(int Colour) : PaletteFxColourPlacement
{
    /// <inheritdoc />
    internal override ColourRun From(IPaletteFxColourOwners owners)
    {
        ColourRun beams = Ensure.NotNull(owners).SamusBeams;
        return beams.Slice(Colour, beams.Count - Colour);
    }
}
