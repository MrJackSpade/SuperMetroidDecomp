using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Botwoon.Artwork;

/// <summary>Editable health-band RGB5 images for Botwoon's sprite palette.</summary>
public sealed class BotwoonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("BotwoonColorCatalog-v1", content =>
        {
            content.Append("health", BotwoonHealthPaletteDefinitions.PaletteCount);
            Span<Bgr555> row = stackalloc Bgr555[BotwoonHealthPaletteDefinitions.ColorsPerPalette];
            for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = HealthColor(band, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Compiled palette rows selected by Botwoon's health band.</summary>
    internal readonly BotwoonHealthPaintDefinitions health;

    /// <summary>Retains the validated color rows as runtime paint definitions.</summary>
    internal BotwoonColorCatalog(Bgr555[][] rows) => health = new(rows);

    /// <summary>Gets one compiled BGR555 word from an independently editable health-band palette.</summary>
    /// <param name="band">Health-band index from zero through seven.</param>
    /// <param name="color">Color index from zero through fifteen within the complete palette.</param>
    /// <returns>The selected packed SNES color word.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Either index is outside its authored range.</exception>
    public Bgr555 HealthColor(int band, int color)
    {
        if ((uint)band >= BotwoonHealthPaletteDefinitions.PaletteCount ||
            (uint)color >= BotwoonHealthPaletteDefinitions.ColorsPerPalette)
            throw new ArgumentOutOfRangeException(nameof(band),
                $"Botwoon health band {band}, color {color} is outside the authored images.");
        return health.Resolve(band, color);
    }
}
