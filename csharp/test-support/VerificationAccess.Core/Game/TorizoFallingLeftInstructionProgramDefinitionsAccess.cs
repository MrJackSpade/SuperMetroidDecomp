using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="TorizoFallingLeftInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoFallingLeftInstructionProgramDefinitionsAccess
{
    /// <summary>Reads the compiled control words for Torizo's left-facing fall sequence.</summary>
    extension(TorizoFallingLeftInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the falling-left instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            TorizoFallingLeftInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
