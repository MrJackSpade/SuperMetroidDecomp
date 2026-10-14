using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="CommonEnemyProjectileInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class CommonEnemyProjectileInstructionProgramDefinitionsAccess
{
    /// <summary>Reads only compiled bank-$86 projectile mechanics words, rejecting addresses outside the layout.</summary>
    extension(CommonEnemyProjectileInstructionProgramDefinitions)
    {
        /// <summary>Requires a word in the compiled bank-$86 projectile mechanics layout.</summary>
        internal static ushort ReadMechanicsWord(ushort address)
        {
            if (CommonEnemyProjectileInstructionProgramDefinitions.TryReadMechanicsWord(address, out ushort value))
                return value;

            throw new InvalidDataException(
                $"Shared enemy-projectile instruction mechanics pointer $86:{address:X4} " +
                "is not compiled.");
        }
    }
}
