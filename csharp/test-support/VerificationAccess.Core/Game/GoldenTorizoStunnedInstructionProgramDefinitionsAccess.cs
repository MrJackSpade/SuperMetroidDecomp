using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="GoldenTorizoStunnedInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoStunnedInstructionProgramDefinitionsAccess
{
    /// <summary>Reads compiled control words from Golden Torizo's stunned reaction sequence.</summary>
    extension(GoldenTorizoStunnedInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the stunned instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            GoldenTorizoStunnedInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
