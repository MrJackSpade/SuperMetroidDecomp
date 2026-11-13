namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>What a cue of Cacatac's animations does: spit a spike, play the spike-release sound, or restore its patrol.</summary>
internal enum CacatacCueKind
{
    /// <summary>Plays the spike-release sound.</summary>
    PlaySpikeSound,
    /// <summary>Restores main AI's patrol from the walking direction.</summary>
    RestorePatrol,
    /// <summary>Spits a spike in the direction its operand selects.</summary>
    SpawnSpike,
}
