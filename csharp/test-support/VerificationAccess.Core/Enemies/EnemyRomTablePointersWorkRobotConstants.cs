
namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.WorkRobot"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersWorkRobotConstants
{
    /// <summary>Parameterized instruction-list pointer words at $A8:CC30.</summary>
    public const int InitialInstructionListWords = 0xa8cc30;

    /// <summary>Provides the work robot initial instruction-list table for spawn-state comparisons.</summary>
    extension(EnemyRomTablePointers.WorkRobot)
    {
        /// <inheritdoc cref="EnemyRomTablePointersWorkRobotConstants.InitialInstructionListWords"/>
        internal static int InitialInstructionListWords => EnemyRomTablePointersWorkRobotConstants.InitialInstructionListWords;
    }
}
