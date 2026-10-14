using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="TorizoJumpBackLeftInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoJumpBackLeftInstructionProgramDefinitionsAccess
{
    /// <summary>Reads compiled control words from the left-facing jump-back sequence.</summary>
    extension(TorizoJumpBackLeftInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the left jump-back instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            TorizoJumpBackLeftInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
