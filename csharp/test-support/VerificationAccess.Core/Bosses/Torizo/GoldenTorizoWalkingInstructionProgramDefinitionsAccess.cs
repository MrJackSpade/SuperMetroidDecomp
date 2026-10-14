
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="GoldenTorizoWalkingInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoWalkingInstructionProgramDefinitionsAccess
{
    /// <summary>Reads bank-relative mechanics words from Golden Torizo's walking programs.</summary>
    extension(GoldenTorizoWalkingInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the walking instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            GoldenTorizoWalkingInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
