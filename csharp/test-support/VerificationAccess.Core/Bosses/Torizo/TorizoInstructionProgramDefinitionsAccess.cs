
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="TorizoInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class TorizoInstructionProgramDefinitionsAccess
{
    /// <summary>Reads bank-$AA mechanics words from the shared Bomb and Golden Torizo instruction layout.</summary>
    extension(TorizoInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the common Torizo instruction layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            TorizoInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
