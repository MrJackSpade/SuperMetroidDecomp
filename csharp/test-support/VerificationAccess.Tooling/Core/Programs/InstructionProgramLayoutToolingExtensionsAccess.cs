using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Programs;

/// <summary>Verification access to <see cref="InstructionProgramLayoutToolingExtensions"/> members production does not use.</summary>
internal static class InstructionProgramLayoutToolingExtensionsAccess
{
    /// <summary>Exposes <see cref="InstructionProgramLayoutToolingExtensions"/> state that only verification reads.</summary>
    extension(InstructionProgramLayout self)
    {
        /// <summary>The word containing <paramref name="bankAddress"/>, checking the earlier start first.</summary>
        internal bool TryWordContaining(int bankAddress, out ushort start, out InstructionWord word)
        {
            if (bankAddress > 0 && self.index.Value.TryGet((ushort)(bankAddress - 1), out word))
            {
                start = (ushort)(bankAddress - 1);
                return true;
            }
            if (bankAddress <= ushort.MaxValue && self.index.Value.TryGet((ushort)bankAddress, out word))
            {
                start = (ushort)bankAddress;
                return true;
            }
            start = 0;
            word = default;
            return false;
        }
        /// <summary>True when either byte of a mechanics word lies at <paramref name="longAddress"/>.</summary>
        internal bool IsCompiledMechanicsByte(int longAddress)
        {
            if (SnesAddressMath.BankNumber(longAddress) != self.Bank) return false;
            int bankAddress = SnesAddressMath.OffsetInBank(longAddress);
            return self.TryWordContaining(bankAddress, out _, out InstructionWord word) && !word.IsPresentation;
        }
    }
}
