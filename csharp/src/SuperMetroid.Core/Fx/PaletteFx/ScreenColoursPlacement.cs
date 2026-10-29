using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// A palette-FX colour group a frontend screen holds (#1464): a title or cinematic effect's colours, which the screen's
/// colour owner resolves by the group's name to the colours it holds for them. Gameplay holds no screen effect's colours.
/// </summary>
/// <param name="Colours">The colour group the screen resolves.</param>
internal sealed record ScreenColoursPlacement(PaletteFxColours Colours) : PaletteFxColourPlacement
{
    /// <inheritdoc />
    internal override ColourRun From(IPaletteFxColourOwners owners) => Ensure.NotNull(owners).ScreenColours(Colours);
}
