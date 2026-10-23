using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The colours palette-FX objects write on a frontend screen (#1405): the screen's own background and sprite rows. A
/// screen has no Samus, so her suit's and beams' targets are the sprite rows her colours would occupy.
/// </summary>
/// <param name="palette">The screen's colours.</param>
internal sealed class ScreenPaletteFxColours(ScreenPalette palette) : IPaletteFxColourOwners
{
    /// <summary>The sprite row Samus's suit colours occupy in gameplay.</summary>
    private const int SuitRow = 4;

    /// <summary>The sprite row Samus's beam colours occupy in gameplay.</summary>
    private const int BeamRow = 6;

    /// <inheritdoc />
    public ColourRun Colours(PaletteFxColourOwner owner) => owner switch
    {
        PaletteFxColourOwner.Background => palette.Background,
        PaletteFxColourOwner.Sprite => palette.Sprites,
        PaletteFxColourOwner.SamusSuit => palette.SpriteRow(SuitRow),
        PaletteFxColourOwner.SamusBeams => palette.SpriteRow(BeamRow),
        _ => throw new ArgumentOutOfRangeException(nameof(owner), owner, "Unknown palette-FX colour owner."),
    };
}
