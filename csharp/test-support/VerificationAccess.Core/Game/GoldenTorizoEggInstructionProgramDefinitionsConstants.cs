using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="GoldenTorizoEggInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoEggInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Hatched_Left_1</c> at $86:B152.</summary>
    public const ushort HatchedLeftLoop = 0xb152;
    /// <summary><c>InstList_EnemyProjectile_GoldenTorizoEgg_Hatched_Right_1</c> at $86:B16D.</summary>
    public const ushort HatchedRightLoop = 0xb16d;

    /// <summary>Provides Golden Torizo egg loops for its hatched left and right states.</summary>
    extension(GoldenTorizoEggInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoEggInstructionProgramDefinitionsConstants.HatchedLeftLoop"/>
        internal static ushort HatchedLeftLoop => GoldenTorizoEggInstructionProgramDefinitionsConstants.HatchedLeftLoop;
        /// <inheritdoc cref="GoldenTorizoEggInstructionProgramDefinitionsConstants.HatchedRightLoop"/>
        internal static ushort HatchedRightLoop => GoldenTorizoEggInstructionProgramDefinitionsConstants.HatchedRightLoop;
    }
}
