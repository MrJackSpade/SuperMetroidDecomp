
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="GoldenTorizoRightOrbInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoRightOrbInstructionProgramDefinitionsAccess
{
    /// <summary>Reads the right-facing Chozo orb's compiled instruction mechanics.</summary>
    extension(GoldenTorizoRightOrbInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the right orb instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            GoldenTorizoRightOrbInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
