using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="EnemyRomTablePointers.TourianStatue"/> that only verification reads.</summary>
internal static class EnemyRomTablePointersTourianStatueConstants
{
    /// <summary>Statue instruction-list table at $AA:D810.</summary>
    public const int InstructionListWords = 0xaad810;

    /// <summary>Provides the Tourian statue instruction-list table address for native program verification.</summary>
    extension(EnemyRomTablePointers.TourianStatue)
    {
        /// <inheritdoc cref="EnemyRomTablePointersTourianStatueConstants.InstructionListWords"/>
        internal static int InstructionListWords => EnemyRomTablePointersTourianStatueConstants.InstructionListWords;
    }
}
