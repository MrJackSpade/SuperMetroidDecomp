using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="SamusSpritemapArtworkCatalog"/> members production does not use.</summary>
internal static class SamusSpritemapArtworkCatalogAccess
{
    /// <summary>Inspects installed Samus compositions.</summary>
    extension(SamusSpritemapArtworkCatalog self)
    {
        /// <summary>The stored compositions, each at its identity's ordinal, without recreating stock art.</summary>
        internal IReadOnlyList<SamusSpritemapDefinition> Definitions => self.definitions;
    }

    /// <summary>Where the cartridge stores the tables the catalog installs.</summary>
    extension(SamusSpritemapArtworkCatalog)
    {
        /// <summary>$92:808D: the spritemap pointer of each pose-half table entry.</summary>
        internal static int PointerTableAddress => SamusCartridgeTables.SpritemapPointers;

        /// <summary>$92:9263: each pose's first upper-half table entry.</summary>
        internal static int TopBaseAddress => SamusCartridgeTables.SpritemapTopBases;

        /// <summary>$92:945D: each pose's first lower-half table entry.</summary>
        internal static int BottomBaseAddress => SamusCartridgeTables.SpritemapBottomBases;
    }
}
