using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="GoldenTorizoInitialInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoInitialInstructionProgramDefinitionsAccess
{
    /// <summary>Searches the Golden Torizo initial stream for a bank-relative mechanics word.</summary>
    extension(GoldenTorizoInitialInstructionProgramDefinitions)
    {
        /// <summary>Searches the initial Torizo instruction stream's compiled mechanics words.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value)
        {
            for (int index = 0; index < GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWordCount; index++)
            {
                InstructionMechanicsWord word = GoldenTorizoInitialInstructionProgramDefinitions.MechanicsWord(index);
                if (word.Address == address)
                {
                    value = word.Value;
                    return true;
                }
            }
            value = 0;
            return false;
        }
    }
}
