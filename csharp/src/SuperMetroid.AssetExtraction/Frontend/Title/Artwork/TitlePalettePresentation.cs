using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>
/// Editable initial title screen colours (#1464) and ambient title-screen colour frames, as the import reads them: the
/// import pre-colours the title's sheets in these colours and in each ambient record (#1512), and takes the title's backdrop
/// from them once, into the scene colours the game reads (#1544); the game loads no palette document. Animation programs and
/// destination selection remain compiled engine behaviour.
/// </summary>
public sealed partial class TitlePalettePresentation : IPaletteFxColorSource
{
    /// <summary>The selected initial colours: the background colours, then the sprite colours, in the cartridge's order.</summary>
    internal readonly ImmutableArray<RgbColor> colours;
    /// <summary>Every ambient-program colour the import reads.</summary>
    internal readonly Dictionary<PaletteFxColorKey, RgbColor> animatedColors;

    /// <summary>Holds the import's initial colours, every ambient-program colour and the skip-only inks.</summary>
    /// <param name="colours">The selected initial colours in the cartridge's order, which the import pipeline decoded.</param>
    /// <param name="animatedColors">Every ambient-program colour the import reads.</param>
    /// <param name="skipCopyrightWhite">The selected skip-only white.</param>
    /// <param name="skipCopyrightTint">The selected skip-only copyright tint.</param>
    internal TitlePalettePresentation(
        ImmutableArray<RgbColor> colours,
        Dictionary<PaletteFxColorKey, RgbColor> animatedColors,
        RgbColor skipCopyrightWhite,
        RgbColor skipCopyrightTint)
    {
        this.colours = colours;
        this.animatedColors = animatedColors;
        SkipCopyrightWhite = skipCopyrightWhite;
        SkipCopyrightTint = skipCopyrightTint;
    }

    /// <summary>The selected initial colours: the background colours, then the sprite colours, in the cartridge's order.</summary>
    public ImmutableArray<RgbColor> Colours => colours;

    /// <summary>Selected copyright white the title fast-skip route restores to the copyright glyph's white.</summary>
    public RgbColor SkipCopyrightWhite { get; }
    /// <summary>The copyright glyph's tint the fast skip restores.</summary>
    public RgbColor SkipCopyrightTint { get; }
    /// <summary>
    /// The colour the title shows behind its floor and objects, where nothing is drawn: its first background colour (#1512),
    /// which the import records in the scene colours.
    /// </summary>
    public RgbColor Backdrop => colours[0];

    /// <inheritdoc />
    /// <param name="key">A color the tube-light or display program writes.</param>
    /// <param name="color">Selected colour when found, otherwise black.</param>
    /// <returns>True when the key identifies an installed ambient colour; false for other records.</returns>
    public bool TryReadColor(PaletteFxColorKey key, out RgbColor color) =>
        animatedColors.TryGetValue(key, out color);
}
