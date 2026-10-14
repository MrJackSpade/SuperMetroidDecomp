using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="GoldenTorizoRightSonicInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoRightSonicInstructionProgramDefinitionsAccess
{
    /// <summary>Reads control words from Golden Torizo's rightward sonic-boom sequence.</summary>
    extension(GoldenTorizoRightSonicInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the right sonic instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            GoldenTorizoRightSonicInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
