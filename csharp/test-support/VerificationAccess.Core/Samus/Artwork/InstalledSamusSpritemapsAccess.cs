using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="InstalledSamusSpritemaps"/> members production does not use.</summary>
internal static class InstalledSamusSpritemapsAccess
{
    /// <summary>Inspects installed Samus compositions.</summary>
    extension(InstalledSamusSpritemaps self)
    {
        /// <summary>The stored compositions, each at its identity's ordinal, without recreating stock art.</summary>
        internal IReadOnlyList<SamusSpritemapDefinition> Definitions => self.Source.definitions;

        /// <summary>The cartridge's per-pose first upper-half table entries the import mapped to pose-half frames.</summary>
        internal ReadOnlySpan<ushort> TopBases => self.Source.TopBases;

        /// <summary>The cartridge's per-pose first lower-half table entries the import mapped to pose-half frames.</summary>
        internal ReadOnlySpan<ushort> BottomBases => self.Source.BottomBases;

        /// <summary>The cartridge's pose-half table entries the import mapped to pose-half frames.</summary>
        internal ReadOnlySpan<SamusSpritemapId?> Entries => self.Source.Entries;

        /// <summary>The table entry at which <paramref name="pose"/>'s upper half begins.</summary>
        internal ushort TopBase(SamusPoseId pose) => self.Source.TopBase(pose);

        /// <summary>The table entry at which <paramref name="pose"/>'s lower half begins.</summary>
        internal ushort BottomBase(SamusPoseId pose) => self.Source.BottomBase(pose);
    }

    /// <summary>Where the cartridge stores the tables the catalog installs.</summary>
    extension(InstalledSamusSpritemaps)
    {
        /// <summary>$92:808D: the spritemap pointer of each pose-half table entry.</summary>
        internal static int PointerTableAddress => SamusSourceDefinitions.SpritemapPointers.Value;

        /// <summary>$92:9263: each pose's first upper-half table entry.</summary>
        internal static int TopBaseAddress => SamusSourceDefinitions.SpritemapTopBases.Value;

        /// <summary>$92:945D: each pose's first lower-half table entry.</summary>
        internal static int BottomBaseAddress => SamusSourceDefinitions.SpritemapBottomBases.Value;
    }
}
