using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The colours palette-FX objects write on a frontend screen (#1405, #1464): the screen's own background and sprite
/// colours, and the screen's colours for Samus's suit and beams.
/// </summary>
/// <param name="palette">The screen's colours.</param>
internal sealed class ScreenPaletteFxColours(ScreenPalette palette) : IPaletteFxColourOwners
{
    /// <inheritdoc />
    public ColourRun BackgroundSet(BackgroundColourSet set) =>
        throw new InvalidOperationException($"A frontend screen holds no gameplay background set {set}.");

    /// <inheritdoc />
    public ColourRun ScreenBackgroundSet(int set) => palette.BackgroundSets.Set(set);

    /// <inheritdoc />
    public ColourRun SpriteSets => palette.SpriteSets.All;

    /// <inheritdoc />
    public ColourRun SamusSuit => palette.Colours(ScreenColourGroup.SamusSuit);

    /// <inheritdoc />
    public ColourRun SamusBeams => palette.Colours(ScreenColourGroup.SamusBeams);
}
