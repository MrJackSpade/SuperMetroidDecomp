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
    private TitlePalettePresentation(
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

    /// <summary>Keeps the selected title colours, dropping each ambient colour the shared ambient calculation already produces.</summary>
    /// <param name="colours">The selected screen colours by owner, which the import pipeline decoded.</param>
    /// <param name="animatedColors">Every selected ambient-program colour.</param>
    /// <param name="skipCopyrightWhite">The selected skip-only white.</param>
    /// <param name="skipCopyrightRed">The selected skip-only red.</param>
    internal static TitlePalettePresentation Create(ScreenColours colours,
        Dictionary<PaletteFxColorKey, RgbColor> animatedColors, RgbColor skipCopyrightWhite, RgbColor skipCopyrightRed)
    {
        foreach (var program in TitleScreenAmbientPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < program.FrameCount; frame++)
        for (int index = 0; index < program.ColorsPerFrame; index++)
        {
            PaletteFxColorKey key = program.Color(frame, index);
            if (TitleAmbientColors.TryCalculate(key, colours, animatedColors, out RgbColor calculated) &&
                animatedColors[key] == calculated) animatedColors.Remove(key);
        }
        return new TitlePalettePresentation(colours, animatedColors, skipCopyrightWhite, skipCopyrightRed);
    }

    /// <summary>Selected BGR555 copyright white the title fast-skip route and title-screen presentation rebinding restore to the copyright glyph's white.</summary>
    public RgbColor SkipCopyrightWhite { get; }
    /// <summary>Selected BGR555 copyright tint the fast skip restores to the copyright glyph's red; the legacy Red name does not restrict its channels, and the stock word is $7D80.</summary>
    public RgbColor SkipCopyrightRed { get; }

    /// <inheritdoc />
    /// <param name="key">A color the tube-light or display program writes.</param>
    /// <param name="color">Selected native BGR555 word when found, otherwise zero.</param>
    /// <returns>True when the key identifies an authored or matching calculated ambient color; false for other records.</returns>
    public bool TryReadColor(PaletteFxColorKey key, out RgbColor color) =>
        animatedColors.TryGetValue(key, out color) ||
        TitleAmbientColors.TryCalculate(key, colours, animatedColors, out color);

    /// <summary>Replaces every colour of the screen with the authored palette.</summary>
    /// <param name="destination">The screen's colours; ambient animation and skip-only replacements are applied separately by the title state.</param>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is null.</exception>
    public void Apply(ScreenPalette destination) => colours.LoadTo(destination);
}
