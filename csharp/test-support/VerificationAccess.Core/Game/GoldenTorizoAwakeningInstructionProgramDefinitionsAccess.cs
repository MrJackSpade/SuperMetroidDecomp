using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="GoldenTorizoAwakeningInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoAwakeningInstructionProgramDefinitionsAccess
{
    /// <summary>Finds compiled control words in Golden Torizo's awakening instruction sequence.</summary>
    extension(GoldenTorizoAwakeningInstructionProgramDefinitions)
    {
        /// <summary>Searches compiled awakening mechanics words by bank-relative address.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value)
        {
            for (int index = 0; index < GoldenTorizoAwakeningInstructionProgramDefinitionsTooling.MechanicsWordCount; index++)
            {
                var word = GoldenTorizoAwakeningInstructionProgramDefinitionsTooling.MechanicsWord(index);
                if (word.Address != address) continue;
                value = word.Value;
                return true;
            }
            value = 0;
            return false;
        }
    }
}
