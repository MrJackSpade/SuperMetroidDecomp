using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Phantoon.Artwork;

/// <summary>
/// Verification's native view of <see cref="PhantoonColorCatalog"/>'s power-on targets (#1464): the cartridge keeps them
/// as its background colours in its order, a group per powered part, and Core keeps them by the part each paints.
/// </summary>
internal static class PhantoonColorCatalogAccess
{
    /// <summary>Native views of the catalog.</summary>
    extension(PhantoonColorCatalog self)
    {
        /// <summary>The power-on target the cartridge holds at position <paramref name="position"/> of its background colours.</summary>
        internal RgbColor ResolvePowerOnAtNative(int position) =>
            self.ResolvePowerOn(PhantoonColorDefinitions.PoweredParts[position / WreckedShipPowerPaintDefinitions.PartColourCount],
                position % WreckedShipPowerPaintDefinitions.PartColourCount);
    }
}
