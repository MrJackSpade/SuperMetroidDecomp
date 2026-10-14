namespace SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Programs;

/// <summary>Verification access to <see cref="GoldenTorizoJumpLandingInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoJumpLandingInstructionProgramDefinitionsAccess
{
    /// <summary>Searches Golden Torizo's jump-landing stream for a compiled control word.</summary>
    extension(GoldenTorizoJumpLandingInstructionProgramDefinitions)
    {
        /// <summary>Searches the jump-landing stream's compiled mechanics words.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value)
        {
            for (int index = 0; index < GoldenTorizoJumpLandingInstructionProgramDefinitions.MechanicsWordCount; index++)
            {
                InstructionMechanicsWord word = GoldenTorizoJumpLandingInstructionProgramDefinitions.MechanicsWord(index);
                if (word.Address != address) continue;
                value = word.Value;
                return true;
            }
            value = 0;
            return false;
        }
    }
}
