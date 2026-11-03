using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>
/// Editable initial title screen colours (#1464) and ambient title-screen colour frames, as the import reads them: the
/// import pre-colours the title's sheets in these colours and in each ambient record (#1512), and the game keeps only the
/// title's backdrop. Animation programs and destination selection remain compiled engine behaviour.
/// </summary>
public sealed class TitlePalettePresentation : IPaletteFxColorSource
{
    /// <summary>The selected initial colours: the background colours, then the sprite colours, in the cartridge's order.</summary>
    internal readonly ImmutableArray<RgbColor> colours;
    /// <summary>Ambient samples retained only when they differ from the calculated paint.</summary>
    internal readonly Dictionary<PaletteFxColorKey, RgbColor> animatedColors;

    /// <summary>Retains the selected initial colours, ambient edits, and independent skip-only inks.</summary>
    /// <param name="colours">The selected initial colours in the cartridge's order, which the import pipeline decoded.</param>
    /// <param name="animatedColors">The selected ambient-program colours the import found the shared calculation does not produce.</param>
    /// <param name="skipCopyrightWhite">The selected skip-only white.</param>
    /// <param name="skipCopyrightRed">The selected skip-only red.</param>
    internal TitlePalettePresentation(
        ImmutableArray<RgbColor> colours,
        Dictionary<PaletteFxColorKey, RgbColor> animatedColors,
        RgbColor skipCopyrightWhite,
        RgbColor skipCopyrightRed)
    {
        this.colours = colours;
        this.animatedColors = animatedColors;
        SkipCopyrightWhite = skipCopyrightWhite;
        SkipCopyrightRed = skipCopyrightRed;
    }

    /// <summary>The selected initial colours: the background colours, then the sprite colours, in the cartridge's order.</summary>
    public ImmutableArray<RgbColor> Colours => colours;

    /// <summary>Selected copyright white the title fast-skip route restores to the copyright glyph's white.</summary>
    public RgbColor SkipCopyrightWhite { get; }
    /// <summary>Selected copyright tint the fast skip restores to the copyright glyph's red; the legacy Red name does not restrict its channels.</summary>
    public RgbColor SkipCopyrightRed { get; }
    /// <summary>The colour the title shows behind its floor and objects, where nothing is drawn: its first background colour (#1512).</summary>
    public RgbColor Backdrop => colours[0];

    /// <inheritdoc />
    /// <param name="key">A color the tube-light or display program writes.</param>
    /// <param name="color">Selected colour when found, otherwise black.</param>
    /// <returns>True when the key identifies an authored or matching calculated ambient color; false for other records.</returns>
    public bool TryReadColor(PaletteFxColorKey key, out RgbColor color) =>
        animatedColors.TryGetValue(key, out color) ||
        TitleAmbientColors.TryCalculate(key, colours, animatedColors, out color);
}
