using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="GoldenTorizoLeftOrbInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoLeftOrbInstructionProgramDefinitionsAccess
{
    /// <summary>Reads the left-facing Chozo orb's compiled instruction mechanics.</summary>
    extension(GoldenTorizoLeftOrbInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the left orb instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
