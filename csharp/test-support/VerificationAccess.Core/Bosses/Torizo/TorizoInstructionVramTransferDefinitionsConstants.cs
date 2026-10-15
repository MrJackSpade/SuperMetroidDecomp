
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="TorizoInstructionVramTransferDefinitions"/> that only verification reads.</summary>
internal static class TorizoInstructionVramTransferDefinitionsConstants
{
    /// <summary>Bank $AA containing the Torizo instruction-controlled VRAM transfer records.</summary>
    public const byte Bank = 0xaa;

    /// <summary>
    /// The cartridge site of each transfer, in <see cref="TorizoInstructionVramTransferDefinitions.Entries"/> order:
    /// the cartridge oracle finds each descriptor by its instruction.
    /// </summary>
    internal static readonly TransferSite[] Sites =
    [
        new(0xb0f1), new(0xb0fa), new(0xb103), new(0xb10c), new(0xb161), new(0xb16a),
        new(0xb173), new(0xb17c), new(0xb189), new(0xb192), new(0xb19b), new(0xb1a4),
        new(0xb1ad), new(0xb88f), new(0xb89c), new(0xb8a9), new(0xb8b6), new(0xb8cb),
        new(0xb8d8), new(0xb8e5), new(0xb8f2), new(0xc9cb), new(0xca10), new(0xca1d),
        new(0xca2a), new(0xca37), new(0xca4c), new(0xca59), new(0xca66), new(0xca73),
        new(0xd035), new(0xd03e), new(0xd04b), new(0xd054), new(0xd061), new(0xd06a),
        new(0xd08f), new(0xd098), new(0xd0a5), new(0xd0ae), new(0xd0bb), new(0xd0c4),
        new(0xd0d1), new(0xd0da), new(0xd137), new(0xd144), new(0xd151), new(0xd15e),
        new(0xd1a5), new(0xd1b2), new(0xd1bf), new(0xd1cc),
    ];

    /// <summary>Where one transfer sits in the cartridge: its <c>$814B</c> instruction in bank $AA.</summary>
    /// <param name="Instruction">The bank-$AA address of the transfer's <c>$814B</c> instruction.</param>
    internal readonly record struct TransferSite(ushort Instruction);

    /// <summary>Provides the bank of Torizo instruction VRAM transfer records.</summary>
    extension(TorizoInstructionVramTransferDefinitions)
    {
        /// <inheritdoc cref="TorizoInstructionVramTransferDefinitionsConstants.Bank"/>
        internal static byte Bank => TorizoInstructionVramTransferDefinitionsConstants.Bank;
        /// <inheritdoc cref="TorizoInstructionVramTransferDefinitionsConstants.Sites"/>
        internal static ReadOnlySpan<TorizoInstructionVramTransferDefinitionsConstants.TransferSite> Sites => TorizoInstructionVramTransferDefinitionsConstants.Sites;
    }
}
