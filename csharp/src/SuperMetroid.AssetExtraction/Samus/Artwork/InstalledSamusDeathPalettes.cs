using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Death;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>
/// The installed suit, suitless, and whiteout colours of Samus's death sequence and its explosion's step order, every
/// colour and step kept as installed. Explosion durations have their own owner.
/// </summary>
public sealed class InstalledSamusDeathPalettes
{
    /// <summary>Three suited families in pointer-table order: Power, Varia, then Gravity.</summary>
    public const int SuitCount = 3;
    /// <summary>Sixteen RGB5 words per death-flash step, one OBJ colour set, including the transparent sprite ink at index zero.</summary>
    public const int ColorCount = SamusColourSets.ColoursPerSet;

    /// <summary>The installed suited colours, by suit, death-flash step and ink.</summary>
    private readonly RgbColor[][][] suited;
    /// <summary>The installed suitless colours, by death-flash step and ink.</summary>
    private readonly RgbColor[][] suitless;
    /// <summary>The installed whiteout shades, by whiteout counter.</summary>
    private readonly RgbColor[] whiteout;
    /// <summary>The installed death-flash step each explosion frame selects.</summary>
    private readonly int[] explosionPaletteIndices;

    /// <summary>Keeps copies of the complete installed death artwork; no caller arrays are retained or modified.</summary>
    /// <param name="suited">Each suit family's death-flash steps of RGB5 words, in pointer-table order; <see cref="SuitCount"/>, <see cref="SamusShadeDefinitions.Death.ExplosionShadeCount"/> and <see cref="ColorCount"/> give the dimensions.</param>
    /// <param name="suitless">The suitless death-flash steps of RGB5 words in pointer-table order, separate from suited artwork.</param>
    /// <param name="whiteout">The whiteout's RGB5 shades corresponding to ShadesOfWhite, indexed by whiteout counter rather than explosion frame.</param>
    /// <param name="explosionPaletteIndices">One death-flash step selector per explosion record, corresponding to the odd bytes; animation durations are not part of this input.</param>
    /// <exception cref="ArgumentNullException">A top-level input array is null.</exception>
    /// <exception cref="InvalidDataException">An input has the wrong dimensions, a nested family or step is null, or an explosion selector names no death-flash step.</exception>
    public InstalledSamusDeathPalettes(RgbColor[][][] suited, RgbColor[][] suitless,
        RgbColor[] whiteout, int[] explosionPaletteIndices)
    {
        Ensure.NotNull(suited);
        Ensure.NotNull(suitless);
        Ensure.NotNull(whiteout);
        Ensure.NotNull(explosionPaletteIndices);
        FixedGridShape deathSteps = new(SamusShadeDefinitions.Death.ExplosionShadeCount, ColorCount);
        if (suited.Length != SuitCount ||
            suited.Any(family => !deathSteps.Fits(family)) ||
            !deathSteps.Fits(suitless) ||
            whiteout.Length != SamusShadeDefinitions.Death.WhiteoutShadeCount ||
            explosionPaletteIndices.Length != SamusDeathSheetDefinitions.Explosion.Count ||
            explosionPaletteIndices.Any(index => index is < 0 or >= SamusShadeDefinitions.Death.ExplosionShadeCount))
            throw new InvalidDataException("Samus death-palette artwork has an invalid shape or RGB5 color.");
        this.suited = [.. suited.Select(Copy)];
        this.suitless = Copy(suitless);
        this.whiteout = (RgbColor[])whiteout.Clone();
        this.explosionPaletteIndices = [.. explosionPaletteIndices];
    }

    /// <summary>Returns one installed suited death colour.</summary>
    /// <param name="suit">The suit whose death colours are read.</param>
    /// <param name="palette">Death-flash step, below <see cref="SamusShadeDefinitions.Death.ExplosionShadeCount"/>: base, flash, the whitening steps, then the final foreground step.</param>
    /// <param name="color">Ink index 0..15, copied by the consumer to colours 192..207; index-zero pixels remain transparent.</param>
    /// <returns>The installed colour; no palette is written.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Any index is outside its documented range.</exception>
    public RgbColor SuitedColor(SamusSuit suit, int palette, int color)
    {
        // The installed families are listed in the suits' order (SamusSuitListLayout.Order).
        int family = SamusSuitListLayout.Order.IndexOf(suit);
        if (family < 0)
            throw new ArgumentOutOfRangeException(nameof(suit), suit, "Samus has no such suit.");
        return Step(suited[family], palette, color);
    }

    /// <summary>Returns one installed suitless death colour.</summary>
    /// <param name="palette">Death-flash step 0..9.</param>
    /// <param name="color">Ink index 0..15, copied by the consumer to colours 240..255; index-zero pixels remain transparent.</param>
    /// <returns>The installed colour, without changing the colours or advancing the death sequence.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside its documented range.</exception>
    public RgbColor SuitlessColor(int palette, int color) => Step(suitless, palette, color);

    /// <summary>Returns one installed ShadesOfWhite entry.</summary>
    /// <param name="index">Whiteout shade, not an explosion frame; the terminal call selects the last shade explicitly.</param>
    /// <returns>The RGB5 word used by the consumer to fill colours 0..191 and 208..239, excluding suited and suitless Samus palettes.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> names no whiteout shade.</exception>
    public RgbColor WhiteoutColor(int index)
    {
        if ((uint)index >= whiteout.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return whiteout[index];
    }

    /// <summary>Returns the installed death-flash step an explosion frame selects.</summary>
    /// <param name="frame">Explosion drawing index, selecting one of the explosion's timing/selector records.</param>
    /// <returns>A death-flash step for both foreground palette families, not a RGB5 word or countdown duration.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> names no explosion record.</exception>
    private int ExplosionPaletteIndex(int frame)
    {
        if ((uint)frame >= explosionPaletteIndices.Length)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return explosionPaletteIndices[frame];
    }

    /// <summary>
    /// The order the death sequence steps through and the room's whiteout colours, which gameplay reads (#1512); the
    /// suit and suitless colours stay here, where the import pre-colours her death shades from them.
    /// </summary>
    public SamusDeathSteps Steps => new(
        [.. Enumerable.Range(0, SamusDeathSheetDefinitions.Explosion.Count).Select(frame => new SamusDeathShade(ExplosionPaletteIndex(frame)))],
        [.. Enumerable.Range(0, SamusShadeDefinitions.Death.WhiteoutShadeCount).Select(WhiteoutColor)]);

    /// <summary>The colour at <paramref name="palette"/> and <paramref name="color"/> of one family's death-flash steps.</summary>
    private static RgbColor Step(RgbColor[][] steps, int palette, int color)
    {
        if ((uint)palette >= steps.Length)
            throw new ArgumentOutOfRangeException(nameof(palette));
        if ((uint)color >= ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return steps[palette][color];
    }

    /// <summary>Copies one family's death-flash steps.</summary>
    private static RgbColor[][] Copy(RgbColor[][] steps) => [.. steps.Select(step => (RgbColor[])step.Clone())];
}
