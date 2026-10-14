using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="TorizoInstructionVramTransferDefinitions"/> that only verification reads.</summary>
internal static class TorizoInstructionVramTransferDefinitionsConstants
{
    /// <summary>Bank $AA containing the Torizo instruction-controlled VRAM transfer records.</summary>
    public const byte Bank = 0xaa;

    /// <summary>Provides the bank of Torizo instruction VRAM transfer records.</summary>
    extension(TorizoInstructionVramTransferDefinitions)
    {
        /// <inheritdoc cref="TorizoInstructionVramTransferDefinitionsConstants.Bank"/>
        internal static byte Bank => TorizoInstructionVramTransferDefinitionsConstants.Bank;
    }
}
