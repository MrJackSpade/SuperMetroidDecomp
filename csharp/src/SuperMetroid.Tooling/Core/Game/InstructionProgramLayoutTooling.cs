namespace SuperMetroid.Core.Game;

/// <summary>Development-tool instance members of <see cref="InstructionProgramLayout"/>.</summary>
internal static class InstructionProgramLayoutToolingExtensions
{
    extension(InstructionProgramLayout self)
    {
        /// <summary>The <paramref name="index"/>th mechanics word in address order.</summary>
        internal (ushort Address, ushort Value) MechanicsWord(int index)
        {
            if ((uint)index >= self.MechanicsWordCount) throw new IndexOutOfRangeException();
            var (address, word) = self.index.Value.Words[self.index.Value.Mechanics[index]];
            return (address, word.Value);
        }
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
            if (longAddress >> 16 != self.Bank) return false;
            int bankAddress = longAddress & ushort.MaxValue;
            return self.TryWordContaining(bankAddress, out _, out InstructionWord word) && !word.IsPresentation;
        }
    }
}
