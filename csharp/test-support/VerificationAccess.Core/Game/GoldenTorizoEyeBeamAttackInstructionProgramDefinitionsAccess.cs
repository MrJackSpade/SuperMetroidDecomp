using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="GoldenTorizoEyeBeamAttackInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class GoldenTorizoEyeBeamAttackInstructionProgramDefinitionsAccess
{
    /// <summary>Reads compiled mechanics words from the Golden eye-beam attack layout.</summary>
    extension(GoldenTorizoEyeBeamAttackInstructionProgramDefinitions)
    {
        /// <summary>Reads a mechanics word from the compiled eye-beam program layout.</summary>
        internal static bool TryReadMechanicsWord(ushort address, out ushort value) =>
            GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.Layout.TryReadMechanicsWord(address, out value);
    }
}
