using SuperMetroid.Core.Assets;


namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="TorizoArtworkSwapDefinitions"/> members production does not use.</summary>
internal static class TorizoArtworkSwapDefinitionsAccess
{
    /// <summary>The cartridge source a swap's characters were extracted from: its page's source plus its page byte.</summary>
    extension(TorizoArtworkSwapDefinition swap)
    {
        /// <summary>The swap's cartridge source address.</summary>
        internal int SourceAddress => SuperMetroid.Core.Bosses.Torizo.Artwork.TorizoSwapArtworkDefinitions.All[(int)swap.Page].SourceAddress + swap.PageByte;
    }

    /// <summary>Reads the Torizo instruction stream's VRAM transfer records in source order.</summary>
    extension(TorizoArtworkSwapDefinitions)
    {
        /// <summary>Compiled native VRAM transfer records in source order.</summary>
        internal static ReadOnlySpan<TorizoArtworkSwapDefinition> All => TorizoArtworkSwapDefinitions.Entries;
    }
}
