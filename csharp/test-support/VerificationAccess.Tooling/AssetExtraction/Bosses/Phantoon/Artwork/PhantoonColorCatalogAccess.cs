using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

/// <summary>
/// Verification's native view of <see cref="PhantoonColorCatalog"/>'s power-on targets (#1464): the cartridge keeps them
/// as its background colours in its order, a group per powered part, and Core keeps them by the part each paints.
/// </summary>
internal static class PhantoonColorCatalogAccess
{
    /// <summary>
    /// The power-on targets the cartridge holds among its background colours (#2604): a group of colours for each part the
    /// power-on paints (<see cref="SuperMetroid.AssetExtraction.CartridgeTables.PhantoonSourceDefinitions.PowerOnGroups"/>).
    /// </summary>
    internal static int PowerOnNativeColourCount =>
        SuperMetroid.AssetExtraction.CartridgeTables.PhantoonSourceDefinitions.PowerOnGroups.Count * PhantoonColorFormat.PoweredPartColourCount;

    /// <summary>Native views of the catalog.</summary>
    extension(PhantoonColorCatalog self)
    {
        /// <summary>The power-on target the cartridge holds at position <paramref name="position"/> of its background colours.</summary>
        internal RgbColor ResolvePowerOnAtNative(int position) =>
            self.ResolvePowerOn(SuperMetroid.AssetExtraction.CartridgeTables.PhantoonSourceDefinitions.PowerOnGroups
                    .Single(part => part.Value == position / PhantoonColorFormat.PoweredPartColourCount).Key,
                position % PhantoonColorFormat.PoweredPartColourCount);

        /// <summary>The health colour the cartridge holds at <paramref name="color"/> of its health palette <paramref name="palette"/>.</summary>
        internal RgbColor ResolveHealthAtNative(int palette, int color) =>
            self.ResolveHealth(SuperMetroid.AssetExtraction.CartridgeTables.PhantoonSourceDefinitions.HealthBandPalettes
                .Single(band => band.Value == palette).Key, color);
    }
}
