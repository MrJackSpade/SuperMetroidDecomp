using SuperMetroid.Core.Assets;


namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="TorizoInstructionVramTransferDefinitions"/> members production does not use.</summary>
internal static class TorizoInstructionVramTransferDefinitionsAccess
{
    /// <summary>Reads the Torizo instruction stream's VRAM transfer records in source order.</summary>
    extension(TorizoInstructionVramTransferDefinitions)
    {
        /// <summary>Compiled native VRAM transfer records in source order.</summary>
        internal static ReadOnlySpan<TileTransferDefinition> All => TorizoInstructionVramTransferDefinitions.Entries;
    }
}
