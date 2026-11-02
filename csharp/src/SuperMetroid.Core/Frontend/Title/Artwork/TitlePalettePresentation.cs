using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// Editable initial title screen colours, by owner (#1464), and ambient title-screen color frames.
/// Animation programs and destination selection remain compiled engine behavior.
/// </summary>
public sealed class TitlePalettePresentation : IPaletteFxColorSource
{
    /// <summary>The selected initial screen colours by owner.</summary>
    internal readonly ScreenColours colours;
    /// <summary>Ambient samples retained only when they differ from the calculated paint.</summary>
    internal readonly Dictionary<PaletteFxColorKey, RgbColor> animatedColors;

    /// <summary>Retains the selected initial image, ambient edits, and independent skip-only inks.</summary>
    /// <param name="colours">The selected screen colours by owner, which the import pipeline decoded.</param>
    /// <param name="animatedColors">The selected ambient-program colours the import found the shared calculation does not produce.</param>
    /// <param name="skipCopyrightWhite">The selected skip-only white.</param>
    /// <param name="skipCopyrightRed">The selected skip-only red.</param>
    internal TitlePalettePresentation(
        ScreenColours colours,
        Dictionary<PaletteFxColorKey, RgbColor> animatedColors,
        RgbColor skipCopyrightWhite,
        RgbColor skipCopyrightRed)
    {
        this.colours = colours;
        this.animatedColors = animatedColors;
        SkipCopyrightWhite = skipCopyrightWhite;
        SkipCopyrightRed = skipCopyrightRed;
    }

    /// <summary>Selected BGR555 copyright white the title fast-skip route and title-screen presentation rebinding restore to the copyright glyph's white.</summary>
    public RgbColor SkipCopyrightWhite { get; }
    /// <summary>Selected BGR555 copyright tint the fast skip restores to the copyright glyph's red; the legacy Red name does not restrict its channels, and the stock word is 32128.</summary>
    public RgbColor SkipCopyrightRed { get; }

    /// <inheritdoc />
    /// <param name="key">A color the tube-light or display program writes.</param>
    /// <param name="color">Selected BGR555 word when found, otherwise zero.</param>
    /// <returns>True when the key identifies an authored or matching calculated ambient color; false for other records.</returns>
    public bool TryReadColor(PaletteFxColorKey key, out RgbColor color) =>
        animatedColors.TryGetValue(key, out color) ||
        TitleAmbientColors.TryCalculate(key, colours, animatedColors, out color);

    /// <summary>Replaces every colour of the screen with the authored palette.</summary>
    /// <param name="destination">The screen's colours; ambient animation and skip-only replacements are applied separately by the title state.</param>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is null.</exception>
    public void Apply(ScreenPalette destination) => colours.LoadTo(destination);
}
