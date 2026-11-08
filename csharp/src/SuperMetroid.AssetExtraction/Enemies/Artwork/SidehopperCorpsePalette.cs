using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The corpse target and the drain's last stage share corpse paint. Slot5 is white; slots8..15
/// interpolate RGB5 endpoints to nearest over seven intervals. The auxiliary
/// import includes only slots0..14; its stock paint has a separate calculated owner. This helper preserves independently supplied15-color content. The standalone16-word target uses reviewed paints and material composition.
/// </summary>
internal sealed class SidehopperCorpsePalette
{
    /// <summary>Stores either the complete supplied colors or independent colors plus a fade endpoint.</summary>
    internal readonly RgbColor[] independentOrSupplied;
    /// <summary>Marks palettes whose white slot and seven-step gradient can be reproduced exactly.</summary>
    private readonly bool calculated;
    /// <summary>Uses reviewed stock paint definitions for a matching sixteen-color target.</summary>
    private readonly bool stockTarget;
    /// <summary>Number of colors in the imported fifteen-color prefix or full target palette.</summary>
    [IntegerQuantity("number of colors in the palette")]
    internal int Count { get; }

    /// <summary>Keeps the stock, calculated, or independent paint ownership the import selected.</summary>
    /// <param name="count">Fifteen or sixteen colors.</param>
    /// <param name="paint">Whether the palette is stock paint, independent colors with a calculated gradient, or supplied colors.</param>
    /// <param name="independentOrSupplied">The independent colors and gradient endpoint, or every supplied color; empty for stock paint.</param>
    internal SidehopperCorpsePalette(int count, SidehopperPaletteOwnership paint, RgbColor[] independentOrSupplied)
    {
        Count = count;
        stockTarget = paint == SidehopperPaletteOwnership.Stock;
        calculated = paint == SidehopperPaletteOwnership.Calculated;
        this.independentOrSupplied = independentOrSupplied;
    }

    /// <summary>Resolves a stock, independently supplied, or reconstructed corpse palette color.</summary>
    internal RgbColor Resolve(int color)
    {
        if ((uint)color >= Count) throw new ArgumentOutOfRangeException(nameof(color));
        if (stockTarget) return SidehopperCorpsePaint.Color(color);
        if (!calculated) return independentOrSupplied[color];
        if (color == 5) return new RgbColor(255, 255, 255);
        if (color < 8) return independentOrSupplied[color < 5 ? color : color - 1];
        return Interpolate(independentOrSupplied[7], independentOrSupplied[8], color - 8);
    }

    /// <summary>Interpolates RGB5 channels with the game's seven-interval nearest rounding.</summary>
    internal static RgbColor Interpolate(RgbColor start, RgbColor end, int phase)
    {
        return start.FadeLevels.Zip(end.FadeLevels, (_, a, b) => ((a * (7 - phase) + b * phase + 3) / 7)).ToColour();
    }
}
