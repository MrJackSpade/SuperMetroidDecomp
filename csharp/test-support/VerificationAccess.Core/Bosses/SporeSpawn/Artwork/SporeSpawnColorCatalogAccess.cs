using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;

/// <summary>Verification access to <see cref="SporeSpawnColorCatalog"/> members production does not use.</summary>
internal static class SporeSpawnColorCatalogAccess
{
    /// <summary>Exposes <see cref="SporeSpawnColorCatalog"/> state that only verification reads.</summary>
    extension(SporeSpawnColorCatalog self)
    {
        /// <summary>Resolves one death-script layer's packed RGB5 ink without writing a palette; the caller chooses current colours or target-image ownership.</summary>
        /// <param name="layer">Mutually exclusive sprite, level, or background colour set.</param>
        /// <param name="frame">Zero-based phase, not a byte offset: 0–7 for sprite, 0–6 for level/background.</param>
        /// <param name="color">Ink index 0–15, including the colour set's retained transparent-slot word.</param>
        public RgbColor ResolveDeath(SporeSpawnDeathPaletteLayer layer, int frame, int color) =>
            SporeSpawnColorsConstants.Death(self, layer, frame, color);
    }
}
