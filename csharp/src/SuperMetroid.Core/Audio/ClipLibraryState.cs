namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound library's clip playback (#1305): the sound it plays, from which frame, whether it
/// still holds the library (before its logical end), the library's priority and low-health mode,
/// and which layers an upload has silenced until their next key-on.
/// </summary>
internal sealed class ClipLibraryState
{
    /// <summary>The playing sound, or null.</summary>
    internal SoundEffectClipEntry? Sound { get; private set; }

    /// <summary>Absolute frame of the sound's frame zero (the accepting poll).</summary>
    internal long Start { get; private set; }

    /// <summary>Whether the sound still holds the library: it has not reached its logical end.</summary>
    internal bool Active { get; private set; }

    /// <summary>Whether the library holds a high-priority sound, which admits only requests exempt from priority; otherwise any request replaces the sound.</summary>
    internal bool HighPriority { get; private set; }

    /// <summary>Whether library 3 is in low-health mode, which rejects every request except those exempt from it.</summary>
    internal bool InLowHealthMode { get; private set; }

    /// <summary>Per layer, the clip frame before which an upload has silenced it; zero when audible.</summary>
    internal long[] SilencedUntil { get; private set; } = [];

    /// <summary>Whether the library's priority or low-health mode turns away a request under <paramref name="policy"/>.</summary>
    internal bool Rejects(SoundEffectClipPolicy policy) =>
        (!policy.IgnoresLowHealthMode && InLowHealthMode) || (!policy.IgnoresPriority && HighPriority);

    /// <summary>
    /// Starts <paramref name="sound"/> at <paramref name="now"/> in place of the playing sound, whose
    /// priority it drops while that sound still held the library, then applies the request's
    /// priority and low-health effects.
    /// </summary>
    internal void Play(SoundEffectClipEntry sound, long now, SoundEffectClipPolicy policy)
    {
        if (Sound is not null && Active)
            HighPriority = false;
        Sound = sound;
        Start = now;
        Active = true;
        SilencedUntil = new long[sound.Layers.Count];
        HighPriority = policy.Priority switch
        {
            SoundEffectPriorityEffect.Low => false,
            SoundEffectPriorityEffect.High => true,
            SoundEffectPriorityEffect.Unchanged => HighPriority,
            _ => throw new InvalidDataException($"Unknown priority effect {policy.Priority}."),
        };
        InLowHealthMode = policy.LowHealthMode switch
        {
            LowHealthModeEffect.Unchanged => InLowHealthMode,
            LowHealthModeEffect.Clear => false,
            LowHealthModeEffect.Enter => true,
            _ => throw new InvalidDataException($"Unknown low-health mode effect {policy.LowHealthMode}."),
        };
    }

    /// <summary>Ends the sound at a poll after its logical end, releasing the library and its priority.</summary>
    internal void EndIfFinished(long now)
    {
        if (Active && Sound?.ActiveFrames is long active && now - Start > active)
        {
            Active = false;
            HighPriority = false;
        }
    }

    /// <summary>Drops the sound, which an upload does once the sound has passed its logical end.</summary>
    internal void Clear() => Sound = null;

    /// <summary>
    /// Continues the sound as <paramref name="continued"/> from the new bank context, each layer
    /// silent until <paramref name="silencedUntil"/>, which an upload does while the sound still holds the library.
    /// </summary>
    internal void Continue(SoundEffectClipEntry continued, long[] silencedUntil)
    {
        Sound = continued;
        SilencedUntil = silencedUntil;
    }
}
