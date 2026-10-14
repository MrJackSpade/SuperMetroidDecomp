
namespace SuperMetroid.Core.Enemies.Stoke;

/// <summary>Cartridge values of <see cref="StokeProjectileInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class StokeProjectileInstructionProgramDefinitionsConstants
{
    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the projectile loop at $86:DB14.
    /// </summary>
    public const ushort LoopCommand = 0xdb14;

    /// <summary>Provides the loop command in Stoke projectile instructions for native program decoding.</summary>
    extension(StokeProjectileInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="StokeProjectileInstructionProgramDefinitionsConstants.LoopCommand"/>
        internal static ushort LoopCommand => StokeProjectileInstructionProgramDefinitionsConstants.LoopCommand;
    }
}
