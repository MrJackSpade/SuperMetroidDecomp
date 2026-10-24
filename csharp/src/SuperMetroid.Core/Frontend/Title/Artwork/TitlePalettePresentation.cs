using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>
/// Editable initial 256-color title palette and ambient title-screen color frames.
/// Animation programs and destination selection remain compiled engine behavior.
/// </summary>
public sealed class TitlePalettePresentation : IPaletteFxColorSource
{
    /// <summary>Compiled initial CGRAM image in native slot order.</summary>
    internal readonly Bgr555[] colors;
    /// <summary>Ambient samples retained only when they differ from the calculated paint.</summary>
    internal readonly Dictionary<PaletteFxColorKey, Bgr555> animatedColors;

    /// <summary>Retains the selected initial image, ambient edits, and independent skip-only inks.</summary>
    private TitlePalettePresentation(
        Bgr555[] colors,
        Dictionary<PaletteFxColorKey, Bgr555> animatedColors,
        Bgr555 skipCopyrightWhite,
        Bgr555 skipCopyrightRed)
    {
        this.colors = colors;
        this.animatedColors = animatedColors;
        SkipCopyrightWhite = skipCopyrightWhite;
        SkipCopyrightRed = skipCopyrightRed;
    }

    /// <summary>Keeps the selected title colours, dropping each ambient colour the shared ambient calculation already produces.</summary>
    /// <param name="colors">The 256 selected screen colours.</param>
    /// <param name="animatedColors">Every selected ambient-program colour.</param>
    /// <param name="skipCopyrightWhite">The selected skip-only white.</param>
    /// <param name="skipCopyrightRed">The selected skip-only red.</param>
    internal static TitlePalettePresentation Create(Bgr555[] colors,
        Dictionary<PaletteFxColorKey, Bgr555> animatedColors, Bgr555 skipCopyrightWhite, Bgr555 skipCopyrightRed)
    {
        foreach (var program in TitleScreenAmbientPaletteFxColorDefinitions.All)
        for (int frame = 0; frame < program.FrameCount; frame++)
        for (int index = 0; index < program.ColorsPerFrame; index++)
        {
            PaletteFxColorKey key = program.Color(frame, index);
            if (TitleAmbientColorDefinitions.TryCalculate(key, colors, animatedColors, out Bgr555 calculated) &&
                animatedColors[key] == calculated) animatedColors.Remove(key);
        }
        return new TitlePalettePresentation(colors, animatedColors, skipCopyrightWhite, skipCopyrightRed);
    }

    /// <summary>Selected BGR555 copyright highlight restored at CGRAM color 201 by the title fast-skip route and title-screen presentation rebinding.</summary>
    public Bgr555 SkipCopyrightWhite { get; }
    /// <summary>Selected BGR555 copyright tint restored at CGRAM color 202 on fast skip; the legacy Red name does not restrict its channels, and the stock word is $7D80.</summary>
    public Bgr555 SkipCopyrightRed { get; }

    /// <inheritdoc />
    /// <param name="key">A color the tube-light or display program writes.</param>
    /// <param name="color">Selected native BGR555 word when found, otherwise zero.</param>
    /// <returns>True when the key identifies an authored or matching calculated ambient color; false for other records.</returns>
    public bool TryReadColor(PaletteFxColorKey key, out Bgr555 color) =>
        animatedColors.TryGetValue(key, out color) ||
        TitleAmbientColorDefinitions.TryCalculate(key, colors, animatedColors, out color);

    /// <summary>Replaces every colour of the screen with the authored palette.</summary>
    /// <param name="destination">The screen's colours; ambient animation and skip-only replacements are applied separately by the title state.</param>
    /// <exception cref="ArgumentNullException"><paramref name="destination"/> is null.</exception>
    public void Apply(ScreenPalette destination) => Ensure.NotNull(destination).Load(colors);
}
