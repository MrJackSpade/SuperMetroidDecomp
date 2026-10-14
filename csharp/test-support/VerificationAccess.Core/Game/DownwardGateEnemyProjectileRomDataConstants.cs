using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="DownwardGateEnemyProjectileRomData"/> that only verification reads.</summary>
internal static class DownwardGateEnemyProjectileRomDataConstants
{
    /// <summary>Instruction pointer that keeps the downward gate projectile closed and idle.</summary>
    public const ushort ClosedSleepInstruction = DownwardGateProjectileInstructionProgramDefinitions.ClosedSleep;

    /// <summary>Provides the closed-gate sleep instruction used when checking downward gate projectile behavior.</summary>
    extension(DownwardGateEnemyProjectileRomData)
    {
        /// <inheritdoc cref="DownwardGateEnemyProjectileRomDataConstants.ClosedSleepInstruction"/>
        internal static ushort ClosedSleepInstruction => DownwardGateEnemyProjectileRomDataConstants.ClosedSleepInstruction;
    }
}
