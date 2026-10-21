namespace SuperMetroid.Core.Programs;

/// <summary>Development-tool instance members of <see cref="InstructionProgramLayout"/>.</summary>
internal static class InstructionProgramLayoutToolingExtensions
{
    /// <summary>Lets development tools inspect indexed native instruction words by address.</summary>
    extension(InstructionProgramLayout self)
    {
        /// <summary>The <paramref name="index"/>th mechanics word in address order.</summary>
        internal AddressedValue MechanicsWord(int index)
        {
            if ((uint)index >= self.MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
            var (address, word) = self.index.Value.Words[self.index.Value.Mechanics[index]];
            return new AddressedValue(address, word.Value);
        }
    }
}
