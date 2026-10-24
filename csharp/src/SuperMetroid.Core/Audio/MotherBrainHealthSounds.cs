namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge definitions for Mother Brain encounter incremental energy audio.</summary>
internal static class MotherBrainHealthSounds
{
    /// <summary>$A9:C54A, HandlePlayingGainingLosingIncrementalEnergySFX: first audible energy value.</summary>
    public const ushort MinimumEnergy = 81;
    /// <summary>$A9:C552: mask of the enemy-main execution counter; zero selects every eighth call.</summary>
    public const ushort EnemyClockMask = 7;
}
