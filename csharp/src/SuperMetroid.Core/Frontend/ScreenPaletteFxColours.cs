using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The colours palette-FX objects write on a frontend screen (#1405, #1464): the screen colour group each title or
/// cinematic effect animates, the screen's sprite colours, and the screen's colours for Samus's suit and beams.
/// </summary>
/// <param name="palette">The screen's colours.</param>
internal sealed class ScreenPaletteFxColours(ScreenPalette palette) : IPaletteFxColourOwners
{
    /// <inheritdoc />
    public ColourRun BackgroundSet(BackgroundColourSet set) =>
        throw new InvalidOperationException($"A frontend screen holds no gameplay background set {set}.");

    /// <inheritdoc />
    public ColourRun ScreenColours(PaletteFxColours colours) =>
        palette.Colours(ScreenPaletteFxColourDefinitions.Groups.TryGetValue(colours, out ScreenColourGroup group)
            ? group
            : throw new ArgumentOutOfRangeException(nameof(colours), colours, "A frontend screen holds no colours for this effect."));

    /// <inheritdoc />
    public ColourRun SpriteSets => palette.SpriteSets.All;

    /// <inheritdoc />
    public ColourRun SamusSuit => palette.Colours(ScreenColourGroup.SamusSuit);

    /// <inheritdoc />
    public ColourRun SamusBeams => palette.Colours(ScreenColourGroup.SamusBeams);
}
