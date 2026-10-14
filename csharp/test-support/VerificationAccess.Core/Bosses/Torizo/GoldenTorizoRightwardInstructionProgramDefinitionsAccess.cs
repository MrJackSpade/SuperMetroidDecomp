
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="GoldenTorizoRightwardInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoRightwardInstructionProgramDefinitionsAccess
{
    /// <summary>Reads compiled mechanics from the rightward Golden Torizo movement sequence.</summary>
    extension(GoldenTorizoRightwardInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the rightward instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            GoldenTorizoRightwardInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
