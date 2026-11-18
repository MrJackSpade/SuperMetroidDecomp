namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>What a signal cue of Cacatac's animations does: play the spike-release sound, or restore its patrol.</summary>
internal enum CacatacCueKind
{
    /// <summary>Plays the spike-release sound.</summary>
    PlaySpikeSound,
    /// <summary>Restores main AI's patrol from the walking direction.</summary>
    RestorePatrol,
}
