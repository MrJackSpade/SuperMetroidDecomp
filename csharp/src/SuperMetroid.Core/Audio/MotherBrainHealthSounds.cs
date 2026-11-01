namespace SuperMetroid.Core.Audio;

/// <summary>Definitions for the Mother Brain encounter's incremental energy sounds.</summary>
internal static class MotherBrainHealthSounds
{
    /// <summary>The first energy value at which gaining or losing energy is audible.</summary>
    public const int MinimumEnergy = 81;
    /// <summary>Mask of the enemy-update counter; zero selects every eighth call.</summary>
    public const int EnemyClockMask = 7;
}
