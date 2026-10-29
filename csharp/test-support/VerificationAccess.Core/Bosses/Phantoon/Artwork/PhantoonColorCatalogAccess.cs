using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// Verification's native view of <see cref="PhantoonColorCatalog"/>'s power-on targets (#1464): the cartridge keeps them
/// as its background colours in its order, and Core keeps them by the set each targets.
/// </summary>
internal static class PhantoonColorCatalogAccess
{
    /// <summary>Native views of the catalog.</summary>
    extension(PhantoonColorCatalog self)
    {
        /// <summary>The power-on target the cartridge holds at position <paramref name="position"/> of its background colours.</summary>
        internal RgbColor ResolvePowerOnAtNative(int position) =>
            self.ResolvePowerOn(NativeBackgroundSets.AtNativeSet(position / ColourSets.SetColours), position % ColourSets.SetColours);
    }
}
