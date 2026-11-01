using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using Band = SuperMetroid.Core.Bosses.Crocomire.Artwork.CrocomireColorCatalog.Band;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// Crocomire's five material colour transfers: how a colour resolves from its edit or the calculated stock paint, how a
/// transfer is installed and how the installed colours are identified.
/// </summary>
internal static class CrocomirePalette
{
    /// <summary>Hashes each complete resolved transfer in the original color order.</summary>
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

    /// <summary>Returns an edited color or calculates the corresponding stock paint.</summary>
    internal static RgbColor Get(Dictionary<CrocomireColorKey, RgbColor> edits, Band band, int index)
    {
        if ((uint)index >= CrocomireColorCatalog.Counts[band]) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new CrocomireColorKey(band, index), out RgbColor edited)
            ? edited
            : CrocomireColorCatalog.StockPaint[band](index);
    }

    /// <summary>Installs the initialization transfers in order: the wall colours, then the projectile colours.</summary>
    internal static void ApplyInitial(CrocomireColorCatalog colors, BossColours destination)
    {
        Apply(colors, destination.CrocomireInitialWall, Band.InitialWall);
        Apply(colors, destination.CrocomireInitialProjectile, Band.InitialProjectile);
    }

    /// <summary>Writes one complete material band to its selected colour run.</summary>
    internal static void Apply(CrocomireColorCatalog colors, ColourRun destination, Band band) =>
        Apply(colors, new ColourRunSequence(destination), band);

    /// <summary>Writes one complete material band across its destination runs, run after run.</summary>
    private static void Apply(CrocomireColorCatalog colors, ColourRunSequence destination, Band band)
    {
        for (int color = 0; color < CrocomireColorCatalog.Counts[band]; color++)
            destination.Write(color, colors.Get(band, color));
    }
}
