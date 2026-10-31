namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge definitions for Mother Brain encounter incremental energy audio.</summary>
internal static class MotherBrainHealthSounds
{
    /// <summary>$A9:C54A, HandlePlayingGainingLosingIncrementalEnergySFX: first audible energy value.</summary>
    public const int MinimumEnergy = 81;
    /// <summary>$A9:C552: mask of the enemy-main execution counter; zero selects every eighth call.</summary>
    public const int EnemyClockMask = 7;
}
