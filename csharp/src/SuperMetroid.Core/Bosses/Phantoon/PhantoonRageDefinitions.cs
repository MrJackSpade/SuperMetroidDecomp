namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The shape and pacing of Phantoon's enraged flame waves, independent of how each flame then moves.</summary>
internal static class PhantoonRageDefinitions
{
    /// <summary>Round count before closing the eye and fading out.</summary>
    internal const int WaveCount = 8;
    /// <summary>AI calls separating successive waves.</summary>
    internal const int WaveInterval = 128;
    /// <summary>Room-library sound emitted once after each complete wave.</summary>
    internal static SoundEffectId WaveSound => RoomSound.PhantoonSuperMissiled;
    /// <summary>The wave sound's request priority.</summary>
    internal static readonly SoundRequestPriority WaveSoundPriority = SoundRequestPriority.Normal;
}
