using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="GoldenTorizoLeftTurnInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoLeftTurnInstructionProgramDefinitionsAccess
{
    /// <summary>Reads the compiled turn-left sequence's instruction control words.</summary>
    extension(GoldenTorizoLeftTurnInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the left-turn instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            GoldenTorizoLeftTurnInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
