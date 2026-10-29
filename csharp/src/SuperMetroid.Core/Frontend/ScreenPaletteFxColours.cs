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
    public ColourRun Colours(PaletteFxColourOwner owner) => owner switch
    {
        PaletteFxColourOwner.Background => palette.Background,
        PaletteFxColourOwner.Sprite => palette.Sprites,
        PaletteFxColourOwner.SamusSuit => palette.Colours(ScreenColourGroup.SamusSuit),
        PaletteFxColourOwner.SamusBeams => palette.Colours(ScreenColourGroup.SamusBeams),
        _ => throw new ArgumentOutOfRangeException(nameof(owner), owner, "Unknown palette-FX colour owner."),
    };
}
