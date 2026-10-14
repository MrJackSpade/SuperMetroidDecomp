
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="GoldenTorizoLeftFootOrbInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoLeftFootOrbInstructionProgramDefinitionsAccess
{
    /// <summary>Reads the left-foot orb's bank-relative compiled instruction mechanics.</summary>
    extension(GoldenTorizoLeftFootOrbInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the left-foot orb instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
