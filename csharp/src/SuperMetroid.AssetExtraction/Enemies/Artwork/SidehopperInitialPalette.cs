using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The drain's first stage and the live target share the initial Sidehopper palette. The drain
/// imports its first15 words; the live target imports16. White slots1/9 and
/// repeated slots14=11/15=12 and the body/detail/claw shades calculate. The standalone16-color target uses reviewed paints; the shifted15-color stock auxiliary has a separate calculated paint owner.
/// Each instance owns its supplied content; edits never couple separate catalogs.
/// </summary>
internal sealed class SidehopperInitialPalette
{
    /// <summary>Cyan body/leg colors3180 and18C0.
    /// The exact shadow halves each RGB5 channel. This selected ratio is reviewed for the standalone target only; the shifted auxiliary disposition is separate.</summary>
    private const int BodyShadowDivisor = 2;
    /// <summary>Independent source paints for calculated slots, or the full supplied palette for noncalculated content.</summary>
    internal readonly RgbColor[] independentOrSupplied;
    /// <summary>Whether white, repeated, shadow, and midpoint slots can be reconstructed from selected paints.</summary>
    private readonly bool calculated;
    /// <summary>Whether all sixteen colors use the reviewed standalone stock paint catalog.</summary>
    private readonly bool stockTarget;
    /// <summary>Fifteen for the shifted auxiliary import or sixteen for the standalone target.</summary>
    [IntegerQuantity("number of colors in the palette")]
    internal int Count { get; }

    /// <summary>Keeps the stock, calculated, or independent paint ownership the import selected.</summary>
    /// <param name="count">Fifteen or sixteen colors.</param>
    /// <param name="paint">Whether the palette is stock paint, independent paints with calculated shades, or supplied colors.</param>
    /// <param name="independentOrSupplied">The independent paints of a calculated palette, or every supplied color; empty for stock paint.</param>
    internal SidehopperInitialPalette(int count, SidehopperPaletteOwnership paint, RgbColor[] independentOrSupplied)
    {
        Count = count;
        stockTarget = paint == SidehopperPaletteOwnership.Stock;
        calculated = paint == SidehopperPaletteOwnership.Calculated;
        this.independentOrSupplied = independentOrSupplied;
    }

    /// <summary>Resolves one palette slot, deriving dependent shades only when the supplied palette matched their formulas.</summary>
    internal RgbColor Resolve(int color)
    {
        if ((uint)color >= Count) throw new ArgumentOutOfRangeException(nameof(color));
        if (stockTarget) return SidehopperInitialPaint.Color(color);
        if (!calculated) return independentOrSupplied[color];
        return color switch
        {
            1 or 9 => new RgbColor(255, 255, 255),
            11 => ClawMidpoint(Resolve(10), Resolve(12)),
            14 => Resolve(11),
            15 => Resolve(12),
            0 => independentOrSupplied[0],
            4 => BodyShadow(Resolve(3)),
            7 => DetailMidpoint(Resolve(6), Resolve(8)),
            < 4 => independentOrSupplied[color - 1],
            < 7 => independentOrSupplied[color - 2],
            < 9 => independentOrSupplied[color - 3],
            < 11 => independentOrSupplied[color - 4],
            _ => independentOrSupplied[color - 5],
        };
    }
    /// <summary>Inner-detail ink7 is the channel-floor midpoint
    /// of installed inner-detail paints at slots6/8 (5EC0/35A0).</summary>
    internal static RgbColor DetailMidpoint(RgbColor light, RgbColor dark)
    {
        return light.FadeLevels.Zip(dark.FadeLevels, (_, a, b) => ((a + b) / 2)).ToColour();
    }
    /// <summary>Halves each RGB5 channel of the selected body paint for the standalone shadow slot.</summary>
    internal static RgbColor BodyShadow(RgbColor light)
    {
        return light.FadeLevels.Map((_, a) => (a / BodyShadowDivisor)).ToColour();
    }
    /// <summary>Yellow claw shade0237 is the ceiling-channel midpoint of039C/00D1.
    /// Independently supplied light/dark endpoints remain editable.</summary>
    internal static RgbColor ClawMidpoint(RgbColor light, RgbColor dark)
    {
        return light.FadeLevels.Zip(dark.FadeLevels, (_, a, b) => ((a + b + 1) / 2)).ToColour();
    }
}
