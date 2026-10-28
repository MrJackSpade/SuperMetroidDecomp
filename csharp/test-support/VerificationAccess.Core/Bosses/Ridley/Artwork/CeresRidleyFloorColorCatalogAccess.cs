using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>Verification access to <see cref="CeresRidleyFloorColorCatalog"/> members production does not use.</summary>
internal static class CeresRidleyFloorColorCatalogAccess
{
    /// <summary>Exposes <see cref="CeresRidleyFloorColorCatalog"/> state that only verification reads.</summary>
    extension(CeresRidleyFloorColorCatalog self)
    {
        /// <summary>Reads an authored color by the high byte of the native zoom word.</summary>
        public RgbColor Resolve(int zoomHighByte, int color) => CeresRidleyFloorShades.Resolve(self.rows, zoomHighByte, color);
    }
}
