namespace SuperMetroid.Core.Audio;

/// <summary>
/// One sound library's clip playback (#1305): the sound it plays, from which frame, whether it
/// still holds the library (before its logical end), the library's priority and low-health mode,
/// and which layers an upload has silenced until their next key-on.
/// </summary>
internal sealed class ClipLibraryState
{
    /// <summary>The playing sound, or null.</summary>
    internal SoundEffectClipEntry? Sound;

    /// <summary>Absolute frame of the sound's frame zero (the accepting poll).</summary>
    internal long Start;

    /// <summary>Whether the sound still holds the library: it has not reached its logical end.</summary>
    internal bool Active;

    /// <summary>Whether the library holds a high-priority sound, which admits only requests exempt from priority; otherwise any request replaces the sound.</summary>
    internal bool HighPriority;

    /// <summary>Whether library 3 is in low-health mode, which rejects every request except those exempt from it.</summary>
    internal bool InLowHealthMode;

    /// <summary>Per layer, the clip frame before which an upload has silenced it; zero when audible.</summary>
    internal long[] SilencedUntil = [];
}
