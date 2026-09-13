namespace SuperMetroid.Core.Audio;

/// <summary>Cartridge definitions for Mother Brain encounter incremental energy audio.</summary>
internal static class MotherBrainHealthSounds
{
    /// <summary>$A9:C54A, HandlePlayingGainingLosingIncrementalEnergySFX: first audible energy value.</summary>
    public const ushort MinimumEnergy = 81;
    /// <summary>$A9:C552: mask of the enemy-main execution counter; zero selects every eighth call.</summary>
    public const ushort EnemyClockMask = 7;
    /// <summary>$A9:C557: library-three $2D incremental energy sound, queued through Max3.</summary>
    public static readonly SoundEffectId IncrementalEnergy = new(SoundEffectLibrary.Library3, 0x2d); // magic-number-audit: allow(AudioId) - named cartridge SFX identity
}
