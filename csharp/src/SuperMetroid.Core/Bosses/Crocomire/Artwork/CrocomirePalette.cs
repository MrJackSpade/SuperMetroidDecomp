using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using Band = SuperMetroid.Core.Bosses.Crocomire.Artwork.CrocomireColorCatalog.Band;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Crocomire's five material colour transfers: which supplied colours are edits of the calculated stock paint, how a
/// colour resolves, how a transfer is installed and how the installed colours are identified.
/// </summary>
internal static class CrocomirePalette
{
    /// <summary>Hashes each complete resolved transfer in native color order.</summary>
    internal static string ContentIdentity(CrocomireColorCatalog colors) =>
        SelectedPresentationHash.Create("CrocomireColorCatalog-v1", content =>
        {
            Span<RgbColor> buffer = stackalloc RgbColor[CrocomireColorCatalog.Counts.Values.Max()];
            foreach ((Band band, string label) in CrocomireColorCatalog.IdentityOrder)
            {
                Span<RgbColor> transfer = buffer[..CrocomireColorCatalog.Counts[band]];
                for (int color = 0; color < transfer.Length; color++) transfer[color] = colors.Get(band, color);
                content.AppendColors(label, transfer);
            }
        });

    /// <summary>Keeps each supplied color of every band that differs from its calculated stock paint.</summary>
    internal static Dictionary<CrocomireColorKey, RgbColor> Edits(Dictionary<Band, RgbColor[]> supplied)
    {
        var edits = new Dictionary<CrocomireColorKey, RgbColor>();
        foreach ((Band band, RgbColor[] colors) in supplied)
        {
            for (int color = 0; color < colors.Length; color++)
                if (colors[color] != CrocomireColorCatalog.StockPaint[band](color))
                    edits.Add(new CrocomireColorKey(band, color), colors[color]);
        }
        return edits;
    }

    /// <summary>Returns an edited color or calculates the corresponding native stock paint.</summary>
    internal static RgbColor Get(Dictionary<CrocomireColorKey, RgbColor> edits, Band band, int index)
    {
        if ((uint)index >= CrocomireColorCatalog.Counts[band]) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new CrocomireColorKey(band, index), out RgbColor edited)
            ? edited
            : CrocomireColorCatalog.StockPaint[band](index);
    }

    /// <summary>Installs the native initialization transfers in order: the wall row, then the projectile row.</summary>
    internal static void ApplyInitial(CrocomireColorCatalog colors, BossColours destination)
    {
        Apply(colors, destination.CrocomireInitialWall, Band.InitialWall);
        Apply(colors, destination.CrocomireInitialProjectile, Band.InitialProjectile);
    }

    /// <summary>Writes one complete material band to its selected CGRAM destination.</summary>
    internal static void Apply(CrocomireColorCatalog colors, ColourRun destination, Band band)
    {
        for (int color = 0; color < CrocomireColorCatalog.Counts[band]; color++)
            destination.Write(color, colors.Get(band, color));
    }
}
