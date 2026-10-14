
namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>Cartridge values of <see cref="NuclearWaffleProjectileInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class NuclearWaffleProjectileInstructionProgramDefinitionsConstants
{
    /// <summary>
    /// <c>Instruction_EnemyProjectile_GotoY</c> closing the body loop at $86:BB8E.
    /// </summary>
    public const ushort LoopCommand = 0xbb8e;

    /// <summary>Provides the loop command used when decoding Nuclear Waffle projectile instructions.</summary>
    extension(NuclearWaffleProjectileInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="NuclearWaffleProjectileInstructionProgramDefinitionsConstants.LoopCommand"/>
        internal static ushort LoopCommand => NuclearWaffleProjectileInstructionProgramDefinitionsConstants.LoopCommand;
    }
}
