namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The shape and pacing of Phantoon's enraged flame waves, independent of how each flame then moves.</summary>
internal static class PhantoonRageDefinitions
{
    /// <summary>$A7:D8BB: first direction of the seven-flame even wave; descends through zero.</summary>
    internal const int EvenWaveFirstDirection = 6;
    /// <summary>$A7:D8D0: first direction of the eight-flame odd wave.</summary>
    internal const int OddWaveFirstDirection = 15;
    /// <summary>$A7:D8E1: inclusive final odd-wave direction after DEY/CPY/BPL.</summary>
    internal const int OddWaveLastDirection = 8;
    /// <summary>$A7:D8F4: round count before closing the eye and fading out.</summary>
    internal const int WaveCount = 8;
    /// <summary>$A7:D8F9: AI calls separating successive waves.</summary>
    internal const int WaveInterval = 128;
    /// <summary>$A7:D8E6: library-three sound emitted once after each complete wave.</summary>
    internal static SoundEffectId WaveSound => Library3Sound.PhantoonSuperMissiled;
    /// <summary>The wave sound's request priority.</summary>
    internal static readonly SoundRequestPriority WaveSoundPriority = SoundRequestPriority.Normal;
}
