
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="TorizoJumpBackInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoJumpBackInstructionProgramDefinitionsAccess
{
    /// <summary>Reads compiled control words from the common Torizo jump-back sequence.</summary>
    extension(TorizoJumpBackInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the jump-back instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            TorizoJumpBackInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
