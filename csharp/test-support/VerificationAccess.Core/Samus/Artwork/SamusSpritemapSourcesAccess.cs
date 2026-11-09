using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="SamusSpritemapSources"/> members the import does not use.</summary>
internal static class SamusSpritemapSourcesAccess
{
    /// <summary>Inspects installed Samus compositions.</summary>
    extension(SamusSpritemapSources self)
    {
        /// <summary>The stored compositions, each at its identity's ordinal, without recreating stock art.</summary>
        internal IReadOnlyList<SamusSpritemapDefinition> Definitions => self.definitions;

        /// <summary>Whether <paramref name="frame"/> is an animation frame installed for its pose half.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The pose has no record.</exception>
        internal bool Covers(SamusBodyHalfFrame frame) => (uint)frame.AnimationFrame < (uint)self.Frames(frame).Count;
    }

    /// <summary>Where the cartridge stores the tables the catalog installs.</summary>
    extension(SamusSpritemapSources)
    {
        /// <summary>$92:808D: the spritemap pointer of each pose-half table entry.</summary>
        internal static int PointerTableAddress => SamusSourceDefinitions.SpritemapPointers.Value;

        /// <summary>$92:9263: each pose's first upper-half table entry.</summary>
        internal static int TopBaseAddress => SamusSourceDefinitions.SpritemapTopBases.Value;

        /// <summary>$92:945D: each pose's first lower-half table entry.</summary>
        internal static int BottomBaseAddress => SamusSourceDefinitions.SpritemapBottomBases.Value;
    }
}
